using System;
using System.Globalization;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class NumberFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var format = parameter != null ? (string)parameter : "N0";

        switch (value)
        {
            case double dValue:
                return dValue.ToString(format);
            case float sValue:
                return sValue.ToString(format);
            case decimal mValue:
                return mValue.ToString(format);
            case int iValue:
                return iValue.ToString(format);
            case long lValue:
                return lValue.ToString(format);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}