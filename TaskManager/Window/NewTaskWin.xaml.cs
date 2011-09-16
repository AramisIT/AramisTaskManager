using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManager.Converter;
using TaskManager.DB_classes;
using TaskManager.SQLite;

namespace TaskManager.Window
{
    public partial class NewTaskWin
    {
        private List<User> users = new List<User>();
        private readonly ObservableCollection<Comment> taskComments = new ObservableCollection<Comment>();
        private readonly Task OldTask;
        public Task NewTask { get; private set; }
        private bool IsUpdateble { get; set; }
        public bool IsChanged { get; private set; }

        public NewTaskWin()
        {
            NewTask = new Task();
            
            InitializeComponent();
            init();
        }

        public NewTaskWin(Task task)
        {
            OldTask = task;
            NewTask = new Task(task);
            IsUpdateble = true;
            taskComments = Comment.UncheckIsNew(NewTask.GetCommentsInCollection());
            
            InitializeComponent();
            init();

            if(task.TimePlan > 0)
            {
                TimePlan.IsEnabled = false;
            }
        }

        private void init()
        {
            isClosed = true;
            DataContext = NewTask;
            users = User.GetUsers();
            Performer.ItemsSource = users;
            Performer.Text = NewTask.Performer!=null ? NewTask.Performer.Name : null;
            comments.ItemsSource = taskComments;

            taskProject.ItemsSource = Project.GetProjects();
            if(NewTask.Project!=null)
            {
                taskProject.Text = NewTask.Project.Description;
            }

            CurrUserToEnabledConverter enabledConverter = TryFindResource("currUserToEnabledConverter") as CurrUserToEnabledConverter;
            enabledConverter.IsNew = NewTask.Performer == null || 
                NewTask.Performer.GUID == Settings.CurrentUser.GUID;
            enabledConverter.IsDelete = NewTask.IsDelete;
        }

        #region Expand/Collapse
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            expandArea(true);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            expandArea(false);
        }

        private void expandArea(bool isExpand)
        {
            int rowIndex = Grid.GetRow(mainArea);

            if (rowIndex + 2 > mainArea.RowDefinitions.Count) return;

            var lastRow = mainArea.RowDefinitions[rowIndex + 2];

            if(isExpand)
            {
                lastRow.Height = new GridLength(390);
                Height += 300;

                if(Top+Height>Settings.MainScreenHeight)
                {
                    Top -= Top + Height - Settings.MainScreenHeight;

                    if(Top<0)
                    {
                        Top = 0;
                        Height = Settings.MainScreenHeight;
                    }
                }
            }
            else
            {
                lastRow.Height = new GridLength(90);
                Height -= 300;
            }
        }
        #endregion

        #region Result
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (NewTask.IsValid)
            {
                ResultBtn = ResultButtons.Ok;
                CloseWin();
                IsChanged = !Task.Equal(NewTask, OldTask);

                if (IsUpdateble)
                {
                    if (IsChanged)
                    {
                        SQLiteWorker.UpdateTask(NewTask);
                    }
                }
                else
                {
                    SQLiteWorker.AddTask(NewTask);
                }

                SQLiteWorker.SetFlagOfImportantForTaskByGUID(NewTask.IsImportant, NewTask.GUID);

                foreach (Comment comment in taskComments)
                {
                    if(!string.IsNullOrWhiteSpace(comment.Data) && comment.IsNew)
                    {
                        comment.IsNew = false;
                        comment.SaveComment();
                    }
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ResultBtn = ResultButtons.Cancel;
            CloseWin();
        }
        #endregion

        #region Робота з коментарями
        private void addNewComment_Click(object sender, RoutedEventArgs e)
        {
            taskComments.Add(new Comment
                                 {
                                     TaskGUID = NewTask.GUID,
                                     IsNew = true
                                 });
        }
        #endregion

        private void TimeAdjustment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TimeAdjustment.Text))
            {
                TimeAdjustment.Text = "0";
            }
            else
            {
                double newHours;
                double.TryParse(TimeAdjustment.Text.Replace('.', ','), out newHours);
                double oldHours;
                double.TryParse(TimePlan.Text.Replace('.', ','), out oldHours);

                TimeAdjustment.Background = new SolidColorBrush(newHours > oldHours ? Colors.OrangeRed : Colors.YellowGreen);
            }
        }
    }
}