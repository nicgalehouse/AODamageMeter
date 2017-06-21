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

            switch (selectedViewingMode)
            {
                case ViewingMode.Fights: return "Fights";
                case ViewingMode.ViewingModes: return "Viewing Modes";
                case ViewingMode.DamageDone: return "Damage Done";
                case ViewingMode.DamageDoneInfo: return $"{selectedCharacter?.UncoloredName}'s Damage Done";
                case ViewingMode.DamageTaken: return "Damage Taken";
                case ViewingMode.DamageTakenInfo: return $"{selectedCharacter?.UncoloredName}'s Damage Taken";
                case ViewingMode.OwnersHealingDone: return $"{selectedCharacter?.UncoloredName}'s Healing Done";
                case ViewingMode.OwnersHealingTaken: return $"{selectedCharacter?.UncoloredName}'s Healing Taken";
                case ViewingMode.OwnersCasts: return $"{selectedCharacter?.UncoloredName}'s Casts";
                default: throw new NotImplementedException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
