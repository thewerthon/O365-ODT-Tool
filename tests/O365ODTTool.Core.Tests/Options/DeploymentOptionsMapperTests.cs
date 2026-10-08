using O365ODTTool.Core.Catalogs;
using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Options;
using O365ODTTool.Core.Tests.Fixtures;
using O365ODTTool.Core.Xml;

namespace O365ODTTool.Core.Tests.Options;

public sealed class DeploymentOptionsMapperTests {

	private readonly DeploymentOptionsMapper mapper = new();
	private readonly ConfigurationReader reader = new();

	[Fact]
	public void ToConfiguration_FromDefaults_ProducesCdnInstallation() {

		var configuration = mapper.ToConfiguration(new DeploymentOptions());

		var product = Assert.Single(configuration.Products);

		Assert.Equal("64", configuration.OfficeClientEdition);
		Assert.Equal(ChannelIds.Current, configuration.Channel);
		Assert.Null(configuration.SourcePath);
		Assert.Equal(ProductIds.Microsoft365AppsForEnterprise, product.Id);
		Assert.Equal([LanguageIds.BrazilianPortuguese], product.Languages);
		Assert.Equal([AppIds.Bing, AppIds.Groove, AppIds.Lync], product.ExcludedApps);
		Assert.True(configuration.MigrateArchitecture);
		Assert.True(configuration.RemoveMsi);
		Assert.True(configuration.UpdatesEnabled);
		Assert.Equal("Full", configuration.DisplayLevel);
		Assert.True(configuration.AcceptEula);
		Assert.Null(configuration.Version);
		Assert.Equal("TRUE", configuration.GetProperty(OdtPropertyNames.ForceAppShutdown));
		Assert.Equal("0", configuration.GetProperty(OdtPropertyNames.SharedComputerLicensing));
		Assert.Null(configuration.GetProperty(OdtPropertyNames.AutoActivate));

	}

	[Fact]
	public void ToConfiguration_WritesEverySelectedProduct() {

		var options = new DeploymentOptions {
			SuiteProductId = "ProPlus2024Volume",
			SuiteProductKey = "FOO",
			VisioProductId = "VisioPro2024Volume",
			ProjectProductId = "ProjectPro2024Volume",
			ProofingLanguages = ["es-es"],
			AdditionalLanguages = ["en-us", "pt-br"],
		};

		var products = mapper.ToConfiguration(options).Products;

		Assert.Equal(["ProPlus2024Volume", "VisioPro2024Volume", "ProjectPro2024Volume", ProductIds.ProofingTools], products.Select(static product => product.Id));
		Assert.Equal("FOO", products[0].ProductKey);
		Assert.Equal([LanguageIds.BrazilianPortuguese, "en-us"], products[0].Languages);
		Assert.Equal(["es-es"], products[3].Languages);
		Assert.Empty(products[3].ExcludedApps);

	}

	[Fact]
	public void ToConfiguration_WithoutSuite_OmitsIt() {

		var options = new DeploymentOptions { SuiteProductId = null, VisioProductId = "VisioProRetail" };

		Assert.Equal(["VisioProRetail"], mapper.ToConfiguration(options).Products.Select(static product => product.Id));

	}

	[Theory]
	[InlineData(LicensingMode.UserBased, "0", "0")]
	[InlineData(LicensingMode.SharedComputer, "1", "0")]
	[InlineData(LicensingMode.DeviceBased, "0", "1")]
	public void ToConfiguration_WritesLicensingProperties(LicensingMode licensing, string shared, string device) {

		var configuration = mapper.ToConfiguration(new DeploymentOptions { Licensing = licensing });

		Assert.Equal(shared, configuration.GetProperty(OdtPropertyNames.SharedComputerLicensing));
		Assert.Equal(device, configuration.GetProperty(OdtPropertyNames.DeviceBasedLicensing));
		Assert.Equal(licensing, mapper.ToOptions(configuration).Licensing);

	}

