namespace TaskManager.Window
{
    public partial class MessageWin
    {
        public MessageWin(string topic, string msg, double height = -1)
        {
            InitializeComponent();
            Topic.Text = topic;
            Message.Text = msg;

            if (height != -1)
            {
                Height = height;
            }
        }

        private void CloseWin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }

    public static class MessageWindows
    {
        public static void Show(string topic, string msg, double height = -1)
        {
            MessageWin mw = new MessageWin(topic, msg, height);
            mw.ShowDialog();
        }
    }
}
