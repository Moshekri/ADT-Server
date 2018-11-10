using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn
{
    public static class NlogHelper
    {

        public static void CreateLogEntry(string msg, string eventID, LogLevel level, Logger logger)
        {

            LogEventInfo info = new LogEventInfo();
            info.Message = msg;
            info.Properties.Add("EventID", eventID);
            info.Level = level;
            logger.Log(info);
        }

        public static void LogException(Exception ex, Logger logger)
        {
            CreateLogEntry(ex.Message, "100", LogLevel.Error,logger);
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                CreateLogEntry(ex.Message, "100", LogLevel.Error,logger);
            }
        }




    }
}
