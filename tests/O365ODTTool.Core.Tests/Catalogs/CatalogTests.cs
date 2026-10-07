using O365ODTTool.Core.Catalogs;

namespace O365ODTTool.Core.Tests.Catalogs;

public sealed class CatalogTests {

	[Fact]
	public void Products_HaveUniqueIdentifiers() => AssertUnique(ProductCatalog.All.Select(static product => product.Id));

	[Fact]
	public void Channels_HaveUniqueIdentifiers() => AssertUnique(ChannelCatalog.All.Select(static channel => channel.Id));

	[Fact]
	public void Apps_HaveUniqueIdentifiers() => AssertUnique(AppCatalog.All.Select(static app => app.Id));

	[Fact]
	public void Languages_HaveUniqueIdentifiers() => AssertUnique(LanguageCatalog.Special.Concat(LanguageCatalog.Concrete).Select(static language => language.Id));

	[Fact]
	public void VolumeProducts_RequireKnownPerpetualChannels() {

		foreach (var product in ProductCatalog.All.Where(static product => product.IsVolume)) {

			var channel = ChannelCatalog.Find(product.RequiredChannel);

			Assert.NotNull(channel);
			Assert.True(channel!.IsPerpetual, product.Id);

		}

	}

	[Fact]
	public void EveryEditableCategory_HasProducts() {

		Assert.NotEmpty(ProductCatalog.OfCategory(ProductCategory.Suite));
		Assert.NotEmpty(ProductCatalog.OfCategory(ProductCategory.Visio));
		Assert.NotEmpty(ProductCatalog.OfCategory(ProductCategory.Project));

	}

	[Theory]
	[InlineData("o365proplusretail", ProductIds.Microsoft365AppsForEnterprise)]
	[InlineData("PROOFINGTOOLS", ProductIds.ProofingTools)]
	public void ProductFind_IgnoresCase(string id, string expected) => Assert.Equal(expected, ProductCatalog.Find(id)?.Id);

	[Theory]
	[InlineData("Current", ChannelIds.Current)]
	[InlineData("monthly", ChannelIds.Current)]
	[InlineData("Broad", ChannelIds.SemiAnnual)]
	[InlineData("Targeted", ChannelIds.SemiAnnualPreview)]
	[InlineData("InsiderFast", ChannelIds.BetaChannel)]
	[InlineData("FirstReleaseCurrent", ChannelIds.CurrentPreview)]
	[InlineData(" perpetualvl2024 ", ChannelIds.PerpetualVL2024)]
	public void ChannelFind_ResolvesIdentifiersAndAliases(string id, string expected) => Assert.Equal(expected, ChannelCatalog.Find(id)?.Id);

	[Theory]
	[InlineData(null)]
	[InlineData("Foo")]
	public void ChannelFind_ReturnsNullForUnknown(string? id) => Assert.Null(ChannelCatalog.Find(id));

	[Fact]
	public void LanguageFind_ResolvesSpecialAndConcrete() {

		Assert.True(LanguageCatalog.Find("matchos")!.IsSpecial);
		Assert.Equal(LanguageIds.BrazilianPortuguese, LanguageCatalog.Find("PT-BR")!.Id);

	}

	[Fact]
	public void AppFind_IgnoresCase() => Assert.Equal("OutlookForWindows", AppCatalog.Find("outlookforwindows")?.Id);

	private static void AssertUnique(IEnumerable<string> ids) {

		var list = ids.ToList();

		Assert.Equal(list.Count, list.Distinct(StringComparer.OrdinalIgnoreCase).Count());

	}

}
