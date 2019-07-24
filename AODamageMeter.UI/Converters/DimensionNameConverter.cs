using System;
using System.Globalization;
using System.Windows.Data;

namespace AODamageMeter.UI.Converters
{
    [ValueConversion(typeof(Dimension), typeof(string))]
    public class DimensionNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((Dimension)value).GetName();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DimensionHelpers.GetDimensionOrDefault((string)value);
    }
}
