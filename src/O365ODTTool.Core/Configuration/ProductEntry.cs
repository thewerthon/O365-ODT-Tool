namespace O365ODTTool.Core.Configuration;

/// <summary>
/// Represents a product declared in a configuration file.
/// </summary>
/// <param name="Id">The product identifier.</param>
/// <param name="Languages">The language identifiers to deploy for the product.</param>
/// <param name="ExcludedApps">The application identifiers excluded from the product.</param>
/// <param name="ProductKey">The optional product key.</param>
public sealed record ProductEntry(string Id, IReadOnlyList<string> Languages, IReadOnlyList<string> ExcludedApps, string? ProductKey = null);
