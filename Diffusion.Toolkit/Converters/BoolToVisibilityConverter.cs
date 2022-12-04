using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using static SQLite.SQLite3;

namespace Diffusion.Toolkit.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? 0.35 : 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



public class MultiBoolToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
    {
        return values.Cast<bool>().Aggregate(true, (a, b) =>  a && b) ? Visibility.Visible : Visibility.Hidden;
    }
    public object[] ConvertBack(object value, Type[] targetTypes,
        object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException("Cannot convert back");
    }
}