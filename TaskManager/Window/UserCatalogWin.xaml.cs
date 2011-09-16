using System.Collections.Generic;
using TaskManager.DB_classes;
using TaskManager.SQLite;

namespace TaskManager.Window
{
    public partial class UserCatalogWin
    {
        public UserCatalogWin()
        {
            InitializeComponent();
            List<User> users = User.GetUsers(true);
            usersCatalog.ItemsSource = users;
        }

        public void RefreshList()
        {
            List<User> users = User.GetUsers(true);
            usersCatalog.ItemsSource = users;
        }

        #region Операции с пелем
        private void usersCatalog_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            editUser();
        }

        private void addUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NewUserWin nuw = new NewUserWin();
            nuw.ShowDialog();
            RefreshList();
        }

        private void delUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (usersCatalog.SelectedItem != null)
            {
                User user = (User)usersCatalog.SelectedItem;

                SQLiteWorker.DeleteUserByGUID(user.GUID);

                RefreshList();
            }
        }

        private void EditUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            editUser();
        }

        private void editUser()
        {
            if (usersCatalog.SelectedItem != null)
            {
                User user = (User) usersCatalog.SelectedItem;

                NewUserWin nuw = new NewUserWin(user);
                nuw.ShowDialog();

                RefreshList();
            }
        }
        #endregion
    }
}
