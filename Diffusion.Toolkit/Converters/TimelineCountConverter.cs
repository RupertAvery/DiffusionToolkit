using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace Diffusion.Toolkit.Converters;

public class TimelineCountConverter : IValueConverter
{
    static Color FromHex(string color)
    {
        var r = System.Convert.ToByte(color.Substring(1, 2), 16);
        var g = System.Convert.ToByte(color.Substring(3, 2), 16);
        var b = System.Convert.ToByte(color.Substring(5, 2), 16);
        return Color.FromArgb(255, r, g, b);
    }

    static SolidColorBrush none = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
    static SolidColorBrush over = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

    static SolidColorBrush low1 = new SolidColorBrush(FromHex("#009a17"));
    static SolidColorBrush low2 = new SolidColorBrush(FromHex("#05cf0b"));
    static SolidColorBrush low3 = new SolidColorBrush(FromHex("#09ff00"));

    static SolidColorBrush mid1 = new SolidColorBrush(FromHex("#2bff0c"));
    static SolidColorBrush mid2 = new SolidColorBrush(FromHex("#61ff1e"));
    static SolidColorBrush mid3 = new SolidColorBrush(FromHex("#8eff2e"));

    static SolidColorBrush high1 = new SolidColorBrush(FromHex("#b5ff3c"));
    static SolidColorBrush high2 = new SolidColorBrush(FromHex("#d4ff46"));
    static SolidColorBrush high3 = new SolidColorBrush(FromHex("#fbff53"));

    //private static SolidColorBrush[] colors = new[] { low1, low2, low3, mid1, mid2, mid3, high1, high2, high3 };
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //var intValue = (int)value;
        //int maxValue = 3000;
        //var interval = maxValue / colors.Length ;


        //if (intValue == 0)
        //{
        //    return none;
        //}
        //else if (intValue > maxValue)
        //{
        //    return over;
        //}
        //else
        //{
        //    var index = intValue / interval;
        //    return  colors[index];
        //}


        return (int)value switch
        {
            0 => none,
            > 0 and <= 50 => low1,
            > 50 and <= 100 => low2,
            > 100 and <= 250 => low3,
            > 250 and <= 500 => mid1,
            > 500 and <= 1000 => mid2,
            > 1000 and <= 1500 => mid3,
            > 1500 and <= 2000 => high1,
            > 2000 and <= 5000 => high2,
            > 5000 and <= 10000 => high3,
            _ => over
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}