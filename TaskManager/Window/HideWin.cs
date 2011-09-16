using System.Windows;

namespace TaskManager.Window
{
    public class HideWin : System.Windows.Window
    {
        public enum ResultButtons{None, Close, Ok, Cancel}
        public bool isClosed { private get; set; }
        public ResultButtons ResultBtn { get; protected set; }

        public HideWin()
        {
            Hide();
            Closing += HideWin_Closing;
            StateChanged += HideWin_StateChanged;
        }

        #region Приховати/Показати вікно
        public void CloseWin()
        {
            isClosed = true;
            Close();
        }

        public void ShowWin()
        {
            Show();
        }
        #endregion

        #region Приховування вікна
        void HideWin_StateChanged(object sender, System.EventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Hide();
            }
        }

        void HideWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();

            if(!isClosed)
            {
                e.Cancel = true;
            }
        }
        #endregion
    }
}
