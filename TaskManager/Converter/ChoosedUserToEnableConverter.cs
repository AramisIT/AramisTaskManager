using System;
using System.Globalization;
using System.Windows.Data;
using TaskManager.DB_classes;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(User), typeof(bool))]
    public class ChoosedUserToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
