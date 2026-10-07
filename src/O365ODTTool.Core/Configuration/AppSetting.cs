namespace O365ODTTool.Core.Configuration;

/// <summary>
/// Represents an application setting declared in a configuration file.
/// </summary>
/// <param name="Scope">The setting scope, either a setup or a user setting.</param>
/// <param name="Name">The setting name.</param>
/// <param name="Value">The setting value.</param>
/// <param name="Key">The registry key of a user setting.</param>
/// <param name="Type">The registry value type of a user setting.</param>
/// <param name="App">The application the user setting applies to.</param>
/// <param name="Id">The policy identifier of a user setting.</param>
public sealed record AppSetting(AppSettingScope Scope, string Name, string? Value, string? Key = null, string? Type = null, string? App = null, string? Id = null);
