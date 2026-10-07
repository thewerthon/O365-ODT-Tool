using O365ODTTool.Core.Setup;

namespace O365ODTTool.Core.Tests.Setup;

public sealed class ProcessRunnerTests {

	private static readonly string commandProcessor = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";

	private readonly ProcessRunner runner = new();

	[Fact]
	public async Task RunAsync_ReportsOutputAndExitCode() {

		var output = new SynchronousProgress();

		var exitCode = await runner.RunAsync(new ProcessRequest(commandProcessor, "/c echo Foo& echo Bar 1>&2& exit /b 7", Path.GetTempPath()), output, CancellationToken.None);

		Assert.Equal(7, exitCode);
		Assert.Contains("Foo", output.Messages);
		Assert.Contains(output.Messages, static message => message.Trim() == "Bar");

	}

	[Fact]
	public async Task RunAsync_TerminatesOnCancellation() {

		using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

		var request = new ProcessRequest(commandProcessor, "/c ping -n 30 127.0.0.1 >nul", Path.GetTempPath());

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.RunAsync(request, new SynchronousProgress(), cancellation.Token));

	}

	[Fact]
	public async Task RunAsync_WrapsStartFailures() {

		var request = new ProcessRequest(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".exe"), string.Empty, Path.GetTempPath());

		await Assert.ThrowsAsync<SetupException>(() => runner.RunAsync(request, new SynchronousProgress(), CancellationToken.None));

	}

}
