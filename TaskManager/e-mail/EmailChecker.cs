using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows.Threading;
using TaskManager.SQLite;
using TaskManager.Window;
using Timer = System.Timers.Timer;

namespace TaskManager.e_mail
{
    public class EmailChecker
    {
        public EmailChecker(double period = 500)
        {
            Timer timer = new Timer(period) {Enabled = true};
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckMail();
        }

        public static void CheckMail()
        {
            string resultTopic = string.Empty;
            string resultMsg = string.Empty;

            try
            {
                List<MailParts> querys = Messanger.CheckInbox(ref resultTopic, ref resultMsg);

                if (querys != null)
                {
                    foreach (MailParts query in querys)
                    {
                        SQLiteWorker.ExecuteNonQuery(query.Command, query.Parameter, false, true);
                    }
                }
            }
            catch (Exception)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new ThreadStart(() => MessageWindows.Show(
                        "Не удалось проверить почту",
                        "Возможно не правильный логин либо пароль")));

            }
        }
    }
}
