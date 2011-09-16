using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class ExistSelElementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            return (int)value != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
