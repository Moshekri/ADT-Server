
// using nlog logger
using System;
using System.Threading;
using System.IO;
using ApplicationLogger;
using AdtSvrCmn;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using System.Net.Sockets;
using AdtSvrCmn.EventArguments;
using TranslationManager;
using MuseTcpServer;
using NLog;
using System.Text;
using System.Net.NetworkInformation;
using System.Linq;
using HebrewNameNormalizer;
using System.Configuration;

namespace ADTServ
{
    /// <summary>
    /// This class manages all HL7 related operations 
    /// it will get the message from muse , query the web service for patient details and send the response back to muse
    /// </summary>
    public class Hl7Mgr
    {
        #region Events
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;
        public event EventHandler MessageRecived;
        public event EventHandler MessageSent;
        #endregion

        #region Private Fields
        public CancellationTokenSource Tokensource { get; set; }
        public Server Server { get; set; }
        private readonly int port;
        private readonly IApplicationLogger ologger;
        private const string source = "Hl7Manager";
        ITranslator tManager;
        private ApplicationConfiguration _config;
        private IPatientInfoSoapSource WebServiceClient;
        private IHl7Parser hl7MessageParser;
        Exception exceptionFromWebService;
        private Hl7ManagerData _data;
        NLog.Logger logger;
        private object locker;
        #endregion

