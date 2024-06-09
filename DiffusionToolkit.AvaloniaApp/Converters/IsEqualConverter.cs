using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;

namespace DiffusionToolkit.AvaloniaApp.Converters
{
    public class IsEqualConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string strParam)
            {
                var intParameter = int.Parse(strParam);
                return intValue == intParameter;
            }

            return value.Equals(parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
