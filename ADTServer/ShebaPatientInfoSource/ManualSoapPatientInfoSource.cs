using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn;
using AdtSvrCmn.Objects;
using System.Net;
using System.IO;
using AdtSvrCmn.EventArguments;
using System.Configuration;

namespace ShebaPatientInfoSource
{
    public class ManualSoapPatientInfoSource : IPatientInfoSoapSource
    {
        Logger logger;
        ApplicationConfiguration _config;


        public ManualSoapPatientInfoSource()
        {

        }
        public ManualSoapPatientInfoSource(ApplicationConfiguration config)
        {
            _config = config;
            logger = LogManager.GetCurrentClassLogger();
        }

        public event EventHandler RequestingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler ErrorGettingPatientInfo;


        public CompletePatientInformation GetPatientInfo(string patientId, string pidType)
        {
            logger.Trace("Inside GetPatientInfo");
            // prepare objects
            CompletePatientInformation patientInfo = new CompletePatientInformation();
            byte[] buffer = new byte[100000];
            string responseMessage = string.Empty;

            // Create request
            string requestMessage = BuildRequestMessage(patientId);
            WebRequest request = GetWebRequest(_config.SoapRequestTarget, requestMessage.Length);
            logger.Info($"Sending request for patient Data :  patient id {pidType}{patientId}");
            logger.Debug(requestMessage);


            //Send Request to web service
            try
            {
                var requestStream = request.GetRequestStream();
                StreamWriter sw = new StreamWriter(requestStream);
                sw.Write(requestMessage);
                sw.Flush();
            }
            catch (Exception ex)
            {
                NlogHelper.CreateLogEntry(ex.Message, "100", LogLevel.Error, logger);
                return null;
            }

            //Get response from web service

            try
            {
                using (var response = request.GetResponse())
                {

                    var st = response.GetResponseStream();
                    st.Read(buffer, 0, buffer.Length);
                    responseMessage = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

                    RecevingPatientInfo?.Invoke(this, new PatientInfoEventArgs() { Message = responseMessage });
                }
            }
            catch (Exception ex)
            {
                ErrorGettingPatientInfo?.Invoke(this, new PatientInfoEventArgs() { Message = ex.Message });
                NlogHelper.CreateLogEntry(ex.Message, "200", LogLevel.Error, logger);
                return null;
            }
           
            patientInfo = ParseResponse(responseMessage);
            return patientInfo;
        }
        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Info("entered GetPatientInfo(PatientId patientId) ");
            if (patientId != null)
            {
                return GetPatientInfo(patientId.ID, patientId.SugId);
            }
            return null;

        }

        #region Helpers
        private CompletePatientInformation ParseResponse(string responseMessage)
        {
            CompletePatientInformation patientInfo = new CompletePatientInformation();

            var statusMessage = GetDataFromMW300D(responseMessage, "Errstatus");
            var severity = GetDataFromMW300D(responseMessage, "Errstatus");

            var status = GetDataFromMW300D(responseMessage, "Errstatus");
            var firstName = GetDataFromMW300D(responseMessage, "Engpname").TrimEnd(' ').TrimStart(' ');
            var lastName = GetDataFromMW300D(responseMessage, "Engfname").TrimEnd(' ').TrimStart(' ');
            var gender = GetDataFromMW300D(responseMessage, "Sex");
            var patientId = GetDataFromMW300D(responseMessage, "Patientid");
            var weight = "";
            var height = "";

            switch (gender)
            {
                case "ז":
                    gender = "M";
                    break;
                case "נ":
                    gender = "F";
                    break;
                default:
                    gender = "";
                    break;
            }
            var dateOfBirth = GetDataFromMW300D(responseMessage, "Birthdate");

            var birthYear =int.Parse( dateOfBirth.Substring(0, 4));
            var birthMonth= int.Parse( dateOfBirth.Substring(4, 2));
            var birthDay = int.Parse(dateOfBirth.Substring(6, 2));

            DateTime dob = new DateTime(birthYear, birthMonth, birthDay);



            patientInfo.Age = (DateTime.Now.Year - dob.Year).ToString();
            int ageNumber = int.Parse(patientInfo.Age);
            patientInfo.ResponseStatusMessage = statusMessage;
            patientInfo.Severity = severity.Trim();
            patientInfo.FirstName = firstName;
            patientInfo.LastName = lastName;
            patientInfo.Gender = gender;
            patientInfo.GenderDesc = gender == "M" ? "Male" : "Female";
            patientInfo.PatientId = patientId;
            patientInfo.ResponseStatus = status;
            patientInfo.Height = height;
            patientInfo.Weight = weight;
            patientInfo.DOB = (new DateTime(birthYear,birthMonth,birthDay)).ToString();

            if (patientInfo.ResponseStatus != "500")
            {
                logger.Error(patientInfo.ResponseStatus);
            }



            return patientInfo;
        }

        private WebRequest GetWebRequest(string target, int contentLength)
        {
            var request = WebRequest.Create(_config.SoapRequestTarget);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.ContentLength = contentLength;
            request.Headers.Add("SOAPAction: \"GetPatientDetails\"");
            return request;
        }

        private string GetDataFromMW300D(string message, string itemToFind)
        {
            itemToFind = itemToFind.ToLower();
            int start = message.IndexOf($"<{itemToFind}>") + $"<{itemToFind}>".Length;
            int end = message.IndexOf($"</{itemToFind}>");
            if (start != -1 && end != -1)
            {
                string data = message.Substring(start, end - start);
                return data;
            }
            else
            {
                return string.Empty;
            }

        }

        public string BuildRequestMessage(string pid)
        {
            string userName = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            string sendingApp = ConfigurationManager.AppSettings["SendingApp"];
            string runningUserName = ConfigurationManager.AppSettings["RunningUserName"];



            StringBuilder sb = new StringBuilder();

            sb.Append("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

            sb.Append("<s:Header>");

            sb.Append("<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">");
            sb.Append("<UsernameToken>");
            sb.Append($"<Username>{userName}</Username>");
            sb.Append($"<Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">{password}</Password>");
            sb.Append("</UsernameToken>");
            sb.Append("</Security>");

            sb.Append("</s:Header>");

            sb.Append($"<s:Body>");
            sb.Append("<GetPatientDetails xmlns=\"http://tempuri.org\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.Append($"<SendingApp>{sendingApp}</SendingApp>");
            sb.Append("<DataSource>MF</DataSource>");
            sb.Append("<QueryType>ImutID</QueryType>");
            sb.Append($"<PatientID>{pid}</PatientID>");
            sb.Append("<BirthDate>18081965</BirthDate>");
            sb.Append($"<AppUserName>{runningUserName}</AppUserName>");
            sb.Append("</GetPatientDetails>");
            sb.Append("</s:Body>");
            sb.Append("</s:Envelope>");




            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CustumerId"></param>
        /// <returns></returns>
        [Obsolete("Do Not Use - Will Throw An Exception when used")]
        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            throw new NotImplementedException();
        }

        private void LogEvent(LogLevel level, int eventId, string message)
        {
            LogEventInfo logEventInfo = new LogEventInfo();
            logEventInfo.Level = level;
            logEventInfo.Properties.Add("EventID", eventId);
            logEventInfo.Message = message;
            logger.Log(logEventInfo);

        }
        #endregion
    }
}
