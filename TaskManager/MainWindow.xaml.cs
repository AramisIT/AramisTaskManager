using System;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows.Input;
using TaskManager.DB_classes;
using TaskManager.e_mail;
using TaskManager.SQLite;
using TaskManager.Window;

namespace TaskManager
{
    public partial class MainWindow
    {
        private SQLiteWorker.QueryData lastData;
        ObservableCollection<Task> tasks = new ObservableCollection<Task>();

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //e-mail checker
                new EmailChecker(5000);

                isClosed = true;
                ShowWin();
                notify.TrayMouseDoubleClick += notify_TrayMouseDoubleClick;
                MyCommand.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));

                //Current Settings
                Settings.CurrentUser = User.GetCurrentUser();
                Settings.StartupDirectory = Convert.ToInt32(SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.StartupDirectory));
                ChangeDirectory((DirectoryNumber) Settings.StartupDirectory);
                Settings.MainScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                Settings.SMTPserver = SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.SmtpServer);
                Settings.SMTPport = Convert.ToInt32(SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.SmtpPort));
                Settings.POPserver = SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.PopServer);
                Settings.POPport = Convert.ToInt32(SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.PopPort));
                Settings.EmailUser = SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.EmailUser);
                Settings.EmailPass = SQLiteWorker.GetSettingProperty(
                    SQLiteWorker.SettingProperty.EmailPass);

                DataContext = Settings.CurrentUser;
            }
            catch(Exception exc)
            {
                MessageWindows.Show("MainWindow::Exception", exc.Message);
            }
        }

        void notify_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowWin();
        }

        private void HideWin_Closed(object sender, EventArgs e)
        {
            notify.Dispose();
        }

        #region Изменение директории
        private enum DirectoryNumber
        {
            MyImportant = 0, MyToPerform, MyNotCompled, MyAll, PutImportant=5, PutNotCompled, PutAll, All=9
        }

        private void ChangeDirectory(DirectoryNumber dir)
        {
            switch (dir)
            {
                case DirectoryNumber.MyImportant:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Performer=@GUID",
                                       Join = "ImportantTasks it on it.TaskGUID=GUID",
                                       Parameters =
                                           new List<SQLiteParameter>
                                               {new SQLiteParameter("GUID", Settings.CurrentUser.GUID)}
                                   };
                    getTasks(lastData, "Мои важные задания");
                    break;
                case DirectoryNumber.MyToPerform:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Performer=@GUID",
                                       Join = "ToPerformTasks pt on pt.TaskGUID=GUID",
                                       Parameters =
                                           new List<SQLiteParameter> { new SQLiteParameter("GUID", Settings.CurrentUser.GUID) }
                                   };
                    getTasks(lastData, "Мои задания к выполнению");
                    break;
                case DirectoryNumber.MyNotCompled:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Performer=@GUID and Percents<>@Percents",
                                       Join = "",
                                       Parameters = new List<SQLiteParameter>
                                                        {
                                                            new SQLiteParameter("GUID", Settings.CurrentUser.GUID),
                                                            new SQLiteParameter("Percents", 100)
                                                        }
                                   };
                    getTasks(lastData, "Мои незавершенные задания");
                    break;
                case DirectoryNumber.MyAll:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Performer=@GUID",
                                       Parameters =
                                           new List<SQLiteParameter>
                                               {new SQLiteParameter("GUID", Settings.CurrentUser.GUID)}
                                   };
                    getTasks(lastData, "Все мои задания");
                    break;
                case DirectoryNumber.PutImportant:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Customer=@GUID",
                                       Join = "ImportantTasks it on it.TaskGUID=GUID",
                                       Parameters =
                                           new List<SQLiteParameter>
                                               {new SQLiteParameter("GUID", Settings.CurrentUser.GUID)}
                                   };
                    getTasks(lastData, "Поставленные важные задания");
                    break;
                case DirectoryNumber.PutNotCompled:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Customer=@GUID and Percents<>@Percents",
                                       Join = "",
                                       Parameters = new List<SQLiteParameter>
                                                        {
                                                            new SQLiteParameter("GUID", Settings.CurrentUser.GUID),
                                                            new SQLiteParameter("Percents", 100)
                                                        }
                                   };
                    getTasks(lastData, "Поставленные незавершенные задания");
                    break;
                case DirectoryNumber.PutAll:
                    lastData = new SQLiteWorker.QueryData
                                   {
                                       Where = "Customer=@GUID",
                                       Join = "",
                                       Parameters =
                                           new List<SQLiteParameter>
                                               {new SQLiteParameter("GUID", Settings.CurrentUser.GUID)}
                                   };
                    getTasks(lastData, "Все поставленные задания");
                    break;
                case DirectoryNumber.All:
                    lastData = new SQLiteWorker.QueryData();
                    getTasks(lastData, "Все поставленные задания", true);
                    break;
            }
        }

        #endregion

        #region Контекстное меню
        private void getTasks(SQLiteWorker.QueryData query, string caption = "", bool all = false)
        {
            tasks = Task.GetTasks(query, all);
            tasksGrid.ItemsSource = tasks;
            
            if(!string.IsNullOrEmpty(caption))
            {
                Topic.Content = caption;
            }
        }

        #region My
        private void MyImportant_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.MyImportant);
        }

        private void MyToPerform_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.MyToPerform);
        }

        private void MyPutNotCompled_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.MyNotCompled);
        }

        private void MyAll_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.MyAll);
        }

        #endregion

        #region Put
        private void PutImportant_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.PutImportant);
        }

        private void PutNotCompleted_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.PutNotCompled);
        }

        private void PutAll_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.PutAll);
        }
        #endregion 

        #region Else
        private void NewTask_Click(object sender, RoutedEventArgs e)
        {
            addNewTaskClick();
        }

        private void All_Click(object sender, RoutedEventArgs e)
        {
            ChangeDirectory(DirectoryNumber.All);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            CloseWin();
        }
        #endregion
        #endregion

        #region Операции над задачами
        private void addNewTaskClick()
        {
            NewTaskWin ntw = new NewTaskWin();
            ntw.ShowDialog();

            if (ntw.ResultBtn == ResultButtons.Ok)
            {
                tasks.Add(ntw.NewTask);
            }
        }

        private void addNewTask_Click_1(object sender, RoutedEventArgs e)
        {
            addNewTaskClick();
        }

        private void tasksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Task task = (Task)tasksGrid.SelectedItem;

            if (task != null)
            {
                int index = tasksGrid.SelectedIndex;

                if(task.IsNew)
                {
                    task.UnflagedNewTask();
                    tasks[index] = new Task(task);
                }

                NewTaskWin ntw = new NewTaskWin(task);
                ntw.ShowDialog();
                
                if(ntw.ResultBtn == ResultButtons.Ok)
                {
                    if (ntw.NewTask.IsImportant != tasks[index].IsImportant)
                    {
                        RefreshList();
                    }
                    else
                    {
                        if(ntw.IsChanged)
                        {
                            tasks[index] = new Task(ntw.NewTask);
                        }
                    }
                }
            }
        }

        private void deleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (tasksGrid.SelectedItem != null)
            {
                Task t = (Task)tasksGrid.SelectedItem;
                
                SQLiteWorker.DeleteTask(t.GUID, t.IsImportant);

                int index = tasksGrid.SelectedIndex;
                tasks[index] = new Task(t){IsDelete = true};
            }
            else
            {
                MessageWindows.Show("Выберите одну из работ", "Для удаления нужно выбрать хотя бы одну работу");
            }
        }

        //private void refreshTask_Click(object sender, RoutedEventArgs e)
        //{
        //    RefreshList();
        //}

        private void RefreshList()
        {
            int index = tasksGrid.SelectedIndex;
            getTasks(lastData);

            if (tasksGrid.Items.Count > index && index != -1)
            {
                tasksGrid.SelectedIndex = index;
            }
        }
        #endregion

        #region Изменение настроек
        private void SetSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingWin sw = new SettingWin();
            sw.ShowDialog();

            if (sw.ResultBtn == ResultButtons.Ok)
            {
                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.CurrentUser, sw.CurrentUser);
                
                if(sw.StartupDirectoryIsChanged)
                {
                    SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.StartupDirectory, sw.StartupDir);
                }

                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.SmtpServer, sw.SmtpServer);
                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.SmtpPort, sw.SmtpPort);
                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.PopServer, sw.PopServer);
                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.PopPort, sw.PopPort);
                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.EmailUser, sw.EmailUser);
                SQLiteWorker.UpdateSettingProperty(SQLiteWorker.SettingProperty.EmailPass, sw.EmailPass);

                if (lastData.Parameters != null)
                {
                    //RefreshList изменение GUID'a
                    int index = -1;

                    foreach (SQLiteParameter p in lastData.Parameters)
                    {
                        if (p.ParameterName == "GUID")
                        {
                            User user = DataContext as User;

                            if (user != null && p.Value.Equals(user.GUID))
                            {
                                index = lastData.Parameters.IndexOf(p);
                            }
                        }
                    }

                    if(index!=-1)
                    {
                        lastData.Parameters[index].Value = Settings.CurrentUser.GUID;
                    }
                }

                DataContext = Settings.CurrentUser;
                RefreshList();
            }
        }
        #endregion

        #region Справочники
        private void OpenUserCatalog_Click(object sender, RoutedEventArgs e)
        {
            UserCatalogWin ucw = new UserCatalogWin();
            ucw.ShowDialog();
        }

        private void openProjectCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.CurrentUser != null)
            {
                ProjectCatalogWin pcw = new ProjectCatalogWin();
                pcw.ShowDialog();
            }
            else
            {
                MessageWindows.Show("Пользователь не выбран", "Для работы сначала выберите пользователя");
            }
        }

        private void OpenBeginedWorks_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.CurrentUser != null)
            {
                BeginTasksWin btw = new BeginTasksWin();
                btw.ShowDialog();

                if (btw.ResultBtn == ResultButtons.Ok)
                {
                    RefreshList();
                }
            }
            else
            {
                MessageWindows.Show("Пользователь не выбран", "Для работы сначала выберите пользователя");
            }
        }

        private void openLogWin_Click(object sender, RoutedEventArgs e)
        {
            LogWin lw = new LogWin();
            lw.ShowDialog();
        }
        #endregion

        #region Filter&Search
        #region Show/Hide
        public static RoutedCommand MyCommand = new RoutedCommand();

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            showHideSearchPanel();
        }

        private void ShowSearchPanel_Unchecked(object sender, RoutedEventArgs e)
        {
            showHideSearchPanel();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowSearchPanel.IsChecked = !ShowSearchPanel.IsChecked;
        }

        private void showHideSearchPanel()
        {
            if (filterPanel.Visibility == Visibility.Visible)
            {
                tasksGrid.Margin = new Thickness(
                    tasksGrid.Margin.Left,
                    tasksGrid.Margin.Top,
                    tasksGrid.Margin.Right,
                    tasksGrid.Margin.Bottom - 25);
                filterPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                tasksGrid.Margin = new Thickness(
                    tasksGrid.Margin.Left,
                    tasksGrid.Margin.Top,
                    tasksGrid.Margin.Right,
                    tasksGrid.Margin.Bottom + 25);
                filterPanel.Visibility = Visibility.Visible;
            }

            if (filterPanel.Visibility == Visibility.Visible)
            {
                BuildFilterQuery();
            }
            else
            {
                RefreshList();
            }
        }
        #endregion

        #region Search and show result
        private void filterPanel_KeyDown_2(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildFilterQuery();
            }
        }

        private void DateEndFilter_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BuildFilterQuery();
        }

        private void DateEndFilter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DateEndFilter.SelectedDate = null;
            BuildFilterQuery();
        }

        private void ImportantFilter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BuildFilterQuery();
        }

        private void BuildFilterQuery()
        {
            StringBuilder where = new StringBuilder();
            SQLiteWorker.QueryData qd = new SQLiteWorker.QueryData
            {
                Where = lastData.Where,
                Join = lastData.Join,
                Parameters = lastData.Parameters ?? new List<SQLiteParameter>()
            };

            if (!string.IsNullOrWhiteSpace(TopicFilter.Text))
            {
                where.AppendFormat(" and Topic like @Topic\r\n");
                qd.Parameters.Add(new SQLiteParameter("Topic", "%" + TopicFilter.Text + "%"));
            }

            if (DateEndFilter.SelectedDate != null)
            {
                where.AppendFormat("\tand EndDate=@EndDate\r\n");
                qd.Parameters.Add(new SQLiteParameter(
                    "EndDate", 
                    SQLiteWorker.DateTimeToSQLiteFormat(DateEndFilter.SelectedDate.Value, false)));
            }

            if (!string.IsNullOrWhiteSpace(CustomerFilter.Text))
            {
                qd.Join = "Users c on c.GUID=Customer and (c.Name like @Customer or c.Name=@Customer)";
                qd.Parameters.Add(new SQLiteParameter("Customer", "%" + CustomerFilter.Text + "%"));
            }

            if (!string.IsNullOrWhiteSpace(PerformerFilter.Text))
            {
                qd.Join += string.Concat(
                    string.IsNullOrWhiteSpace(qd.Join)
                        ? ""
                        : "join ",
                    "Users p on p.GUID=Performer and (p.Name like @Performer or p.Name=@Performer)");

                qd.Parameters.Add(new SQLiteParameter("Performer", "%" + PerformerFilter.Text + "%"));
            }

            if (!string.IsNullOrWhiteSpace(PercentsFilter.Text))
            {
                where.Append("\tand Percents=@Percents\r\n");
                qd.Parameters.Add(new SQLiteParameter("Percents", PercentsFilter.Text));
            }

            if(ImportantFilter.IsChecked)
            {
                qd.Join += string.Concat(
                    string.IsNullOrWhiteSpace(qd.Join)
                        ? ""
                        : " join ",
                    "ImportantTasks i on i.TaskGUID=Tasks.GUID");
            }

            if (!string.IsNullOrWhiteSpace(where.ToString()))
            {
                qd.Where += string.IsNullOrWhiteSpace(qd.Where)
                                ? where.ToString().Remove(0, 4)
                                : where.ToString();
                getTasks(qd);
            }
            else if (!string.IsNullOrWhiteSpace(qd.Join))
            {
                getTasks(qd);
            }
            else
            {
                RefreshList();
            }
        }
        #endregion
        #endregion

        private void CheckEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailChecker.CheckMail();
            RefreshList();
        }
    }
}
