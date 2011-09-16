using System;
using System.Globalization;
using System.Windows.Data;
using TaskManager.DB_classes;

namespace TaskManager.Converter
{
    [ValueConversion(typeof(User), typeof(bool))]
    public class CurrUserToEnabledConverter : IValueConverter
    {
        public bool IsNew { get; set; }
        public bool IsDelete { get; set;}

        public CurrUserToEnabledConverter()
        {
            IsNew = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "...";

            return !IsDelete && (IsNew || ((User)value).GUID == Settings.CurrentUser.GUID);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
