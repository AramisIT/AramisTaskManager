using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TaskManager.DB_classes;
using TaskManager.e_mail;
using TaskManager.Window;

namespace TaskManager.SQLite
{
    public static class SQLiteWorker
    {
        public struct QueryData
        {
            public string Where;
            public string Join;
            public List<SQLiteParameter> Parameters;
        }

        private static string ConnectionString
        {
            get
            {
                return string.Format(
                    ConfigurationSettings.AppSettings["ConectionString"],
                    GetPathToDB);
            }
        }

        public static string GetPathToDB
        {
            get
            {
                return string.Concat(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "\\TaskDB.db");
            }
        }

        public static string GenerateGUID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }

        private static string doubleToString(double d)
        {
            return string.Format("{0:0.###}", d).Replace(',', '.');
        }

        public static string DateTimeToSQLiteFormat(DateTime d, bool addTime = true)
        {
            return string.Concat(
                string.Format("{0}-{1:00}-{2:00}", d.Year, d.Month, d.Day),  
                addTime ? string.Concat(" ", d.ToLongTimeString(),".000") : "");
        }

        public static string ExecuteNonQuery(string command, List<SQLiteParameter> parameters = null, bool send = false, bool fromMail = false)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            
            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand(command, connection);
                
                if (parameters != null)
                {
                    foreach (SQLiteParameter parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }
                }

                cmd.ExecuteNonQuery();

