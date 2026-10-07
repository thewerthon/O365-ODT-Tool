namespace O365ODTTool.Core.Setup;

/// <summary>
/// Runs external processes.
/// </summary>
public interface IProcessRunner {

	/// <summary>
	/// Runs a process to completion, reporting each output line, and returns its exit code.
	/// </summary>
	/// <exception cref="OperationCanceledException">The process was terminated because of cancellation.</exception>
	Task<int> RunAsync(ProcessRequest request, IProgress<string> output, CancellationToken cancellationToken);

}
