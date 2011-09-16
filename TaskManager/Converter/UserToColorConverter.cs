using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.DB_classes;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(User), typeof(SolidColorBrush))]
    public class UserToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.White);

            User user = value as User;

            if (user == null)
                return new SolidColorBrush(Colors.White);

            if (user.IsDelete)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 131, 131));
            }

            return new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
