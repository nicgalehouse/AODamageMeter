using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Anarchy_Online_Damage_Meter.View.Helpers
{
    public class RowWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)values[0] * (double)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}