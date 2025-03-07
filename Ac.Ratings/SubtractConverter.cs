using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ac.Ratings;

public class SubtractConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is double width && double.TryParse(parameter?.ToString(), out double subtract)) {
            double result = width - subtract;
            if (result > 0) {
                return result; // Return the calculated width if positive
            }
            else {
                return DependencyProperty.UnsetValue; // Prevent setting an invalid width
            }
        }

        return DependencyProperty.UnsetValue; // Fallback if conversion fails
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}