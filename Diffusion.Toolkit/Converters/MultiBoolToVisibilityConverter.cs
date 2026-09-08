using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class MultiBoolToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
    {return values.Select(d => d is bool ? d : false).Cast<bool>().Aggregate(true, (a, b) =>  a && b) ? Visibility.Visible : Visibility.Hidden;
    }
    public object[] ConvertBack(object value, Type[] targetTypes,
        object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException("Cannot convert back");
    }
}


public class MultiBoolToVisibilityCollapsedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
    {
        return values.Select(d => d is bool ? d : false).Cast<bool>().Aggregate(true, (a, b) => a && b) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object[] ConvertBack(object value, Type[] targetTypes,
        object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException("Cannot convert back");
    }
}
