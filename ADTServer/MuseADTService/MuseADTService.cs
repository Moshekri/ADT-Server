
#define MEUHEDET  

using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using ADTServ;
using ApplicationLogger;
using System.Threading;
using AdtSvrCmn;
using System.IO;
using AdtSvrCmn.Interfaces;
using GlobalApplicationConfigurationManager;
using LeumitWebServiceDataClient;

using System.Configuration;
using MuseHl7Parser;
using AdtSvrCmn.Objects;
using LeumitPatientIdParser;
using NLog;




namespace MuseADTService
{

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public uint dwServiceType;
        public ServiceState dwCurrentState;
        public uint dwControlsAccepted;
        public uint dwWin32ExitCode;
        public uint dwServiceSpecificExitCode;
        public uint dwCheckPoint;
        public uint dwWaitHint;
    };

    public partial class MuseADTService : ServiceBase
    {
        #region fields
        ServiceStatus serviceStatus = new ServiceStatus();


        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        Hl7Mgr hl7manager;
        Logger logger;
        ApplicationConfiguration configuration;

        #endregion

        public MuseADTService()
        {
            try
            {
                logger = LogManager.GetCurrentClassLogger();
                logger.Debug("Loading Configuration files...");
                IPatientIdHandler pidHandler = new PidParser();
                try
                {
                    var configFilePath = ConfigurationManager.AppSettings["ConfigFileFullPath"];
                    ApplicationConfigurationManager configManager = new ApplicationConfigurationManager(configFilePath);
                    configuration = configManager.GetConfig();
                    logger.Debug($"Database folder from configuration : {configuration.DataBaseFolder}");
                }
                catch (Exception ex)
                {
                    logger.Debug("Configuration loading catch block");
                    logger.Debug(ex.Message);
                    Environment.Exit(2);
                }

                InitializeComponent();
                IPatientInfoSoapSource pInfoSource = null;
                IPatientIdHandler pidParser = null;
//#if DEBUG
//                // using refelction for loading the files
//                //pInfoSource = plugInManager.GetWebServiceClientApp();
//                //pidParser = plugInManager.GetIdPatientHandlers();

//                //leumit
//                //pidParser = new LeumitPatientIdParser.PidParser() ;
//                //pInfoSource = new LeumitWebServiceDataClient.LeumitPatientInfoClient(configuration);


//                //meuhedet
//                logger.Debug("Loading Patient Id Parser...");
//                pidParser = new MeuhedetPatientIdParser.MeuhedetIdParser();
//                logger.Debug("Patient Id Parser loaded");
//                logger.Debug("Loading Web service client...");
//                pInfoSource = new MeuhedetWebServiceDataClient.MeuhedetPatientDataRequester(configuration);
//                logger.Debug("Web service client loaded");
//#else

#if REFLECTION || DEBUG && REFLECTION       
                PlugInManager.PlugInManager plugInManager = new PlugInManager.PlugInManager(configuration.PlugInFolderPath, configuration);
                
                logger.Debug("Loading Web service client...");
                pInfoSource = plugInManager.GetWebServiceClientApp();
                logger.Debug("Web service client loaded");

                logger.Debug("Loading Patient Id Parser...");
                pidParser = plugInManager.GetIdPatientHandlers();
                logger.Debug("Patient Id Parser loaded");
#elif LEUMIT|| DEBUG && LEUMIT
                
                logger.Debug("Loading Leumit Patient Id Parser...");
                pidParser = new LeumitPatientIdParser.PidParser();
                logger.Debug("Patient Id Parser loaded");

                logger.Debug("Loading Leumit Web service client...");
                pInfoSource = new LeumitWebServiceDataClient.LeumitPatientInfoClient(configuration);
                logger.Debug("Web service client loaded");
#elif MEUHEDET || DEBUG && MEUHEDET
                logger.Debug("Loading Meuhedet Patient Id Parser...");
                pidParser = new MeuhedetPatientIdParser.MeuhedetIdParser();
                logger.Debug("Patient Id Parser loaded");

                logger.Debug("Loading Meuhedet Web service client...");
                pInfoSource = new MeuhedetWebServiceDataClient.MeuhedetPatientDataRequester(configuration);
                logger.Debug("Web service client loaded");
#endif


                if (pInfoSource == null)
                {
                    NlogHelper.CreateLogEntry("Cannot Start Service - no patient info web service client plugin loaded", "1000", LogLevel.Debug, logger);
                    throw new Exception("Missing PlugIn");
                }
                if (pidParser == null)
                {
                    NlogHelper.CreateLogEntry("Cannot Start Service - no patient id parser plugin loaded", "1000", LogLevel.Debug, logger);
                    throw new Exception("Missing PlugIn");
                }

//#endif

                hl7manager = new Hl7Mgr(new Hl7ManagerData()
                {
                    Config = configuration,
                    PatientInformationSource = pInfoSource,
                    IHl7Parser = new Hl7Parser(),
                    PidHandler = pidParser

                });

                logger.Info($"Server ip = {configuration.ServerIp} , Port = {configuration.ServerListeningPort}");
            }
            catch (Exception ex)
            {
                NlogHelper.CreateLogEntry(ex.Message, "1000", LogLevel.Debug, logger);
                Environment.Exit(2);
            }

        }

        public void StartForDebug()
        {
            OnStart(null);
            
        }

        protected override void OnStart(string[] args)
        {

            try
            {
                logger.Info("Starting ADT Service....");
                // start the fascade service manager
                hl7manager.Start();
                logger.Info("ADT serive started successfuly");
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message);
                Environment.Exit(2);
            }

#if DEBUG
            Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
            try
            {
                //// Update the service state to Running.  
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message);
                Environment.Exit(2);
            }

        }

        protected override void OnStop()
        {
            try
            {
                logger.Info("Stopping ADT Service....");
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                hl7manager.Stop();


                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                logger.Info("ADT serive Stopped successfuly");
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message);
                Environment.Exit(2);
            }
        }



    }

}
