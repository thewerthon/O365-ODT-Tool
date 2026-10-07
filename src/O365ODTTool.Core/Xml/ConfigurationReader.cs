using System.Xml;
using System.Xml.Linq;
using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Xml;

/// <summary>
/// Reads configurations from their XML representation.
/// </summary>
public sealed class ConfigurationReader : IConfigurationReader {

	private static readonly XmlReaderSettings readerSettings = new() {
		DtdProcessing = DtdProcessing.Prohibit,
		XmlResolver = null,
		IgnoreComments = false,
	};

	/// <inheritdoc />
	public OfficeConfiguration Read(string xml) {

		var document = Parse(xml);
		var root = document.Root;

		if (root is null || root.Name.LocalName != OdtNames.Configuration) throw new ConfigurationFormatException($"The root element must be '{OdtNames.Configuration}'.");

		return new OfficeConfiguration(new XElement(root));

	}

	/// <inheritdoc />
	public OfficeConfiguration ReadFile(string path) {

		string xml;

		try {

			xml = File.ReadAllText(path);

		} catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) {

			throw new ConfigurationFormatException($"Unable to read '{path}': {exception.Message}", exception);

		}

		return Read(xml);

	}

	private static XDocument Parse(string xml) {

		try {

			using var stringReader = new StringReader(xml);
			using var xmlReader = XmlReader.Create(stringReader, readerSettings);

			return XDocument.Load(xmlReader);

		} catch (XmlException exception) {

			throw new ConfigurationFormatException($"Invalid XML: {exception.Message}", exception);

		}

	}

}
