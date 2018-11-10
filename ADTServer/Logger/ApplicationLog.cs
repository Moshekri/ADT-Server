using System;
using System.Text;
using System.IO;
using AdtSvrCmn.Interfaces;
using System.Threading;
using AdtSvrCmn.Objects;
using NLog;

namespace ApplicationLogger
{
    public sealed class Log : IApplicationLogger
    {
        private static Log _logger;
        private string path;
        private string fileName;
        private string fullpath;
        public static object locker;
        Logger logger;


        private Log(string logPath, string logFileName)
        {
            path = logPath;
            fileName = logFileName;
            fullpath = Path.Combine(path, fileName);
            locker = new object();
        }

        public static IApplicationLogger GetInstance(string path, string fileName)
        {

            if (_logger == null)
            {
                _logger = new Log(path, fileName);
            }

            return _logger;
        }

        IApplicationLogger IApplicationLogger.GetInstance(string path, string fileName)
        {
            return GetInstance(path, fileName);
        }

        public void MakeLogEntry(string data, string source)
        {
            //check if folder exists
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"Thread : {Thread.CurrentThread.ManagedThreadId} | {DateTime.Now,-15} - {source,-25}:{data}{Environment.NewLine}");
            CheckLogSize();
            lock (locker)
            {
                File.AppendAllText(fullpath, sb.ToString(), Encoding.UTF8);
            }
            

        }

        public void LogExecption(Exception ex, string source)
        {
            bool innerExceptionExists = false;

            do
            {
                MakeLogEntry(ex.Message, source);
                ex = ex.InnerException;
                innerExceptionExists = null == ex;
            } while (!innerExceptionExists);
        }

        public void LogMessage(string eMessage,string source)
        {
            var lines = eMessage.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                MakeLogEntry(line, source);
            }
        }

        private void CheckLogSize()
        {
            var fullLogPath = Path.Combine(path, fileName);
            FileInfo logfileInfo = new FileInfo(fullLogPath);
            if (File.Exists(logfileInfo.FullName))
            {
                if (logfileInfo.Length > 5000000)
                {
                    try
                    {
                        string newfileName = "Adtlod" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();
                        newfileName = newfileName.Replace('/', '_');
                        newfileName = newfileName.Replace(':', '_');
                        newfileName = Path.Combine(path, newfileName);
                        logfileInfo.MoveTo(newfileName);

                    }
                    catch (Exception ex)
                    {

                        this.LogExecption(ex, "logger");
                    }
                }
            }
            else
            {
                File.Create(logfileInfo.FullName);
            }

        }

        

    }

}
