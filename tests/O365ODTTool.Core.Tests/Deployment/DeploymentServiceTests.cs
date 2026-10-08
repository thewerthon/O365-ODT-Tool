using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Deployment;
using O365ODTTool.Core.Setup;
using O365ODTTool.Core.Tests.Setup;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Tests.Deployment;

public sealed class DeploymentServiceTests : IDisposable {

	private static readonly byte[] setupContent = [1, 2, 3];

	private readonly TemporaryFolder folder = new();
	private readonly FakeProcessRunner processRunner = new();
	private readonly ConfigurationReader reader = new();

	public DeploymentServiceTests() {

		Directory.CreateDirectory(ToolFolder);
		File.WriteAllBytes(SetupPath, setupContent);

	}

	[Fact]
	public async Task InstallAsync_RunsConfigureWithTransientFile() {

		var exitCode = await CreateService().InstallAsync(new OfficeConfiguration { Channel = "Current" }, new SynchronousProgress(), CancellationToken.None);

		var request = Assert.Single(processRunner.Requests);

		Assert.Equal(0, exitCode);
		Assert.Equal(SetupPath, request.FileName);
		Assert.StartsWith("/configure \"", request.Arguments, StringComparison.Ordinal);
		Assert.Equal("Current", reader.Read(processRunner.Configurations.Single()).Channel);
		Assert.Empty(Directory.GetFiles(TemporaryFolder));

	}

