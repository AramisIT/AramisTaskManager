using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TaskManager.DB_classes;

namespace TaskManager.Window
{
    public partial class SettingWin
    {
        private List<User> users = new List<User>();
        public string CurrentUser 
        { 
            get 
            {
            return CurrUser.SelectedValue != null 
                ? ((User)CurrUser.SelectedValue).GUID 
                : null;
            } 
        }
        public string StartupDir { get { return StartupDirectory.SelectedIndex.ToString(); } }
        public string SmtpServer { get { return smtpServer.Text; } }
        public string SmtpPort { get { return smtpPort.Text; } }
        public string PopServer { get { return popServer.Text; } }
        public string PopPort { get { return popPort.Text; } }
        public string EmailUser { get { return emailUser.Text; } }
        public string EmailPass { get { return emailPass.Password; } }

        public bool StartupDirectoryIsChanged;

        public SettingWin()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            isClosed = true;
            //User
            users = User.GetUsers();
            CurrUser.ItemsSource = users;
            if (Settings.CurrentUser != null)
            {
                CurrUser.SelectedValue = Settings.CurrentUser;
                CurrUser.Text = Settings.CurrentUser.Name;
            }
            //Startup directory
            StartupDirectory.SelectedIndex = Settings.StartupDirectory;

            if (StartupDirectory.Items != null && StartupDirectory.Items[StartupDirectory.SelectedIndex] != null)
            {
                ComboBoxItem item = StartupDirectory.Items[StartupDirectory.SelectedIndex] as ComboBoxItem;
                StartupDirectory.Text = item.Content.ToString();
            }
            //email settings
            smtpServer.Text = Settings.SMTPserver;
            smtpPort.Text = Settings.SMTPport.ToString();
            popServer.Text = Settings.POPserver;
            popPort.Text = Settings.POPport.ToString();
            emailUser.Text = Settings.EmailUser;
            emailPass.Password = Settings.EmailPass;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ResultBtn = ResultButtons.Ok;

            Settings.CurrentUser = (User) CurrUser.SelectedValue;
            StartupDirectoryIsChanged = Settings.StartupDirectory != StartupDirectory.SelectedIndex;
            Settings.StartupDirectory = StartupDirectory.SelectedIndex;
            Settings.SMTPserver = SmtpServer;
            Settings.SMTPport = Convert.ToInt32(SmtpPort);
            Settings.POPserver = PopServer;
            Settings.POPport = Convert.ToInt32(PopPort);
            Settings.EmailUser = EmailUser;
            Settings.EmailPass = EmailPass;

            CloseWin();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResultBtn = ResultButtons.Cancel;
            CloseWin();
        }
    }
}
