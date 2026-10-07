using System.Xml.Linq;

namespace O365ODTTool.Core.Configuration;

/// <summary>
/// Represents an Office Deployment Tool configuration, exposing typed access to its options while preserving every unrecognized element and attribute.
/// </summary>
public sealed class OfficeConfiguration {

	private const string companySettingName = "Company";

	// Mirrors the element order produced by the Office Customization Tool; unknown elements are kept after the known ones.
	private static readonly string[] elementOrder = [
		OdtNames.Info,
		OdtNames.Add,
		OdtNames.Property,
		OdtNames.Updates,
		OdtNames.RemoveMsi,
		OdtNames.Remove,
		OdtNames.AppSettings,
		OdtNames.Display,
		OdtNames.Logging,
	];

	/// <summary>
	/// Initializes an empty configuration.
	/// </summary>
	public OfficeConfiguration() : this(new XElement(OdtNames.Configuration)) { }

	internal OfficeConfiguration(XElement root) => Root = root;

	internal XElement Root { get; }

	/// <summary>Gets or sets the configuration description.</summary>
	public string? Description {
		get => GetAttribute(OdtNames.Info, OdtNames.Description);
		set => SetAttribute(OdtNames.Info, OdtNames.Description, value);
	}

	/// <summary>Gets or sets the architecture, either <c>64</c> or <c>32</c>.</summary>
	public string? OfficeClientEdition {
		get => GetAttribute(OdtNames.Add, OdtNames.OfficeClientEdition);
		set => SetAttribute(OdtNames.Add, OdtNames.OfficeClientEdition, value);
	}

	/// <summary>Gets or sets the installation channel.</summary>
	public string? Channel {
		get => GetAttribute(OdtNames.Add, OdtNames.Channel);
		set => SetAttribute(OdtNames.Add, OdtNames.Channel, value);
	}

	/// <summary>Gets or sets the version to deploy, or <see langword="null"/> for the latest one.</summary>
	public string? Version {
		get => GetAttribute(OdtNames.Add, OdtNames.Version);
		set => SetAttribute(OdtNames.Add, OdtNames.Version, value);
	}

	/// <summary>Gets or sets the installation files location.</summary>
	public string? SourcePath {
		get => GetAttribute(OdtNames.Add, OdtNames.SourcePath);
		set => SetAttribute(OdtNames.Add, OdtNames.SourcePath, value);
	}

	/// <summary>Gets or sets the download location used by the Configuration Manager integration.</summary>
	public string? DownloadPath {
		get => GetAttribute(OdtNames.Add, OdtNames.DownloadPath);
		set => SetAttribute(OdtNames.Add, OdtNames.DownloadPath, value);
	}

	/// <summary>Gets or sets whether the content delivery network is used when the source location is unavailable.</summary>
	public bool? AllowCdnFallback {
		get => GetBoolean(OdtNames.Add, OdtNames.AllowCdnFallback);
		set => SetBoolean(OdtNames.Add, OdtNames.AllowCdnFallback, value);
	}

	/// <summary>Gets or sets whether an existing installation with another architecture is migrated.</summary>
	public bool? MigrateArchitecture {
		get => GetBoolean(OdtNames.Add, OdtNames.MigrateArch);
		set => SetBoolean(OdtNames.Add, OdtNames.MigrateArch, value);
	}

	/// <summary>Gets or sets whether updates are enabled.</summary>
	public bool? UpdatesEnabled {
		get => GetBoolean(OdtNames.Updates, OdtNames.Enabled);
		set => SetBoolean(OdtNames.Updates, OdtNames.Enabled, value);
	}

	/// <summary>Gets or sets the setup user interface level.</summary>
	public string? DisplayLevel {
		get => GetAttribute(OdtNames.Display, OdtNames.Level);
		set => SetAttribute(OdtNames.Display, OdtNames.Level, value);
	}

	/// <summary>Gets or sets whether the license agreement is accepted automatically.</summary>
	public bool? AcceptEula {
		get => GetBoolean(OdtNames.Display, OdtNames.AcceptEula);
		set => SetBoolean(OdtNames.Display, OdtNames.AcceptEula, value);
	}

	/// <summary>Gets or sets whether previous MSI installations are removed.</summary>
	public bool RemoveMsi {
		get => Root.Element(OdtNames.RemoveMsi) is not null;
		set => SetRemoveMsi(value);
	}

	/// <summary>Gets or sets the organization name.</summary>
	public string? Company {
		get => FindCompanySetting()?.Attribute(OdtNames.Value)?.Value;
		set => SetCompany(value);
	}

	/// <summary>Gets the declared products.</summary>
	public IReadOnlyList<ProductEntry> Products => Root.Element(OdtNames.Add)?.Elements(OdtNames.Product).Select(ToProductEntry).ToList() ?? [];

	/// <summary>Gets the declared application settings.</summary>
	public IReadOnlyList<AppSetting> AppSettings => Root.Element(OdtNames.AppSettings)?.Elements().Select(ToAppSetting).OfType<AppSetting>().ToList() ?? [];

	/// <summary>
	/// Gets the value of a property, or <see langword="null"/> when it is not declared.
	/// </summary>
	public string? GetProperty(string name) => FindProperty(name)?.Attribute(OdtNames.Value)?.Value;

	/// <summary>
	/// Sets the value of a property, removing it when the value is <see langword="null"/>.
	/// </summary>
	public void SetProperty(string name, string? value) {

		var property = FindProperty(name);

		if (value is null) {

			property?.Remove();

			return;

		}

		if (property is null) {

			property = new XElement(OdtNames.Property, new XAttribute(OdtNames.Name, name));

			InsertElement(property);

		}

		property.SetAttributeValue(OdtNames.Value, value);

	}

