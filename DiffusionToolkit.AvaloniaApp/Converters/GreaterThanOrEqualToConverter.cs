using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiffusionToolkit.AvaloniaApp.Converters;

public class GreaterThanOrEqualToConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value == null) return false;
        return (int?)value >= int.Parse((string)parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}