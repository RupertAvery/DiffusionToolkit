using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Converters;

public class FolderStateVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((FolderState)value)
        {
            case FolderState.Collapsed:
                return Visibility.Collapsed;
            case FolderState.Expanded:
                return Visibility.Visible;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}