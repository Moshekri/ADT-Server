using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using ClalitWebServiceDataClient.ClalitPatientNameService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.ServiceModel;
using System.Net;
using System.Configuration;

namespace ClalitWebServiceDataClient
{
    public class ClalitPatientInfoSource : IPatientInfoSoapSource
    {
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;
        Logger logger;
        public ClalitPatientInfoSource()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            logger.Debug("Loading Clalit webService Client");
            CompletePatientInformation patientInformation = new CompletePatientInformation();
            
       

            using (var client = new TranslationPatientNamesServiceClient("TranslationPatientNamesService"))
            {

                string username = ConfigurationManager.AppSettings["user"];
                string password = ConfigurationManager.AppSettings["password"];
                string domain = ConfigurationManager.AppSettings["domain"];

                client.ClientCredentials.UserName.UserName = username;
                client.ClientCredentials.UserName.Password = password;
                client.ClientCredentials.Windows.ClientCredential.UserName = username;
                client.ClientCredentials.Windows.AllowNtlm = true;
                client.ClientCredentials.Windows.ClientCredential.Domain = domain;
                client.ClientCredentials.Windows.ClientCredential.Password = password;
                var s = new System.Security.SecureString();
                foreach (var item in password)
                {
                    s.AppendChar(item);
                }
                client.ClientCredentials.Windows.ClientCredential.SecurePassword = s;






                TranslationPatientNamesRequest request = new TranslationPatientNamesRequest();
                TranslationPatientNamesResponse response;
                
                request.MessageInfo = new TranslationPatientNamesMessageInfo();

                request.MessageInfo.RequestDatetime = DateTime.Now;
                logger.Debug($"Request date/time = {request.MessageInfo.RequestDatetime}");

                request.MessageInfo.RequestID = Guid.NewGuid().ToString();
                logger.Debug($"Request id : {request.MessageInfo.RequestID}");

                request.MessageInfo.RequestingApplication = 670;
                logger.Debug($"Requesting application : {request.MessageInfo.RequestingApplication}");

                request.MessageInfo.ServingApplication = 10;

                request.MessageInfo.ServingSite = 120;
                request.MessageInfo.RequestingSite = 120;



                request.Parameters = new TranslationPatientNamesParameters();

                long patientId;

                if (long.TryParse(CustumerId, out patientId))
                {
                    request.Parameters.PatientID = patientId;
                    logger.Debug($"Request information for PID : {patientId}");
                }
                else
                {
                    logger.Debug($"Patient ID Not in currect Format (was {CustumerId})");
                    logger.Error($"Patient ID Not in currect Format (was {CustumerId})");
                    throw new ArgumentException($"Patient ID Not in currect Format (was {CustumerId})");
                }

               
                try
                {
                    logger.Debug("Sending request...");
         
                     response = client.TranslationPatientNamesQuery(request);
                   
                }
                catch (Exception ex)
                {

                    throw;
                }

                logger.Debug("Recived response : " +
                    $"Response status code : {response.StatusCode}" +
                    $"Response status descreption : {response.StatusDescription}" +
                    $"Request id  :  {response.MessageInfo.RequestID}");
                    
                patientInformation.DOB = response.Results.BirthDate.ToShortDateString();
                patientInformation.FirstName = response.Results.FirstNameEng;
                patientInformation.LastName = response.Results.LastNameEng;
                patientInformation.Age = (DateTime.Now.Year - response.Results.BirthDate.Year).ToString();
                patientInformation.Gender = response.Results.GenderID.ToString() + "/" + response.Results.GenderIDSpecified.ToString();
                patientInformation.ResponseStatus = response.StatusCode.ToString() ;
                patientInformation.ResponseStatusMessage = response.StatusDescription;
                patientInformation.PatientId = response.Results.PatientID.ToString() ;
                
                logger.Debug($" Patient id : {patientInformation.PatientId} DOB {patientInformation.DOB} , First Name {patientInformation.FirstName},Last Name{patientInformation.LastName},Age {patientInformation.Age} , Gender {patientInformation.Gender}");
                return patientInformation;
            }
        }

        public CompletePatientInformation GetPatientInfo(string CustumerId, string pidType)
        {
            throw new NotImplementedException();
        }

        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
         return   GetPatientInfo(patientId.ID);
        }
    }


    

}
