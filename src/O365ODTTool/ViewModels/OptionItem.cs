namespace O365ODTTool.ViewModels;

/// <summary>
/// Represents a choice displayed in a drop-down list.
/// </summary>
/// <param name="Id">The choice identifier, or <see langword="null"/> for the empty choice.</param>
/// <param name="DisplayName">The choice display name.</param>
public sealed record OptionItem(string? Id, string DisplayName);
