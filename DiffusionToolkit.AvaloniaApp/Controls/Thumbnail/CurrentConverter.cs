using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class CurrentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
       return (bool)value ? Brushes.Red : Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}