                if(send)
                {
                    if (command.Contains("insert into TASKS "))
                    {
                        int index = 0;

                        for (int i = 0; i != 13; i++)
                        {
                            index = command.IndexOf(',', index)+1;
                        }

                        index++;
                        if (command[index] == '0')
                        {
                            command = command.Remove(index, 1).Insert(index, "1");
                        }
                    }

                    bool isSended = Messanger.SendQuery(command, parameters);
                    convertQueryToLogRecord(command, parameters, isSended 
                        ? LogRecord.LogColor.Send 
                        : LogRecord.LogColor.Error);
                }
            }
            catch(Exception exc)
            {
                MessageWindows.Show("Ошибка при выполнении запроса", exc.Message);
            }
            finally
            {
                if(connection.State ==ConnectionState.Open)
                {
                    connection.Close();
                }

                if (fromMail)
                {
                    convertQueryToLogRecord(command, parameters);
                }
            }

            return null;
        }

        #region TASK - Задание
        public static string AddTask(Task task)
        {
            return ExecuteNonQuery(string.Format(
                "insert into TASKS values('{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7},'{8}',{9},{10},{11},{12},'{13}','{14}','{15}')",
                task.GUID,
                DateTimeToSQLiteFormat(task.OrderDate.Date, false),
                DateTimeToSQLiteFormat(task.EndDate.Date, false),
                task.Performer.GUID,
                Settings.CurrentUser.GUID,
                task.Topic,
                task.Description,
                task.Priority,
                task.Project.GUID,
                doubleToString(task.TimePlan),
                doubleToString(task.TimeAdjustment),
                doubleToString(task.TimeFact),
                doubleToString(task.Percents),
                task.IsNew ? 1:0, 0, null),
                                   null, true);
        }

        public static string AddTask(IEnumerable<Task> tasks)
        {
            string result = null;

            foreach (Task task in tasks)
            {
                result = AddTask(task);
            }

            return result;
        }

        public static string UpdateTask(Task task)
        {
            return ExecuteNonQuery(string.Format(
                "update TASKS set OrderDate='{0}',EndDate='{1}',Performer='{2}', Topic='{3}',Description='{4}',Priority={5},Project='{6}',TimePlan={7},TimeAdjustment={8},TimeFact={9},Percents={10},IsNew='{11}'\r\nwhere GUID='{12}'",
                DateTimeToSQLiteFormat(task.OrderDate.Date, false),
                DateTimeToSQLiteFormat(task.EndDate.Date, false),
                task.Performer.GUID,
                task.Topic,
                task.Description,
                task.Priority,
                task.Project.GUID,
                doubleToString(task.TimePlan),
                doubleToString(task.TimeAdjustment),
                doubleToString(task.TimeFact),
                doubleToString(task.Percents),
                task.IsNew,
                task.GUID),
                                   null, true);
        }
        
        public static void DeleteTask(string guid, bool isImportant)
        {
            //ExecuteNonQuery(
            //    "delete from Tasks where guid=@GUID",
            //    new List<SQLiteParameter> {new SQLiteParameter("GUID", guid)});

            //if (isImportant)
            //{
            //    ExecuteNonQuery("delete from ImportantTasks where TaskGUID=@GUID",
            //                    new List<SQLiteParameter> {new SQLiteParameter("GUID", guid)});
            //}

            ExecuteNonQuery(
                "update Tasks set IsDelete='1' where guid=@GUID",
                new List<SQLiteParameter> { new SQLiteParameter("GUID", guid) },
                true);
        }

        public static ObservableCollection<Task> GetTasksInCollection(QueryData query, bool all = false)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            if(!all)
            {
                query.Where += query.Where == null ? " (TASKS.IsDelete='False' or TASKS.IsDelete='0')" : " and (TASKS.IsDelete='False' or TASKS.IsDelete='0')";
            }
            string command = string.Concat(
                "select distinct * \r\nfrom TASKS ",
                string.IsNullOrEmpty(query.Join) ? "" : "\r\njoin " + query.Join,
                string.IsNullOrEmpty(query.Where) ? "" : "\r\nwhere " + query.Where);
            ObservableCollection<Task> tasks = new ObservableCollection<Task>();

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand(command, connection);
                if (query.Parameters != null)
                {
                    foreach (SQLiteParameter p in query.Parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string guid = reader.GetString(reader.GetOrdinal("GUID"));
                    object isNew = reader["IsNew"];
                    object isDelete = reader["IsDelete"];

                    Task task = new Task(false)
                    {
                        GUID = guid,
                        OrderDate = Convert.ToDateTime(reader.GetString(reader.GetOrdinal("OrderDate"))),
                        EndDate = Convert.ToDateTime(reader.GetString(reader.GetOrdinal("EndDate"))),
                        Performer = GetUserFromGUID(reader.GetString(reader.GetOrdinal("Performer"))),
                        Customer = GetUserFromGUID(reader.GetString(reader.GetOrdinal("Customer"))),
                        Topic = reader.GetString(reader.GetOrdinal("Topic")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        Priority = reader.GetByte(reader.GetOrdinal("Priority")),
                        Project = new Project(false) { GUID = reader.GetString(reader.GetOrdinal("Project")) },
                        TimePlan = reader.GetDouble(reader.GetOrdinal("TimePlan")),
                        TimeAdjustment = reader.GetDouble(reader.GetOrdinal("TimeAdjustment")),
                        TimeFact = reader.GetDouble(reader.GetOrdinal("TimeFact")),
                        Percents = reader.GetDouble(reader.GetOrdinal("Percents")),
                        IsImportant = GetImportantFlagForTask(guid),
                        IsNew = Convert.ToBoolean(isNew),
                        IsDelete = Convert.ToBoolean(isDelete)
                    };
                    tasks.Add(task);
                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetTasksInCollection:Exception", exc.Message);
                return tasks;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            foreach (Task task in tasks)
            {
                task.Project = GetParentProjectFromGUID(task.Project.GUID, all);
            }

            return tasks;
        }

        public static void UnflagedNewTask(string guid)
        {
            ExecuteNonQuery("update TASKS set IsNew='0' where GUID=@GUID",
                            new List<SQLiteParameter> {new SQLiteParameter("GUID", guid)});
        }
        #endregion

        #region USER - Пользователь
        public static void AddNewUser(User user)
        {
            ExecuteNonQuery(string.Format(
                    "insert into  Users values('{0}', '{1}', '{2}', '{3}')",
                    user.GUID,
                    user.Name,
                    user.Email, 
                    user.IsDelete), null, true);
        }

        public static void UpdateUser(User user)
        {
            ExecuteNonQuery("update Users set Name=@Name, Email=@Email where GUID=@GUID",
                            new List<SQLiteParameter>
                                {
                                    new SQLiteParameter("GUID", user.GUID),
                                    new SQLiteParameter("Name", user.Name),
                                    new SQLiteParameter("Email", user.Email)
                                }, 
                                true);
        }

        public static void DeleteUserByGUID(string guid)
        {
            //ExecuteNonQuery("delete from Users where GUID=@GUID",
            //                new List<SQLiteParameter>
            //                    {
            //                        new SQLiteParameter("GUID", guid)
            //                    });

            ExecuteNonQuery("update Users set IsDelete='1' where GUID=@GUID",
                            new List<SQLiteParameter>
                                {
                                    new SQLiteParameter("GUID", guid)
                                },
                            true);
        }

        public static User GetUserFromGUID(string guid)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            User user = new User(guid);

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand("select name, Email, IsDelete from USERS where guid=@GUID", connection);
                cmd.Parameters.Add(new SQLiteParameter("GUID", guid));
                SQLiteDataReader reader = cmd.ExecuteReader();

                reader.Read();
                user.Name = reader.GetString(reader.GetOrdinal("name"));
                user.Email = reader.GetString(reader.GetOrdinal("Email"));
                object o = reader["IsDelete"];
                user.IsDelete = Convert.ToBoolean(o);
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetUserFromGUID:Exception", exc.Message);
                return null;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return user;
        }

        public static List<User> GetUsers(bool all = false)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            List<User> users = new List<User>();

            try
            {
                connection.Open();
                string command = string.Concat("select GUID, name, Email, IsDelete from USERS",
                    all ? "" : " where (IsDelete='False' or IsDelete='0')");
                SQLiteCommand cmd = new SQLiteCommand(command, connection);
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    object o = reader["IsDelete"];
                    
                    users.Add(new User(false)
                                  {
                                      GUID = reader.GetString(reader.GetOrdinal("GUID")),
                                      Name = reader.GetString(reader.GetOrdinal("name")),
                                      Email = reader.GetString(reader.GetOrdinal("Email")),
                                      IsDelete = Convert.ToBoolean(o)
                                  });

                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetUsers:Exception", exc.Message);
                return users;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return users;
        }
        #endregion

        #region IMPORTANT
        public static bool GetImportantFlagForTask(string guid)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand("select 1 from ImportantTasks where TaskGUID=@GUID", connection);
                cmd.Parameters.Add(new SQLiteParameter("GUID", guid));
                object o = cmd.ExecuteScalar();

                return o != null;
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetImportantFlagForTask:Exception", exc.Message);
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public static void SetFlagOfImportantForTaskByGUID(bool flag, string guid)
        {
                if (flag)
                {
                    ExecuteNonQuery(string.Format("insert into ImportantTasks values('{0}')", guid));
                }
                else
                {
                    ExecuteNonQuery("delete from ImportantTasks where TaskGUID=@GUID",
                                    new List<SQLiteParameter>
                                        {
                                            new SQLiteParameter("GUID", guid)
                                        });
                }
        }
        #endregion

        #region SETTING
        public enum SettingProperty
        {
            CurrentUser, 
            StartupDirectory,
            SmtpServer,
            SmtpPort,
            PopServer,
            PopPort,
            EmailUser,
            EmailPass
        }

        private static string getStringFromEnum(SettingProperty property)
        {
            switch (property)
            {
                case SettingProperty.CurrentUser:
                    return "[CurrentUser]";
                case SettingProperty.StartupDirectory:
                    return "[StartupDirectory]";
                case SettingProperty.SmtpServer:
                    return "[SmtpServer]";
                case SettingProperty.SmtpPort:
                    return "[SmtpPort]";
                case SettingProperty.PopServer:
                    return "[PopServer]";
                case SettingProperty.PopPort:
                    return "[PopPort]";
                case SettingProperty.EmailUser:
                    return "[EmailUser]";
                case SettingProperty.EmailPass:
                    return "[EmailPass]";
            }

            return string.Empty;
        }

        public static void UpdateSettingProperty(SettingProperty property, string value)
        {
            ExecuteNonQuery(string.Format("update Setting set {0}=@value", getStringFromEnum(property)),
                            new List<SQLiteParameter>
                                {
                                    new SQLiteParameter("value", value)
                                });
        }

        public static string GetSettingProperty(SettingProperty property)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand(
                    string.Format("select {0} from Setting where ID=@id", getStringFromEnum(property)),
                    connection);
                cmd.Parameters.Add(new SQLiteParameter("id", 1));
                object o = cmd.ExecuteScalar();

                return o!=null ? o.ToString() : null;
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetSettingProperty:Exception", exc.Message);
                return null;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
        #endregion

        #region COMMENTS
        public static void AddNewComment(Comment comment)
        {
            ExecuteNonQuery(string.Format(
                "insert into Comments values('{0}','{1}','{2}','{3}','{4}','{5}')",
                comment.GUID,
                comment.TaskGUID,
                comment.User.GUID,
                DateTimeToSQLiteFormat(comment.Date),
                comment.Data,
                comment.IsNew)
                , null, true);
        }

        public static void UncheckNewComment(string guid)
        {
            ExecuteNonQuery("update Comments set IsNew='False' where GUID=@GUID",
                            new List<SQLiteParameter>
                                {
                                    new SQLiteParameter("GUID", guid)
                                },
                                true);
        }

        public static List<Comment> GetCommentForTask(string guid)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            List<Comment> comments = new List<Comment>();

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from Comments where TaskGUID=@GUID", connection);
                cmd.Parameters.Add(new SQLiteParameter("GUID", guid));
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    comments.Add(new Comment(false)
                    {
                        GUID = reader.GetString(reader.GetOrdinal("GUID")),
                        TaskGUID = reader.GetString(reader.GetOrdinal("TaskGUID")),
                        User = GetUserFromGUID(reader.GetString(reader.GetOrdinal("UserGUID"))),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Data = reader.GetString(reader.GetOrdinal("Data")),
                        IsNew = Convert.ToBoolean(reader["IsNew"])
                    });
                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetCommentForTask:Exception", exc.Message);
                return comments;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return comments;
        }

        public static ObservableCollection<Comment> GetCommentInCollectionForTask(string guid)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            ObservableCollection<Comment> comments = new ObservableCollection<Comment>();

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from Comments where TaskGUID=@GUID", connection);
                cmd.Parameters.Add(new SQLiteParameter("GUID", guid));
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    comments.Add(new Comment(false)
                    {
                        GUID = reader.GetString(reader.GetOrdinal("GUID")),
                        TaskGUID = reader.GetString(reader.GetOrdinal("TaskGUID")),
                        User = GetUserFromGUID(reader.GetString(reader.GetOrdinal("UserGUID"))),
                        Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        Data = reader.GetString(reader.GetOrdinal("Data")),
                        IsNew = Convert.ToBoolean(reader["IsNew"])
                    });
                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetCommentInCollectionForTask:Exception", exc.Message);
                return comments;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return comments;
        }
        #endregion

        #region PROJECT
        public static void AddNewProject(Project prj)
        {
            ExecuteNonQuery(
                string.Format(
                    "insert into Projects values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                    prj.GUID,
                    prj.Description,
                    DateTimeToSQLiteFormat(prj.DateStart),
                    DateTimeToSQLiteFormat(prj.DateEnd),
                    DateTimeToSQLiteFormat(prj.DateCreate),
                    prj.Customer.GUID,
                    prj.Leader.GUID,
                    prj.Percents,
                    prj.Parent == null ? "" : prj.Parent.GUID,
                    prj.Type,
                    "...", "...", "...", prj.IsDelete),
                    null, true);
        }

        public static void UpdateProject(Project prj)
        {
            ExecuteNonQuery(
                string.Format(
                    "update Projects set Description='{0}',DateStart='{1}',DateEnd='{2}',DateCreate='{3}',Customer='{4}',Leader='{5}',Percents='{6}',Parent='{7}',IsDelete='{8}' where GUID=@GUID",
                    prj.Description,
                    DateTimeToSQLiteFormat(prj.DateStart),
                    DateTimeToSQLiteFormat(prj.DateEnd),
                    DateTimeToSQLiteFormat(prj.DateCreate),
                    prj.Customer.GUID,
                    prj.Leader.GUID,
                    prj.Percents,
                    prj.Parent,
                    prj.IsDelete ? 1:0),
                new List<SQLiteParameter>
                    {
                        new SQLiteParameter("GUID", prj.GUID)
                    },
                    true);
        }

        public static void DeleteProject(string guid)
        {
            //ExecuteNonQuery(
            //    "delete from Projects where guid=@GUID",
            //    new List<SQLiteParameter> {new SQLiteParameter("GUID", guid)});
            ExecuteNonQuery(
                "update Projects set IsDelete='1' where guid=@GUID",
                new List<SQLiteParameter> { new SQLiteParameter("GUID", guid) },
                true);
        }

        public static ObservableCollection<Project> GetProjectsInCollection(bool all = false)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            ObservableCollection<Project> projects = new ObservableCollection<Project>();

            try
            {
                connection.Open();
                string command = string.Concat("select * from Projects", all ? "" : " where (IsDelete='False' or IsDelete='0')");
                SQLiteCommand cmd = new SQLiteCommand(command, connection);
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string GUID = reader.GetString(reader.GetOrdinal("GUID"));
                    object o = reader["IsDelete"];

                    projects.Add(new Project(false)
                    {
                        GUID = GUID,
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateStart = reader.GetDateTime(reader.GetOrdinal("DateStart")),
                        DateEnd = reader.GetDateTime(reader.GetOrdinal("DateEnd")),
                        DateCreate = reader.GetDateTime(reader.GetOrdinal("DateCreate")),
                        Customer = GetUserFromGUID(reader.GetString(reader.GetOrdinal("Customer"))),
                        Leader = GetUserFromGUID(reader.GetString(reader.GetOrdinal("Leader"))),
                        Percents = reader.GetDouble(reader.GetOrdinal("Percents")),
                        Parent = null,
                        Type = reader.GetInt32(reader.GetOrdinal("Type")),
                        IsDelete = Convert.ToBoolean(o)
                    });
                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetProjectsInCollection:Exception", exc.Message);
                return projects;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return projects;
        }

        public static Project GetParentProjectFromGUID(string guid, bool all = false)
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            Project parent = new Project(false);

            try
            {
                connection.Open();
                string command = string.Concat("select * from Projects where GUID=@GUID", all ? "" : " and (IsDelete='False' or IsDelete='0')");
                SQLiteCommand cmd = new SQLiteCommand(command, connection);
                cmd.Parameters.Add(new SQLiteParameter("GUID", guid));
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //string parentGUID = reader.GetString(reader.GetOrdinal("Parent"));
                    object o = reader["IsDelete"];

                    parent = new Project(false)
                    {
                        GUID = reader.GetString(reader.GetOrdinal("GUID")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateStart = reader.GetDateTime(reader.GetOrdinal("DateStart")),
                        DateEnd = reader.GetDateTime(reader.GetOrdinal("DateEnd")),
                        DateCreate = reader.GetDateTime(reader.GetOrdinal("DateCreate")),
                        Customer = GetUserFromGUID(reader.GetString(reader.GetOrdinal("Customer"))),
                        Leader = GetUserFromGUID(reader.GetString(reader.GetOrdinal("Leader"))),
                        Percents = reader.GetDouble(reader.GetOrdinal("Percents")),
                        Parent = null,/*string.IsNullOrWhiteSpace(parentGUID) ? null : GetParentProjectFromGUID(parentGUID)*/
                        Type = reader.GetInt32(reader.GetOrdinal("Type")),
                        IsDelete = Convert.ToBoolean(o)
                    };
                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetParentProjectFromGUID:Exception", exc.Message);
                return parent;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return parent;
        }
        #endregion

        #region TO_PERFORM_TASKS
        public static void SetBeginFlagForTask(string guid, bool add)
        {
            ExecuteNonQuery(
                add
                    ? "insert into ToPerformTasks values('0',@GUID)"
                    : "delete from ToPerformTasks where TaskGUID=@GUID",
                new List<SQLiteParameter>
                    {
                        new SQLiteParameter("GUID", guid)
                    }
                );
        }
        #endregion

        #region LOG
        private static void convertQueryToLogRecord(string query, List<SQLiteParameter> parameters, LogRecord.LogColor type = LogRecord.LogColor.UnKnown)
        {
            StringBuilder additionalInfo = new StringBuilder();
            additionalInfo.Append(query.Replace("'", "\""));

            if(parameters!=null && parameters.Count!=0)
            {
                additionalInfo.Append(" [Параметры: ");
                foreach (SQLiteParameter parameter in parameters)
                {
                    additionalInfo.AppendFormat("{0} = {1}", parameter.ParameterName, parameter.Value);
                }
                additionalInfo.Append("]");
            }

            string mainInfo = "Запрос.";
            query = query.ToUpper();

            if (type == LogRecord.LogColor.UnKnown)
            {
                if (query.Contains("INSERT INTO "))
                {
                    mainInfo = string.Format(
                        "Добавление записи в таблицу {0}",
                        getTableNameFromTypedQuery(query, "INSERT INTO "));
                    type = LogRecord.LogColor.Insert;
                }
                else if (query.Contains("UPDATE "))
                {
                    mainInfo = string.Format(
                        "Обновление записи в таблице {0}",
                        getTableNameFromTypedQuery(query, "UPDATE "));
                    type = LogRecord.LogColor.Update;
                }
            }
            else
            {
                if(type == LogRecord.LogColor.Send)
                {
                    mainInfo = "Отправка обновлений по почте";
                }
            }

            AddLogRecord(mainInfo, additionalInfo.ToString().Replace("\r\n", ""), type);
        }

        private static string getTableNameFromTypedQuery(string query, string begining)
        {
            string[] table = query.Replace(begining, "").Split(
                   new[] { " " }, StringSplitOptions.RemoveEmptyEntries);


            return table[0] ?? "<...>";
        }

        public static void AddLogRecord(string mainInfo, string additionalInfo, LogRecord.LogColor type)
        {
            ExecuteNonQuery(string.Format(
                "insert into LOG values('{0}', '{1}', '{2}', '{3}')",
                DateTimeToSQLiteFormat(DateTime.Now), mainInfo, additionalInfo, (byte)type));
        }

        public static List<LogRecord> GetLogRecords()
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            string command = string.Concat("select * \r\nfrom LOG");
            List<LogRecord> records = new List<LogRecord>();

            try
            {
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand(command, connection);
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    LogRecord record = new LogRecord
                    {
                        LogDateTime = reader.GetString(reader.GetOrdinal("DateTime")),
                        MainInfo = reader.GetString(reader.GetOrdinal("MainInfo")),
                        AddInfo = reader.GetString(reader.GetOrdinal("Description")),
                        Color = (LogRecord.LogColor)reader.GetByte(reader.GetOrdinal("Type"))
                    };
                    records.Add(record);
                }
            }
            catch (Exception exc)
            {
                MessageWindows.Show("GetLogRecords:Exception", exc.Message);
                return records;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return records;
        }
        #endregion
    }
}