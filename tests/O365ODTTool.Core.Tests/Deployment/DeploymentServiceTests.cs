using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Deployment;
using O365ODTTool.Core.Setup;
using O365ODTTool.Core.Tests.Setup;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Tests.Deployment;

public sealed class DeploymentServiceTests : IDisposable {

	private readonly TemporaryFolder folder = new();
	private readonly FakeSetupProvider setupProvider = new();
	private readonly FakeProcessRunner processRunner = new();
	private readonly ConfigurationReader reader = new();

	[Fact]
	public async Task InstallAsync_RunsConfigureWithTransientFile() {

		var exitCode = await CreateService().InstallAsync(new OfficeConfiguration { Channel = "Current" }, new SynchronousProgress(), CancellationToken.None);

		var request = Assert.Single(processRunner.Requests);

		Assert.Equal(0, exitCode);
		Assert.Equal(setupProvider.GetSetupPath(ToolFolder), request.FileName);
		Assert.StartsWith("/configure \"", request.Arguments, StringComparison.Ordinal);
		Assert.Equal("Current", reader.Read(processRunner.Configurations.Single()).Channel);
		Assert.Empty(Directory.GetFiles(TemporaryFolder));

	}

	[Fact]
	public async Task DownloadAsync_SetsSourcePathAndPreparesOfflineFolder() {

		var target = folder.Combine("download");

		var exitCode = await CreateService().DownloadAsync(new OfficeConfiguration { Channel = "Current" }, target, new SynchronousProgress(), CancellationToken.None);

		var request = Assert.Single(processRunner.Requests);
		var offlineConfiguration = reader.ReadFile(Path.Combine(target, DeploymentService.OfflineConfigurationFileName));

		Assert.Equal(0, exitCode);
		Assert.StartsWith("/download \"", request.Arguments, StringComparison.Ordinal);
		Assert.Equal(target, reader.Read(processRunner.Configurations.Single()).SourcePath);
		Assert.Equal(target, offlineConfiguration.SourcePath);
		Assert.True(File.Exists(Path.Combine(target, SetupProvider.SetupFileName)));

	}

	[Fact]
	public async Task DownloadAsync_DoesNotChangeInputConfiguration() {

		var configuration = new OfficeConfiguration { Channel = "Current" };

		await CreateService().DownloadAsync(configuration, folder.Combine("download"), new SynchronousProgress(), CancellationToken.None);

		Assert.Null(configuration.SourcePath);

	}

	[Fact]
	public async Task DownloadAsync_SkipsOfflineFolderOnFailure() {

		var target = folder.Combine("download");

		processRunner.ExitCode = 17002;

		var exitCode = await CreateService().DownloadAsync(new OfficeConfiguration(), target, new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(17002, exitCode);
		Assert.Empty(Directory.GetFiles(target));

	}

	[Fact]
	public async Task InstallAsync_ReportsExitCodeDescription() {

		var log = new SynchronousProgress();

		processRunner.ExitCode = 17004;

		await CreateService().InstallAsync(new OfficeConfiguration(), log, CancellationToken.None);

		Assert.Contains(SetupExitCodes.Describe(17004), log.Messages);

	}

	public void Dispose() => folder.Dispose();

	private string ToolFolder => folder.Combine("tool");

	private string TemporaryFolder => folder.Combine("temp");

	private DeploymentService CreateService() {

		Directory.CreateDirectory(ToolFolder);
		File.WriteAllBytes(setupProvider.GetSetupPath(ToolFolder), [1]);

		return new DeploymentService(setupProvider, processRunner, new ConfigurationWriter(), ToolFolder, TemporaryFolder);

	}

	private sealed class FakeSetupProvider : ISetupProvider {

		public string GetSetupPath(string folder) => Path.Combine(folder, SetupProvider.SetupFileName);

		public Task<string> EnsureSetupAsync(string folder, IProgress<string> log, CancellationToken cancellationToken) => Task.FromResult(GetSetupPath(folder));

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
