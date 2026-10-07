namespace O365ODTTool.Core.Setup;

/// <summary>
/// Provides the Office Deployment Tool setup executable.
/// </summary>
public interface ISetupProvider {

	/// <summary>
	/// Gets the setup executable path inside a folder.
	/// </summary>
	string GetSetupPath(string folder);

	/// <summary>
	/// Ensures a trusted setup executable exists inside a folder, downloading it when missing, and returns its path.
	/// </summary>
	/// <exception cref="SetupException">The executable could not be obtained or is not trusted.</exception>
	Task<string> EnsureSetupAsync(string folder, IProgress<string> log, CancellationToken cancellationToken);

}
