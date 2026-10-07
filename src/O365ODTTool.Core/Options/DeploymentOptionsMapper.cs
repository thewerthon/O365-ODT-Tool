using O365ODTTool.Core.Catalogs;
using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Options;

/// <summary>
/// Converts between configurations and the deployment choices editable by the user.
/// </summary>
public sealed class DeploymentOptionsMapper : IDeploymentOptionsMapper {

	private const string x64Edition = "64";
	private const string x86Edition = "32";

	private static readonly ProductCategory[] managedCategories = [ProductCategory.Suite, ProductCategory.Visio, ProductCategory.Project, ProductCategory.ProofingTools];

	/// <inheritdoc />
	public bool IsManagedProduct(string productId) => ProductCatalog.Find(productId) is { } product && managedCategories.Contains(product.Category);

	/// <inheritdoc />
	public DeploymentOptions ToOptions(OfficeConfiguration configuration) {

		var defaults = new DeploymentOptions();
		var products = configuration.Products;
		var suite = FindProduct(products, ProductCategory.Suite);
		var visio = FindProduct(products, ProductCategory.Visio);
		var project = FindProduct(products, ProductCategory.Project);
		var proofing = FindProduct(products, ProductCategory.ProofingTools);
		var primaryProduct = suite ?? visio ?? project;
		var languages = primaryProduct?.Languages.Select(NormalizeLanguage).ToList() ?? [];

		return new DeploymentOptions {
			Architecture = string.Equals(configuration.OfficeClientEdition?.Trim(), x86Edition, StringComparison.Ordinal) ? OfficeArchitecture.X86 : OfficeArchitecture.X64,
			SuiteProductId = suite?.Id,
			SuiteProductKey = suite?.ProductKey,
			VisioProductId = visio?.Id,
			VisioProductKey = visio?.ProductKey,
			ProjectProductId = project?.Id,
			ProjectProductKey = project?.ProductKey,
			Channel = ChannelCatalog.Find(configuration.Channel)?.Id ?? ProductCatalog.Find(primaryProduct?.Id)?.RequiredChannel ?? defaults.Channel,
			Version = NullIfBlank(configuration.Version),
			ExcludedApps = primaryProduct is null ? defaults.ExcludedApps : new HashSet<string>(primaryProduct.ExcludedApps.Select(NormalizeApp), StringComparer.OrdinalIgnoreCase),
			PrimaryLanguage = languages.FirstOrDefault() ?? defaults.PrimaryLanguage,
			AdditionalLanguages = languages.Skip(1).ToList(),
			ProofingLanguages = proofing?.Languages.Select(NormalizeLanguage).ToList() ?? [],
			DisplayLevel = string.Equals(configuration.DisplayLevel, nameof(SetupDisplayLevel.None), StringComparison.OrdinalIgnoreCase) ? SetupDisplayLevel.None : SetupDisplayLevel.Full,
			AcceptEula = configuration.AcceptEula ?? false,
			MigrateArchitecture = configuration.MigrateArchitecture ?? false,
			RemoveMsi = configuration.RemoveMsi,
			ForceAppShutdown = GetBooleanProperty(configuration, OdtPropertyNames.ForceAppShutdown),
			PinIconsToTaskbar = GetBooleanProperty(configuration, OdtPropertyNames.PinIconsToTaskbar),
			UpdatesEnabled = configuration.UpdatesEnabled ?? true,
			Licensing = GetLicensing(configuration),
			AutoActivate = GetBooleanProperty(configuration, OdtPropertyNames.AutoActivate),
			Company = configuration.Company,
			Description = configuration.Description,
		};

	}

	/// <inheritdoc />
	public OfficeConfiguration ToConfiguration(DeploymentOptions options, OfficeConfiguration? baseConfiguration = null) {

		var configuration = baseConfiguration?.Clone() ?? new OfficeConfiguration();

		ApplyInstallation(options, configuration);
		ApplyProducts(options, configuration);
		ApplyProperties(options, configuration);
		ApplyGeneral(options, configuration);

		return configuration;

	}

	private static ProductEntry? FindProduct(IReadOnlyList<ProductEntry> products, ProductCategory category) {

		foreach (var product in products) {

			var definition = ProductCatalog.Find(product.Id);

			if (definition?.Category == category) return product with { Id = definition.Id };

		}

		return null;

	}

