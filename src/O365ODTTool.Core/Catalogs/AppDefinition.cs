namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Describes an application that can be excluded from a deployment.
/// </summary>
/// <param name="Id">The application identifier.</param>
/// <param name="DisplayName">The application display name.</param>
public sealed record AppDefinition(string Id, string DisplayName);
