using System.Reflection;
using Aftermath.Attributes;
using Aftermath.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aftermath.Hooks;

/// <summary>
/// Manages the execution of method hooks.
/// </summary>
public class HookManager
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<HookManager> _logger;
	private readonly AftermathOptions _options;

	/// <summary>
	/// Initializes a new instance of the <see cref="HookManager"/> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider for resolving hook dependencies.</param>
	/// <param name="logger">The logger for recording hook execution information.</param>
	/// <param name="options">The options for configuring hook behavior.</param>
	public HookManager(IServiceProvider serviceProvider, ILogger<HookManager> logger, AftermathOptions options)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}

	/// <summary>
	/// Executes all the post-execution hooks for a method.
	/// </summary>
	/// <param name="context">The context containing information about the method execution.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task ExecutePostHooksAsync(MethodExecutionContext context)
	{
		if (_options.IsReleaseMode &&
				(context.Method.GetCustomAttribute<SkipHooksInReleaseAttribute>() != null ||
				 context.Method.DeclaringType?.GetCustomAttribute<SkipHooksInReleaseAttribute>() != null))
		{
			_logger.LogDebug("Skipping hooks for {Method} in Release mode due to SkipHooksInRelease attribute",
					$"{context.Method.DeclaringType?.Name}.{context.Method.Name}");
			return;
		}

		var callAfterAttributes = context.Method
				.GetCustomAttributes<CallAfterAttribute>()
				.OrderBy(attr => attr.Order)
				.ToList();

		if (!callAfterAttributes.Any())
			return;

		_logger.LogDebug("Executing {Count} post-execution hooks for {Method}",
				callAfterAttributes.Count, $"{context.Method.DeclaringType?.Name}.{context.Method.Name}");

		foreach (var attr in callAfterAttributes)
		{
			try
			{
				var executeWhenAttr = context.Method.GetCustomAttribute<ExecuteWhenAttribute>();
				if (executeWhenAttr != null)
				{
					var conditionMethod = context.Method.DeclaringType?.GetMethod(
							executeWhenAttr.ConditionMethodName,
							BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

					if (conditionMethod != null)
					{
						bool shouldExecute;
						if (conditionMethod.IsStatic)
						{
							shouldExecute = (bool)conditionMethod.Invoke(null, new object[] { context });
						}
						else
						{
							shouldExecute = (bool)conditionMethod.Invoke(context.Instance, new object[] { context });
						}

						if (!shouldExecute)
						{
							_logger.LogDebug("Skipping hook {Hook} for {Method} due to condition method {ConditionMethod} returning false",
									$"{attr.TargetType.Name}.{attr.MethodName}",
									$"{context.Method.DeclaringType?.Name}.{context.Method.Name}",
									executeWhenAttr.ConditionMethodName);
							continue;
						}
					}
				}

				await ExecuteHookMethodAsync(context, attr);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error executing post-hook {HookMethod} for {SourceMethod}",
						$"{attr.TargetType.Name}.{attr.MethodName}",
						$"{context.Method.DeclaringType?.Name}.{context.Method.Name}");

				if (!attr.ContinueOnError)
				{
					throw new HookExecutionException(
							$"Hook method '{attr.TargetType.Name}.{attr.MethodName}' failed and ContinueOnError is false",
							ex);
				}
			}
		}
	}

	private async Task ExecuteHookMethodAsync(MethodExecutionContext context, CallAfterAttribute attribute)
	{
		var targetType = attribute.TargetType;
		var methodInfo = targetType.GetMethod(attribute.MethodName,
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Static | BindingFlags.Instance);

		if (methodInfo == null)
		{
			throw new InvalidOperationException(
					$"Hook method '{attribute.MethodName}' not found in type '{targetType.FullName}'");
		}

		object targetInstance = null;
		if (!methodInfo.IsStatic)
		{
			targetInstance = _serviceProvider.GetService(targetType);

			if (targetInstance == null)
			{
				if (_options.AutoCreateInstancesNotInDI)
				{
					try
					{
						targetInstance = ActivatorUtilities.CreateInstance(_serviceProvider, targetType);
						_logger.LogDebug("Created instance of {Type} using ActivatorUtilities", targetType.Name);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Failed to create instance of {Type} using ActivatorUtilities", targetType.Name);

						targetInstance = Activator.CreateInstance(targetType);
						_logger.LogDebug("Created instance of {Type} using parameterless constructor", targetType.Name);
					}
				}
				else
				{
					throw new InvalidOperationException(
							$"Hook type '{targetType.FullName}' is not registered in DI and AutoCreateInstancesNotInDI is false");
				}
			}
		}

		var parameters = PrepareParameters(context, methodInfo, attribute);

		_logger.LogDebug("Invoking hook method {Method}", $"{targetType.Name}.{methodInfo.Name}");

		var result = methodInfo.Invoke(targetInstance, parameters);

		if (result is Task task)
		{
			await task.ConfigureAwait(false);

			if (task.GetType().IsGenericType)
			{
				var resultProperty = task.GetType().GetProperty("Result");
				var taskResult = resultProperty?.GetValue(task);
				_logger.LogDebug("Hook method {Method} completed with result: {Result}",
						$"{targetType.Name}.{methodInfo.Name}", taskResult);
			}
			else
			{
				_logger.LogDebug("Hook method {Method} completed successfully",
						$"{targetType.Name}.{methodInfo.Name}");
			}
		}
		else if (result != null)
		{
			_logger.LogDebug("Hook method {Method} returned: {Result}",
					$"{targetType.Name}.{methodInfo.Name}", result);
		}
		else
		{
			_logger.LogDebug("Hook method {Method} completed successfully",
					$"{targetType.Name}.{methodInfo.Name}");
		}
	}

	private object[] PrepareParameters(MethodExecutionContext context, MethodInfo hookMethod,
		CallAfterAttribute attribute)
	{
		var hookParameters = hookMethod.GetParameters();
		var result = new object[hookParameters.Length];

		var parameterMappings = context.Method.GetCustomAttributes<MapParameterAttribute>()
			.ToDictionary(m => m.TargetParameterName, m => m.SourceParameterName);

		var returnValueMappings = context.Method.GetCustomAttributes<MapReturnValueAttribute>()
			.ToDictionary(m => m.TargetParameterName);

		for (int i = 0; i < hookParameters.Length; i++)
		{
			var paramInfo = hookParameters[i];
			var paramName = paramInfo.Name;

			if (returnValueMappings.ContainsKey(paramName) && attribute.IncludeReturnValue)
			{
				result[i] = context.Result;
				continue;
			}

			if (paramName == "returnValue" && attribute.IncludeReturnValue)
			{
				result[i] = context.Result;
				continue;
			}

			if (paramName == "context" && paramInfo.ParameterType == typeof(MethodExecutionContext))
			{
				result[i] = context;
				continue;
			}

			if (attribute.IncludeParameters)
			{
				var sourceParams = context.Method.GetParameters();
				var matchingParamIndex = Array.FindIndex(sourceParams, p => p.Name == paramName);

				if (matchingParamIndex >= 0 && matchingParamIndex < context.Arguments.Length)
				{
					result[i] = context.Arguments[matchingParamIndex];
					continue;
				}
			}

			if (_options.AutoResolveParametersFromDI)
			{
				var service = _serviceProvider.GetService(paramInfo.ParameterType);
				if (service != null)
				{
					result[i] = service;
					continue;
				}
			}

			result[i] = paramInfo.HasDefaultValue ? paramInfo.DefaultValue : null;
		}

		return result;
	}
}