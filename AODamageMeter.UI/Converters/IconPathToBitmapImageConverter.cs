using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AODamageMeter.UI.Converters
{
    // http://stackoverflow.com/questions/347614/wpf-image-resources, except using dictionary for convenience.
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class IconPathToBitmapImageConverter : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> _iconPathBitmapImages = new Dictionary<string, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string iconPath = (string)value;
            if (_iconPathBitmapImages.TryGetValue(iconPath, out BitmapImage image))
                return image;

            image = new BitmapImage(new Uri($"pack://application:,,,{iconPath}"));
            image.Freeze(); // https://msdn.microsoft.com/en-us/library/bb613565.aspx#Anchor_2

            return _iconPathBitmapImages[iconPath] = image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
