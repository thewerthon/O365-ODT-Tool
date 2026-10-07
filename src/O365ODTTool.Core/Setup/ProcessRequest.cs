namespace O365ODTTool.Core.Setup;

/// <summary>
/// Describes a process to run.
/// </summary>
/// <param name="FileName">The executable path.</param>
/// <param name="Arguments">The command line arguments.</param>
/// <param name="WorkingDirectory">The working directory.</param>
public sealed record ProcessRequest(string FileName, string Arguments, string WorkingDirectory);
