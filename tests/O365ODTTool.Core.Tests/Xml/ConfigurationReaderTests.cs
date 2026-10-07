using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Tests.Fixtures;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Tests.Xml;

public sealed class ConfigurationReaderTests {

	private readonly ConfigurationReader reader = new();

	[Fact]
	public void ReadFile_ParsesOfficeCustomizationToolExport() {

		var configuration = reader.ReadFile(FixtureLoader.GetPath(FixtureLoader.OfficeCustomizationTool));

		Assert.Equal("32", configuration.OfficeClientEdition);
		Assert.Equal("MonthlyEnterprise", configuration.Channel);
		Assert.Equal(4, configuration.Products.Count);
		Assert.Equal("Contoso", configuration.Company);
		Assert.Equal(2, configuration.AppSettings.Count(static setting => setting.Scope == AppSettingScope.User));
		Assert.Equal("1", configuration.GetProperty("SharedComputerLicensing"));
		Assert.Equal("None", configuration.DisplayLevel);
		Assert.True(configuration.RemoveMsi);

	}

	[Theory]
	[InlineData("")]
	[InlineData("<Configuration>")]
	[InlineData("not xml")]
	public void Read_RejectsInvalidXml(string xml) => Assert.Throws<ConfigurationFormatException>(() => reader.Read(xml));

	[Fact]
	public void Read_RejectsWrongRootElement() => Assert.Throws<ConfigurationFormatException>(() => reader.Read("<Foo />"));

	[Fact]
	public void Read_RejectsDocumentTypeDefinitions() {

		const string xml = "<!DOCTYPE Configuration [<!ENTITY foo \"bar\">]><Configuration Foo=\"&foo;\" />";

		Assert.Throws<ConfigurationFormatException>(() => reader.Read(xml));

	}

	[Fact]
	public void ReadFile_RejectsMissingFile() => Assert.Throws<ConfigurationFormatException>(() => reader.ReadFile(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.xml")));

}
