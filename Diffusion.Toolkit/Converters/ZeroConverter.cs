using System;
using System.Globalization;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class ZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case double dValue:
                return dValue == 0 ? "" : value;
            case float sValue:
                return sValue == 0 ? "" : value;
            case decimal mValue:
                return mValue == 0 ? "" : value;
            case int iValue:
                return iValue == 0 ? "" : value;
            case long lValue:
                return lValue == 0 ? "" : value;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}