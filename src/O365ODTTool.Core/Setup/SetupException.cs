namespace O365ODTTool.Core.Setup;

/// <summary>
/// Represents an error raised while obtaining or running setup.
/// </summary>
public sealed class SetupException(string message, Exception? innerException = null) : Exception(message, innerException);
