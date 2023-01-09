using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Brush = Avalonia.Media.Brush;

namespace OpenSimHud.Converters;

public class DistanceToColorConverter : IValueConverter
{
    public static readonly DistanceToColorConverter Instance = new();

    public IBrush Convert(double distanceValue)
    {
        if (distanceValue > 10)
        {
            return Brush.Parse("Green");
        }
        if (distanceValue > 5)
        {
            return Brush.Parse("Orange");
        }

        return Brush.Parse("Red");
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double distanceValue && parameter is Brush brush && targetType.IsAssignableTo(typeof(Brush)))
        {
            if (distanceValue > 10)
            {
                return Brush.Parse("Black");
            }
            if (distanceValue > 5)
            {
                return Brush.Parse("Orange");
            }

            return Brush.Parse("Red");
        }
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}