using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.DB_classes;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(User), typeof(SolidColorBrush))]
    public class ProjectToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.White);

            Project prj = value as Project;

            if (prj == null)
                return new SolidColorBrush(Colors.White);

            if (prj.IsDelete)
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
