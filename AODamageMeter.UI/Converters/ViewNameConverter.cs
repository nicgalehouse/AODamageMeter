using System;
using System.Globalization;
using System.Windows.Data;

namespace AODamageMeter.UI.Converters
{
    public class ViewNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedViewingMode = (ViewingMode)values[0];
            var selectedCharacter = (Character)values[1];

            return selectedViewingMode == ViewingMode.ViewingModes ? "Viewing Modes"
                : selectedViewingMode == ViewingMode.DamageDone ? "Damage Done"
                : selectedViewingMode == ViewingMode.DamageDoneInfo ? $"{selectedCharacter?.UncoloredName}'s Damage Done"
                : selectedViewingMode == ViewingMode.DamageTaken ? "Damage Taken"
                : selectedViewingMode == ViewingMode.DamageTakenInfo ? $"{selectedCharacter?.UncoloredName}'s Damage Taken"
                : throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
