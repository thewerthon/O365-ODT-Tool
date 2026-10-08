using O365ODTTool.Core.Catalogs;
using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Options;
using O365ODTTool.Core.Tests.Fixtures;
using O365ODTTool.Core.Validation;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Tests.Validation;

public sealed class ConfigurationValidatorTests {

	private readonly ConfigurationValidator validator = new();
	private readonly DeploymentOptionsMapper mapper = new();

	[Fact]
	public void Validate_AcceptsDefaults() => Assert.Empty(validator.Validate(mapper.ToConfiguration(new DeploymentOptions())));

	[Fact]
	public void Validate_AcceptsOfficeCustomizationToolExport() => Assert.Empty(validator.Validate(new ConfigurationReader().ReadFile(FixtureLoader.GetPath(FixtureLoader.OfficeCustomizationTool))));

	[Fact]
	public void Validate_RequiresProducts() => AssertError(mapper.ToConfiguration(new DeploymentOptions { SuiteProductId = null }), "at least one product");

	[Fact]
	public void Validate_RequiresLanguages() {

		var configuration = new OfficeConfiguration();

		configuration.AddProduct(new ProductEntry(ProductIds.Microsoft365AppsForEnterprise, [], []));

		AssertError(configuration, "has no language");

	}

	[Fact]
	public void Validate_RejectsVolumeProductOnWrongChannel() => AssertError(mapper.ToConfiguration(new DeploymentOptions { SuiteProductId = "ProPlus2024Volume", Channel = ChannelIds.PerpetualVL2021 }), "requires");

	[Fact]
	public void Validate_RejectsSubscriptionProductOnPerpetualChannel() => AssertError(mapper.ToConfiguration(new DeploymentOptions { Channel = ChannelIds.PerpetualVL2024 }), "cannot be deployed");

	[Fact]
	public void Validate_AcceptsVolumeProductOnMatchingChannel() {

		var options = new DeploymentOptions { SuiteProductId = "ProPlus2024Volume", VisioProductId = "VisioStd2024Volume", ProofingLanguages = ["es-es"], Channel = ChannelIds.PerpetualVL2024 };

		Assert.Empty(validator.Validate(mapper.ToConfiguration(options)));

	}

	[Theory]
	[InlineData("16.0")]
	[InlineData("15.0.1.1")]
	[InlineData("latest")]
	public void Validate_RejectsInvalidVersion(string version) => AssertError(WithVersion(version), "Version");

	[Fact]
	public void Validate_AcceptsValidVersion() => Assert.Empty(validator.Validate(WithVersion("16.0.17928.20156")));

	[Fact]
	public void Validate_RejectsInvalidArchitecture() => AssertError(new OfficeConfiguration { OfficeClientEdition = "86" }, "Architecture");

	[Fact]
	public void Validate_WarnsAboutUnknownValues() {

		var configuration = new OfficeConfiguration { Channel = "Foo" };

		configuration.AddProduct(new ProductEntry("Bar", ["xx-xx"], []));

		var issues = validator.Validate(configuration);

		Assert.Equal(3, issues.Count);
		Assert.All(issues, static issue => Assert.Equal(ValidationSeverity.Warning, issue.Severity));

	}

	private OfficeConfiguration WithVersion(string version) {

		var configuration = mapper.ToConfiguration(new DeploymentOptions());

		configuration.Version = version;

		return configuration;

	}

	private void AssertError(OfficeConfiguration configuration, string fragment) {

		var issues = validator.Validate(configuration);

		Assert.Contains(issues, issue => issue.Severity == ValidationSeverity.Error && issue.Message.Contains(fragment));

	}

}
