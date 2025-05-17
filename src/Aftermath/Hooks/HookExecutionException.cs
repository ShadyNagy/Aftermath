namespace Aftermath.Hooks;

/// <summary>
/// Exception that is thrown when a hook method fails to execute.
/// </summary>
public class HookExecutionException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="HookExecutionException"/> class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public HookExecutionException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HookExecutionException"/> class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public HookExecutionException(string message, Exception innerException) : base(message, innerException)
	{
	}
}