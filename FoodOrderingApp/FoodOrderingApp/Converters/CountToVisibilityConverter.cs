using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FoodOrderingApp.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // If ShowEmpty parameter, show when count is 0
        if (parameter?.ToString() == "ShowEmpty")
        {
            if (value is ICollection collection)
            {
                return collection.Count == 0;
            }
            if (value is int count)
            {
                return count == 0;
            }
            return true;
        }

        // Default: show when count > 0
        if (value is ICollection itemsCollection)
        {
            return itemsCollection.Count > 0;
        }
        if (value is int itemCount)
        {
            return itemCount > 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
