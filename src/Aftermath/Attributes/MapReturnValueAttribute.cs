namespace Aftermath.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MapReturnValueAttribute : Attribute
{
	public string TargetParameterName { get; }

	public MapReturnValueAttribute(string targetParameterName)
	{
		TargetParameterName = targetParameterName ?? throw new ArgumentNullException(nameof(targetParameterName));
	}
}