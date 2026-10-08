using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Deployment;

/// <summary>
/// Runs setup operations for configurations.
/// </summary>
public interface IDeploymentService {

	/// <summary>
	/// Downloads the installation files for later installations, completing any previous download, and returns the setup exit code.
	/// </summary>
	Task<int> DownloadAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken);

	/// <summary>
	/// Installs or reconfigures the products, reusing downloaded files when available, and returns the setup exit code.
	/// </summary>
	Task<int> InstallAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken);

}
