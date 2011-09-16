using TaskManager.DB_classes;

namespace TaskManager.Window
{
    public partial class LogWin
    {
        public LogWin()
        {
            InitializeComponent();
            LogList.ItemsSource = LogRecord.GetLogRecords();
        }
    }
}
