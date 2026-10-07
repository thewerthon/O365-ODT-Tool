namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Defines the categories of deployable products.
/// </summary>
public enum ProductCategory {

	/// <summary>An office suite.</summary>
	Suite,

	/// <summary>A diagramming product.</summary>
	Visio,

	/// <summary>A project management product.</summary>
	Project,

	/// <summary>The proofing tools product.</summary>
	ProofingTools,

	/// <summary>The language pack product.</summary>
	LanguagePack,

	/// <summary>A standalone product without a dedicated choice.</summary>
	Other,

}
