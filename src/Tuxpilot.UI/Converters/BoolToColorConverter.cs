using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Tuxpilot.UI.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool success)
        {
            return success 
                ? new SolidColorBrush(Color.Parse("Success")) // Vert
                : new SolidColorBrush(Color.Parse("Danger")); // Rouge
        }
        
        return new SolidColorBrush(Color.Parse("TextMuted")); // Gris par défaut
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}