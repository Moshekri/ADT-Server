using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using ApplicationLogger;
using DbLayer;
using TranslationManager;
using ClalitWebServiceDataClient;

namespace TranslationTests
{
    [TestClass]
    public class TranslationToHebrewTests
    {
        private ITranslator translator;
        private ApplicationConfiguration config;
        private IApplicationLogger log;
        private IDbConnector dbConnector;


        private string[] firstNames = new String[]
        {
            "חמי",
            "משה",
            "דוד",
            "מוחמד"

        };

        private string[] lastNames = new string[]
        {
            "קריכלי",
            " בן סימון ",
            "בן-- אבו",
            "אבו       טיר"
        };

        [TestInitialize]
        public void SetUpConfiguration()
        {

            config = new ApplicationConfiguration()
            {
                DataBaseFileName = "Test.db",
                DataBaseFolder = @"c:\ProgramData\Tests",
                GoogleCredentialFileName = "cred.json",
                GoogleCredentialFilePath = @"c:\ProgramData\Tests",
                LogFileName = "Log.txt",
                LogFilePath = @"c:\ProgramData\Tests",
                ServerIp = "127.0.0.1",
                ServerListeningPort = "9005",
                SourceSystem = "45",
                TempDataBaseFileName = "Tempdb.db"
            };
            log = Log.GetInstance(config.LogFilePath, config.LogFileName);
            HebrewNameNormalizer.Normalizer norm = new HebrewNameNormalizer.Normalizer();
            TranslationManagerData tMgrData = new TranslationManagerData()
            {
                Config = config,
                Logger = log,
                NameNormalyzer = norm
            };
            translator = TranslationManagerFactory.GetTranslationManager(tMgrData);
            dbConnector = new Db(config);
        }
        [TestMethod]
        public void GetValue()
        {
            ClalitPatientInfoSource s = new ClalitPatientInfoSource();
            s.GetPatientInfo("302833");
        }
               
    }
}
