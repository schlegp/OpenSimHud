using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpenSimHud.Converters;

public class DistanceToOpacityConverter : IValueConverter
{
    public static readonly DistanceToOpacityConverter Instance = new();
    private double MaxVisibleDistance = 20;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double distanceValue && parameter is double && targetType.IsAssignableTo(typeof(double)))
        {
            if (distanceValue > MaxVisibleDistance)
            {
                return 0.0;
            }

            return 1 - (distanceValue / MaxVisibleDistance);
        }
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}