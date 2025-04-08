using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class InverseBoolToVisibilityMultiConverter : IMultiValueConverter
{
    public object Convert(object[]? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?[0] is bool && value[1] is bool)
        {
            return (bool)value[0] && (bool)value[1] ? Visibility.Hidden : Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}