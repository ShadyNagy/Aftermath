namespace Aftermath.Hooks;

/// <summary>
/// Defines policies for handling exceptions thrown by hooks.
/// </summary>
public enum HookErrorPolicy
{
	/// <summary>
	/// Continue executing subsequent hooks even if a hook throws an exception.
	/// </summary>
	ContinueWithNextHook,

	/// <summary>
	/// Stop executing hooks if any hook throws an exception.
	/// </summary>
	StopExecutingHooks,

	/// <summary>
	/// Rethrow the exception after all hooks have been executed or attempted.
	/// </summary>
	RethrowAfterAllHooks
}