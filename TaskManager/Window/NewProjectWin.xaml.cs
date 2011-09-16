using System.Collections.Generic;
using System.Windows;
using TaskManager.DB_classes;
using TaskManager.SQLite;

namespace TaskManager.Window
{
    public partial class NewProjectWin
    {
        public Project NewProject { get; set; }
        private List<User> users = new List<User>();
        private readonly bool isUpdateble;

        public NewProjectWin()
        {
            NewProject = new Project();
            InitializeComponent();
            init();
        }

        public NewProjectWin(Project prj)
        {
            NewProject = new Project(prj);
            isUpdateble = true;
            InitializeComponent();
            init();
        }

        private void init()
        {
            users = User.GetUsers();
            
            Leader.ItemsSource = users;
            if (NewProject.Leader != null)
            {
                Leader.Text = NewProject.Leader.Name;
            }

            isClosed = true;
            DataContext = NewProject;
        }

        #region Result
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if(NewProject.IsValid)
            {
                ResultBtn = ResultButtons.Ok;
                CloseWin();

                if(isUpdateble)
                {
                    SQLiteWorker.UpdateProject(NewProject);
                }
                else
                {
                    SQLiteWorker.AddNewProject(NewProject);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResultBtn = ResultButtons.Cancel;
            CloseWin();
        }
        #endregion
    }
}
