using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using TaskManager.DB_classes;
using TaskManager.SQLite;

namespace TaskManager.Window
{
    public partial class BeginTasksWin
    {
        private ObservableCollection<Task> allMyTasks = new ObservableCollection<Task>();
        private ObservableCollection<Task> beginedTasks = new ObservableCollection<Task>();

        #region Initialization
        public BeginTasksWin()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            isClosed = true;
            
            beginedTasks = Task.GetMyBeginedTasks();
            beginedTastsGrid.ItemsSource = beginedTasks;

            allMyTasks = Task.GetTasks(new SQLiteWorker.QueryData
                                           {
                                               Where = "Performer=@GUID and Percents<>@Percents",
                                               Join = "",
                                               Parameters = new List<SQLiteParameter>
                                                                {
                                                                    new SQLiteParameter("GUID",
                                                                                        Settings.CurrentUser.GUID),
                                                                    new SQLiteParameter("Percents", 100)
                                                                }
                                           });
            foreach (Task task in beginedTasks)
            {
                int index = -1;
                foreach (Task allMyTask in allMyTasks)
                {
                    if (allMyTask.GUID == task.GUID)
                    {
                        index = allMyTasks.IndexOf(allMyTask);
                    }
                }

                if (index != -1)
                {
                    allMyTasks.RemoveAt(index);
                }
            }

            allMyTasksGrid.ItemsSource = allMyTasks;
        }
        #endregion

        #region Task navigation
        private void Add_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();

            foreach (Task t in allMyTasksGrid.SelectedItems)
            {
                beginedTasks.Add(new Task(t));
                indexes.Add(allMyTasksGrid.Items.IndexOf(t));
            }

            List<int> removedIndexes = new List<int>();
            foreach(int index in indexes)
            {
                int delay = removedIndexes.Count(i => i < index);

                removedIndexes.Add(index);
                allMyTasks.RemoveAt(index - delay);
            }
        }

        private void Remove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();

            foreach (Task t in beginedTastsGrid.SelectedItems)
            {
                allMyTasks.Add(new Task(t));
                indexes.Add(beginedTastsGrid.Items.IndexOf(t));
            }

            List<int> removedIndexes = new List<int>();
            foreach (int index in indexes)
            {
                int delay = removedIndexes.Count(i => i < index);

                removedIndexes.Add(index);
                beginedTasks.RemoveAt(index - delay);
            }
        }

        private void Apply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Task.SetBeginFlagForTasks(allMyTasks, beginedTasks);
            CloseWin();
            ResultBtn = ResultButtons.Ok;
        }
        #endregion
    }
}
