namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Lists the products supported by the Office Deployment Tool.
/// </summary>
public static class ProductCatalog {

	/// <summary>Gets every known product.</summary>
	public static IReadOnlyList<ProductDefinition> All { get; } = [
		new(ProductIds.Microsoft365AppsForEnterprise, "Microsoft 365 Apps for enterprise", ProductCategory.Suite),
		new("O365BusinessRetail", "Microsoft 365 Apps for business", ProductCategory.Suite),
		new("O365HomePremRetail", "Microsoft 365 Personal / Family", ProductCategory.Suite),
		new("HomeBusiness2024Retail", "Office Home & Business 2024", ProductCategory.Suite),
		new("Home2024Retail", "Office Home 2024", ProductCategory.Suite),
		new("ProPlus2024Volume", "Office LTSC Professional Plus 2024", ProductCategory.Suite, ChannelIds.PerpetualVL2024),
		new("Standard2024Volume", "Office LTSC Standard 2024", ProductCategory.Suite, ChannelIds.PerpetualVL2024),
		new("ProPlus2021Volume", "Office LTSC Professional Plus 2021", ProductCategory.Suite, ChannelIds.PerpetualVL2021),
		new("Standard2021Volume", "Office LTSC Standard 2021", ProductCategory.Suite, ChannelIds.PerpetualVL2021),
		new("ProPlus2019Volume", "Office Professional Plus 2019", ProductCategory.Suite, ChannelIds.PerpetualVL2019),
		new("Standard2019Volume", "Office Standard 2019", ProductCategory.Suite, ChannelIds.PerpetualVL2019),
		new("VisioProRetail", "Visio Plan 2", ProductCategory.Visio),
		new("VisioPro2024Volume", "Visio LTSC Professional 2024", ProductCategory.Visio, ChannelIds.PerpetualVL2024),
		new("VisioStd2024Volume", "Visio LTSC Standard 2024", ProductCategory.Visio, ChannelIds.PerpetualVL2024),
		new("VisioPro2021Volume", "Visio LTSC Professional 2021", ProductCategory.Visio, ChannelIds.PerpetualVL2021),
		new("VisioStd2021Volume", "Visio LTSC Standard 2021", ProductCategory.Visio, ChannelIds.PerpetualVL2021),
		new("VisioPro2019Volume", "Visio Professional 2019", ProductCategory.Visio, ChannelIds.PerpetualVL2019),
		new("VisioStd2019Volume", "Visio Standard 2019", ProductCategory.Visio, ChannelIds.PerpetualVL2019),
		new("ProjectProRetail", "Project Online Desktop Client", ProductCategory.Project),
		new("ProjectPro2024Volume", "Project Professional 2024", ProductCategory.Project, ChannelIds.PerpetualVL2024),
		new("ProjectStd2024Volume", "Project Standard 2024", ProductCategory.Project, ChannelIds.PerpetualVL2024),
		new("ProjectPro2021Volume", "Project Professional 2021", ProductCategory.Project, ChannelIds.PerpetualVL2021),
		new("ProjectStd2021Volume", "Project Standard 2021", ProductCategory.Project, ChannelIds.PerpetualVL2021),
		new("ProjectPro2019Volume", "Project Professional 2019", ProductCategory.Project, ChannelIds.PerpetualVL2019),
		new("ProjectStd2019Volume", "Project Standard 2019", ProductCategory.Project, ChannelIds.PerpetualVL2019),
		new(ProductIds.ProofingTools, "Proofing Tools", ProductCategory.ProofingTools),
		new(ProductIds.LanguagePack, "Language Pack", ProductCategory.LanguagePack),
		new("AccessRuntimeRetail", "Access Runtime", ProductCategory.Other),
	];

	/// <summary>
	/// Finds a product by identifier, ignoring case.
	/// </summary>
	public static ProductDefinition? Find(string? id) => All.FirstOrDefault(product => string.Equals(product.Id, id, StringComparison.OrdinalIgnoreCase));

	/// <summary>
	/// Lists the products of a category.
	/// </summary>
	public static IReadOnlyList<ProductDefinition> OfCategory(ProductCategory category) => All.Where(product => product.Category == category).ToList();

}
