namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Lists the update channels supported by the Office Deployment Tool.
/// </summary>
public static class ChannelCatalog {

	private static readonly Dictionary<string, string> legacyAliases = new(StringComparer.OrdinalIgnoreCase) {
		["Monthly"] = ChannelIds.Current,
		["Insiders"] = ChannelIds.BetaChannel,
		["InsiderFast"] = ChannelIds.BetaChannel,
		["FirstReleaseCurrent"] = ChannelIds.CurrentPreview,
		["MonthlyTargeted"] = ChannelIds.CurrentPreview,
		["Broad"] = ChannelIds.SemiAnnual,
		["Deferred"] = ChannelIds.SemiAnnual,
		["Targeted"] = ChannelIds.SemiAnnualPreview,
		["FirstReleaseDeferred"] = ChannelIds.SemiAnnualPreview,
	};

	/// <summary>Gets every known channel.</summary>
	public static IReadOnlyList<ChannelDefinition> All { get; } = [
		new(ChannelIds.Current, "Current Channel"),
		new(ChannelIds.CurrentPreview, "Current Channel (Preview)"),
		new(ChannelIds.MonthlyEnterprise, "Monthly Enterprise Channel"),
		new(ChannelIds.SemiAnnual, "Semi-Annual Enterprise Channel"),
		new(ChannelIds.SemiAnnualPreview, "Semi-Annual Enterprise Channel (Preview)"),
		new(ChannelIds.BetaChannel, "Beta Channel"),
		new(ChannelIds.PerpetualVL2024, "Office LTSC 2024 Perpetual Enterprise", IsPerpetual: true),
		new(ChannelIds.PerpetualVL2021, "Office LTSC 2021 Perpetual Enterprise", IsPerpetual: true),
		new(ChannelIds.PerpetualVL2019, "Office 2019 Perpetual Enterprise", IsPerpetual: true),
	];

	/// <summary>
	/// Finds a channel by identifier or legacy alias, ignoring case.
	/// </summary>
	public static ChannelDefinition? Find(string? id) {

		if (id is null) return null;

		var canonicalId = legacyAliases.TryGetValue(id.Trim(), out var alias) ? alias : id.Trim();

		return All.FirstOrDefault(channel => string.Equals(channel.Id, canonicalId, StringComparison.OrdinalIgnoreCase));

	}

}
