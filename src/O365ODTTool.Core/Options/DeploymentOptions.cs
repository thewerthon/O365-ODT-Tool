using O365ODTTool.Core.Catalogs;

namespace O365ODTTool.Core.Options;

/// <summary>
/// Represents the deployment choices editable by the user.
/// </summary>
public sealed class DeploymentOptions {

	/// <summary>Gets or sets the architecture.</summary>
	public OfficeArchitecture Architecture { get; set; } = OfficeArchitecture.X64;

	/// <summary>Gets or sets the suite identifier, or <see langword="null"/> for none.</summary>
	public string? SuiteProductId { get; set; } = ProductIds.Microsoft365AppsForEnterprise;

	/// <summary>Gets or sets the suite product key.</summary>
	public string? SuiteProductKey { get; set; }

	/// <summary>Gets or sets the diagramming product identifier, or <see langword="null"/> for none.</summary>
	public string? VisioProductId { get; set; }

	/// <summary>Gets or sets the diagramming product key.</summary>
	public string? VisioProductKey { get; set; }

	/// <summary>Gets or sets the project management product identifier, or <see langword="null"/> for none.</summary>
	public string? ProjectProductId { get; set; }

	/// <summary>Gets or sets the project management product key.</summary>
	public string? ProjectProductKey { get; set; }

	/// <summary>Gets or sets the channel identifier.</summary>
	public string Channel { get; set; } = ChannelIds.Current;

	/// <summary>Gets or sets the version to deploy, or <see langword="null"/> for the latest one.</summary>
	public string? Version { get; set; }

	/// <summary>Gets or sets the excluded application identifiers.</summary>
	public ISet<string> ExcludedApps { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { AppIds.Bing, AppIds.Groove, AppIds.Lync };

	/// <summary>Gets or sets the primary language identifier.</summary>
	public string PrimaryLanguage { get; set; } = LanguageIds.BrazilianPortuguese;

	/// <summary>Gets or sets the additional language identifiers.</summary>
	public IList<string> AdditionalLanguages { get; set; } = [];

	/// <summary>Gets or sets the proofing tools language identifiers.</summary>
	public IList<string> ProofingLanguages { get; set; } = [];

	/// <summary>Gets or sets the setup user interface level.</summary>
	public SetupDisplayLevel DisplayLevel { get; set; } = SetupDisplayLevel.Full;

	/// <summary>Gets or sets whether the license agreement is accepted automatically.</summary>
	public bool AcceptEula { get; set; } = true;

	/// <summary>Gets or sets whether an existing installation with another architecture is migrated.</summary>
	public bool MigrateArchitecture { get; set; } = true;

	/// <summary>Gets or sets whether previous MSI installations are removed.</summary>
	public bool RemoveMsi { get; set; } = true;

	/// <summary>Gets or sets whether running applications are closed during setup.</summary>
	public bool ForceAppShutdown { get; set; }

	/// <summary>Gets or sets whether application shortcuts are pinned to the taskbar.</summary>
	public bool PinIconsToTaskbar { get; set; }

	/// <summary>Gets or sets whether updates are enabled.</summary>
	public bool UpdatesEnabled { get; set; } = true;

	/// <summary>Gets or sets the activation mode.</summary>
	public LicensingMode Licensing { get; set; } = LicensingMode.UserBased;

	/// <summary>Gets or sets whether volume products are activated automatically.</summary>
	public bool AutoActivate { get; set; }

	/// <summary>Gets or sets the organization name.</summary>
	public string? Company { get; set; }

	/// <summary>Gets or sets the configuration description.</summary>
	public string? Description { get; set; }

}
