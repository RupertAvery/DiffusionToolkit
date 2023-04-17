using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if((int?)value == null) return Visibility.Collapsed;
        return (int?)value >= int.Parse((string)parameter, CultureInfo.InvariantCulture) ? Visibility.Visible : Visibility.Collapsed;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = (string?)value;

        if (string.IsNullOrEmpty(str)) return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}