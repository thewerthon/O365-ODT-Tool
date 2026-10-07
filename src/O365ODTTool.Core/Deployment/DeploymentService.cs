using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Setup;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Deployment;

/// <summary>
/// Runs setup operations for configurations, obtaining setup next to the tool when needed.
/// </summary>
/// <param name="setupProvider">The setup executable provider.</param>
/// <param name="processRunner">The process runner.</param>
/// <param name="writer">The configuration writer.</param>
/// <param name="toolFolder">The folder that holds the setup executable.</param>
/// <param name="temporaryFolder">The folder that holds the transient configuration files.</param>
public sealed class DeploymentService(ISetupProvider setupProvider, IProcessRunner processRunner, IConfigurationWriter writer, string toolFolder, string temporaryFolder) : IDeploymentService {

	/// <summary>The configuration file name written into download folders.</summary>
	public const string OfflineConfigurationFileName = "configuration.xml";

	private const string downloadSwitch = "/download";
	private const string configureSwitch = "/configure";

	/// <inheritdoc />
	public async Task<int> DownloadAsync(OfficeConfiguration configuration, string targetFolder, IProgress<string> log, CancellationToken cancellationToken) {

		var folder = Path.GetFullPath(targetFolder);

		Directory.CreateDirectory(folder);

		var downloadConfiguration = configuration.Clone();

		downloadConfiguration.SourcePath = folder;

		var exitCode = await RunAsync(SetupMode.Download, downloadConfiguration, log, cancellationToken).ConfigureAwait(false);

		if (exitCode != SetupExitCodes.Success) return exitCode;

		PrepareOfflineInstallation(downloadConfiguration, folder, log);

		return exitCode;

	}

	/// <inheritdoc />
	public Task<int> InstallAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) => RunAsync(SetupMode.Configure, configuration, log, cancellationToken);

	private static bool IsSamePath(string first, string second) => string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.OrdinalIgnoreCase);

	private async Task<int> RunAsync(SetupMode mode, OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) {

		var setupPath = await setupProvider.EnsureSetupAsync(toolFolder, log, cancellationToken).ConfigureAwait(false);

		Directory.CreateDirectory(temporaryFolder);

		var configurationPath = Path.Combine(temporaryFolder, $"O365ODTTool-{mode}-{Guid.NewGuid():N}.xml");

		writer.WriteFile(configuration, configurationPath);

		try {

			var setupSwitch = mode switch {
				SetupMode.Download => downloadSwitch,
				_ => configureSwitch,
			};
			var arguments = $"{setupSwitch} \"{configurationPath}\"";

			log.Report($"Running {SetupProvider.SetupFileName} {arguments}");

			var exitCode = await processRunner.RunAsync(new ProcessRequest(setupPath, arguments, toolFolder), log, cancellationToken).ConfigureAwait(false);

			log.Report(SetupExitCodes.Describe(exitCode));

			return exitCode;

		} finally {

			File.Delete(configurationPath);

		}

	}

	private void PrepareOfflineInstallation(OfficeConfiguration configuration, string folder, IProgress<string> log) {

		var setupSource = setupProvider.GetSetupPath(toolFolder);
		var setupTarget = setupProvider.GetSetupPath(folder);
		var configurationPath = Path.Combine(folder, OfflineConfigurationFileName);

		if (!IsSamePath(setupSource, setupTarget)) File.Copy(setupSource, setupTarget, overwrite: true);

		writer.WriteFile(configuration, configurationPath);

		log.Report($"Offline installation ready: \"{setupTarget}\" {configureSwitch} \"{configurationPath}\"");

	}

}
