using System;
using System.Globalization;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class AddConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case double dValue:
                return dValue + double.Parse((string)parameter);
            case float sValue:
                return sValue + float.Parse((string)parameter);
            case decimal mValue:
                return mValue + decimal.Parse((string)parameter);
            case int iValue:
                return iValue + int.Parse((string)parameter);
            case long lValue:
                return lValue + long.Parse((string)parameter);
            case string strValue:
                return strValue + (string)parameter;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}