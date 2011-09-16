using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.DB_classes;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(Task), typeof(SolidColorBrush))]
    public class TaskToRowColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.White);

            Task task = value as Task;

            if (task == null)
                return new SolidColorBrush(Colors.White);

            if(task.IsDelete)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 131, 131));
            }

            if(task.IsNew)
            {
                return new SolidColorBrush(Colors.LightGreen);
            }

            return new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
