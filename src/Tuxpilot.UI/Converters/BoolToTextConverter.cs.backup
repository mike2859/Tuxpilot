using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tuxpilot.UI.Converters;


public class BoolToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool success)
        {
            return success ? "✅ Succès" : "❌ Échec";
        }
        
        return "❓ Inconnu";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}