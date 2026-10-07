namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Lists the applications that can be excluded from a deployment.
/// </summary>
public static class AppCatalog {

	/// <summary>Gets every known application.</summary>
	public static IReadOnlyList<AppDefinition> All { get; } = [
		new("Access", "Access"),
		new(AppIds.Bing, "Microsoft Search in Bing"),
		new("Excel", "Excel"),
		new(AppIds.Groove, "OneDrive (Groove)"),
		new(AppIds.Lync, "Skype for Business"),
		new("OneDrive", "OneDrive"),
		new("OneNote", "OneNote"),
		new("Outlook", "Outlook (classic)"),
		new("OutlookForWindows", "Outlook (new)"),
		new("PowerPoint", "PowerPoint"),
		new("Publisher", "Publisher"),
		new("Teams", "Teams"),
		new("Word", "Word"),
	];

	/// <summary>
	/// Finds an application by identifier, ignoring case.
	/// </summary>
	public static AppDefinition? Find(string? id) => All.FirstOrDefault(app => string.Equals(app.Id, id, StringComparison.OrdinalIgnoreCase));

}
