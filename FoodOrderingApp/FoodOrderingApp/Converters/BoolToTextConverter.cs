using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.Converters;

public class BoolToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;

        if (parameter?.ToString() == "Header")
        {
            return flag ? "Edit Address" : "Add New Address";
        }

        return flag ? "Update Address" : "Add Address";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
