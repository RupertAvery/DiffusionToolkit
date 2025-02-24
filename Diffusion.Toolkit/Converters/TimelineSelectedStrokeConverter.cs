using System;
using System.Globalization;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class TimelineSelectedStrokeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}