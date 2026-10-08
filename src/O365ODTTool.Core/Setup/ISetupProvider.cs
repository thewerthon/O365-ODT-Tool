namespace O365ODTTool.Core.Setup;

/// <summary>
/// Provides the Office Deployment Tool setup executable.
/// </summary>
public interface ISetupProvider {

	/// <summary>
	/// Ensures a trusted setup executable is available, downloading it when missing, and returns its path.
	/// </summary>
	/// <exception cref="SetupException">The executable could not be obtained or is not trusted.</exception>
	Task<string> EnsureSetupAsync(IProgress<string> log, CancellationToken cancellationToken);

}
