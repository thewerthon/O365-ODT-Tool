namespace O365ODTTool.Core.Xml;

/// <summary>
/// Represents an error raised when a text is not a valid configuration.
/// </summary>
public sealed class ConfigurationFormatException(string message, Exception? innerException = null) : Exception(message, innerException);
