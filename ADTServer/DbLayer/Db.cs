using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Threading;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;

namespace DbLayer
{
    public class Db : IDbConnector
    {
        private readonly string _dbFilePath;
        private readonly string _tempDbFile;
        private const string Source = "Database Connector";
        private ApplicationConfiguration _config;
        private static object locker;


        public Db(ApplicationConfiguration configuration)
        {
            _dbFilePath = Path.Combine(configuration.DataBaseFolder, configuration.DataBaseFileName);
            _tempDbFile = configuration.TempDataBaseFileName;
            _config = configuration;
            locker = new object();
        }

        public Dictionary<string, string> GetData()
        {
            BinaryFormatter bf = new BinaryFormatter();
            if (!File.Exists(_dbFilePath))
            {
                CreateNewDbFile();
                return new Dictionary<string, string>();
            }
            try
            {
                var fs = File.OpenRead(_dbFilePath);

                var data = bf.Deserialize(fs) as Dictionary<string, string>;
                fs.Close();
                return data;
            }
            catch (Exception)
            {
                throw;
            }



        }

        private void CreateNewDbFile()
        {
            lock (locker)
            {
                if (!Directory.Exists(Path.GetDirectoryName(_dbFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_dbFilePath));
                }

                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(_dbFilePath));
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                AddDirectorySecurity(Path.GetDirectoryName(_dbFilePath), userName, FileSystemRights.FullControl, AccessControlType.Allow);

                var fs = File.Create(_dbFilePath);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, new Dictionary<string, string>());
                fs.Close();
            }

        }

        public bool SaveData(Dictionary<string, string> data)
        {

            bool isTempDbSuccessfullySaved = SaveTempDb(data);
            if (isTempDbSuccessfullySaved)
            {
                try
                {
                    ReplaceOldDbFileWithNewTempDbFile(data);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private void ReplaceOldDbFileWithNewTempDbFile(Dictionary<string, string> data)
        {

            string newFileDbFileName = _dbFilePath + DateTime.Now.Ticks % 100;
            string dbFilePath = Path.Combine(_config.DataBaseFolder, _config.DataBaseFileName);
            try
            {
                lock (locker)
                {
                    if (File.Exists(newFileDbFileName))
                    {
                        File.Delete(newFileDbFileName);
                    }
                    SaveFile(data, newFileDbFileName);
                    if (File.Exists(dbFilePath))
                    {
                        File.Copy(dbFilePath, dbFilePath + ".tmp");
                        File.Delete(dbFilePath);
                    }
                    try
                    {
                        File.Copy(newFileDbFileName, dbFilePath);
                        File.Delete(dbFilePath + ".tmp");
                    }
                    catch (Exception)
                    {

                        Thread.Sleep(1500);
                        File.Copy(dbFilePath + ".tmp", dbFilePath);
                        File.Delete(dbFilePath + ".tmp");
                    }


                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SaveFile(Dictionary<string, string> data, string fullFilePath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            lock (locker)
            {
                using (var dbfileStream = File.Create(fullFilePath))
                {
                    bf.Serialize(dbfileStream, data);
                }
            }
        }

        private bool SaveTempDb(Dictionary<string, string> data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string tempDbFile = Path.Combine(_config.DataBaseFolder, _config.TempDataBaseFileName);
            try
            {
                lock (locker)
                {
                    using (var tempDbFileSream = File.Create(tempDbFile))
                    {
                        bf.Serialize(tempDbFileSream, data);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveData(Dictionary<string, string> data, string path)
        {
            try
            {
                lock (locker)
                {
                    var fileStream = File.Create(path);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fileStream, data);
                    fileStream.Flush();
                    fileStream.Close();
                }

            }
            catch (Exception)
            {
                throw;
            }

            return true;

        }

        private void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account,
                Rights,
                ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }

        public void SaveSingleEntry(string key, string value)
        {
            Dictionary<string, string> data = GetData();
            if (!data.ContainsKey(key))
            {
                data.Add(key, value);
            }
            try
            {
                SaveData(data);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public string GetValue(string key)
        {
            string temp = "";
            var data = GetData();
            if (data.ContainsKey(key))
            {
                
                 data.TryGetValue(key ,out temp);
            }
            return temp;
        }
    }
}
