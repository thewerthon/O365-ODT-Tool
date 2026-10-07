namespace O365ODTTool.Core.Configuration;

/// <summary>
/// Parses and formats boolean values as written in configuration files.
/// </summary>
public static class OdtBoolean {

	/// <summary>Textual true value.</summary>
	public const string True = "TRUE";

	/// <summary>Textual false value.</summary>
	public const string False = "FALSE";

	/// <summary>Numeric true value.</summary>
	public const string NumericTrue = "1";

	/// <summary>Numeric false value.</summary>
	public const string NumericFalse = "0";

	/// <summary>
	/// Parses a textual or numeric boolean value, returning <see langword="null"/> when it is missing or unrecognized.
	/// </summary>
	public static bool? Parse(string? value) => value?.Trim().ToUpperInvariant() switch {
		True or NumericTrue => true,
		False or NumericFalse => false,
		_ => null,
	};

	/// <summary>
	/// Formats a boolean value as textual <c>TRUE</c> or <c>FALSE</c>.
	/// </summary>
	public static string Format(bool value) => value ? True : False;

	/// <summary>
	/// Formats a boolean value as numeric <c>1</c> or <c>0</c>.
	/// </summary>
	public static string FormatNumeric(bool value) => value ? NumericTrue : NumericFalse;

}
