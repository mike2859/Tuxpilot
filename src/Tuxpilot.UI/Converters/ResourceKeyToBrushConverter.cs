using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Tuxpilot.UI.Converters;

public class ResourceKeyToBrushConverter : IValueConverter
{
    public static readonly ResourceKeyToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string key || string.IsNullOrWhiteSpace(key))
            return Brushes.Transparent;

        if (Application.Current?.TryFindResource(key, out var res) == true)
            return res as IBrush;

        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
