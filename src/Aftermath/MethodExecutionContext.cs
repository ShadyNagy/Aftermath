using System.Reflection;

namespace Aftermath;

/// <summary>
/// Provides context information about a method execution for hook methods.
/// </summary>
public class MethodExecutionContext
{
	/// <summary>
	/// Gets the method that was executed.
	/// </summary>
	public MethodInfo Method { get; internal set; }

	/// <summary>
	/// Gets the instance on which the method was executed, or null if the method is static.
	/// </summary>
	public object Instance { get; internal set; }

	/// <summary>
	/// Gets the arguments passed to the method.
	/// </summary>
	public object[] Arguments { get; internal set; }

	/// <summary>
	/// Gets the result returned by the method, or null if the method returns void or an exception was thrown.
	/// </summary>
	public object Result { get; internal set; }

	/// <summary>
	/// Gets the time it took to execute the method.
	/// </summary>
	public TimeSpan ExecutionTime { get; internal set; }

	/// <summary>
	/// Gets the exception thrown by the method, or null if no exception was thrown.
	/// </summary>
	public Exception Exception { get; internal set; }

	/// <summary>
	/// Gets a dictionary of custom data that can be shared between hooks.
	/// </summary>
	public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

	/// <summary>
	/// Gets the timestamp when the method execution started.
	/// </summary>
	public DateTimeOffset StartTime { get; internal set; }

	/// <summary>
	/// Gets the calling method information, if available.
	/// </summary>
	public MethodBase CallingMethod { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the method execution was successful (no exception was thrown).
	/// </summary>
	public bool IsSuccess => Exception == null;

	/// <summary>
	/// Gets a parameter value by name.
	/// </summary>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <returns>The parameter value, or null if not found.</returns>
	public object GetParameterValue(string parameterName)
	{
		var parameters = Method.GetParameters();
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].Name == parameterName && i < Arguments.Length)
			{
				return Arguments[i];
			}
		}

		return null;
	}

	/// <summary>
	/// Gets a parameter value by name and attempts to cast it to the specified type.
	/// </summary>
	/// <typeparam name="T">The type to cast the parameter value to.</typeparam>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <returns>The parameter value cast to type T, or default(T) if not found or cannot be cast.</returns>
	public T GetParameterValue<T>(string parameterName)
	{
		var value = GetParameterValue(parameterName);
		if (value is T typedValue)
		{
			return typedValue;
		}

		return default;
	}

	/// <summary>
	/// Gets the result of the method execution cast to the specified type.
	/// </summary>
	/// <typeparam name="T">The type to cast the result to.</typeparam>
	/// <returns>The result cast to type T, or default(T) if the result is null or cannot be cast.</returns>
	public T GetResult<T>()
	{
		if (Result is T typedResult)
		{
			return typedResult;
		}

		return default;
	}

	/// <summary>
	/// Creates a human-readable string representation of the method execution.
	/// </summary>
	/// <returns>A string that represents the method execution.</returns>
	public override string ToString()
	{
		var methodName = Method.DeclaringType?.FullName + "." + Method.Name;
		var status = IsSuccess ? "succeeded" : "failed";
		var time = ExecutionTime.TotalMilliseconds.ToString("F2");

		return $"Method {methodName} {status} in {time}ms";
	}
}