using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PinRef.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var boolValue = value is true;
		var invert = parameter is "Invert";

		if (invert)
			boolValue = !boolValue;

		return boolValue ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
