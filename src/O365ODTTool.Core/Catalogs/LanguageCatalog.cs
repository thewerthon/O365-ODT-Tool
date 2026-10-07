namespace O365ODTTool.Core.Catalogs;

/// <summary>
/// Lists the languages supported by the Office Deployment Tool.
/// </summary>
public static class LanguageCatalog {

	/// <summary>Gets the dynamic languages resolved by setup.</summary>
	public static IReadOnlyList<LanguageDefinition> Special { get; } = [
		new(LanguageIds.MatchOS, "Match operating system", IsSpecial: true),
		new(LanguageIds.MatchInstalled, "Match installed Office languages", IsSpecial: true),
		new(LanguageIds.MatchPreviousMsi, "Match previous MSI languages", IsSpecial: true),
	];

	/// <summary>Gets the concrete languages.</summary>
	public static IReadOnlyList<LanguageDefinition> Concrete { get; } = [
		new("ar-sa", "Arabic"),
		new("bg-bg", "Bulgarian"),
		new("zh-cn", "Chinese (Simplified)"),
		new("zh-tw", "Chinese (Traditional)"),
		new("hr-hr", "Croatian"),
		new("cs-cz", "Czech"),
		new("da-dk", "Danish"),
		new("nl-nl", "Dutch"),
		new("en-us", "English (United States)"),
		new("en-gb", "English (United Kingdom)"),
		new("et-ee", "Estonian"),
		new("fi-fi", "Finnish"),
		new("fr-fr", "French (France)"),
		new("fr-ca", "French (Canada)"),
		new("de-de", "German"),
		new("el-gr", "Greek"),
		new("he-il", "Hebrew"),
		new("hi-in", "Hindi"),
		new("hu-hu", "Hungarian"),
		new("id-id", "Indonesian"),
		new("it-it", "Italian"),
		new("ja-jp", "Japanese"),
		new("kk-kz", "Kazakh"),
		new("ko-kr", "Korean"),
		new("lv-lv", "Latvian"),
		new("lt-lt", "Lithuanian"),
		new("ms-my", "Malay"),
		new("nb-no", "Norwegian (Bokmål)"),
		new("pl-pl", "Polish"),
		new(LanguageIds.BrazilianPortuguese, "Portuguese (Brazil)"),
		new("pt-pt", "Portuguese (Portugal)"),
		new("ro-ro", "Romanian"),
		new("ru-ru", "Russian"),
		new("sr-latn-rs", "Serbian (Latin)"),
		new("sk-sk", "Slovak"),
		new("sl-si", "Slovenian"),
		new("es-es", "Spanish (Spain)"),
		new("es-mx", "Spanish (Mexico)"),
		new("sv-se", "Swedish"),
		new("th-th", "Thai"),
		new("tr-tr", "Turkish"),
		new("uk-ua", "Ukrainian"),
		new("vi-vn", "Vietnamese"),
	];

	/// <summary>
	/// Finds a special or concrete language by identifier, ignoring case.
	/// </summary>
	public static LanguageDefinition? Find(string? id) => Special.Concat(Concrete).FirstOrDefault(language => string.Equals(language.Id, id, StringComparison.OrdinalIgnoreCase));

}
