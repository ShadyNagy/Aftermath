namespace Aftermath.Attributes;

/// <summary>
/// Injects a custom value into a parameter of the target hook method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class InjectParameterAttribute : Attribute
{
	/// <summary>
	/// Gets the name of the parameter in the target hook method.
	/// </summary>
	public string TargetParameterName { get; }

	/// <summary>
	/// Gets the value to inject into the target parameter.
	/// </summary>
	public object Value { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="InjectParameterAttribute"/> class.
	/// </summary>
	/// <param name="targetParameterName">The name of the parameter in the target hook method.</param>
	/// <param name="value">The value to inject into the target parameter.</param>
	public InjectParameterAttribute(string targetParameterName, object value)
	{
		TargetParameterName = targetParameterName ?? throw new ArgumentNullException(nameof(targetParameterName));
		Value = value;
	}
}