	/// <summary>
	/// Adds a product declaration.
	/// </summary>
	public void AddProduct(ProductEntry product) {

		var element = new XElement(OdtNames.Product, new XAttribute(OdtNames.Id, product.Id));

		if (!string.IsNullOrWhiteSpace(product.ProductKey)) element.SetAttributeValue(OdtNames.ProductKey, product.ProductKey);

		element.Add(product.Languages.Select(static language => new XElement(OdtNames.Language, new XAttribute(OdtNames.Id, language))));
		element.Add(product.ExcludedApps.Select(static app => new XElement(OdtNames.ExcludeApp, new XAttribute(OdtNames.Id, app))));

		GetOrCreateElement(OdtNames.Add).Add(element);

	}

	/// <summary>
	/// Removes every product whose identifier matches the predicate.
	/// </summary>
	public void RemoveProducts(Func<string, bool> predicate) {

		var products = Root.Element(OdtNames.Add)?.Elements(OdtNames.Product).Where(element => predicate(GetIdentifier(element))).ToList() ?? [];

		products.ForEach(static product => product.Remove());

	}

	/// <summary>
	/// Creates a deep copy of the configuration.
	/// </summary>
	public OfficeConfiguration Clone() => new(new XElement(Root));

	private static ProductEntry ToProductEntry(XElement element) {

		var languages = element.Elements(OdtNames.Language).Select(GetIdentifier).Where(static id => id.Length > 0).ToList();
		var excludedApps = element.Elements(OdtNames.ExcludeApp).Select(GetIdentifier).Where(static id => id.Length > 0).ToList();

		return new ProductEntry(GetIdentifier(element), languages, excludedApps, FindAttribute(element, OdtNames.ProductKey)?.Value);

	}

	private static AppSetting? ToAppSetting(XElement element) {

		AppSettingScope? scope = element.Name.LocalName switch {
			OdtNames.Setup => AppSettingScope.Setup,
			OdtNames.User => AppSettingScope.User,
			_ => null,
		};

		if (scope is null) return null;

		return new AppSetting(
			scope.Value,
			FindAttribute(element, OdtNames.Name)?.Value ?? string.Empty,
			FindAttribute(element, OdtNames.Value)?.Value,
			FindAttribute(element, OdtNames.Key)?.Value,
			FindAttribute(element, OdtNames.Type)?.Value,
			FindAttribute(element, OdtNames.App)?.Value,
			FindAttribute(element, OdtNames.SettingId)?.Value);

	}

	private static string GetIdentifier(XElement element) => FindAttribute(element, OdtNames.Id)?.Value.Trim() ?? string.Empty;

	private static XAttribute? FindAttribute(XElement element, string name) => element.Attributes().FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));

	private static void RemoveIfEmpty(XElement? element) {

		if (element is null || element.HasAttributes || element.HasElements) return;

		element.Remove();

	}

	private static int GetRank(string elementName) {

		var index = Array.IndexOf(elementOrder, elementName);

		return index < 0 ? elementOrder.Length : index;

	}

	private XElement? FindProperty(string name) => Root.Elements(OdtNames.Property).FirstOrDefault(element => string.Equals(FindAttribute(element, OdtNames.Name)?.Value, name, StringComparison.OrdinalIgnoreCase));

	private XElement? FindCompanySetting() => Root.Element(OdtNames.AppSettings)?.Elements(OdtNames.Setup).FirstOrDefault(static element => string.Equals(FindAttribute(element, OdtNames.Name)?.Value, companySettingName, StringComparison.OrdinalIgnoreCase));

	private string? GetAttribute(string elementName, string attributeName) {

		var element = Root.Element(elementName);

		return element is null ? null : FindAttribute(element, attributeName)?.Value;

	}

	private void SetAttribute(string elementName, string attributeName, string? value) {

		if (value is null) {

			var element = Root.Element(elementName);

			if (element is not null) FindAttribute(element, attributeName)?.Remove();

			RemoveIfEmpty(element);

			return;

		}

		var target = GetOrCreateElement(elementName);
		var attribute = FindAttribute(target, attributeName);

		if (attribute is null) {

			target.SetAttributeValue(attributeName, value);

			return;

		}

		attribute.Value = value;

	}

	private bool? GetBoolean(string elementName, string attributeName) => OdtBoolean.Parse(GetAttribute(elementName, attributeName));

	private void SetBoolean(string elementName, string attributeName, bool? value) => SetAttribute(elementName, attributeName, value is null ? null : OdtBoolean.Format(value.Value));

	private void SetRemoveMsi(bool value) {

		if (value) {

			GetOrCreateElement(OdtNames.RemoveMsi);

			return;

		}

		Root.Element(OdtNames.RemoveMsi)?.Remove();

	}

	private void SetCompany(string? value) {

		var setting = FindCompanySetting();

		if (string.IsNullOrWhiteSpace(value)) {

			setting?.Remove();
			RemoveIfEmpty(Root.Element(OdtNames.AppSettings));

			return;

		}

		if (setting is null) {

			setting = new XElement(OdtNames.Setup, new XAttribute(OdtNames.Name, companySettingName));

			GetOrCreateElement(OdtNames.AppSettings).AddFirst(setting);

		}

		setting.SetAttributeValue(OdtNames.Value, value);

	}

	private XElement GetOrCreateElement(string elementName) {

		var element = Root.Element(elementName);

		if (element is not null) return element;

		element = new XElement(elementName);

		InsertElement(element);

		return element;

	}

	private void InsertElement(XElement element) {

		var rank = GetRank(element.Name.LocalName);
		var successor = Root.Elements().FirstOrDefault(existing => GetRank(existing.Name.LocalName) > rank);

		if (successor is null) {

			Root.Add(element);

			return;

		}

		successor.AddBeforeSelf(element);

	}

}
