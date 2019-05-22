using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace LeumitWebServiceDataClient
{
    internal class LicenseChecker
    {
        Logger logger;
        public LicenseChecker()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        public void CheckLicense(string path = @"C:\ProgramData\KriSoftware\ADTServer\cred\cred.json")
        {
            int daysThreshhold = 14;
            logger.Info("Checking License for expiration, will create an error in the windows error log if there are less then 14 days left");
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                int numberOfDaysLeft = 365 - (int)(DateTime.Now - fi.CreationTime.Date).TotalDays;
                if (numberOfDaysLeft < daysThreshhold)
                {
                    logger.Info($"!!!!!!------  LESS THAN {daysThreshhold} DAYS LEFT UNTIL THE LICENSE EXPIRES , PLEASE UPDATE LICENSE ------!!!!!!!!");
                    LogEvent(LogLevel.Error, 23000, $"License will expire in {numberOfDaysLeft} days");
                }
                else
                {
                    logger.Info($"There are {numberOfDaysLeft} days until license expires ");
                }
            }
            else
            {
                logger.Error($"Error when trying to open file : \"{path}\" for license checking");
            }


        }

        private void LogEvent(LogLevel level, int eventId, string message)
        {
            LogEventInfo logEventInfo = new LogEventInfo();
            logEventInfo.Level = level;
            logEventInfo.Properties.Add("EventID", eventId);
            logEventInfo.Message = message;
            logger.Log(logEventInfo);

        }
    }
}
