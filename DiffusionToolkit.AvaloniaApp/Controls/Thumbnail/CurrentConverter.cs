using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DiffusionToolkit.AvaloniaApp.Controls.Thumbnail;

public class CurrentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
       return (bool)value ? new SolidColorBrush(Color.Parse("#80306fdb")) : Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class BackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var thumbnail = (ThumbnailViewModel)value;

        if (thumbnail.IsCurrent)
        {
            return new SolidColorBrush(Color.Parse("#80306fdb"));
        }
        else if (thumbnail.IsSelected)
        {
            return new SolidColorBrush(Color.Parse("#80306fdb"));
        }

        return  Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}