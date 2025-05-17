using System.Diagnostics;
using System.Reflection;
using Aftermath.Attributes;
using Castle.DynamicProxy;

namespace Aftermath.Hooks;

/// <summary>
/// Intercepts method calls to execute hooks after method execution.
/// </summary>
internal class HookInterceptor : IInterceptor
{
	private readonly HookManager _hookManager;

	/// <summary>
	/// Initializes a new instance of the <see cref="HookInterceptor"/> class.
	/// </summary>
	/// <param name="hookManager">The hook manager for executing hooks.</param>
	public HookInterceptor(HookManager hookManager)
	{
		_hookManager = hookManager ?? throw new ArgumentNullException(nameof(hookManager));
	}

	/// <inheritdoc />
	public void Intercept(IInvocation invocation)
	{
		var methodInfo = invocation.Method;

		var hasHooks = methodInfo.GetCustomAttributes<CallAfterAttribute>().Any();

		if (!hasHooks)
		{
			invocation.Proceed();
			return;
		}

		var stopwatch = Stopwatch.StartNew();
		var startTime = DateTimeOffset.UtcNow;

		try
		{
			MethodBase callingMethod = new StackTrace().GetFrame(2)?.GetMethod();

			invocation.Proceed();

			if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
			{
				HandleAsyncMethod(invocation, methodInfo, stopwatch, startTime, callingMethod);
				return;
			}

			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = invocation.InvocationTarget,
				Arguments = invocation.Arguments,
				Result = invocation.ReturnValue,
				ExecutionTime = stopwatch.Elapsed,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			var hookTask = _hookManager.ExecutePostHooksAsync(context);

			hookTask.ConfigureAwait(false).GetAwaiter().GetResult();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = invocation.InvocationTarget,
				Arguments = invocation.Arguments,
				ExecutionTime = stopwatch.Elapsed,
				Exception = ex,
				StartTime = startTime
			};

			var hookTask = _hookManager.ExecutePostHooksAsync(context);

			hookTask.ConfigureAwait(false).GetAwaiter().GetResult();

			throw;
		}
	}

	private void HandleAsyncMethod(IInvocation invocation, MethodInfo methodInfo, Stopwatch stopwatch,
			DateTimeOffset startTime, MethodBase callingMethod)
	{
		if (methodInfo.ReturnType == typeof(Task))
		{
			invocation.ReturnValue = HandleTaskMethod(
					(Task)invocation.ReturnValue,
					methodInfo,
					invocation.InvocationTarget,
					invocation.Arguments,
					stopwatch,
					startTime,
					callingMethod);
		}
		else if (methodInfo.ReturnType.IsGenericType &&
						 methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
		{
			var genericArgType = methodInfo.ReturnType.GetGenericArguments()[0];
			var genericMethod = typeof(HookInterceptor)
					.GetMethod(nameof(HandleTaskTMethod), BindingFlags.NonPublic | BindingFlags.Instance)
					.MakeGenericMethod(genericArgType);

			invocation.ReturnValue = genericMethod.Invoke(this, new object[]
			{
										invocation.ReturnValue,
										methodInfo,
										invocation.InvocationTarget,
										invocation.Arguments,
										stopwatch,
										startTime,
										callingMethod
			});
		}
		else if (methodInfo.ReturnType == typeof(ValueTask))
		{
			invocation.ReturnValue = HandleValueTaskMethod(
					(ValueTask)invocation.ReturnValue,
					methodInfo,
					invocation.InvocationTarget,
					invocation.Arguments,
					stopwatch,
					startTime,
					callingMethod);
		}
		else if (methodInfo.ReturnType.IsGenericType &&
						 methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
		{
			var genericArgType = methodInfo.ReturnType.GetGenericArguments()[0];
			var genericMethod = typeof(HookInterceptor)
					.GetMethod(nameof(HandleValueTaskTMethod), BindingFlags.NonPublic | BindingFlags.Instance)
					.MakeGenericMethod(genericArgType);

			invocation.ReturnValue = genericMethod.Invoke(this, new object[]
			{
										invocation.ReturnValue,
										methodInfo,
										invocation.InvocationTarget,
										invocation.Arguments,
										stopwatch,
										startTime,
										callingMethod
			});
		}
	}

	private async Task HandleTaskMethod(Task task, MethodInfo methodInfo, object instance, object[] arguments,
			Stopwatch stopwatch, DateTimeOffset startTime, MethodBase callingMethod)
	{
		try
		{
			await task.ConfigureAwait(false);

			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				Result = null,
				ExecutionTime = stopwatch.Elapsed,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				ExecutionTime = stopwatch.Elapsed,
				Exception = ex,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);

			throw;
		}
	}

	private async Task<T> HandleTaskTMethod<T>(Task<T> task, MethodInfo methodInfo, object instance,
			object[] arguments, Stopwatch stopwatch, DateTimeOffset startTime, MethodBase callingMethod)
	{
		try
		{
			var result = await task.ConfigureAwait(false);

			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				Result = result,
				ExecutionTime = stopwatch.Elapsed,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);

			return result;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				ExecutionTime = stopwatch.Elapsed,
				Exception = ex,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);

			throw;
		}
	}

	private async ValueTask HandleValueTaskMethod(ValueTask task, MethodInfo methodInfo, object instance,
			object[] arguments, Stopwatch stopwatch, DateTimeOffset startTime, MethodBase callingMethod)
	{
		try
		{
			await task.ConfigureAwait(false);

			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				Result = null,
				ExecutionTime = stopwatch.Elapsed,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				ExecutionTime = stopwatch.Elapsed,
				Exception = ex,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);

			throw;
		}
	}

	private async ValueTask<T> HandleValueTaskTMethod<T>(ValueTask<T> task, MethodInfo methodInfo, object instance,
			object[] arguments, Stopwatch stopwatch, DateTimeOffset startTime, MethodBase callingMethod)
	{
		try
		{
			var result = await task.ConfigureAwait(false);

			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				Result = result,
				ExecutionTime = stopwatch.Elapsed,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);

			return result;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			var context = new MethodExecutionContext
			{
				Method = methodInfo,
				Instance = instance,
				Arguments = arguments,
				ExecutionTime = stopwatch.Elapsed,
				Exception = ex,
				StartTime = startTime,
				CallingMethod = callingMethod
			};

			await _hookManager.ExecutePostHooksAsync(context).ConfigureAwait(false);

			throw;
		}
	}
}