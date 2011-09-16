using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.DB_classes;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(LogRecord), typeof(SolidColorBrush))]
    public class LogRecordTypeToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogRecord.LogColor color = (LogRecord.LogColor)value;

            switch (color)
            {
                case LogRecord.LogColor.Insert:
                    return new SolidColorBrush(Colors.LightGreen);
                case LogRecord.LogColor.Update:
                    return new SolidColorBrush(Colors.LightBlue);
                case LogRecord.LogColor.Send:
                    return new SolidColorBrush(Colors.Yellow);
                case LogRecord.LogColor.Error:
                    return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Brushes.LightGray;
        }
    }
}
