using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Xml;

/// <summary>
/// Writes configurations to their XML representation.
/// </summary>
public interface IConfigurationWriter {

	/// <summary>
	/// Serializes a configuration to XML text.
	/// </summary>
	string Write(OfficeConfiguration configuration);

	/// <summary>
	/// Serializes a configuration to an XML file, replacing any existing file.
	/// </summary>
	void WriteFile(OfficeConfiguration configuration, string path);

}
