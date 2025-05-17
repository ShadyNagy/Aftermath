namespace Aftermath.Attributes;

/// <summary>
/// Indicates that hooks should be skipped in Release mode for performance reasons.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SkipHooksInReleaseAttribute : Attribute
{
}