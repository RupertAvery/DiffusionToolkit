﻿using Diffusion.Toolkit.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diffusion.Toolkit.Converters;

public class ValueVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return Visibility.Hidden;

        return value.ToString().Equals(parameter) ? Visibility.Visible : Visibility.Hidden;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}