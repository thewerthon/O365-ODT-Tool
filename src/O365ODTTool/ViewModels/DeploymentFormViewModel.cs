using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using O365ODTTool.Core.Catalogs;
using O365ODTTool.Core.Options;

namespace O365ODTTool.ViewModels;

/// <summary>
/// Holds the editable deployment choices displayed by the form.
/// </summary>
public sealed partial class DeploymentFormViewModel : ObservableObject {

	private const string noneDisplayName = "None";

	private bool isLoading;

	/// <summary>
	/// Initializes the form with the default choices.
	/// </summary>
	public DeploymentFormViewModel() {

		SuiteOptions = CreateProductOptions(ProductCategory.Suite);
		VisioOptions = CreateProductOptions(ProductCategory.Visio);
		ProjectOptions = CreateProductOptions(ProductCategory.Project);
		ChannelOptions = ChannelCatalog.All.Select(static channel => new OptionItem(channel.Id, channel.DisplayName)).ToList();
		PrimaryLanguageOptions = new(LanguageCatalog.Concrete.Concat(LanguageCatalog.Special).Select(static language => new OptionItem(language.Id, language.DisplayName)));
		Apps = CreateSelectableItems(AppCatalog.All.Select(static app => (app.Id, app.DisplayName)));
		AdditionalLanguages = CreateSelectableItems(LanguageCatalog.Concrete.Select(static language => (language.Id, language.DisplayName)));
		ProofingLanguages = CreateSelectableItems(LanguageCatalog.Concrete.Select(static language => (language.Id, language.DisplayName)));

		Load(new DeploymentOptions());

	}

	/// <summary>Occurs when any choice changes.</summary>
	public event EventHandler? Changed;

	/// <summary>Gets the suite choices.</summary>
	public IReadOnlyList<OptionItem> SuiteOptions { get; }

	/// <summary>Gets the diagramming product choices.</summary>
	public IReadOnlyList<OptionItem> VisioOptions { get; }

	/// <summary>Gets the project management product choices.</summary>
	public IReadOnlyList<OptionItem> ProjectOptions { get; }

	/// <summary>Gets the channel choices.</summary>
	public IReadOnlyList<OptionItem> ChannelOptions { get; }

	/// <summary>Gets the primary language choices.</summary>
	public ObservableCollection<OptionItem> PrimaryLanguageOptions { get; }

	/// <summary>Gets the applications, selected when installed.</summary>
	public ObservableCollection<SelectableItem> Apps { get; }

	/// <summary>Gets the additional languages.</summary>
	public ObservableCollection<SelectableItem> AdditionalLanguages { get; }

	/// <summary>Gets the proofing tools languages.</summary>
	public ObservableCollection<SelectableItem> ProofingLanguages { get; }

	/// <summary>Gets or sets the architecture.</summary>
	[ObservableProperty]
	public partial OfficeArchitecture Architecture { get; set; }

	/// <summary>Gets or sets whether an existing installation with another architecture is migrated.</summary>
	[ObservableProperty]
	public partial bool MigrateArchitecture { get; set; }

	/// <summary>Gets or sets the suite identifier.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsSuiteVolume))]
	public partial string? SuiteProductId { get; set; }

	/// <summary>Gets or sets the suite product key.</summary>
	[ObservableProperty]
	public partial string? SuiteProductKey { get; set; }

	/// <summary>Gets or sets the diagramming product identifier.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsVisioVolume))]
	public partial string? VisioProductId { get; set; }

	/// <summary>Gets or sets the diagramming product key.</summary>
	[ObservableProperty]
	public partial string? VisioProductKey { get; set; }

	/// <summary>Gets or sets the project management product identifier.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsProjectVolume))]
	public partial string? ProjectProductId { get; set; }

	/// <summary>Gets or sets the project management product key.</summary>
	[ObservableProperty]
	public partial string? ProjectProductKey { get; set; }

	/// <summary>Gets or sets the channel identifier.</summary>
	[ObservableProperty]
	public partial string? Channel { get; set; }

	/// <summary>Gets or sets whether a specific version is deployed instead of the latest one.</summary>
	[ObservableProperty]
	public partial bool IsSpecificVersion { get; set; }

	/// <summary>Gets or sets the specific version.</summary>
	[ObservableProperty]
	public partial string? Version { get; set; }

	/// <summary>Gets or sets the primary language identifier.</summary>
	[ObservableProperty]
	public partial string? PrimaryLanguage { get; set; }

