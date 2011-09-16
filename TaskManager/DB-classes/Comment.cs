using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TaskManager.SQLite;

namespace TaskManager.DB_classes
{
    public class Comment :  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region Fileds
        public string GUID { get; set; }
        public string TaskGUID { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
        private string data;
        public string Data
        {
            get { return data; }
            set { data = value;
                NotifyPropertyChanged("Data");
            }
        }
        public bool IsNew { get; set; }
        #endregion

        public Comment(bool generate = true)
        {
            if (generate)
            {
                GUID = SQLiteWorker.GenerateGUID();
                Date = DateTime.Now;
                User = new User(Settings.CurrentUser);
                IsNew = false;
            }
        }

        public Comment(Comment comment)
        {
            GUID = comment.GUID;
            TaskGUID = comment.TaskGUID;
            User = new User(comment.User);
            Date = new DateTime(comment.Date.Year, comment.Date.Month, comment.Date.Day, comment.Date.Hour,
                                comment.Date.Minute, comment.Date.Second, comment.Date.Millisecond);
            Data = comment.Data;
            IsNew = comment.IsNew;
        }

        public static List<Comment> GetCommentsForTask(string guid)
        {
            return SQLiteWorker.GetCommentForTask(guid);
        }

        public void SaveComment()
        {
            SQLiteWorker.AddNewComment(this);
        }

        public static ObservableCollection<Comment> UncheckIsNew(ObservableCollection<Comment> comments)
        {
            foreach (Comment comment in comments)
            {
                if(comment.IsNew)
                {
                    comment.IsNew = false;
                    SQLiteWorker.UncheckNewComment(comment.GUID);
                }
            }

            return comments;
        }
    }
}
