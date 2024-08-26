using System;
using System.Globalization;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var opacity = 0.5d;
        return (bool)value ? opacity : 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}