using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(object), typeof(double))]
    public class NotNullObjectToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? 0.5 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
