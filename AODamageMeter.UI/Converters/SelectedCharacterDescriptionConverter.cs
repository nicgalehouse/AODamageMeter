using System;
using System.Globalization;
using System.Windows.Data;

namespace AODamageMeter.UI.Converters
{
    public class SelectedCharacterDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string characterName = (string)value;

            return string.IsNullOrWhiteSpace(characterName) ? "No character selected" : $"{characterName} selected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
