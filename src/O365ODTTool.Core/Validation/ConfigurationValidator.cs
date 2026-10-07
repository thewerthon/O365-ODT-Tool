using System.Text.RegularExpressions;
using O365ODTTool.Core.Catalogs;
using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Validation;

/// <summary>
/// Detects problems in configurations before they reach setup.
/// </summary>
public sealed class ConfigurationValidator : IConfigurationValidator {

	private static readonly string[] supportedEditions = ["64", "32"];
	private static readonly Regex versionPattern = new(@"^16\.0\.\d+\.\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	/// <inheritdoc />
	public IReadOnlyList<ValidationIssue> Validate(OfficeConfiguration configuration) {

		var issues = new List<ValidationIssue>();
		var channel = ChannelCatalog.Find(configuration.Channel);

		ValidateEdition(configuration, issues);
		ValidateChannel(configuration, channel, issues);
		ValidateVersion(configuration, issues);
		ValidateProducts(configuration, channel, issues);

		return issues;

	}

	private static void ValidateEdition(OfficeConfiguration configuration, List<ValidationIssue> issues) {

		var edition = configuration.OfficeClientEdition;

		if (edition is null || supportedEditions.Contains(edition.Trim())) return;

		issues.Add(new(ValidationSeverity.Error, $"Architecture '{edition}' is invalid; use 64 or 32."));

	}

	private static void ValidateChannel(OfficeConfiguration configuration, ChannelDefinition? channel, List<ValidationIssue> issues) {

		if (configuration.Channel is null || channel is not null) return;

		issues.Add(new(ValidationSeverity.Warning, $"Channel '{configuration.Channel}' is unknown."));

	}

	private static void ValidateVersion(OfficeConfiguration configuration, List<ValidationIssue> issues) {

		var version = configuration.Version;

		if (version is null || versionPattern.IsMatch(version.Trim())) return;

		issues.Add(new(ValidationSeverity.Error, $"Version '{version}' is invalid; use the 16.0.xxxxx.yyyyy format."));

	}

	private static void ValidateProducts(OfficeConfiguration configuration, ChannelDefinition? channel, List<ValidationIssue> issues) {

		var products = configuration.Products;

		if (products.Count == 0) {

			issues.Add(new(ValidationSeverity.Error, "Select at least one product."));

			return;

		}

		foreach (var product in products) {

			ValidateProductLanguages(product, issues);
			ValidateProductChannel(product, channel, issues);

		}

	}

	private static void ValidateProductLanguages(ProductEntry product, List<ValidationIssue> issues) {

		if (product.Languages.Count == 0) {

			issues.Add(new(ValidationSeverity.Error, $"Product '{product.Id}' has no language."));

			return;

		}

		foreach (var language in product.Languages.Where(static language => LanguageCatalog.Find(language) is null)) {

			issues.Add(new(ValidationSeverity.Warning, $"Language '{language}' of product '{product.Id}' is unknown."));

		}

	}

	private static void ValidateProductChannel(ProductEntry product, ChannelDefinition? channel, List<ValidationIssue> issues) {

		var definition = ProductCatalog.Find(product.Id);

		if (definition is null) {

			issues.Add(new(ValidationSeverity.Warning, $"Product '{product.Id}' is unknown."));

			return;

		}

		if (channel is null || definition.IsChannelAgnostic) return;

		if (definition.IsVolume && !string.Equals(definition.RequiredChannel, channel.Id, StringComparison.OrdinalIgnoreCase)) {

			var requiredChannel = ChannelCatalog.Find(definition.RequiredChannel)?.DisplayName ?? definition.RequiredChannel;

			issues.Add(new(ValidationSeverity.Error, $"{definition.DisplayName} requires the {requiredChannel} channel."));

			return;

		}

		if (!definition.IsVolume && channel.IsPerpetual) issues.Add(new(ValidationSeverity.Error, $"{definition.DisplayName} cannot be deployed from the {channel.DisplayName} channel."));

	}

}
