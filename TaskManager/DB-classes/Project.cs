using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TaskManager.SQLite;

namespace TaskManager.DB_classes
{
    public class Project : INotifyPropertyChanged, IDataErrorInfo
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region IDataErrorInfo
        private readonly Dictionary<string, bool> notValidFields = new Dictionary<string, bool>();
        public bool IsValid { get { return notValidFields.Count == 0; } }
        public string Error { get { return null; } }

        public string this[string name]
        {
            get
            {
                switch (name)
                {
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
                    case "Leader":
                        if (Leader == null)
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

        #region Fields
        public string GUID { get; set; }
        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }
        private DateTime dateStart;
        public DateTime DateStart
        {
            get { return dateStart; }
            set
            {
                dateStart = value;
                NotifyPropertyChanged("DateStart");
            }
        }
        private DateTime dateEnd;
        public DateTime DateEnd
        {
            get { return dateEnd; }
            set
            {
                dateEnd = value;
                NotifyPropertyChanged("Data");
            }
        }
        public DateTime DateCreate { get; set; }
        private User customer;
        public User Customer
        {
            get { return customer; }
            set
            {
                customer = value;
                NotifyPropertyChanged("Customer");
            }
        }
        private User leader;
        public User Leader
        {
            get { return leader; }
            set
            {
                leader = value;
                NotifyPropertyChanged("Leader");
            }
        }
        private double percents;
        public double Percents
        {
            get { return percents; }
            set
            {
                percents = value;
                NotifyPropertyChanged("Percents");
            }
        }
        private Project parent;
        public Project Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                NotifyPropertyChanged("Parent");
            }
        }
        private int type;
        public int Type
        {
            get { return type; }
            set
            {
                type = value;
                NotifyPropertyChanged("Type");
            }
        }
        private bool isDelete;
        public bool IsDelete
        {
            get { return isDelete; }
            set
            {
                isDelete = value;
                NotifyPropertyChanged("IsDelete");
            }
        }
        #endregion

        #region Конструкторы
        public Project(bool generate = true)
        {
            if (generate)
            {
                GUID = SQLiteWorker.GenerateGUID();
                Customer = Settings.CurrentUser;
                DateCreate = DateTime.Now;
                DateStart = DateTime.Now;
                DateEnd = DateTime.Now;
            }
        }

        public Project(Project prj)
        {
            GUID = prj.GUID;
            Description = prj.Description;
            DateStart = prj.DateStart;
            DateEnd = prj.DateEnd;
            DateCreate = prj.DateCreate;
            Customer = new User(prj.Customer);
            Leader = new User(prj.Leader);
            Percents = prj.Percents;
            if(prj.Parent!=null)
            {
                Parent = new Project(prj.Parent);
            }
            Type = prj.Type;
            IsDelete = prj.IsDelete;
        }
        #endregion

        public static void AddProject(Project prj)
        {
            SQLiteWorker.AddNewProject(prj);
        }

        public void DeleteProject()
        {
            SQLiteWorker.DeleteProject(GUID);
        }

        public static ObservableCollection<Project> GetProjects(bool all = false)
        {
            return SQLiteWorker.GetProjectsInCollection(all);
        }
    }
}
