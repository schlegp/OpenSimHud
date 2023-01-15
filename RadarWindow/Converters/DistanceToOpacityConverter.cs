using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace RadarWindow.Converters;

public class DistanceToOpacityConverter : IValueConverter
{
    public static readonly DistanceToOpacityConverter Instance = new();
    private readonly double _maxVisibleDistance = 20;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double distanceValue && parameter is double && targetType.IsAssignableTo(typeof(double)))
        {
            if (distanceValue > _maxVisibleDistance)
            {
                return 0.0;
            }

            return 1 - (distanceValue / _maxVisibleDistance);
        }
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}