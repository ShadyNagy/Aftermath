namespace Aftermath.Attributes;

/// <summary>
/// Specifies a method in the source class that determines whether the hook should be executed.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ExecuteWhenAttribute : Attribute
{
	/// <summary>
	/// Gets the name of the method that determines whether the hook should be executed.
	/// </summary>
	public string ConditionMethodName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ExecuteWhenAttribute"/> class.
	/// </summary>
	/// <param name="conditionMethodName">The name of the method that determines whether the hook should be executed.</param>
	public ExecuteWhenAttribute(string conditionMethodName)
	{
		ConditionMethodName = conditionMethodName ?? throw new ArgumentNullException(nameof(conditionMethodName));
	}
}