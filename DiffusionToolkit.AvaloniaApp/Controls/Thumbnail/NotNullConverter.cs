using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class NotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}