using System.Windows;
using TaskManager.DB_classes;
using TaskManager.SQLite;

namespace TaskManager.Window
{
    public partial class NewUserWin
    {
        private User NewUser { get; set; }
        private readonly bool isUpdateble;

        public NewUserWin()
        {
            NewUser = new User();
            InitializeComponent();
            init();
        }

        public NewUserWin(User user)
        {
            NewUser = new User(user);
            isUpdateble = true;
            InitializeComponent();
            init();
        }

        private void init()
        {
            isClosed = true;
            DataContext = NewUser;
        }

        #region Result
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if(NewUser.IsValid)
            {
                ResultBtn = ResultButtons.Ok;
                CloseWin();

                if(isUpdateble)
                {
                    SQLiteWorker.UpdateUser(NewUser);
                }
                else
                {
                    SQLiteWorker.AddNewUser(NewUser);
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
