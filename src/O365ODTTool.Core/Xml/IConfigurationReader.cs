using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Xml;

/// <summary>
/// Reads configurations from their XML representation.
/// </summary>
public interface IConfigurationReader {

	/// <summary>
	/// Parses a configuration from XML text.
	/// </summary>
	/// <exception cref="ConfigurationFormatException">The text is not a valid configuration.</exception>
	OfficeConfiguration Read(string xml);

	/// <summary>
	/// Parses a configuration from an XML file.
	/// </summary>
	/// <exception cref="ConfigurationFormatException">The file is not a valid configuration.</exception>
	OfficeConfiguration ReadFile(string path);

}
