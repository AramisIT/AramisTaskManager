using System.Collections.ObjectModel;
using TaskManager.DB_classes;

namespace TaskManager.Window
{
    public partial class ProjectCatalogWin
    {
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();

        public ProjectCatalogWin()
        {
            //todo: Изменить внешний вид!!
            InitializeComponent();
            projects = Project.GetProjects(true);
            PrjCatalog.ItemsSource = projects;
        }

        public void RefreshList()
        {
            projects = Project.GetProjects(true);
        }

        #region Операции с пелем
        private void usersCatalog_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            editUser();
        }

        private void addUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NewProjectWin npw = new NewProjectWin();
            npw.ShowDialog();
            
            if(npw.ResultBtn == HideWin.ResultButtons.Ok)
            {
                projects.Add(npw.NewProject);
            }
        }

        private void delUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (PrjCatalog.SelectedItem != null)
            {
                int index = PrjCatalog.SelectedIndex;

                Project prj = (Project)PrjCatalog.SelectedItem;
                prj.DeleteProject();

                projects[index] = new Project(prj){IsDelete = true};
            }
        }

        private void EditUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            editUser();
        }

        private void editUser()
        {
            if (PrjCatalog.SelectedItem != null)
            {
                int index = PrjCatalog.SelectedIndex;
                Project prj = (Project)PrjCatalog.SelectedItem;

                NewProjectWin npw = new NewProjectWin(prj);
                npw.ShowDialog();

                if (npw.ResultBtn == HideWin.ResultButtons.Ok)
                {
                    projects[index] = npw.NewProject;
                }
            }
        }
        #endregion
    }
}