        #region Constructor
        public Hl7Mgr(Hl7ManagerData data)
        {
            logger = LogManager.GetCurrentClassLogger();
            locker = new object();

            _data = data;
            logger.Debug("Loading hl7Manager");
            hl7MessageParser = data.IHl7Parser;
            _config = data.Config;
            WebServiceClient = data.PatientInformationSource;

            try
            {
                logger.Debug("Loading cred file");
                string cred = Path.Combine(_config.GoogleCredentialFilePath, _config.GoogleCredentialFileName);
                logger.Debug("Cred file Loaded !");

                string db = Path.Combine(_config.DataBaseFolder, _config.DataBaseFileName);
                logger.Debug("Setting tcp server paerameters");
                port = int.Parse(_config.ServerListeningPort);
                logger.Debug($"Parameters set : port = {_config.ServerListeningPort}");
                ologger = Log.GetInstance(_config.LogFilePath, _config.LogFileName);
                Normalizer normalizer = new Normalizer();

                TranslationManagerData tMgrData = new TranslationManagerData()
                {
                    Config = _config,
                    Logger = ologger,
                    NameNormalyzer = normalizer
                };
                if (_config.MustGetTranslation)
                {
                    tManager = TranslationManagerFactory.GetTranslationManager(tMgrData);
                }
                Tokensource = new CancellationTokenSource();
                Server = new Server(_config, Tokensource.Token);
            }
            catch (Exception ex)
            {
                Exception newException = new Exception("Error Constructing Hl7Manager Class", ex);
                logger.Debug(newException);
                throw newException;
            }

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the server - listening for incoming messages from muse
        /// </summary>
        public void Start()
        {
            RegisterServerEvents();
            logger.Debug("Starting Server");
            Server.StartServerThread();
            //logger.Debug("Server Started");
        }

        /// <summary>
        /// Stops the server 
        /// </summary>
        public void Stop()
        {
            Tokensource.Cancel();
            UnRegisterServerEvents();
            logger.Debug("stopping Server");
            Server.StopServer();
            logger.Debug("Server Stopped");

        }
        #endregion

        #region Events

        #region Event Registration / unRegistration
        /// <summary>
        /// Register to the Server class events
        /// </summary>
        private void RegisterServerEvents()
        {
            Server.MessageRecieved += Server_MessageRecieved;
            Server.ServerStarted += Server_ServerStarted;
            Server.ServerStopped += Server_ServerStopped;
            Server.MessageSent += Server_MessageSent;
            WebServiceClient.ErrorGettingPatientInfo += WebServiceClient_ErrorGettingPatientInfo;
            WebServiceClient.RecevingPatientInfo += WebServiceClient_RecevingPatientInfo;
            WebServiceClient.RequestingPatientInfo += WebServiceClient_RequestingPatientInfo;
        }

        /// <summary>
        /// UnRegister to the Server class events
        /// </summary>
        private void UnRegisterServerEvents()
        {
            Server.MessageRecieved -= Server_MessageRecieved;
            Server.ServerStarted -= Server_ServerStarted;
            Server.ServerStopped -= Server_ServerStopped;
            Server.MessageSent -= Server_MessageSent;
            WebServiceClient.ErrorGettingPatientInfo -= WebServiceClient_ErrorGettingPatientInfo;
            WebServiceClient.RecevingPatientInfo -= WebServiceClient_RecevingPatientInfo;
            WebServiceClient.RequestingPatientInfo -= WebServiceClient_RequestingPatientInfo;
        }
        #endregion


        private void Server_MessageSent(object sender, EventArgs e)
        {
            var args = e as MessageSentEventArgs;
            logger.Info($"{Environment.NewLine}Sending Hl7 message back to muse: {Environment.NewLine}{args.Message}{Environment.NewLine}");

        }

        private void Server_ServerStopped(object sender, EventArgs e)
        {
            logger.Debug("Server stopped");
            ServerStopped?.Invoke(this, null);
        }

        private void Server_ServerStarted(object sender, EventArgs e)
        {
            logger.Debug("Server started");
            ServerStarted?.Invoke(this, null);
        }

        private void WebServiceClient_RequestingPatientInfo(object sender, EventArgs e)
        {
            var args = e as PatientInfoEventArgs;
            logger.Info($"Requesting patient info {Environment.NewLine}{args.Message}");
        }

        private void WebServiceClient_RecevingPatientInfo(object sender, EventArgs e)
        {
            var args = e as PatientInfoEventArgs;
            string messageWithoutNewlines = RemoveNewLines(args.Message);
            logger.Info($"Recived Data From WebService : {messageWithoutNewlines}");
        }

        private string RemoveNewLines(string message)
        {
            var lines = message.Split(new char[] { '\r', '\n' });
            StringBuilder sb = new StringBuilder();
            foreach (var item in lines)
            {
                sb.Append(item);
            }
            return sb.ToString();
        }

        private void WebServiceClient_ErrorGettingPatientInfo(object sender, EventArgs e)
        {
            logger.Debug("Exception occured while getting patient information from web service");
        }
        #endregion

        private TcpState GetConnectionState(TcpClient client)
        {
            TcpState stateOfConnection = new TcpState();
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(client.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(client.Client.RemoteEndPoint)).ToArray();

            if (tcpConnections != null && tcpConnections.Length > 0)
            {
                return stateOfConnection = tcpConnections.First().State;
            }
            else
            {
                TcpState noState = TcpState.Closed;
                return noState;
            }
        }
        /// <summary>
        /// this is the Main event - when the muse sends a QRY^Q01 message the message will get here
        /// then the patient details are fetched from meuhedet web service
        /// and the system will compose an ADT^A19 response message and will send it back to the MUSE.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void Server_MessageRecieved(object sender, MessageRecievedEventArgs e)
        {
            logger.Trace("entered Server_MessageRecieved");
            logger.Debug($"inside hl7mgr.Server_MessageRecieved connection state {GetConnectionState(e.SourceClient)}");
            string ResponseMessage = string.Empty;
            var messageType = hl7MessageParser.GetMessageType(e.Message);

            logger.Debug("************************************************************");
            logger.Debug($"HL7 Query Message Recieved from {e.SourceClient.Client.RemoteEndPoint.ToString()}");
            logger.Trace($"inside hl7mgr.Server_MessageRecieved connection state {GetConnectionState(e.SourceClient)}");
            logger.Debug($"{ Environment.NewLine}{e.Message}");
            lock (locker)
            {
                var SiteId = hl7MessageParser.GetSiteID(e.Message);
                var MessageDateTime = hl7MessageParser.GetMessageDateTime(e.Message);
                var MessageControlId = hl7MessageParser.GetMessageControlId(e.Message);
                logger.Info($"Message control id = {MessageControlId}");

                //TODO : add support to handle other messages ?




                // we can only handle  QRY^Q01 messages 
                if (messageType.ToUpper() != "QRY^Q01")
                {
                    NlogHelper.CreateLogEntry("Message recieved was not QRY^Q01 ! ", "800", LogLevel.Error, logger);
                    ResponseMessage = MessageComposer.GetApplicationErrorMessage(SiteId, MessageControlId);
                    return;
                }

                //keep the original pid from the ecg machine for the response message
                string PID = hl7MessageParser.GetPatientId(e.Message);
                string originalPID = PID;


                var parsedIds = _data.PidHandler.ParseID(PID);

                e.PID = PID;
                MessageRecived?.Invoke(this, e);

                
                var patientInformation = GetPatientDemographicInformation(parsedIds);


                if (null != patientInformation)
                {
                    logger.Info(
                        $"Success ! Got patient info from web service for patient id {patientInformation.PatientId} ");
                    logger.Info($"Data  :  First name : {patientInformation.FirstName} , last name : {patientInformation.LastName} ,pid {originalPID}", source);

                    if (_config.MustGetTranslation)
                    {

                        logger.Info($"Trying to translate to english ... {patientInformation.FirstName} , {patientInformation.LastName}", source);

                        var patientNamesData = tManager.GetEnglishName(patientInformation.FirstName, patientInformation.LastName);

                        patientInformation.FirstName = patientNamesData.EnglishFirstName ?? " ";
                        patientInformation.LastName = patientNamesData.EnglishLastName ?? " ";

                        if (patientNamesData.EnglishFirstName == patientNamesData.EnglishLastName && patientNamesData.EnglishLastName == string.Empty)
                        {
                            if (IsEnglishName(patientNamesData.HebrewFirstName))
                            {
                                patientInformation.FirstName = patientNamesData.HebrewFirstName;
                            }
                            if (IsEnglishName(patientNamesData.HebrewLastName))
                            {
                                patientInformation.LastName = patientNamesData.HebrewLastName;
                            }
                        }
                        logger.Info($"English Name : {patientInformation.FirstName} , {patientInformation.LastName}  ", source);
                    }

                    logger.Info($"Date Of birtn  :  {patientInformation.DOB}");

                    //compose a good response message ( hl7)
                    ResponseMessage = MessageComposer.GetA019Message
                        (
                            originalPID,
                            SiteId,
                            patientInformation.FirstName,
                            patientInformation.LastName,
                            patientInformation.Gender,
                            Helper.FormatDateOfBirth(patientInformation.DOB),
                            MessageControlId,
                            patientInformation.Height,
                            patientInformation.Weight
                        );
                }


                // no patient information on the server not an israeli one nor a non israeli one
                else
                {
                    NlogHelper.CreateLogEntry($"Web Service Returned an error when trying to get data for patient id : {originalPID} ", "900", LogLevel.Error, logger);
                    ResponseMessage = MessageComposer.GetApplicationErrorMessage(SiteId, MessageControlId);
                }
                lock (locker)
                {
                    Server.SendResponse(ResponseMessage, e.SourceStream, e.SourceClient);
                    MessageSent?.Invoke(this, new MessageSentEventArgs() { Message = ResponseMessage, PID = originalPID });
                }
                //send the response back to muse

                logger.Trace("left Server_MessageRecieved");
            }


        }

        private bool IsEnglishName(string data)
        {
            int counter = 0;
            string englishLetters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            foreach (var letter in data)
            {
                if (englishLetters.Contains(letter))
                {
                    counter++;
                    if (counter > 2)
                    {
                        return true;
                    }
                }
            }
            return false;

        }

        private CompletePatientInformation GetPatientDemographicInformation(PatientId[] parsedIds)
        {
            CompletePatientInformation IsraeliCustomer;
            CompletePatientInformation forigenCustomer;
            try
            {
                if (parsedIds[0] != null)
                {
                    logger.Info(
                    $"Getting patient information for  patient id {parsedIds[0].ID} type {parsedIds[0].SugId} and checksum digit {parsedIds[0].SifratBikuret} ");
                }
                // get patient information from web service

                //todo: entry point to get patient informatoion from webservice
                IsraeliCustomer = WebServiceClient.GetPatientInfo(parsedIds[0]);

                if (null == IsraeliCustomer || (IsraeliCustomer.ResponseStatus != _config.GoodSoapResponseErrorCode && parsedIds[1] != null))
                {
                    IsraeliCustomer = null;
                    if (parsedIds[1] != null)
                    {
                        logger.Info(
                    $"Getting patient information for  patient id {parsedIds[1].ID} type {parsedIds[1].SugId} and checksum digit {parsedIds[1].SifratBikuret} ");
                        // get patient information from web service
                        forigenCustomer = WebServiceClient.GetPatientInfo(parsedIds[1]);
                        if (null == forigenCustomer || forigenCustomer.ResponseStatus != _config.GoodSoapResponseErrorCode)
                        {
                            return null;
                        }
                        else
                        {

                            return forigenCustomer;
                        }
                    }

                }
                else
                {
                    return IsraeliCustomer;
                }

                return null;

            }
            catch (Exception ex)
            {
                exceptionFromWebService = ex;
                logger.Debug("Error Getting data from web service :", source);
                logger.Debug($"{ex.Message} :");
                logger.Debug(ex.Message, ex, null);
                return null;
            }
        }
    }

}

