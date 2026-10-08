using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Deployment;

/// <summary>
/// Runs setup operations for configurations.
/// </summary>
public interface IDeploymentService {

	/// <summary>
	/// Downloads the installation files into a folder and returns the setup exit code.
	/// </summary>
	Task<int> DownloadAsync(OfficeConfiguration configuration, string targetFolder, IProgress<string> log, CancellationToken cancellationToken);

	/// <summary>
	/// Installs or reconfigures the products and returns the setup exit code.
	/// </summary>
	Task<int> InstallAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken);

}