	[Fact]
	public void ToConfiguration_PreservesNonEditableContentOfBase() {

		var baseConfiguration = reader.Read(FixtureLoader.Read(FixtureLoader.OfficeCustomizationTool));

		var configuration = mapper.ToConfiguration(mapper.ToOptions(baseConfiguration), baseConfiguration);

		Assert.Equal(baseConfiguration.AppSettings, configuration.AppSettings);
		Assert.Equal("0", configuration.GetProperty("SCLCacheOverride"));
		Assert.Contains(configuration.Products, static product => product.Id == "AccessRuntimeRetail");
		Assert.NotNull(configuration.Root.Element("CustomElement"));
		Assert.NotNull(configuration.Root.Element(OdtNames.Logging));
		Assert.NotNull(configuration.Root.Element(OdtNames.RemoveMsi)!.Element("IgnoreProduct"));
		Assert.Equal("05/16/2026 18:30", configuration.Root.Element(OdtNames.Updates)!.Attribute("Deadline")!.Value);
		Assert.Equal("5b6a0ee0-1f6e-4f37-9a4c-6f3b1c2d8e90", configuration.Root.Attribute(OdtNames.Id)!.Value);

	}

	[Fact]
	public void ToConfiguration_KeepsSourceAndVersionOfBase() {

		var baseConfiguration = reader.Read(FixtureLoader.Read(FixtureLoader.OfficeCustomizationTool));

		baseConfiguration.Version = "16.0.17928.20156";

		var configuration = mapper.ToConfiguration(mapper.ToOptions(baseConfiguration), baseConfiguration);

		Assert.Equal(@"\\server\share", configuration.SourcePath);
		Assert.True(configuration.AllowCdnFallback);
		Assert.Equal("16.0.17928.20156", configuration.Version);

	}

	[Fact]
	public void ToConfiguration_KeepsRemoveMsiAttributesOfBase() {

		var baseConfiguration = reader.Read("<Configuration><RemoveMSI All=\"True\" /></Configuration>");

		var configuration = mapper.ToConfiguration(new DeploymentOptions(), baseConfiguration);

		Assert.Equal("True", configuration.Root.Element(OdtNames.RemoveMsi)!.Attribute("All")!.Value);

	}

	[Fact]
	public void ToConfiguration_DoesNotChangeBase() {

		var baseConfiguration = reader.Read(FixtureLoader.Read(FixtureLoader.OfficeCustomizationTool));

		mapper.ToConfiguration(new DeploymentOptions(), baseConfiguration);

		Assert.Equal("MonthlyEnterprise", baseConfiguration.Channel);
		Assert.Equal(4, baseConfiguration.Products.Count);

	}

	[Fact]
	public void ToOptions_ReadsOfficeCustomizationToolExport() {

		var options = mapper.ToOptions(reader.Read(FixtureLoader.Read(FixtureLoader.OfficeCustomizationTool)));

		Assert.Equal(OfficeArchitecture.X86, options.Architecture);
		Assert.Equal(ProductIds.Microsoft365AppsForEnterprise, options.SuiteProductId);
		Assert.Equal("VisioProRetail", options.VisioProductId);
		Assert.Null(options.ProjectProductId);
		Assert.Equal(ChannelIds.MonthlyEnterprise, options.Channel);
		Assert.Equal("en-us", options.PrimaryLanguage);
		Assert.Equal([LanguageIds.BrazilianPortuguese], options.AdditionalLanguages);
		Assert.Equal(["es-es"], options.ProofingLanguages);
		Assert.Equal(new HashSet<string> { AppIds.Groove, AppIds.Lync, "Publisher" }, options.ExcludedApps);
		Assert.Equal(SetupDisplayLevel.None, options.DisplayLevel);
		Assert.True(options.AcceptEula);
		Assert.True(options.MigrateArchitecture);
		Assert.True(options.RemoveMsi);
		Assert.True(options.ForceAppShutdown);
		Assert.False(options.PinIconsToTaskbar);
		Assert.False(options.UpdatesEnabled);
		Assert.Equal(LicensingMode.SharedComputer, options.Licensing);
		Assert.Equal("Contoso", options.Company);
		Assert.Equal("Fixture exported by the web customization tool", options.Description);

	}

