using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Objects;
using NLog;

namespace LeumitWebServiceDataClient
{
    internal class LicenseChecker
    {
        Logger logger;
        int daysThreshhold = 14;
        ApplicationConfiguration _config;
        public LicenseChecker(ApplicationConfiguration config)
        {
            _config = config;
            string configFilePath =  ConfigurationManager.AppSettings["ConfigFileFullPath"];
            string configFileFolder = Path.GetDirectoryName(configFilePath);
            logger = LogManager.GetCurrentClassLogger();
            string licenseAlarmConfigPath = Path.Combine(configFileFolder, "licenseCheckSetting.ini");
            logger.Info($"Checking folder {configFileFolder} for config File \"licenseCheckSetting.ini\" Exsistance");
            if (!File.Exists(licenseAlarmConfigPath))
            {
                logger.Info($"File \"licenseCheckSetting.ini\" does not exsits, trying to create file....");

                try
                {
                    using (var fs = File.Create(licenseAlarmConfigPath))
                    {

                    }
                    string setting = "Number of days until license expires alarm =  14";

                    File.WriteAllText(licenseAlarmConfigPath, setting);
                    logger.Info($"File \"licenseCheckSetting.ini\" Created Successfully at {configFileFolder}");
                }
                catch (Exception ex)
                {
                    logger.Debug($"Error creating file : {ex.Message}");
                    logger.Error(ex.Message);
                }
                daysThreshhold = 14;
                logger.Info("License Check alaram set to 14 days before expiration.");
            }
            else
            {
                try
                {
                    logger.Info($"License settings file exists !! loading settings from file !!");
                    var data = File.ReadAllText(licenseAlarmConfigPath);
                    var dataArray = data.Split('=');
                    daysThreshhold = int.Parse(dataArray[1].Trim());
                    logger.Info($"From File : License Check alaram set to {daysThreshhold} days before expiration.");
                }
                catch (Exception ex)
                {
                    do
                    {
                        logger.Debug(ex.Message);
                        ex = ex.InnerException;
                    } while (ex != null);
                    
                }

            }
            
        }
        public void CheckLicense(string path = @"C:\ProgramData\KriSoftware\ADTServer\cred\cred.json")
        {
            //int daysThreshhold = 14;
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