	private static string NormalizeLanguage(string id) => LanguageCatalog.Find(id)?.Id ?? id;

	private static string NormalizeApp(string id) => AppCatalog.Find(id)?.Id ?? id;

	private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value!.Trim();

	private static bool GetBooleanProperty(OfficeConfiguration configuration, string name) => OdtBoolean.Parse(configuration.GetProperty(name)) ?? false;

	private static LicensingMode GetLicensing(OfficeConfiguration configuration) {

		if (GetBooleanProperty(configuration, OdtPropertyNames.SharedComputerLicensing)) return LicensingMode.SharedComputer;

		if (GetBooleanProperty(configuration, OdtPropertyNames.DeviceBasedLicensing)) return LicensingMode.DeviceBased;

		return LicensingMode.UserBased;

	}

	private static int GetAppOrder(string id) {

		var index = AppCatalog.All.ToList().FindIndex(app => string.Equals(app.Id, id, StringComparison.OrdinalIgnoreCase));

		return index < 0 ? int.MaxValue : index;

	}

	private static void ApplyInstallation(DeploymentOptions options, OfficeConfiguration configuration) {

		configuration.OfficeClientEdition = options.Architecture == OfficeArchitecture.X86 ? x86Edition : x64Edition;
		configuration.Channel = options.Channel;
		configuration.Version = NullIfBlank(options.Version);
		configuration.MigrateArchitecture = options.MigrateArchitecture ? true : null;

		// Installation always comes from the content delivery network, so local source settings are dropped.
		configuration.SourcePath = null;
		configuration.DownloadPath = null;
		configuration.AllowCdnFallback = null;

	}

	private void ApplyProducts(DeploymentOptions options, OfficeConfiguration configuration) {

		configuration.RemoveProducts(IsManagedProduct);

		var languages = new[] { options.PrimaryLanguage }.Concat(options.AdditionalLanguages).Where(static id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		var excludedApps = options.ExcludedApps.OrderBy(GetAppOrder).ThenBy(static id => id, StringComparer.OrdinalIgnoreCase).ToList();
		var selections = new[] {
			(Id: options.SuiteProductId, Key: options.SuiteProductKey),
			(Id: options.VisioProductId, Key: options.VisioProductKey),
			(Id: options.ProjectProductId, Key: options.ProjectProductKey),
		};

		foreach (var (id, key) in selections) {

			if (string.IsNullOrWhiteSpace(id)) continue;

			configuration.AddProduct(new ProductEntry(id!, languages, excludedApps, NullIfBlank(key)));

		}

		var proofingLanguages = options.ProofingLanguages.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

		if (proofingLanguages.Count == 0) return;

		configuration.AddProduct(new ProductEntry(ProductIds.ProofingTools, proofingLanguages, []));

	}

	private static void ApplyProperties(DeploymentOptions options, OfficeConfiguration configuration) {

		configuration.SetProperty(OdtPropertyNames.SharedComputerLicensing, OdtBoolean.FormatNumeric(options.Licensing == LicensingMode.SharedComputer));
		configuration.SetProperty(OdtPropertyNames.DeviceBasedLicensing, OdtBoolean.FormatNumeric(options.Licensing == LicensingMode.DeviceBased));
		configuration.SetProperty(OdtPropertyNames.ForceAppShutdown, OdtBoolean.Format(options.ForceAppShutdown));
		configuration.SetProperty(OdtPropertyNames.PinIconsToTaskbar, OdtBoolean.Format(options.PinIconsToTaskbar));
		configuration.SetProperty(OdtPropertyNames.AutoActivate, options.AutoActivate ? OdtBoolean.NumericTrue : null);

	}

	private static void ApplyGeneral(DeploymentOptions options, OfficeConfiguration configuration) {

		configuration.UpdatesEnabled = options.UpdatesEnabled;
		configuration.RemoveMsi = options.RemoveMsi;
		configuration.DisplayLevel = options.DisplayLevel.ToString();
		configuration.AcceptEula = options.AcceptEula;
		configuration.Company = NullIfBlank(options.Company);
		configuration.Description = NullIfBlank(options.Description);

	}

}
