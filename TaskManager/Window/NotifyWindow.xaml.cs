using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace TaskManager.Window
{
    /// <summary>Окно уведомления</summary>
    public partial class NotifyWindow
    {
        /// <summary>Отступ от границ экрана</summary>
        private const int screenPadding = 15;
        /// <summary>Таймер автоматического закрывания</summary>
        private readonly System.Timers.Timer closeWinTimer;
        /// <summary>Нужно ли автоматически закрывать окно</summary>
        private readonly bool autoClose;
        /// <summary>Окно закрыто</summary>
        public bool IsClosed { get; private set; }
        /// <summary>Ожидаеться ли ответ</summary>
        public bool IsWaitAnswer { get { return !IsClosed && autoClose; } }
        /// <summary>Нажатие на кнопку</summary>
        public event EventHandler<ClickButtonEventArgs> ClickButton;
        /// <summary>Результат диалога</summary>
        public NotifyWinButton Result { get; private set;}

        /// <summary>Окно уведомления</summary>
        /// <param name="height">Высота окна</param>
        /// <param name="topic">Заголовок</param>
        /// <param name="message">Сообщение</param>
        /// <param name="buttons">Набор кнопок (макс. 3)</param>
        /// <param name="close">Параметр автозакрытия</param>
        public NotifyWindow(double height, string topic, string message, IEnumerable<NotifyWinButton> buttons, bool close)
        {
            InitializeComponent();
            Height = height;
            Result = NotifyWinButton.None;
            Topic.Text = topic;
            Msg.Text = message;
            addButtons(buttons);
            autoClose = close;
            Closed += NotifyWindow_Closed;
            
            Screen sc = Screen.PrimaryScreen;
            Left = sc.WorkingArea.Left + screenPadding;
            Top = sc.WorkingArea.Bottom - Height - screenPadding;

            if (autoClose)
            {
                closeWinTimer = new System.Timers.Timer(1500) { Enabled = true };
                closeWinTimer.Elapsed += closeWinTimer_Elapsed;
                closeWinTimer.Start();
                Show();
            } 
            else
            {
                ShowDialog();
            }
        }

        #region
        /// <summary>Добавление кнопок на форму</summary>
        /// <param name="buttons">Набор кнопок</param>
        private void addButtons(IEnumerable<NotifyWinButton> buttons)
        {
            if (buttons != null)
            {
                int count = 0;

                foreach (NotifyWinButton notifyWinButton in buttons)
                {
                    if (count == 3)
                        break;

                    Button btn = new Button
                    {
                        Width = 90,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, count * 95 + 5, 3)
                    };

                    switch (notifyWinButton)
                    {
                        case NotifyWinButton.Ok:
                            btn.Content = "Ок";
                            btn.Click += Ok_Click;
                            break;
                        case NotifyWinButton.Yes:
                            btn.Content = "Да";
                            btn.Click += Yes_Click;
                            break;
                        case NotifyWinButton.No:
                            btn.Content = "Нет";
                            btn.Click += No_Click;
                            break;
                        case NotifyWinButton.Cancel:
                            btn.Content = "Отмена";
                            btn.Click += Cancel_Click;
                            break;
                    }

                    MainArea.Children.Add(btn);
                    count++;
                }
            }
        }

        /// <summary>Закрытие формы</summary>
        void NotifyWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        /// <summary>Закрытие формы по нажатию пр. кнопки мыши (только при автозакрытии)</summary>
        private void Window_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(autoClose)
            {
                Close();
            }
        }

        void closeWinTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Close));
        }

        protected virtual void OnClickButton(ClickButtonEventArgs e)
        {
            EventHandler<ClickButtonEventArgs> handler = ClickButton;

            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Нажатие на кнопки
        void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifyWinButton.Ok;
            Close();
        }

        void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifyWinButton.Yes;
            Close();
        }

        void No_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifyWinButton.No;
            Close();
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = NotifyWinButton.Cancel;
            Close();
        }
        #endregion

        #region Кнопка Close
        private void close_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            close.Opacity = 0.9;
        }

        private void close_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            close.Opacity = 0.4;
        }

        private void close_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            close.Opacity = 1;
        }

        private void close_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }
        #endregion

        #region Остановка-запуск таймера при попадании на форму
        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (autoClose)
            {
                closeWinTimer.Stop();
            }
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
           if(autoClose)
           {
               closeWinTimer.Start();
           }
        }
        #endregion
    }

    public class ClickButtonEventArgs : EventArgs
    {
        private readonly NotifyWinButton button;

        public ClickButtonEventArgs(NotifyWinButton b)
        {
            button = b;
        }
        public NotifyWinButton Remove
        {
            get { return button; }
        }
    }

    /// <summary>Кнопки диалога</summary>
    public enum NotifyWinButton {None, Ok, Yes, No, Cancel}
}
