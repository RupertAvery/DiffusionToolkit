using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Diffusion.Toolkit.Converters;

public class TimelineSelectedConverter : IValueConverter
{
    static SolidColorBrush selected = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            var defaultBrush = (SolidColorBrush)Application.Current.Resources["ForegroundBrush"];
            return defaultBrush;
        }
        else
        {
            var defaultBrush = (SolidColorBrush)Application.Current.Resources["SecondaryBrush"];
            return defaultBrush;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}