using System;
using System.Globalization;
using System.Windows.Data;
using FontAwesome.WPF;

namespace Diffusion.Toolkit.Converters;

public class PopoutIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? FontAwesomeIcon.Columns : FontAwesomeIcon.WindowRestore;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}