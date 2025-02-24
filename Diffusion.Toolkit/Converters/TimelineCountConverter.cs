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

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value switch
        {
            0 => none,
            > 0 and <= 100 => low1,
            > 100 and <= 200 => low2,
            > 200 and <= 300 => low3,
            > 300 and <= 400 => mid1,
            > 400 and <= 500 => mid2,
            > 500 and <= 600 => mid3,
            > 600 and <= 700 => high1,
            > 700 and <= 800 => high2,
            > 800 and <= 900 => high3,
            _ => over
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}