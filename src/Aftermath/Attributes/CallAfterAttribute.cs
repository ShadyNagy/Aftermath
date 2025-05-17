namespace Aftermath.Attributes;

/// <summary>
/// Specifies a method to be called after the decorated method completes execution.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CallAfterAttribute : Attribute
{
	/// <summary>
	/// Gets the type containing the target method to call.
	/// </summary>
	public Type TargetType { get; }

	/// <summary>
	/// Gets the name of the method to call.
	/// </summary>
	public string MethodName { get; }

	/// <summary>
	/// Gets or sets whether to include the return value when mapping parameters to the target method.
	/// Default is true.
	/// </summary>
	public bool IncludeReturnValue { get; set; } = true;

	/// <summary>
	/// Gets or sets whether to include parameters from the source method when mapping to the target method.
	/// Default is true.
	/// </summary>
	public bool IncludeParameters { get; set; } = true;

	/// <summary>
	/// Gets or sets whether to continue executing subsequent hooks if this hook throws an exception.
	/// Default is true.
	/// </summary>
	public bool ContinueOnError { get; set; } = true;

	/// <summary>
	/// Gets or sets the order in which this hook will be executed relative to other hooks on the same method.
	/// Hooks with lower order values execute first.
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CallAfterAttribute"/> class with the specified target type and method name.
	/// </summary>
	/// <param name="targetType">The type containing the target method to call.</param>
	/// <param name="methodName">The name of the method to call.</param>
	public CallAfterAttribute(Type targetType, string methodName)
	{
		TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
		MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CallAfterAttribute"/> class with the specified target type name and method name.
	/// </summary>
	/// <param name="targetTypeName">The fully qualified name of the type containing the target method to call.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <exception cref="TypeLoadException">Thrown when the type specified by <paramref name="targetTypeName"/> cannot be loaded.</exception>
	public CallAfterAttribute(string targetTypeName, string methodName)
	{
		if (string.IsNullOrEmpty(targetTypeName))
			throw new ArgumentNullException(nameof(targetTypeName));

		TargetType = Type.GetType(targetTypeName, throwOnError: true)
				?? throw new TypeLoadException($"Could not load type '{targetTypeName}'");

		MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
	}
}