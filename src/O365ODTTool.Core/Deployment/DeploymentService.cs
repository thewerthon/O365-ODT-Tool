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
/// <param name="localSourceFolder">The folder that receives downloads and whose downloaded files are reused by installations.</param>
public sealed class DeploymentService(ISetupProvider setupProvider, IProcessRunner processRunner, IConfigurationWriter writer, string temporaryFolder, string localSourceFolder) : IDeploymentService {

	/// <summary>The relative path where setup stores downloaded files inside the source folder.</summary>
	public static readonly string DownloadedFilesFolder = Path.Combine("Office", "Data");

	private const string downloadSwitch = "/download";
	private const string configureSwitch = "/configure";

	/// <inheritdoc />
	public async Task<int> DownloadAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) {

		var downloadConfiguration = configuration.Clone();

		// Setup only fetches what the folder lacks, so repeated downloads complete or update the existing files.
		downloadConfiguration.SourcePath = localSourceFolder;

		var setupPath = await setupProvider.EnsureSetupAsync(log, cancellationToken).ConfigureAwait(false);

		return await RunAsync(setupPath, SetupMode.Download, downloadConfiguration, log, cancellationToken).ConfigureAwait(false);

	}

	/// <inheritdoc />
	public async Task<int> InstallAsync(OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) {

		var setupPath = await setupProvider.EnsureSetupAsync(log, cancellationToken).ConfigureAwait(false);

		return await RunAsync(setupPath, SetupMode.Configure, WithLocalSource(configuration, log), log, cancellationToken).ConfigureAwait(false);

	}

	private static bool HasDownloadedFiles(string folder) {

		var filesFolder = Path.Combine(folder, DownloadedFilesFolder);

		return Directory.Exists(filesFolder) && Directory.EnumerateFileSystemEntries(filesFolder).Any();

	}

	// Reuses downloaded files while the content delivery network supplies anything they lack; an imported source path always wins.
	private OfficeConfiguration WithLocalSource(OfficeConfiguration configuration, IProgress<string> log) {

		if (configuration.SourcePath is not null || !HasDownloadedFiles(localSourceFolder)) return configuration;

		var localConfiguration = configuration.Clone();

		localConfiguration.SourcePath = localSourceFolder;
		localConfiguration.AllowCdnFallback ??= true;

		log.Report($"Installing from downloaded files in {localSourceFolder}");

		return localConfiguration;

	}

	private async Task<int> RunAsync(string setupPath, SetupMode mode, OfficeConfiguration configuration, IProgress<string> log, CancellationToken cancellationToken) {

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
