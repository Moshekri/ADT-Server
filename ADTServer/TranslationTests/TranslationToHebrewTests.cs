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

        //[TestMethod]
        //public void CheckHebrewNamesTranslationWhenNoDbExists()
        //{
        //    bool allNamesTranslated = false;
        //    string temp;

        //    config.UseLocalDb = true;

        //    File.Delete(Path.Combine(config.DataBaseFolder, config.DataBaseFileName));
        //    HebrewNameNormalizer.Normalizer norm = new HebrewNameNormalizer.Normalizer();
        //    TranslationManagerData tMgrData = new TranslationManagerData()
        //    {
        //        Config = config,
        //        Logger = log,
        //        NameNormalyzer = norm
        //    };
        //    translator = TranslationManagerFactory.GetTranslationManager(tMgrData);
        //    dbConnector = new Db(config);
        //    for (int i = 0; i < firstNames.Length; i ++)
        //    {
        //        translator.GetEnglishName(firstNames[i], lastNames[i]);
        //    }

        //    var data = dbConnector.GetData();

        //    allNamesTranslated = data.TryGetValue("משה", out temp);
        //    Assert.AreEqual(temp.ToLower(), "moshe");

        //    allNamesTranslated = data.TryGetValue("קריכלי", out temp);
        //    Assert.AreEqual(temp.ToLower(),"Krichli");

        //    allNamesTranslated = data.TryGetValue("בן-סימון", out temp);
        //    Assert.AreEqual(temp,"Ben-Simon");

        //    allNamesTranslated = data.TryGetValue("בן-אבו", out temp);
        //    Assert.AreEqual(temp,"Ben-Abu");


        //    allNamesTranslated = data.TryGetValue("מוחמד", out temp);
        //    Assert.AreEqual(temp,"Mohammed");


        //    allNamesTranslated = data.TryGetValue("דוד", out temp);
        //    Assert.AreEqual(temp,"David");


        //    allNamesTranslated = data.TryGetValue("אבו-טיר", out temp);
        //    Assert.AreEqual(temp,"Abu-Tir");

        //}
        //[TestMethod]
        //public void CheckHebrewNamesTranslationWhenDbExists()
        //{
        //    bool allNamesTranslated = true;
        //    string temp;



        //    for (int i = 0; i < firstNames.Length; i++)
        //    {
        //        translator.GetEnglishName(firstNames[i], lastNames[i]);
        //    }

        //    var data = dbConnector.GetData();


        //    allNamesTranslated = data.TryGetValue("משה", out temp);
        //    Assert.AreEqual(temp, "Moshe");

        //    allNamesTranslated = data.TryGetValue("קריכלי", out temp);
        //    Assert.AreEqual(temp, "Krichli");

        //    allNamesTranslated = data.TryGetValue("בן-סימון", out temp);
        //    Assert.AreEqual(temp, "Ben-Simon");

        //    allNamesTranslated = data.TryGetValue("בן-אבו", out temp);
        //    Assert.AreEqual(temp, "Ben-Abu");


        //    allNamesTranslated = data.TryGetValue("מוחמד", out temp);
        //    Assert.AreEqual(temp, "Mohammed");


        //    allNamesTranslated = data.TryGetValue("דוד", out temp);
        //    Assert.AreEqual(temp, "David");


        //    allNamesTranslated = data.TryGetValue("אבו-טיר", out temp);
        //    Assert.AreEqual(temp, "Abu-Tir");
        //    Assert.IsTrue(allNamesTranslated);
        //}
    }
}
