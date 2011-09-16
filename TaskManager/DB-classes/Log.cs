using System.Collections.Generic;
using TaskManager.SQLite;

namespace TaskManager.DB_classes
{
    /// <summary>Запись в лог</summary>
    public class LogRecord
    {
        public enum LogColor
        {
            Insert,
            Update,
            Send,
            UnKnown,
            Error
        }

        public string LogDateTime { get; set; }
        public string MainInfo { get; set; }
        public string AddInfo { get; set; }
        /// <summary>Цвет операции </summary>
        public LogColor Color { get; set; }

        public static List<LogRecord> GetLogRecords()
        {
            return SQLiteWorker.GetLogRecords();
        }
    }
}
