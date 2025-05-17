namespace Aftermath.Attributes;

/// <summary>
/// Maps a parameter from the source method to a parameter in the target hook method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MapParameterAttribute : Attribute
{
	/// <summary>
	/// Gets the name of the parameter in the source method.
	/// </summary>
	public string SourceParameterName { get; }

	/// <summary>
	/// Gets the name of the parameter in the target hook method.
	/// </summary>
	public string TargetParameterName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MapParameterAttribute"/> class.
	/// </summary>
	/// <param name="sourceParameterName">The name of the parameter in the source method.</param>
	/// <param name="targetParameterName">The name of the parameter in the target hook method.</param>
	public MapParameterAttribute(string sourceParameterName, string targetParameterName)
	{
		SourceParameterName = sourceParameterName ?? throw new ArgumentNullException(nameof(sourceParameterName));
		TargetParameterName = targetParameterName ?? throw new ArgumentNullException(nameof(targetParameterName));
	}
}