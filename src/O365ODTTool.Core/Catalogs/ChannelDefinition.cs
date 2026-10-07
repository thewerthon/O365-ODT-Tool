namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Describes an update channel.
/// </summary>
/// <param name="Id">The channel identifier.</param>
/// <param name="DisplayName">The channel display name.</param>
/// <param name="IsPerpetual">Whether the channel serves volume licensed perpetual products.</param>
public sealed record ChannelDefinition(string Id, string DisplayName, bool IsPerpetual = false);
