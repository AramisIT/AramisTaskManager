using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using TaskManager.DB_classes;
using TaskManager.Window;

namespace TaskManager.e_mail
{
    public struct MailParts
    {
        public string Command;
        public List<SQLiteParameter> Parameter;
    }

    /// <summary>Работа с почтой </summary>
    public class Messanger
    {
        private const string ParameterTag = "<:Parameter:>";

        public static bool SendQuery(string command, List<SQLiteParameter> parameters)
        {
            if (Settings.CurrentUser == null)
            {
                MessageWindows.Show("Обновление не выслано", "Для рассылки обновлений необходимо выбрать пользователя");
                return false;
            }

            try
            {
                //Настройки сервера
                SmtpClient Smtp = new SmtpClient(Settings.SMTPserver, Settings.SMTPport)
                {
                    Credentials = new NetworkCredential(Settings.EmailUser, Settings.EmailPass),
                    EnableSsl = true,
                    Timeout = 60000
                };

                ////Message Body
                StringBuilder body = new StringBuilder(command);

                if (parameters != null)
                {
                    foreach (SQLiteParameter parameter in parameters)
                    {
                        body.AppendFormat("\r\n{0}{1}={2}", ParameterTag, parameter.ParameterName, parameter.Value);
                    }
                }

                //Формирование письма
                MailMessage Message = new MailMessage
                {
                    From = new MailAddress(Settings.CurrentUser.Email, "Aramis-IT", Encoding.UTF8),
                    Subject = "Aramis-IT: Sending query",
                    Body = body.ToString(),
                    HeadersEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                //Формирование MailTo
                List<User> users = User.GetUsers();
                foreach (User user in users)
                {
                    if (!Settings.CurrentUser.Email.Equals(user.Email))
                    {
                        Message.To.Add(user.Email);
                    }
                }

                if(users.Count==0)
                {
                    return true;
                }
                ////Attachment
                //foreach (string file in attach)
                //{
                //    string[] parts = file.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                //    //Прикрепляем файл
                //    Attachment attachment = new Attachment(parts[0], MediaTypeNames.Application.Octet)
                //    {
                //        Name = parts[0],
                //        NameEncoding = Encoding.UTF8
                //    };

                //    Message.Attachments.Add(attachment);
                //}

                //Send
                Smtp.Send(Message);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    string.Format(
                    "Произошла ошибка при отправке почты.\r\nСообщение об ошибке: {0}\r\n{1}",
                    exp.Message, 
                    exp.InnerException != null ? exp.InnerException.Message : ""),
                    "Ошибка отправки почты!", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        public static List<MailParts> CheckInbox(ref string resultTopic, ref string resultMsg)
        
        {
            if (resultMsg == null) throw new ArgumentNullException("resultMsg");
            //Dictionary<int, Message> messages = new Dictionary<int, Message>();
            Pop3Client pop3Client = new Pop3Client();
            List<MailParts> querys = new List<MailParts>();

            try
            {
                if (pop3Client.Connected)
                {
                    pop3Client.Disconnect();
                }

                pop3Client.Connect(Settings.POPserver, Settings.POPport, true);
                pop3Client.Authenticate(Settings.EmailUser, Settings.EmailPass);
                
                int count = pop3Client.GetMessageCount();

                for (int i = count; i >= 1; i -= 1)
                {
                    try
                    {
                        Message message = pop3Client.GetMessage(i);
                        //messages.Add(i, message);

                        if (message.Headers.Subject.Equals("Aramis-IT: Sending query"))
                        {
                            if (message.MessagePart != null &&
                                message.MessagePart.IsText)
                            {
                                string[] parts = message.MessagePart.GetBodyAsText().Split(
                                    new[] { ParameterTag },
                                    StringSplitOptions.RemoveEmptyEntries);
                                MailParts mail = new MailParts { Parameter = new List<SQLiteParameter>() };
                                bool isFirst = true;

                                foreach (string part in parts)
                                {
                                    if (!isFirst)
                                    {
                                        string[] p = part.Split('=');
                                        mail.Parameter.Add(new SQLiteParameter(p[0], p[1].Replace("\r\n", "")));
                                    }
                                    else
                                    {
                                        isFirst = false;
                                        mail.Command = part;
                                    }
                                }

                                querys.Add(mail);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(
                            "TestForm: Message fetching failed: " + e.Message + "\r\n" +
                            "Stack trace:\r\n" +
                            e.StackTrace);
                    }
                }

            }
            catch (InvalidLoginException)
            {
                resultTopic = "POP3 Server Authentication";
                resultMsg = "The server did not accept the user credentials!";
                return null;
            }
            catch (PopServerNotFoundException)
            {
                resultTopic = "POP3 Retrieval";
                resultMsg = "The server could not be found";
                return null;
            }
            catch (PopServerLockedException)
            {
                resultTopic = "POP3 Account Locked";
                resultMsg = "The mailbox is locked. It might be in use or under maintenance.";
                return null;
            }
            catch (LoginDelayException)
            {
                resultTopic = "POP3 Account Login Delay";
                resultMsg = "Login not allowed. Server enforces delay between logins.";
                return null;
            }
            catch (Exception e)
            {
                resultTopic = "POP3 Retrieval";
                resultMsg = "Error occurred retrieving mail. " + e.Message;
                return null;
            }
            finally
            {
                if(pop3Client.Connected)
                {
                    pop3Client.Disconnect();
                }
            }

            //foreach (Message msg in messages.Values)
            //{
            //    if (msg.MessagePart != null &&
            //        msg.MessagePart.MessageParts != null &&
            //        msg.MessagePart.MessageParts.Count >= 1 &&
            //        msg.MessagePart.MessageParts[0].IsText)
            //    {
            //        string body = msg.MessagePart.MessageParts[0].GetBodyAsText();
            //        //querys.Add(body);
            //    }
            //}

            return querys;
        }
    }
}