	[Fact]
	public async Task InstallAsync_KeepsImportedSourcePath() {

		await CreateService().InstallAsync(new OfficeConfiguration { SourcePath = @"\\server\share" }, new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(@"\\server\share", reader.Read(processRunner.Configurations.Single()).SourcePath);

	}

	[Fact]
	public async Task InstallAsync_WithoutDownloadedFiles_UsesConfigurationAsIs() {

		await CreateService().InstallAsync(new OfficeConfiguration { Channel = "Current" }, new SynchronousProgress(), CancellationToken.None);

		var configuration = reader.Read(processRunner.Configurations.Single());

		Assert.Null(configuration.SourcePath);
		Assert.Null(configuration.AllowCdnFallback);

	}

	[Fact]
	public async Task InstallAsync_ReusesDownloadedFilesWithFallback() {

		CreateDownloadedFiles();

		var input = new OfficeConfiguration { Channel = "Current" };

		await CreateService().InstallAsync(input, new SynchronousProgress(), CancellationToken.None);

		var configuration = reader.Read(processRunner.Configurations.Single());

		Assert.Equal(ToolFolder, configuration.SourcePath);
		Assert.True(configuration.AllowCdnFallback);
		Assert.Null(input.SourcePath);

	}

	[Fact]
	public async Task InstallAsync_KeepsImportedFallbackChoice() {

		CreateDownloadedFiles();

		await CreateService().InstallAsync(new OfficeConfiguration { AllowCdnFallback = false }, new SynchronousProgress(), CancellationToken.None);

		Assert.False(reader.Read(processRunner.Configurations.Single()).AllowCdnFallback);

	}

	[Fact]
	public async Task InstallAsync_PrefersImportedSourcePathOverDownloadedFiles() {

		CreateDownloadedFiles();

		await CreateService().InstallAsync(new OfficeConfiguration { SourcePath = @"\\server\share" }, new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(@"\\server\share", reader.Read(processRunner.Configurations.Single()).SourcePath);

	}

	[Fact]
	public async Task InstallAsync_IgnoresEmptyDownloadFolder() {

		Directory.CreateDirectory(Path.Combine(ToolFolder, DeploymentService.DownloadedFilesFolder));

		await CreateService().InstallAsync(new OfficeConfiguration(), new SynchronousProgress(), CancellationToken.None);

		Assert.Null(reader.Read(processRunner.Configurations.Single()).SourcePath);

	}

	[Fact]
	public async Task DownloadAsync_RunsDownloadIntoTargetFolder() {

		var target = folder.Combine("download");

		var exitCode = await CreateService().DownloadAsync(new OfficeConfiguration { Channel = "Current" }, target, new SynchronousProgress(), CancellationToken.None);

		var request = Assert.Single(processRunner.Requests);

		Assert.Equal(0, exitCode);
		Assert.StartsWith("/download \"", request.Arguments, StringComparison.Ordinal);
		Assert.Equal(target, reader.Read(processRunner.Configurations.Single()).SourcePath);

	}

	[Fact]
	public async Task DownloadAsync_PreparesPortableOfflineInstallation() {

		var target = folder.Combine("download");

		await CreateService().DownloadAsync(new OfficeConfiguration { Channel = "Current", SourcePath = @"\\server\share" }, target, new SynchronousProgress(), CancellationToken.None);

		var offlineConfiguration = reader.ReadFile(Path.Combine(target, DeploymentService.OfflineConfigurationFileName));
		var script = File.ReadAllText(Path.Combine(target, DeploymentService.OfflineInstallScriptFileName));

		Assert.Null(offlineConfiguration.SourcePath);
		Assert.Equal("Current", offlineConfiguration.Channel);
		Assert.Equal(setupContent, File.ReadAllBytes(Path.Combine(target, SetupProvider.SetupFileName)));
		Assert.Contains($"\"%~dp0{SetupProvider.SetupFileName}\" /configure \"%~dp0{DeploymentService.OfflineConfigurationFileName}\"", script, StringComparison.Ordinal);

	}

	[Fact]
	public async Task DownloadAsync_SkipsOfflineInstallationOnFailure() {

		var target = folder.Combine("download");

		processRunner.ExitCode = 17002;

		var exitCode = await CreateService().DownloadAsync(new OfficeConfiguration(), target, new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(17002, exitCode);
		Assert.Empty(Directory.GetFiles(target));

	}

	[Fact]
	public async Task DownloadAsync_DoesNotChangeInputConfiguration() {

		var configuration = new OfficeConfiguration { Channel = "Current" };

		await CreateService().DownloadAsync(configuration, folder.Combine("download"), new SynchronousProgress(), CancellationToken.None);

		Assert.Null(configuration.SourcePath);

	}

	[Fact]
	public async Task InstallAsync_ReportsExitCodeDescription() {

		var log = new SynchronousProgress();

		processRunner.ExitCode = 17004;

		var exitCode = await CreateService().InstallAsync(new OfficeConfiguration(), log, CancellationToken.None);

		Assert.Equal(17004, exitCode);
		Assert.Contains(SetupExitCodes.Describe(17004), log.Messages);

	}

	public void Dispose() => folder.Dispose();

	private string ToolFolder => folder.Combine("tool");

	private string SetupPath => Path.Combine(ToolFolder, SetupProvider.SetupFileName);

	private string TemporaryFolder => folder.Combine("temp");

	private DeploymentService CreateService() => new(new FakeSetupProvider(SetupPath), processRunner, new ConfigurationWriter(), TemporaryFolder, ToolFolder);

	private void CreateDownloadedFiles() {

		var filesFolder = Path.Combine(ToolFolder, DeploymentService.DownloadedFilesFolder);

		Directory.CreateDirectory(filesFolder);
		File.WriteAllBytes(Path.Combine(filesFolder, "v64.cab"), [1]);

	}

	private sealed class FakeSetupProvider(string path) : ISetupProvider {

		public Task<string> EnsureSetupAsync(IProgress<string> log, CancellationToken cancellationToken) => Task.FromResult(path);

	}

	private sealed class FakeProcessRunner : IProcessRunner {

		public int ExitCode { get; set; }

		public List<ProcessRequest> Requests { get; } = [];

		public List<string> Configurations { get; } = [];

		public Task<int> RunAsync(ProcessRequest request, IProgress<string> output, CancellationToken cancellationToken) {

			var configurationPath = request.Arguments.Split('"')[1];

			Requests.Add(request);
			Configurations.Add(File.ReadAllText(configurationPath));

			return Task.FromResult(ExitCode);

		}

	}

}
