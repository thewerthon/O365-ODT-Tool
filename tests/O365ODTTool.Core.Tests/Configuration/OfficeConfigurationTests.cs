using System.Xml.Linq;
using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Tests.Configuration;

public sealed class OfficeConfigurationTests {

	[Fact]
	public void NewConfiguration_IsEmpty() {

		var configuration = new OfficeConfiguration();

		Assert.Empty(configuration.Products);
		Assert.Empty(configuration.AppSettings);
		Assert.Null(configuration.Channel);
		Assert.False(configuration.RemoveMsi);
		Assert.False(configuration.Root.HasElements);

	}

	[Fact]
	public void SettingAttributes_CreatesElementsInCanonicalOrder() {

		var configuration = new OfficeConfiguration {
			DisplayLevel = "Full",
			UpdatesEnabled = true,
			Channel = "Current",
			Description = "Foo",
		};

		configuration.SetProperty("Bar", "1");
		configuration.RemoveMsi = true;

		var names = configuration.Root.Elements().Select(static element => element.Name.LocalName).ToArray();

		Assert.Equal([OdtNames.Info, OdtNames.Add, OdtNames.Property, OdtNames.Updates, OdtNames.RemoveMsi, OdtNames.Display], names);

	}

	[Fact]
	public void SettingAttributeToNull_RemovesAttributeAndEmptyElement() {

		var configuration = new OfficeConfiguration { Description = "Foo" };

		configuration.Description = null;

		Assert.Null(configuration.Root.Element(OdtNames.Info));

	}

	[Fact]
	public void SettingAttributeToNull_KeepsElementWithOtherContent() {

		var configuration = new OfficeConfiguration { Channel = "Current", Version = "16.0.1.1" };

		configuration.Version = null;

		Assert.Equal("Current", configuration.Channel);
		Assert.Null(configuration.Version);

	}

	[Fact]
	public void Attributes_AreMatchedIgnoringCase() {

		var configuration = new OfficeConfiguration(XElement.Parse("<Configuration><Display level=\"None\" AcceptEula=\"true\" /></Configuration>"));

		configuration.DisplayLevel = "Full";

		Assert.Equal("Full", configuration.DisplayLevel);
		Assert.True(configuration.AcceptEula);
		Assert.Single(configuration.Root.Element(OdtNames.Display)!.Attributes(), static attribute => attribute.Name.LocalName.Equals("level", StringComparison.OrdinalIgnoreCase));

	}

	[Fact]
	public void Booleans_AreWrittenAsText() {

		var configuration = new OfficeConfiguration { MigrateArchitecture = true, UpdatesEnabled = false };

		Assert.Equal("TRUE", configuration.Root.Element(OdtNames.Add)!.Attribute(OdtNames.MigrateArch)!.Value);
		Assert.Equal("FALSE", configuration.Root.Element(OdtNames.Updates)!.Attribute(OdtNames.Enabled)!.Value);

	}

	[Fact]
	public void SetProperty_AddsUpdatesAndRemoves() {

		var configuration = new OfficeConfiguration();

		configuration.SetProperty("Foo", "1");
		configuration.SetProperty("foo", "0");

		Assert.Equal("0", configuration.GetProperty("FOO"));
		Assert.Single(configuration.Root.Elements(OdtNames.Property));

		configuration.SetProperty("Foo", null);

		Assert.Null(configuration.GetProperty("Foo"));
		Assert.Empty(configuration.Root.Elements(OdtNames.Property));

	}

	[Fact]
	public void SetProperty_AppendsAfterExistingProperties() {

		var configuration = new OfficeConfiguration { UpdatesEnabled = true };

		configuration.SetProperty("Foo", "1");
		configuration.SetProperty("Bar", "1");

		var names = configuration.Root.Elements().Select(static element => (string?)element.Attribute(OdtNames.Name) ?? element.Name.LocalName).ToArray();

		Assert.Equal(["Foo", "Bar", OdtNames.Updates], names);

	}

	[Fact]
	public void RemoveMsi_TogglesElement() {

		var configuration = new OfficeConfiguration { RemoveMsi = true };

		Assert.True(configuration.RemoveMsi);

		configuration.RemoveMsi = false;

		Assert.False(configuration.RemoveMsi);

	}

	[Fact]
	public void Company_IsStoredAsSetupSetting() {

		var configuration = new OfficeConfiguration { Company = "Foo" };

		var setting = Assert.Single(configuration.AppSettings);

		Assert.Equal(new AppSetting(AppSettingScope.Setup, "Company", "Foo"), setting);

		configuration.Company = " ";

		Assert.Null(configuration.Company);
		Assert.Null(configuration.Root.Element(OdtNames.AppSettings));

	}

	[Fact]
	public void Company_KeepsOtherSettings() {

		var configuration = new OfficeConfiguration(XElement.Parse("<Configuration><AppSettings><User Key=\"k\" Name=\"n\" Value=\"v\" Type=\"REG_SZ\" App=\"a\" Id=\"i\" /></AppSettings></Configuration>"));

		configuration.Company = "Foo";
		configuration.Company = null;

		var setting = Assert.Single(configuration.AppSettings);

		Assert.Equal(new AppSetting(AppSettingScope.User, "n", "v", "k", "REG_SZ", "a", "i"), setting);

	}

	[Fact]
	public void AddProduct_WritesProductWithChildren() {

		var configuration = new OfficeConfiguration();

		configuration.AddProduct(new ProductEntry("Foo", ["en-us", "pt-br"], ["Bar"], "KEY"));

		var product = Assert.Single(configuration.Products);

		Assert.Equal("Foo", product.Id);
		Assert.Equal("KEY", product.ProductKey);
		Assert.Equal(["en-us", "pt-br"], product.Languages);
		Assert.Equal(["Bar"], product.ExcludedApps);

	}

	[Fact]
	public void RemoveProducts_RemovesMatchingProductsOnly() {

		var configuration = new OfficeConfiguration();

		configuration.AddProduct(new ProductEntry("Foo", ["en-us"], []));
		configuration.AddProduct(new ProductEntry("Bar", ["en-us"], []));
		configuration.RemoveProducts(static id => id == "Foo");

		Assert.Equal(["Bar"], configuration.Products.Select(static product => product.Id));

	}

	[Fact]
	public void Clone_IsIndependent() {

		var original = new OfficeConfiguration { Channel = "Current" };
		var clone = original.Clone();

		clone.Channel = "BetaChannel";

		Assert.Equal("Current", original.Channel);
		Assert.Equal("BetaChannel", clone.Channel);

	}

}
