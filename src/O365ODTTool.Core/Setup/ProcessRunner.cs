using System.ComponentModel;
using System.Diagnostics;

namespace O365ODTTool.Core.Setup;

/// <summary>
/// Runs external processes without a console window, capturing their output.
/// </summary>
public sealed class ProcessRunner : IProcessRunner {

	private const string treeKillerFileName = "taskkill.exe";
	private const string treeKillerArgumentsFormat = "/PID {0} /T /F";

	/// <inheritdoc />
	public async Task<int> RunAsync(ProcessRequest request, IProgress<string> output, CancellationToken cancellationToken) {

		cancellationToken.ThrowIfCancellationRequested();

		using var process = CreateProcess(request, output);

		var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		process.Exited += (_, _) => exited.TrySetResult(true);

		Start(process, request);

		using (cancellationToken.Register(() => Task.Run(() => KillTree(process)))) {

			await exited.Task.ConfigureAwait(false);

		}

		// Waiting for the output to drain would block on child processes that inherited it, so a cancelled run returns right away.
		cancellationToken.ThrowIfCancellationRequested();

		process.WaitForExit();

		return process.ExitCode;

	}

	private static Process CreateProcess(ProcessRequest request, IProgress<string> output) {

		var process = new Process {
			EnableRaisingEvents = true,
			StartInfo = new ProcessStartInfo(request.FileName, request.Arguments) {
				WorkingDirectory = request.WorkingDirectory,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			},
		};

		process.OutputDataReceived += (_, line) => Report(output, line.Data);
		process.ErrorDataReceived += (_, line) => Report(output, line.Data);

		return process;

	}

	private static void Start(Process process, ProcessRequest request) {

		try {

			process.Start();

		} catch (Win32Exception exception) {

			throw new SetupException($"Unable to start '{request.FileName}': {exception.Message}", exception);

		}

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

	}

	private static void Report(IProgress<string> output, string? line) {

		if (string.IsNullOrWhiteSpace(line)) return;

		output.Report(line!);

	}

	// Setup delegates the work to child processes, so terminating only the parent would leave them running.
	private static void KillTree(Process process) {

		try {

			if (process.HasExited) return;

			using var killer = Process.Start(new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, treeKillerFileName), string.Format(treeKillerArgumentsFormat, process.Id)) {
				UseShellExecute = false,
				CreateNoWindow = true,
			});

			killer?.WaitForExit();

			if (!process.HasExited) process.Kill();

		} catch (Exception exception) when (exception is InvalidOperationException or Win32Exception) {

			// The process exited between the check and the termination request.

		}

	}

}
