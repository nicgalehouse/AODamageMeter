using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AODamageMeter.UI.Converters
{
    // https://msdn.microsoft.com/en-us/library/bb613559(v=vs.110).aspx, except using dictionary for convenience.
    [ValueConversion(typeof(Color), typeof(LinearGradientBrush))]
    public class ColorToRowBrushConverter : IValueConverter
    {
        private static readonly Dictionary<Color, LinearGradientBrush> _colorRowBrushes = new Dictionary<Color, LinearGradientBrush>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            if (_colorRowBrushes.TryGetValue(color, out LinearGradientBrush brush))
                return brush;

            brush = new LinearGradientBrush
            {
                StartPoint = new Point(1, 0),
                EndPoint = new Point(1, 1)
            };
            brush.GradientStops.Add(new GradientStop(Colors.Black, -1.25));
            brush.GradientStops.Add(new GradientStop(color, .5));
            brush.GradientStops.Add(new GradientStop(Colors.Black, 2.25));
            brush.Freeze(); // https://msdn.microsoft.com/en-us/library/bb613565.aspx#Anchor_2

            return _colorRowBrushes[color] = brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
