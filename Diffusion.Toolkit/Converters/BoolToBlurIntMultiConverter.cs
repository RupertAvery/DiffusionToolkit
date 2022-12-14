using System;
using System.Globalization;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class BoolToBlurIntMultiConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value[0] is bool && value[1] is bool)
        {
            return (bool)value[0] && (bool)value[1] ? double.Parse((string)parameter) : 0d;
        }

        return 0d;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}