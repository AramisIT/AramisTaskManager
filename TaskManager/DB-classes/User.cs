using System.Collections.Generic;
using System.ComponentModel;
using TaskManager.SQLite;

namespace TaskManager.DB_classes
{
    public class User : IDataErrorInfo
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsDelete { get; set; }

        public User(bool generate = true)
        {
            if (generate)
            {
                GUID = SQLiteWorker.GenerateGUID();
                IsDelete = false;
            }
        }

        public User(string guid)
        {
            GUID = guid;
        }

        public User(User user)
        {
            if (user != null)
            {
                GUID = user.GUID;
                Name = user.Name;
                Email = user.Email;
                IsDelete = user.IsDelete;
            }
        }

        public static User GetCurrentUser()
        {
            return SQLiteWorker.GetUserFromGUID(
                SQLiteWorker.GetSettingProperty(SQLiteWorker.SettingProperty.CurrentUser));
        }

        public static List<User> GetUsers(bool all = false)
        {
            return SQLiteWorker.GetUsers(all);
        }

        #region validation
        private readonly Dictionary<string, bool> notValidFields = new Dictionary<string, bool>();
        public bool IsValid { get { return notValidFields.Count == 0; } }
        public string Error { get { return null; } }

        public string this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Name":
                        if (string.IsNullOrWhiteSpace(Name))
                        {
                            if (!notValidFields.ContainsKey(name))
                            {
                                notValidFields.Add(name, true);
                            }

                            return "Empty field!";
                        }
                        if (notValidFields.ContainsKey(name))
                        {
                            notValidFields.Remove(name);
                        }

                        break;
                    case "Email":
                        if (string.IsNullOrWhiteSpace(Email))
                        {
                            if (!notValidFields.ContainsKey(name))
                            {
                                notValidFields.Add(name, true);
                            }

                            return "Empty field!";
                        }
                        if (notValidFields.ContainsKey(name))
                        {
                            notValidFields.Remove(name);
                        }

                        break;
                }

                return null;
            }
        }
        #endregion
    }
}