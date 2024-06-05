using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class ForDeletionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)value ? 0.5f : 1.0f;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}