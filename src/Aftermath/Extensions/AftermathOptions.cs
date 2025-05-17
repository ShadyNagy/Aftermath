using Aftermath.Hooks;

namespace Aftermath.Extensions;

/// <summary>
/// Options for configuring Aftermath behavior.
/// </summary>
public class AftermathOptions
{
	/// <summary>
	/// Gets or sets whether instances of hook target types not registered in DI should be auto-created.
	/// Default is true.
	/// </summary>
	public bool AutoCreateInstancesNotInDI { get; set; } = true;

	/// <summary>
	/// Gets or sets whether parameters in hook methods should be automatically resolved from DI when possible.
	/// Default is true.
	/// </summary>
	public bool AutoResolveParametersFromDI { get; set; } = true;

	/// <summary>
	/// Gets or sets whether hooks should be verbose in logging their activity.
	/// Default is false.
	/// </summary>
	public bool VerboseLogging { get; set; } = false;

	/// <summary>
	/// Gets or sets whether we're running in Release mode.
	/// In Release mode, hooks with the SkipHooksInRelease attribute will be skipped.
	/// Default is determined by the absence of the DEBUG symbol.
	/// </summary>
	public bool IsReleaseMode { get; set; } =
#if DEBUG
			false;
#else
      true;
#endif

	/// <summary>
	/// Gets or sets the global timeout for hook executions in milliseconds.
	/// After this timeout, hook executions will be cancelled.
	/// Set to null for no timeout (not recommended for production).
	/// Default is 30000 (30 seconds).
	/// </summary>
	public int? HookExecutionTimeoutMs { get; set; } = 30000;

	/// <summary>
	/// Gets or sets the policy for handling hooks that throw exceptions.
	/// Default is ContinueWithNextHook.
	/// </summary>
	public HookErrorPolicy GlobalErrorPolicy { get; set; } = HookErrorPolicy.ContinueWithNextHook;
}