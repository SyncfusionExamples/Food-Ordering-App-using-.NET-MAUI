using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.Converters;

public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle IsZero parameter for collection count checks
        if (parameter?.ToString() == "IsZero" && value is int count)
        {
            return count == 0;
        }

        if (value == null)
            return false;

        return !string.IsNullOrWhiteSpace(value.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
