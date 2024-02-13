using System;
using System.Globalization;
using System.Windows.Data;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Converters;

public class SizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var image = value as ImageViewModel;

        if (image == null) return "";

        return image.Width ==0 || image.Height ==0 ? "" : $"{image.Width} x {image.Height}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}