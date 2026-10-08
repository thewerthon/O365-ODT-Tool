namespace O365ODTTool.Core.Deployment;

/// <summary>
/// Describes the exit codes returned by setup in user-facing terms.
/// </summary>
public static class SetupExitCodes {

	/// <summary>The success exit code.</summary>
	public const int Success = 0;

	private const string successDescription = "Completed successfully.";
	private const string unknownFailureDescription = "The operation failed.";

	private static readonly Dictionary<int, string> failureDescriptions = new() {
		[17002] = "The operation could not complete. Another installation may be running, or it was cancelled.",
		[17004] = "The selected products or languages are not available for the selected channel.",
	};

	/// <summary>
	/// Describes an exit code; failures include the code so they can be looked up.
	/// </summary>
	public static string Describe(int exitCode) {

		if (exitCode == Success) return successDescription;

		var description = failureDescriptions.TryGetValue(exitCode, out var known) ? known : unknownFailureDescription;

		return $"{description} (error {exitCode})";

	}

}
