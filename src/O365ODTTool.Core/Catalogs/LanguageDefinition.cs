namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Describes a deployable language.
/// </summary>
/// <param name="Id">The language identifier.</param>
/// <param name="DisplayName">The language display name.</param>
/// <param name="IsSpecial">Whether the identifier is a dynamic value resolved by setup.</param>
public sealed record LanguageDefinition(string Id, string DisplayName, bool IsSpecial = false);
