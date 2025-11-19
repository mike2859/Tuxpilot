using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tuxpilot.UI.Converters;


public class BoolToNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUser)
            return isUser ? "Vous" : "Assistant IA";
        
        return "Assistant IA";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}