	/// <summary>Gets or sets the setup user interface level.</summary>
	[ObservableProperty]
	public partial SetupDisplayLevel DisplayLevel { get; set; }

	/// <summary>Gets or sets whether the license agreement is accepted automatically.</summary>
	[ObservableProperty]
	public partial bool AcceptEula { get; set; }

	/// <summary>Gets or sets whether previous MSI installations are removed.</summary>
	[ObservableProperty]
	public partial bool RemoveMsi { get; set; }

	/// <summary>Gets or sets whether running applications are closed during setup.</summary>
	[ObservableProperty]
	public partial bool ForceAppShutdown { get; set; }

	/// <summary>Gets or sets whether application shortcuts are pinned to the taskbar.</summary>
	[ObservableProperty]
	public partial bool PinIconsToTaskbar { get; set; }

	/// <summary>Gets or sets whether updates are enabled.</summary>
	[ObservableProperty]
	public partial bool UpdatesEnabled { get; set; }

	/// <summary>Gets or sets the activation mode.</summary>
	[ObservableProperty]
	public partial LicensingMode Licensing { get; set; }

	/// <summary>Gets or sets whether volume products are activated automatically.</summary>
	[ObservableProperty]
	public partial bool AutoActivate { get; set; }

	/// <summary>Gets or sets the organization name.</summary>
	[ObservableProperty]
	public partial string? Company { get; set; }

	/// <summary>Gets or sets the configuration description.</summary>
	[ObservableProperty]
	public partial string? Description { get; set; }

	/// <summary>Gets a summary of the selected additional languages.</summary>
	public string AdditionalLanguagesSummary => Summarize(AdditionalLanguages);

	/// <summary>Gets a summary of the selected proofing tools languages.</summary>
	public string ProofingLanguagesSummary => Summarize(ProofingLanguages);

	/// <summary>Gets whether the selected suite is a volume product.</summary>
	public bool IsSuiteVolume => IsVolume(SuiteProductId);

	/// <summary>Gets whether the selected diagramming product is a volume product.</summary>
	public bool IsVisioVolume => IsVolume(VisioProductId);

	/// <summary>Gets whether the selected project management product is a volume product.</summary>
	public bool IsProjectVolume => IsVolume(ProjectProductId);

	/// <summary>
	/// Replaces every choice with the given options.
	/// </summary>
	public void Load(DeploymentOptions options) {

		isLoading = true;

		try {

			Architecture = options.Architecture;
			MigrateArchitecture = options.MigrateArchitecture;
			SuiteProductId = options.SuiteProductId;
			SuiteProductKey = options.SuiteProductKey;
			VisioProductId = options.VisioProductId;
			VisioProductKey = options.VisioProductKey;
			ProjectProductId = options.ProjectProductId;
			ProjectProductKey = options.ProjectProductKey;
			Channel = options.Channel;
			IsSpecificVersion = options.Version is not null;
			Version = options.Version;
			PrimaryLanguage = EnsurePrimaryLanguageOption(options.PrimaryLanguage);
			DisplayLevel = options.DisplayLevel;
			AcceptEula = options.AcceptEula;
			RemoveMsi = options.RemoveMsi;
			ForceAppShutdown = options.ForceAppShutdown;
			PinIconsToTaskbar = options.PinIconsToTaskbar;
			UpdatesEnabled = options.UpdatesEnabled;
			Licensing = options.Licensing;
			AutoActivate = options.AutoActivate;
			Company = options.Company;
			Description = options.Description;

			EnsureItems(Apps, options.ExcludedApps);
			Select(Apps, Apps.Select(static app => app.Id).Where(id => !options.ExcludedApps.Contains(id)));
			Select(AdditionalLanguages, options.AdditionalLanguages);
			Select(ProofingLanguages, options.ProofingLanguages);

		} finally {

			isLoading = false;

		}

		NotifySummariesChanged();

	}

