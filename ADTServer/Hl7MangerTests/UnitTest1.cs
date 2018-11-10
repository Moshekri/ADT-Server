using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using AdtSvrCmn;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdtSvrCmn.Objects;
using ADTServ;
using GlobalApplicationConfigurationManager;
using ApplicationLogger;
using DbLayer;
using AdtSvrCmn.Interfaces;
using LeumitWebServiceDataClient.LeumitPatientInformationService;
using TranslationManager;
using MuseHl7Parser;
using LeumitPatientIdParser;

namespace Hl7MangerTests
{
    [TestClass]
    public class Hl7InterationTests
    {
        private ITranslator translator;
        private ApplicationConfiguration config;
        private IApplicationLogger log;
        private IDbConnector dbConnector;
        private string pid = string.Empty;
        private IPatientInfoSoapSource patientInfoSoapSource;
        private Hl7Mgr hl7Manager;

        [TestInitialize]
        public void InitTest()
        {
            ApplicationConfigurationManager configurationManager = new ApplicationConfigurationManager(@"c:\programdata\tests\settings.ini");
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
            configurationManager.SaveConfig(config);
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

            hl7Manager = new Hl7Mgr(new Hl7ManagerData()
            { Config = config,
                PatientInformationSource = new MockPatientInfoSoapSource(config),
                IHl7Parser= new Hl7Parser(),
                PidHandler = new PidParser()

            });
            hl7Manager.MessageRecived += Hl7Manager_MessageRecived;
            patientInfoSoapSource = new LeumitWebServiceDataClient.LeumitPatientInfoClient(config);
        }

        private void Hl7Manager_MessageRecived(object sender, EventArgs e)
        {

            if (e is MessageRecievedEventArgs args)
            {
                pid = args.PID;
            }
        }

        [TestMethod]
        public void Hl7ManagerIntegrationTest()
        {
            hl7Manager.MessageRecived += Hl7Manager_MessageRecived;
            hl7Manager.Start();
            TcpClient client = new TcpClient();
            client.Connect("localhost", int.Parse(config.ServerListeningPort));
            var netStream = client.GetStream();
            var currentPath = Environment.CurrentDirectory;
            try
            {
                var data = File.ReadAllBytes(Path.Combine(currentPath, @"TestResources\QRY_Q01.txt"));// (@"F:\GitHub\ADT-Server\ADTServer\Hl7MangerTests\TestResources\QRY_Q01.txt");
                netStream.Write(data, 0, data.Length);
                netStream.Close();
                client.Close();
                Thread.Sleep(1000);
            }
            catch (Exception)
            {

            }

            Assert.AreEqual(pid, "5555");
        }


    }
}
