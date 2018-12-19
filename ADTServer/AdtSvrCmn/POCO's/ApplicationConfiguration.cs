using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AdtSvrCmn;

namespace AdtSvrCmn.Objects
{
    [Serializable]
    public class ApplicationConfiguration
    {

        public string DataBaseFileName { get; set; }
        public string TempDataBaseFileName { get; set; }
        public string DataBaseFolder { get; set; }
        public string LogFileName { get; set; }
        public string LogFilePath { get; set; }
        public string ServerListeningPort { get; set; }
        public string ServerIp { get; set; }
        public string GoogleCredentialFilePath { get; set; }
        public string GoogleCredentialFileName { get; set; }
        public string SourceSystem { get; set; }
        public string WebTimeOut { get; set; }
        public string SoapRequestTarget { get; set; }
        public bool MustGetTranslation { get; set; }
        public bool UseLocalDb { get; set; }
        public string PassportPidType { get; set; }
        public string PlugInFolderPath { get; set; }
        public string RegularPidType { get; set; }
        public string GoodSoapResponseErrorCode { get; set; }
        public string BadSoapResponseCode { get; set; }
        

        public ApplicationConfiguration()
        {
            DataBaseFolder = @"c:\programData\KriSoftware\ADTServer\Db";
            DataBaseFileName = "db.bin";
            TempDataBaseFileName = "tempdb.bin";
            LogFileName = "AdtLog.txt";
            LogFilePath = @"c:\programData\KriSoftware\ADTServer\Logs";
            ServerListeningPort = "9005";
            ServerIp = "127.0.0.1";
            GoogleCredentialFileName = "cred.json";
            GoogleCredentialFilePath = @"c:\programData\KriSoftware\ADTServer\cred";
            SourceSystem = "45";
            WebTimeOut = "5000";
            SoapRequestTarget = "";
            MustGetTranslation = true;
            UseLocalDb = true;
            PassportPidType = "9";
            PlugInFolderPath = @"c:\programData\KriSoftware\ADTServer\PlugIns";
            RegularPidType = "1";
            GoodSoapResponseErrorCode = "0";
            BadSoapResponseCode = "9";
        }

    }
}