	/// <summary>
	/// Creates options from the current choices.
	/// </summary>
	public DeploymentOptions ToOptions() => new() {
		Architecture = Architecture,
		MigrateArchitecture = MigrateArchitecture,
		SuiteProductId = SuiteProductId,
		SuiteProductKey = IsSuiteVolume ? SuiteProductKey : null,
		VisioProductId = VisioProductId,
		VisioProductKey = IsVisioVolume ? VisioProductKey : null,
		ProjectProductId = ProjectProductId,
		ProjectProductKey = IsProjectVolume ? ProjectProductKey : null,
		Channel = Channel ?? ChannelIds.Current,
		Version = IsSpecificVersion ? Version : null,
		PrimaryLanguage = PrimaryLanguage ?? LanguageIds.BrazilianPortuguese,
		AdditionalLanguages = SelectedIds(AdditionalLanguages),
		ProofingLanguages = SelectedIds(ProofingLanguages),
		ExcludedApps = new HashSet<string>(Apps.Where(static app => !app.IsSelected).Select(static app => app.Id), StringComparer.OrdinalIgnoreCase),
		DisplayLevel = DisplayLevel,
		AcceptEula = AcceptEula,
		RemoveMsi = RemoveMsi,
		ForceAppShutdown = ForceAppShutdown,
		PinIconsToTaskbar = PinIconsToTaskbar,
		UpdatesEnabled = UpdatesEnabled,
		Licensing = Licensing,
		AutoActivate = AutoActivate,
		Company = Company,
		Description = Description,
	};

	/// <inheritdoc />
	protected override void OnPropertyChanged(PropertyChangedEventArgs e) {

		base.OnPropertyChanged(e);

		NotifyChanged();

	}

	private static bool IsVolume(string? productId) => ProductCatalog.Find(productId)?.IsVolume ?? false;

	private static IReadOnlyList<OptionItem> CreateProductOptions(ProductCategory category) => [
		new OptionItem(null, noneDisplayName),
		.. ProductCatalog.OfCategory(category).Select(static product => new OptionItem(product.Id, product.DisplayName)),
	];

	private static List<string> SelectedIds(IEnumerable<SelectableItem> items) => items.Where(static item => item.IsSelected).Select(static item => item.Id).ToList();

	private static void Select(ObservableCollection<SelectableItem> items, IEnumerable<string> ids) {

		var selected = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);

		EnsureItems(items, selected);

		foreach (var item in items) item.IsSelected = selected.Contains(item.Id);

	}

	private static void EnsureItems(ObservableCollection<SelectableItem> items, IEnumerable<string> ids) {

		foreach (var id in ids.Where(id => !items.Any(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))).ToList()) items.Add(new SelectableItem(id, id));

	}

	private ObservableCollection<SelectableItem> CreateSelectableItems(IEnumerable<(string Id, string DisplayName)> definitions) {

		var items = new ObservableCollection<SelectableItem>(definitions.Select(static definition => new SelectableItem(definition.Id, definition.DisplayName)));

		foreach (var item in items) item.PropertyChanged += OnItemChanged;

		items.CollectionChanged += (_, args) => {

			foreach (var item in args.NewItems?.OfType<SelectableItem>() ?? []) item.PropertyChanged += OnItemChanged;

		};

		return items;

	}

	private string EnsurePrimaryLanguageOption(string id) {

		var existing = PrimaryLanguageOptions.FirstOrDefault(option => string.Equals(option.Id, id, StringComparison.OrdinalIgnoreCase));

		if (existing is not null) return existing.Id!;

		PrimaryLanguageOptions.Add(new OptionItem(id, id));

		return id;

	}

	private static string Summarize(IEnumerable<SelectableItem> items) {

		var selected = SelectedIds(items);

		return selected.Count == 0 ? "None selected" : $"Selected: {string.Join(", ", selected)}";

	}

	private void OnItemChanged(object? sender, PropertyChangedEventArgs e) => NotifySummariesChanged();

	private void NotifySummariesChanged() {

		OnPropertyChanged(nameof(AdditionalLanguagesSummary));
		OnPropertyChanged(nameof(ProofingLanguagesSummary));

	}

	private void NotifyChanged() {

		if (isLoading) return;

		Changed?.Invoke(this, EventArgs.Empty);

	}

	partial void OnSuiteProductIdChanged(string? value) => AlignChannel(value);

	partial void OnVisioProductIdChanged(string? value) => AlignChannel(value);

	partial void OnProjectProductIdChanged(string? value) => AlignChannel(value);

	private void AlignChannel(string? productId) {

		if (isLoading) return;

		var product = ProductCatalog.Find(productId);

		if (product is null) return;

		if (product.IsVolume) {

			Channel = product.RequiredChannel;

			return;

		}

		if (ChannelCatalog.Find(Channel)?.IsPerpetual == true) Channel = ChannelIds.Current;

	}

}
