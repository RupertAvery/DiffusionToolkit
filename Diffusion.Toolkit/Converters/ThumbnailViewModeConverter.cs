using System;
using System.Globalization;
using System.Windows.Data;
using Diffusion.Toolkit.Controls;

namespace Diffusion.Toolkit.Converters;

public class ThumbnailViewModeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return false;

        return (ThumbnailViewMode)value == (ThumbnailViewMode)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}