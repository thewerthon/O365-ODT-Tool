using System.Text;
using System.Xml;
using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Xml;

/// <summary>
/// Writes configurations to their XML representation.
/// </summary>
public sealed class ConfigurationWriter : IConfigurationWriter {

	private static readonly XmlWriterSettings writerSettings = new() {
		Indent = true,
		IndentChars = "  ",
		OmitXmlDeclaration = true,
		NewLineChars = Environment.NewLine,
	};

	private static readonly Encoding fileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	/// <inheritdoc />
	public string Write(OfficeConfiguration configuration) {

		var builder = new StringBuilder();

		using (var xmlWriter = XmlWriter.Create(builder, writerSettings)) {

			configuration.Root.WriteTo(xmlWriter);

		}

		return builder.ToString();

	}

	/// <inheritdoc />
	public void WriteFile(OfficeConfiguration configuration, string path) {

		var directory = Path.GetDirectoryName(Path.GetFullPath(path));

		if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

		File.WriteAllText(path, Write(configuration) + Environment.NewLine, fileEncoding);

	}

}