	[Fact]
	public void ToOptions_FromEmptyConfiguration_UsesDefaultsWhereMissing() {

		var options = mapper.ToOptions(new OfficeConfiguration());

		Assert.Null(options.SuiteProductId);
		Assert.Equal(ChannelIds.Current, options.Channel);
		Assert.Equal(LanguageIds.BrazilianPortuguese, options.PrimaryLanguage);
		Assert.True(options.UpdatesEnabled);
		Assert.False(options.AcceptEula);
		Assert.Equal(SetupDisplayLevel.Full, options.DisplayLevel);

	}

	[Fact]
	public void ToOptions_InfersChannelFromVolumeProduct() {

		var configuration = new OfficeConfiguration();

		configuration.AddProduct(new ProductEntry("proplus2021volume", ["en-us"], []));

		var options = mapper.ToOptions(configuration);

		Assert.Equal("ProPlus2021Volume", options.SuiteProductId);
		Assert.Equal(ChannelIds.PerpetualVL2021, options.Channel);

	}

	[Fact]
	public void ToOptions_NormalizesLegacyChannels() => Assert.Equal(ChannelIds.SemiAnnual, mapper.ToOptions(new OfficeConfiguration { Channel = "Broad" }).Channel);

	[Fact]
	public void OptionsRoundTrip_IsStable() {

		var options = new DeploymentOptions {
			Architecture = OfficeArchitecture.X86,
			SuiteProductId = "O365BusinessRetail",
			ProjectProductId = "ProjectProRetail",
			Channel = ChannelIds.BetaChannel,
			ExcludedApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Teams" },
			PrimaryLanguage = "en-us",
			AdditionalLanguages = ["ja-jp"],
			DisplayLevel = SetupDisplayLevel.None,
			AcceptEula = false,
			MigrateArchitecture = false,
			RemoveMsi = false,
			ForceAppShutdown = false,
			PinIconsToTaskbar = true,
			UpdatesEnabled = false,
			AutoActivate = true,
			Company = "Foo",
			Description = "Bar",
		};

		var roundTrip = mapper.ToOptions(mapper.ToConfiguration(options));

		Assert.Equal(options.Architecture, roundTrip.Architecture);
		Assert.Equal(options.SuiteProductId, roundTrip.SuiteProductId);
		Assert.Equal(options.ProjectProductId, roundTrip.ProjectProductId);
		Assert.Equal(options.Channel, roundTrip.Channel);
		Assert.Equal(options.ExcludedApps, roundTrip.ExcludedApps);
		Assert.Equal(options.PrimaryLanguage, roundTrip.PrimaryLanguage);
		Assert.Equal(options.AdditionalLanguages, roundTrip.AdditionalLanguages);
		Assert.Equal(options.DisplayLevel, roundTrip.DisplayLevel);
		Assert.Equal(options.AcceptEula, roundTrip.AcceptEula);
		Assert.Equal(options.MigrateArchitecture, roundTrip.MigrateArchitecture);
		Assert.Equal(options.RemoveMsi, roundTrip.RemoveMsi);
		Assert.Equal(options.ForceAppShutdown, roundTrip.ForceAppShutdown);
		Assert.Equal(options.PinIconsToTaskbar, roundTrip.PinIconsToTaskbar);
		Assert.Equal(options.UpdatesEnabled, roundTrip.UpdatesEnabled);
		Assert.Equal(options.AutoActivate, roundTrip.AutoActivate);
		Assert.Equal(options.Company, roundTrip.Company);
		Assert.Equal(options.Description, roundTrip.Description);

	}

	[Theory]
	[InlineData(ProductIds.Microsoft365AppsForEnterprise, true)]
	[InlineData("VisioProRetail", true)]
	[InlineData(ProductIds.ProofingTools, true)]
	[InlineData(ProductIds.LanguagePack, false)]
	[InlineData("AccessRuntimeRetail", false)]
	public void IsManagedProduct_CoversEditableCategories(string id, bool expected) => Assert.Equal(expected, mapper.IsManagedProduct(id));

}
