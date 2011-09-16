using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TaskManager.SQLite;

namespace TaskManager.UsersControls
{
    public partial class ImportantButton
    {
        public ImportantButton()
        {
            InitializeComponent();
            Loaded += ImportantButton_Loaded;
        }

        void ImportantButton_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeCheckState();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            image.Source = new BitmapImage(new Uri(
                                               IsChecked
                                                   ? "/TaskManager;component/Images/draw-star.ico"
                                                   : "/TaskManager;component/Images/Star-full.ico",
                                               UriKind.Relative));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            image.Source = new BitmapImage(new Uri(
                                               IsChecked
                                                   ? "/TaskManager;component/Images/Fav-2.ico"
                                                   : "/TaskManager;component/Images/Star-empty.ico",
                                               UriKind.Relative));
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeCheckState(true);
        }

        private void ChangeCheckState(bool change = false)
        {
            if (change)
            {
                IsChecked = !IsChecked;

                if (UpdateFlagOfImportant)
                {
                    SQLiteWorker.SetFlagOfImportantForTaskByGUID(IsChecked, TaskGUID);
                }
            }

            if (IsChecked)
            {
                image.Source = new BitmapImage(new Uri(
                                                   "/TaskManager;component/Images/Fav-2.ico",
                                                   UriKind.Relative));
                ToolTip = "Снять пеметку важности";
            }
            else
            {
                image.Source = new BitmapImage(new Uri(
                                                   "/TaskManager;component/Images/Star-empty.ico",
                                                   UriKind.Relative));
                ToolTip = "Отметить задание как важное";
            }
        }

        [DefaultValue(false)]
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked", typeof (bool), typeof (ImportantButton));

        [Category("ImportantButtom Propertes")]
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set
            {
                SetValue(IsCheckedProperty, value);
                ChangeCheckState();
            }
        }

        [DefaultValue(null)]
        public static readonly DependencyProperty TaskGUIDProperty = DependencyProperty.Register(
            "TaskGUID", typeof(string), typeof(ImportantButton));

        [Category("ImportantButtom Propertes")]
        public string TaskGUID
        {
            get { return (string)GetValue(TaskGUIDProperty); }
            set
            {
                SetValue(TaskGUIDProperty, value);
            }
        }

        [DefaultValue(false)]
        public static readonly DependencyProperty UpdateFlagOfImportantProperty = DependencyProperty.Register(
            "UpdateFlagOfImportant", typeof(bool), typeof(ImportantButton));

        [Category("ImportantButtom Propertes")]
        public bool UpdateFlagOfImportant
        {
            get { return (bool)GetValue(UpdateFlagOfImportantProperty); }
            set
            {
                SetValue(UpdateFlagOfImportantProperty, value);
            }
        }
    }
}
