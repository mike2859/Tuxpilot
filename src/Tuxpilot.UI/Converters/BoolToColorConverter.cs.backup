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
                ? new SolidColorBrush(Color.Parse("#10B981")) // Vert
                : new SolidColorBrush(Color.Parse("#EF4444")); // Rouge
        }
        
        return new SolidColorBrush(Color.Parse("#6B7280")); // Gris par défaut
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}