using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace PacmanGame.Views.Converters;

public class MultiplyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return 0;
        if (!double.TryParse(value.ToString(), out var v)) return 0;
        if (parameter == null) return v;
        if (!double.TryParse(parameter.ToString(), out var p)) return v;
        return v * p;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
