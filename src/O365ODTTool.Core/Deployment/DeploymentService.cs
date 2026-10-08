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
/// <param name="localSourceFolder">The folder whose previously downloaded files are reused by installations.</param>
public sealed class DeploymentService(ISetupProvider setupProvider, IProcessRunner processRunner, IConfigurationWriter writer, string temporaryFolder, string localSourceFolder) : IDeploymentService {

	/// <summary>The relative path of the installation files inside a download folder.</summary>
	public static readonly string DownloadedFilesFolder = Path.Combine("Office", "Data");

	/// <summary>The configuration file name written into download folders.</summary>
	public const string OfflineConfigurationFileName = "configuration.xml";

	/// <summary>The installation script file name written into download folders.</summary>
	public const string OfflineInstallScriptFileName = "Install.cmd";

	private const string downloadSwitch = "/download";
	private const string configureSwitch = "/configure";

	// Without a source path, setup installs from the files next to it, so the folder works wherever it is copied.
	private static readonly string offlineInstallScript = string.Join("\r\n",
		"@echo off",
		$"\"%~dp0{SetupProvider.SetupFileName}\" {configureSwitch} \"%~dp0{OfflineConfigurationFileName}\"",
		string.Empty);

	/// <inheritdoc />
	public async Task<int> DownloadAsync(OfficeConfiguration configuration, string targetFolder, IProgress<string> log, CancellationToken cancellationToken) {

		var folder = Path.GetFullPath(targetFolder);

		Directory.CreateDirectory(folder);

		var downloadConfiguration = configuration.Clone();

		downloadConfiguration.SourcePath = folder;

		var setupPath = await setupProvider.EnsureSetupAsync(log, cancellationToken).ConfigureAwait(false);
		var exitCode = await RunAsync(setupPath, SetupMode.Download, downloadConfiguration, log, cancellationToken).ConfigureAwait(false);

		if (exitCode == SetupExitCodes.Success) PrepareOfflineInstallation(configuration, setupPath, folder, log);

		return exitCode;

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

	// Reuses files downloaded next to the tool, while the content delivery network supplies anything they lack; an imported source path always wins.
	private OfficeConfiguration WithLocalSource(OfficeConfiguration configuration, IProgress<string> log) {

		if (configuration.SourcePath is not null || !HasDownloadedFiles(localSourceFolder)) return configuration;

		var localConfiguration = configuration.Clone();

		localConfiguration.SourcePath = localSourceFolder;
		localConfiguration.AllowCdnFallback ??= true;

		log.Report($"Installing from downloaded files in {localSourceFolder}");

		return localConfiguration;

	}

	private static bool IsSamePath(string first, string second) => string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.OrdinalIgnoreCase);

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

	private void PrepareOfflineInstallation(OfficeConfiguration configuration, string setupPath, string folder, IProgress<string> log) {

		var offlineConfiguration = configuration.Clone();
		var setupTarget = Path.Combine(folder, SetupProvider.SetupFileName);

		offlineConfiguration.SourcePath = null;

		if (!IsSamePath(setupPath, setupTarget)) File.Copy(setupPath, setupTarget, overwrite: true);

		writer.WriteFile(offlineConfiguration, Path.Combine(folder, OfflineConfigurationFileName));
		File.WriteAllText(Path.Combine(folder, OfflineInstallScriptFileName), offlineInstallScript);

		log.Report($"Offline installation ready in {folder}");

	}

}
