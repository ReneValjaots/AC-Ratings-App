using System.Globalization;
using System.Windows.Data;

namespace Ac.Ratings;

public class EnumToBooleanConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == null || parameter == null)
            return false;

        string? valueString = value.ToString();
        string? paramString = parameter.ToString();

        if (valueString == null || paramString == null)
            return false;

        return valueString.Equals(paramString, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not bool boolValue || parameter == null)
            return Binding.DoNothing;

        string? paramString = parameter.ToString();
        return boolValue && paramString != null ? paramString : Binding.DoNothing;
    }
}