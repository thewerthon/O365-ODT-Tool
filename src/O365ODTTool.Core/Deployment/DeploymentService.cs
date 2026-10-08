using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Setup;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Deployment;

/// <summary>
/// Runs setup operations for configurations, writing each configuration to a transient file.
/// </summary>
/// <param name="setupProvider">The setup executable provider.</param>
/// <param name="processRunner">The process runner.</param>
/// <param name="writer">The configuration writer.</param>
/// <param name="temporaryFolder">The folder that holds the transient configuration files.</param>
public sealed class DeploymentService(ISetupProvider setupProvider, IProcessRunner processRunner, IConfigurationWriter writer, string temporaryFolder) : IDeploymentService {

	private const string downloadSwitch = "/download";
	private const string configureSwitch = "/configure";

	/// <inheritdoc />
	public Task<int> DownloadAsync(OfficeConfiguration configuration, string targetFolder, IProgress<string> log, CancellationToken cancellationToken) {

		var folder = Path.GetFullPath(targetFolder);

		Directory.CreateDirectory(folder);

		var downloadConfiguration = configuration.Clone();

		downloadConfiguration.SourcePath = folder;

		return RunAsync(SetupMode.Download, downloadConfiguration, log, cancellationToken);

	}

	/// <inheritdoc />
	public Task<int> InstallAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) => RunAsync(SetupMode.Configure, configuration, log, cancellationToken);

	private async Task<int> RunAsync(SetupMode mode, OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) {

		var setupPath = await setupProvider.EnsureSetupAsync(log, cancellationToken).ConfigureAwait(false);

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

			var exitCode = await processRunner.RunAsync(new ProcessRequest(setupPath, arguments, temporaryFolder), log, cancellationToken).ConfigureAwait(false);

			log.Report(SetupExitCodes.Describe(exitCode));

			return exitCode;

		} finally {

			File.Delete(configurationPath);

		}

	}

}
