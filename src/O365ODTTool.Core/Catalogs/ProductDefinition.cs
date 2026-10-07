namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Describes a deployable product.
/// </summary>
/// <param name="Id">The product identifier.</param>
/// <param name="DisplayName">The product display name.</param>
/// <param name="Category">The product category.</param>
/// <param name="RequiredChannel">The channel the product must be deployed from, if any.</param>
public sealed record ProductDefinition(string Id, string DisplayName, ProductCategory Category, string? RequiredChannel = null) {

	/// <summary>Gets whether the product is a volume licensed perpetual product.</summary>
	public bool IsVolume => RequiredChannel is not null;

	/// <summary>Gets whether the product can be deployed from any channel.</summary>
	public bool IsChannelAgnostic => Category is ProductCategory.ProofingTools or ProductCategory.LanguagePack;

}
