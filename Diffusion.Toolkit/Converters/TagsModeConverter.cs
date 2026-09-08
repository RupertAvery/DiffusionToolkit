using System;
using System.Globalization;
using System.Windows.Data;
using Diffusion.Common.Query;

namespace Diffusion.Toolkit.Converters;

public class TagsModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((TagsMode)value).ToString() == (string)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (string)parameter == "AND" ? TagsMode.AND : TagsMode.OR;
    }
}