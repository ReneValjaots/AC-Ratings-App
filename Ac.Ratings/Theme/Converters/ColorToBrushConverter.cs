using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ac.Ratings.Theme.Converters;

[ValueConversion(typeof(Color), typeof(SolidColorBrush))]
public class ColorToBrushConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is Color color) {
            return new SolidColorBrush(color);
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is SolidColorBrush brush) {
            return brush.Color;
        }

        return default(Color);
    }
}