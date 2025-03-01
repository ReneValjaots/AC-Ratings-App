using System.Globalization;
using System.Windows.Data;

namespace Ac.Ratings;

public class EnumToBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value == null || parameter == null) return false;
        return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return (bool)value ? parameter.ToString() : Binding.DoNothing;
    }
}