using System.Xml.Linq;
using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Tests.Fixtures;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Tests.Xml;

public sealed class ConfigurationWriterTests {

	private readonly ConfigurationReader reader = new();
	private readonly ConfigurationWriter writer = new();

	[Fact]
	public void Write_RoundTripsEveryElementAndAttribute() {

		var xml = FixtureLoader.Read(FixtureLoader.OfficeCustomizationTool);
		var written = writer.Write(reader.Read(xml));

		Assert.True(XNode.DeepEquals(Normalize(XElement.Parse(xml)), Normalize(XElement.Parse(written))));

	}

	[Fact]
	public void Write_OmitsDeclarationAndIndents() {

		var configuration = new OfficeConfiguration { Channel = "Current" };

		var written = writer.Write(configuration);

		Assert.StartsWith("<Configuration>", written, StringComparison.Ordinal);
		Assert.Contains($"{Environment.NewLine}  <Add Channel=\"Current\" />", written, StringComparison.Ordinal);

	}

	[Fact]
	public void WriteFile_CreatesFolderAndWritesWithoutByteOrderMark() {

		using var folder = new TemporaryFolder();
		var path = folder.Combine("nested", "configuration.xml");

		writer.WriteFile(new OfficeConfiguration { Channel = "Current" }, path);

		var bytes = File.ReadAllBytes(path);

		Assert.Equal((byte)'<', bytes[0]);
		Assert.Equal("Current", reader.ReadFile(path).Channel);

	}

	private static XElement Normalize(XElement element) {

		var copy = new XElement(element);

		foreach (var text in copy.DescendantNodes().OfType<XText>().Where(static text => string.IsNullOrWhiteSpace(text.Value)).ToList()) text.Remove();

		return copy;

	}

}
