namespace O365ODTTool.Core.Deployment;

/// <summary>
/// Describes the exit codes returned by setup.
/// </summary>
public static class SetupExitCodes {

	/// <summary>The success exit code.</summary>
	public const int Success = 0;

	private static readonly Dictionary<int, string> descriptions = new() {
		[Success] = "Completed successfully.",
		[17002] = "Setup could not complete; another installation may be running, setup was cancelled, or the files are incomplete.",
		[17004] = "The requested products or languages are not available for the selected channel or version.",
	};

	/// <summary>
	/// Describes an exit code.
	/// </summary>
	public static string Describe(int exitCode) {

		var description = descriptions.TryGetValue(exitCode, out var known) ? known : "Setup failed.";

		return $"Exit code {exitCode}: {description}";

	}

}
