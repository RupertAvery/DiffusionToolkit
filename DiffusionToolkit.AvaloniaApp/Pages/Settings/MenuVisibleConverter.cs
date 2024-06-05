using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiffusionToolkit.AvaloniaApp.Pages.Settings;

public class MenuVisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (string)value == (string)parameter;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}