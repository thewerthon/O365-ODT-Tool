using System.ComponentModel;
using System.Diagnostics;

namespace O365ODTTool.Core.Setup;

/// <summary>
/// Runs external processes without a console window, capturing their output.
/// </summary>
public sealed class ProcessRunner : IProcessRunner {

	/// <inheritdoc />
	public async Task<int> RunAsync(ProcessRequest request, IProgress<string> output, CancellationToken cancellationToken) {

		cancellationToken.ThrowIfCancellationRequested();

		using var process = CreateProcess(request, output);

		var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		process.Exited += (_, _) => exited.TrySetResult(true);

		Start(process, request);

		using (cancellationToken.Register(() => Kill(process))) {

			await exited.Task.ConfigureAwait(false);

		}

		// Flushes the asynchronous output readers before the exit code is read.
		process.WaitForExit();

		cancellationToken.ThrowIfCancellationRequested();

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

	private static void Kill(Process process) {

		try {

			if (!process.HasExited) process.Kill();

		} catch (Exception exception) when (exception is InvalidOperationException or Win32Exception) {

			// The process exited between the check and the termination request.

		}

	}

}
