using System.Globalization;
using System.Windows.Data;

namespace O365ODTTool.Converters;

/// <summary>
/// Negates a boolean value in both directions.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public sealed class InverseBooleanConverter : IValueConverter {

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;

}
