using System.Globalization;
using System.Windows.Data;

namespace O365ODTTool.Converters;

/// <summary>
/// Converts an enumeration value to whether it matches the member named by the converter parameter, so radio buttons can bind to a single enumeration property.
/// </summary>
[ValueConversion(typeof(Enum), typeof(bool))]
public sealed class EnumMatchConverter : IValueConverter {

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is Enum && string.Equals(value.ToString(), parameter as string, StringComparison.Ordinal);

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {

		if (value is not true || parameter is not string memberName) return Binding.DoNothing;

		return Enum.Parse(targetType, memberName);

	}

}
