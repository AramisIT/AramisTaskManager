using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using TaskManager.SQLite;
using System.Collections.Generic;

namespace TaskManager.DB_classes
{
    public class Task : IDataErrorInfo
    {
        #region Fields
        public string GUID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime EndDate { get; set; }
        public User Performer { get; set; }
        public User Customer { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public byte Priority { get; set; }
        public Project Project { get; set; }
        public double TimePlan { get; set; }
        public double TimeAdjustment { get; set; }
        public double TimeFact { get; set; }
        public double Percents { get; set; }
        public bool IsImportant { get; set; }
        public bool IsNew { get; set; }
        public bool IsDelete { get; set; }
        #endregion

        #region Constructors
        public Task(bool generate=true)
        {
            if (generate)
            {
                GUID = SQLiteWorker.GenerateGUID();
                OrderDate = DateTime.Now;
                EndDate = DateTime.Now;
                IsNew = false;
                IsDelete = false;
            }
        }

        public Task(Task task)
        {
            GUID = task.GUID;
            OrderDate = task.OrderDate;
            EndDate = task.EndDate;
            Performer = new User(task.Performer);
            Customer = new User(task.Customer);
            Topic = task.Topic;
            Description = task.Description;
            Priority = task.Priority;
            Project = new Project(task.Project);
            TimePlan = task.TimePlan;
            TimeAdjustment = task.TimeAdjustment;
            TimeFact = task.TimeFact;
            Percents = task.Percents;
            IsImportant = task.IsImportant;
            IsNew = task.IsNew;
            IsDelete = task.IsDelete;
        }
        #endregion

        #region Работа с базой
        public static ObservableCollection<Task> GetTasks(SQLiteWorker.QueryData query, bool all = false)
        {
            return SQLiteWorker.GetTasksInCollection(query, all);
        }

        public static ObservableCollection<Task> GetAllMyTasks()
        {
            return GetTasks(new SQLiteWorker.QueryData
                                {
                                    Where = "Performer=@GUID",
                                    Parameters = new List<SQLiteParameter>
                                                     {
                                                         new SQLiteParameter("GUID", Settings.CurrentUser.GUID)
                                                     }
                                });
        }

        public static ObservableCollection<Task> GetMyTasks()
        {
            return GetTasks(new SQLiteWorker.QueryData
                                {
                                    Where = "Performer=@GUID",
                                    Parameters = new List<SQLiteParameter>
                                                     {
                                                         new SQLiteParameter("GUID", Settings.CurrentUser.GUID)
                                                     }
                                });
        }

        public static ObservableCollection<Task> GetMyBeginedTasks()
        {
            return SQLiteWorker.GetTasksInCollection(
                new SQLiteWorker.QueryData
                {
                    Join = "ToPerformTasks on TaskGUID=GUID"
                });
        }

        public List<Comment> GetComments()
        {
            return Comment.GetCommentsForTask(GUID);
        }

        public ObservableCollection<Comment> GetCommentsInCollection()
        {
            return SQLiteWorker.GetCommentInCollectionForTask(GUID);
        }

        public static void SetBeginFlagForTasks(IEnumerable<Task> notBeginedTasks, IEnumerable<Task> beginedTasks)
        {
            ObservableCollection<Task> toPerform = GetMyBeginedTasks();

            foreach (Task task in beginedTasks)
            {
                if (!ContainsTask(toPerform, task))
                {
                    SetBeginFlagForTask(task, true);
                }
            }

            foreach (Task task in notBeginedTasks)
            {
                if (ContainsTask(toPerform, task))
                {
                    SetBeginFlagForTask(task, false);
                }
            }
        }

        public static void SetBeginFlagForTask(Task task, bool add)
        {
            SQLiteWorker.SetBeginFlagForTask(task.GUID, add);
        }

        public void UnflagedNewTask()
        {
            IsNew = false;
            SQLiteWorker.UnflagedNewTask(GUID);
        }
        #endregion

        #region Операции
        public static bool Equal(Task task1, Task task2)
        {
            if (task2 == null)
                return false;

            return task1.OrderDate == task2.OrderDate &&
                   task1.EndDate == task2.EndDate &&
                   task1.Performer.GUID == task2.Performer.GUID &&
                   task1.Topic == task2.Topic &&
                   task1.Description == task2.Description &&
                   task1.Priority == task2.Priority &&
                   task1.TimePlan == task2.TimePlan &&
                   task1.TimeAdjustment == task2.TimeAdjustment &&
                   task1.TimeFact == task2.TimeFact &&
                   task1.Percents == task2.Percents &&
                   task1.IsImportant == task2.IsImportant
                   ;
        }

        public static bool ContainsTask(ObservableCollection<Task> tasks, Task task)
        {
            return tasks.Any(t => t.GUID == task.GUID);
        }
        #endregion

        #region validation
        private readonly Dictionary<string, bool> notValidFields = new Dictionary<string, bool>();
        public bool IsValid { get { return notValidFields.Count == 0; } }
        public string Error { get { return null; } }

        public string this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Topic":
                        if (string.IsNullOrWhiteSpace(Topic))
                        {
                            if (!notValidFields.ContainsKey(name))
                            {
                                notValidFields.Add(name, true);
                            }

                            return "Empty field!";
                        }
                        if (notValidFields.ContainsKey(name))
                        {
                            notValidFields.Remove(name);
                        }

                        break;
                    case "Description":
                        if (string.IsNullOrWhiteSpace(Description))
                        {
                            if (!notValidFields.ContainsKey(name))
                            {
                                notValidFields.Add(name, true);
                            }

                            return "Empty field!";
                        }
                        if (notValidFields.ContainsKey(name))
                        {
                            notValidFields.Remove(name);
                        }

                        break;
                    case "Performer":
                        if (Performer == null)
                        {
                            if (!notValidFields.ContainsKey(name))
                            {
                                notValidFields.Add(name, true);
                            }

                            return "Empty field!";
                        }
                        if (notValidFields.ContainsKey(name))
                        {
                            notValidFields.Remove(name);
                        }

                        break;
                    case "Project":
                        if (Project==null)
                        {
                            if (!notValidFields.ContainsKey(name))
                            {
                                notValidFields.Add(name, true);
                            }

                            return "Empty field!";
                        }
                        if (notValidFields.ContainsKey(name))
                        {
                            notValidFields.Remove(name);
                        }

                        break;
                }

                return null;
            }
        }
        #endregion
    }
}
