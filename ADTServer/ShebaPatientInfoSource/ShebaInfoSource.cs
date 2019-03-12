
using AdtSvrCmn;
using AdtSvrCmn.EventArguments;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace ShebaPatientInfoSource
{
    public class ShebaInfoSource : IPatientInfoSoapSource
    {
        private Logger logger;
        private ApplicationConfiguration _config;

        public ShebaInfoSource()
        {
        }

        public ShebaInfoSource(ApplicationConfiguration config)
        {
            this._config = config;
            this.logger = LogManager.GetCurrentClassLogger();
        }

        public event EventHandler RequestingPatientInfo;

        public event EventHandler RecevingPatientInfo;

        public event EventHandler ErrorGettingPatientInfo;

        public CompletePatientInformation GetPatientInfo(string patientId, string pidType)
        {
            this.logger.Trace("Inside GetPatientInfo");
            CompletePatientInformation patientInformation = new CompletePatientInformation();
            byte[] buffer = new byte[100000];
            string responseMessage = string.Empty;
            string request = BuildRequestMessage(patientId);
            WebRequest webRequest = GetWebRequest(_config.SoapRequestTarget, request.Length);
            logger.Info("Sending request for patient Data :  patient id " + pidType + patientId);
            logger.Debug(request);
            try
            {
                this.logger.Debug("Sending Request....");
                StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
                streamWriter.Write(request);
                streamWriter.Flush();
                this.logger.Debug("Request Sent...");
            }
            catch (Exception ex)
            {
                this.logger.Debug("Cannot sent request to ewb Service");
                NlogHelper.CreateLogEntry(ex.Message, "100", (LogLevel)LogLevel.Error, this.logger);
                return (CompletePatientInformation)null;
            }
            try
            {
                logger.Debug("Trying to get response...");
                using (WebResponse response = webRequest.GetResponse())
                {
                    response.GetResponseStream().Read(buffer, 0, buffer.Length);
                    responseMessage = Encoding.UTF8.GetString(buffer).TrimEnd(new char[1]);
                    EventHandler recevingPatientInfo = RecevingPatientInfo;
                    if (recevingPatientInfo != null)
                    {
                        PatientInfoEventArgs patientInfoEventArgs = new PatientInfoEventArgs();
                        patientInfoEventArgs.Message = (responseMessage);
                        recevingPatientInfo((object)this, (EventArgs)patientInfoEventArgs);
                    }
                    this.logger.Debug("Successfully got response !");
                }
            }
            catch (Exception ex)
            {
                this.logger.Debug("Error getting the response from ws!");
                EventHandler gettingPatientInfo = this.ErrorGettingPatientInfo;
                if (gettingPatientInfo != null)
                {
                    PatientInfoEventArgs patientInfoEventArgs = new PatientInfoEventArgs();
                    patientInfoEventArgs.Message = (ex.Message);
                    gettingPatientInfo((object)this, (EventArgs)patientInfoEventArgs);
                }
                NlogHelper.CreateLogEntry(ex.Message, "200", (LogLevel)LogLevel.Error, this.logger);
                return (CompletePatientInformation)null;
            }
            return ParseResponse(responseMessage);
        }

        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            this.logger = LogManager.GetCurrentClassLogger();
            this.logger.Info("entered GetPatientInfo(PatientId patientId) ");
            if (patientId != null)
                return this.GetPatientInfo(patientId.ID, patientId.SugId);
            return (CompletePatientInformation)null;
        }

        private CompletePatientInformation ParseResponse(
          string responseMessage)
        {
            CompletePatientInformation patientInformation = new CompletePatientInformation();
            string dataFromMw300D1 = this.GetDataFromMW300D(responseMessage, "Errstatus");
            string dataFromMw300D2 = this.GetDataFromMW300D(responseMessage, "Errstatus");
            string dataFromMw300D3 = this.GetDataFromMW300D(responseMessage, "Errstatus");
            string str1 = this.GetDataFromMW300D(responseMessage, "Engpname").TrimEnd(' ').TrimStart(' ');
            string str2 = this.GetDataFromMW300D(responseMessage, "Engfname").TrimEnd(' ').TrimStart(' ');
            string dataFromMw300D4 = this.GetDataFromMW300D(responseMessage, "Sex");
            string dataFromMw300D5 = this.GetDataFromMW300D(responseMessage, "Patientid");
            string str3 = "";
            string str4 = "";
            string str5 = dataFromMw300D4 == "ז" ? "M" : (dataFromMw300D4 == "נ" ? "F" : "");

            patientInformation.DOB = SetDateOfBirth(responseMessage);
            patientInformation.Age = SetAge(responseMessage, patientInformation.DOB);


            int.Parse(patientInformation.Age);
            patientInformation.ResponseStatusMessage = (dataFromMw300D1);
            patientInformation.Severity = (dataFromMw300D2.Trim());
            patientInformation.FirstName = (str1);
            patientInformation.LastName = (str2);
            patientInformation.Gender = (str5);
            patientInformation.GenderDesc = (str5 == "M" ? "Male" : "Female");
            patientInformation.PatientId = (dataFromMw300D5);
            patientInformation.ResponseStatus = (dataFromMw300D3);
            patientInformation.Height = (str4);
            patientInformation.Weight = (str3);

            if (patientInformation.ResponseStatus != "500")
                this.logger.Error(patientInformation.ResponseStatus);
            return patientInformation;
        }

        private string SetAge(string responseMessage, string dateOfBirth)
        {
            return "1";
        }

        private string SetDateOfBirth(string responseMessage)
        {
            string dataFromMw300D6 = this.GetDataFromMW300D(responseMessage, "Birthdate");
            int year;
            int month;
            int day;

            try
            {
                string y = dataFromMw300D6.Substring(0, 4);
                year = int.Parse(y);
            }
            catch (Exception)
            {

                year = 1800;
            }
            try
            {
                string m = dataFromMw300D6.Substring(4, 2);
                month = int.Parse(m);
                if (month == 0)
                {
                    month = 1;
                }
            }
            catch (Exception)
            {
                month = 1;
            }
            try
            {
                day = int.Parse(dataFromMw300D6.Substring(6, 2));
                if (day == 0)
                {
                    day = 1;
                }
            }
            catch (Exception)
            {

                day = 1;
            }
            DateTime dateTime = new DateTime(year, month, day);
            return dateTime.ToString();


            //patientInformation.Age = ((DateTime.Now.Year - dateTime.Year).ToString());
            //patientInformation.DOB = (new DateTime(year, month, day).ToString());
        }

        private WebRequest GetWebRequest(string target, int contentLength)
        {
            WebRequest webRequest = WebRequest.Create(this._config.SoapRequestTarget);
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml; charset=utf-8";
            webRequest.ContentLength = (long)contentLength;
            webRequest.Headers.Add("SOAPAction: \"GetPatientDetails\"");
            return webRequest;
        }

        private string GetDataFromMW300D(string message, string itemToFind)
        {
            this.logger.Debug("getting item " + itemToFind + " from response");
            itemToFind = itemToFind.ToLower();
            int startIndex = message.IndexOf("<" + itemToFind + ">") + ("<" + itemToFind + ">").Length;
            int num = message.IndexOf("</" + itemToFind + ">");
            if (startIndex != -1 && num != -1)
            {
                string str = message.Substring(startIndex, num - startIndex);
                this.logger.Debug("Got data for item " + itemToFind + " : " + str);
                return str;
            }
            this.logger.Debug("Error getting daya for item " + itemToFind);
            return string.Empty;
        }

        public string BuildRequestMessage(string pid)
        {
            string appSetting1 = ConfigurationManager.AppSettings["UserName"];
            string appSetting2 = ConfigurationManager.AppSettings["Password"];
            string appSetting3 = ConfigurationManager.AppSettings["SendingApp"];
            string appSetting4 = ConfigurationManager.AppSettings["RunningUserName"];
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            stringBuilder.Append("<s:Header>");
            stringBuilder.Append("<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">");
            stringBuilder.Append("<UsernameToken>");
            stringBuilder.Append("<Username>" + appSetting1 + "</Username>");
            stringBuilder.Append("<Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">" + appSetting2 + "</Password>");
            stringBuilder.Append("</UsernameToken>");
            stringBuilder.Append("</Security>");
            stringBuilder.Append("</s:Header>");
            stringBuilder.Append("<s:Body>");
            stringBuilder.Append("<GetPatientDetails xmlns=\"http://tempuri.org\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">");
            stringBuilder.Append("<SendingApp>" + appSetting3 + "</SendingApp>");
            stringBuilder.Append("<DataSource>MF</DataSource>");
            stringBuilder.Append("<QueryType>ImutID</QueryType>");
            stringBuilder.Append("<PatientID>" + pid + "</PatientID>");
            stringBuilder.Append("<BirthDate>18081965</BirthDate>");
            stringBuilder.Append("<AppUserName>" + appSetting4 + "</AppUserName>");
            stringBuilder.Append("</GetPatientDetails>");
            stringBuilder.Append("</s:Body>");
            stringBuilder.Append("</s:Envelope>");
            return stringBuilder.ToString();
        }

        [Obsolete("Do Not Use - Will Throw An Exception when used")]
        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            throw new NotImplementedException();
        }

        //private void LogEvent(LogLevel level, int eventId, string message)
        //{
        //    LogEventInfo logEventInfo = new LogEventInfo();
        //    logEventInfo.set_Level(level);
        //    logEventInfo.get_Properties().Add((object)"EventID", (object)eventId);
        //    logEventInfo.set_Message(message);
        //    this.logger.Log(logEventInfo);
        //}
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NLog;
//using AdtSvrCmn.Interfaces;
//using AdtSvrCmn;
//using AdtSvrCmn.Objects;
//using System.Net;
//using System.IO;
//using AdtSvrCmn.EventArguments;
//using System.Configuration;
//using System.Xml.Serialization;

//namespace ShebaPatientInfoSource
//{
//    public class ShebaInfoSource : IPatientInfoSoapSource
//    {
//        Logger logger;
//        ApplicationConfiguration _config;


//        public ShebaInfoSource()
//        {

//        }
//        public ShebaInfoSource(ApplicationConfiguration config)
//        {
//            _config = config;
//            logger = LogManager.GetCurrentClassLogger();
//        }

//        public event EventHandler RequestingPatientInfo;
//        public event EventHandler RecevingPatientInfo;
//        public event EventHandler ErrorGettingPatientInfo;


//        public CompletePatientInformation GetPatientInfo(string patientId, string pidType)
//        {
//            logger.Trace("Inside GetPatientInfo");
//            // prepare objects
//            CompletePatientInformation patientInfo = new CompletePatientInformation();
//            byte[] buffer = new byte[100000];
//            string responseMessage = string.Empty;

//            // Create request
//            string requestMessage = BuildRequestMessage(patientId);
//            WebRequest request = GetWebRequest(_config.SoapRequestTarget, requestMessage.Length);
//            logger.Info($"Sending request for patient Data :  patient id {pidType}{patientId}");
//            logger.Debug(requestMessage);


//            //Send Request to web service
//            try
//            {
//                logger.Debug("Sending Request....");
//                var requestStream = request.GetRequestStream();
//                StreamWriter sw = new StreamWriter(requestStream);
//                sw.Write(requestMessage);
//                sw.Flush();
//                logger.Debug("Request Sent...");
//            }
//            catch (Exception ex)
//            {
//                logger.Debug("Cannot sent request to ewb Service");
//                NlogHelper.CreateLogEntry(ex.Message, "100", LogLevel.Error, logger);
//                return null;
//            }

//            //Get response from web service

//            try
//            {
//                logger.Debug("Trying to get response...");
//                using (var response = request.GetResponse())
//                {

//                    var st = response.GetResponseStream();
//                    st.Read(buffer, 0, buffer.Length);
//                    responseMessage = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

//                    RecevingPatientInfo?.Invoke(this, new PatientInfoEventArgs() { Message = responseMessage });
//                    logger.Debug("Successfully got response !");
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Debug("Error getting the response from ws!");
//                ErrorGettingPatientInfo?.Invoke(this, new PatientInfoEventArgs() { Message = ex.Message });
//                NlogHelper.CreateLogEntry(ex.Message, "200", LogLevel.Error, logger);
//                return null;
//            }

//            patientInfo = ParseResponse(responseMessage);
//            return patientInfo;
//        }
//        public CompletePatientInformation GetPatientInfo(PatientId patientId)
//        {
//            logger = LogManager.GetCurrentClassLogger();
//            logger.Info("entered GetPatientInfo(PatientId patientId) ");
//            if (patientId != null)
//            {
//                return GetPatientInfo(patientId.ID, patientId.SugId);
//            }
//            return null;

//        }

//        #region Helpers
//        private CompletePatientInformation ParseResponse(string responseMessage)
//        {
//            CompletePatientInformation patientInfo = new CompletePatientInformation();

//            XmlSerializer xs = new XmlSerializer(typeof(ShebaDemographicsProxy.GetPatientDetailsResponse));
//            StringReader sr = new StringReader(responseMessage);
//            StringBuilder sb = new StringBuilder();
//            StringWriter sw = new StringWriter(sb);

//            xs.Serialize(sw, new ShebaDemographicsProxy.GetPatientDetailsResponse());

//            var obj = xs.Deserialize(sr) as ShebaDemographicsProxy.GetPatientDetailsResponse;


//            var statusMessage = GetDataFromMW300D(responseMessage, "Errstatus");
//            var severity = GetDataFromMW300D(responseMessage, "Errstatus");

//            var status = GetDataFromMW300D(responseMessage, "Errstatus");
//            var firstName = GetDataFromMW300D(responseMessage, "Engpname").TrimEnd(' ').TrimStart(' ');
//            var lastName = GetDataFromMW300D(responseMessage, "Engfname").TrimEnd(' ').TrimStart(' ');
//            var gender = GetDataFromMW300D(responseMessage, "Sex");
//            var patientId = GetDataFromMW300D(responseMessage, "Patientid");
//            var weight = "";
//            var height = "";

//            switch (gender)
//            {
//                case "ז":
//                    gender = "M";
//                    break;
//                case "נ":
//                    gender = "F";
//                    break;
//                default:
//                    gender = "";
//                    break;
//            }
//            var dateOfBirth = GetDataFromMW300D(responseMessage, "Birthdate");


//            int birthYear;
//            int birthMonth;
//            int birthDay;
//            try
//            {
//                birthYear = int.Parse(dateOfBirth.Substring(0, 4));
//            }
//            catch (Exception)
//            {

//                birthYear = 1800;
//            }
//            try
//            {
//                birthMonth = int.Parse(dateOfBirth.Substring(4, 2));
//            }
//            catch (Exception)
//            {

//                birthMonth = 1;
//            }

//            try
//            {
//                birthDay = int.Parse(dateOfBirth.Substring(6, 2));
//            }
//            catch (Exception)
//            {

//                birthDay = 1;
//            }

//            DateTime dob = new DateTime(birthYear, birthMonth, birthDay);



//            patientInfo.Age = (DateTime.Now.Year - dob.Year).ToString();
//            int ageNumber = int.Parse(patientInfo.Age);
//            patientInfo.ResponseStatusMessage = statusMessage;
//            patientInfo.Severity = severity.Trim();
//            patientInfo.FirstName = firstName;
//            patientInfo.LastName = lastName;
//            patientInfo.Gender = gender;
//            patientInfo.GenderDesc = gender == "M" ? "Male" : "Female";
//            patientInfo.PatientId = patientId;
//            patientInfo.ResponseStatus = status;
//            patientInfo.Height = height;
//            patientInfo.Weight = weight;
//            patientInfo.DOB = (new DateTime(birthYear, birthMonth, birthDay)).ToString();

//            if (patientInfo.ResponseStatus != "500")
//            {
//                logger.Error(patientInfo.ResponseStatus);
//            }



//            return patientInfo;
//        }

//        private void Xs_UnknownNode(object sender, XmlNodeEventArgs e)
//        {
//            var el = e.Name;
//        }

//        private void Xs_UnknownAttribute(object sender, XmlAttributeEventArgs e)
//        {
//            var el = e.Attr; ;
//        }

//        private void Xs_UnknownElement(object sender, XmlElementEventArgs e)
//        {
//            var el = e.Element;
//        }

//        private WebRequest GetWebRequest(string target, int contentLength)
//        {
//            var request = WebRequest.Create(_config.SoapRequestTarget);
//            request.Method = "POST";
//            request.ContentType = "text/xml; charset=utf-8";
//            request.ContentLength = contentLength;
//            request.Headers.Add("SOAPAction: \"GetPatientDetails\"");
//            return request;
//        }

//        private string GetDataFromMW300D(string message, string itemToFind)
//        {
//            logger.Debug($"getting item {itemToFind} from response");
//            itemToFind = itemToFind.ToLower();
//            int start = message.IndexOf($"<{itemToFind}>") + $"<{itemToFind}>".Length;
//            int end = message.IndexOf($"</{itemToFind}>");
//            if (start != -1 && end != -1)
//            {
//                string data = message.Substring(start, end - start);
//                logger.Debug($"Got data for item {itemToFind} : {data}");
//                return data;
//            }
//            else
//            {
//                logger.Debug($"Error getting daya for item {itemToFind}");
//                return string.Empty;
//            }

//        }

//        public string BuildRequestMessage(string pid)
//        {
//            string userName = ConfigurationManager.AppSettings["UserName"];
//            string password = ConfigurationManager.AppSettings["Password"];
//            string sendingApp = ConfigurationManager.AppSettings["SendingApp"];
//            string runningUserName = ConfigurationManager.AppSettings["RunningUserName"];



//            StringBuilder sb = new StringBuilder();

//            sb.Append("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");

//            sb.Append("<s:Header>");

//            sb.Append("<Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">");
//            sb.Append("<UsernameToken>");
//            sb.Append($"<Username>{userName}</Username>");
//            sb.Append($"<Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">{password}</Password>");
//            sb.Append("</UsernameToken>");
//            sb.Append("</Security>");

//            sb.Append("</s:Header>");

//            sb.Append($"<s:Body>");
//            sb.Append("<GetPatientDetails xmlns=\"http://tempuri.org\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">");
//            sb.Append($"<SendingApp>{sendingApp}</SendingApp>");
//            sb.Append("<DataSource>MF</DataSource>");
//            sb.Append("<QueryType>ImutID</QueryType>");
//            sb.Append($"<PatientID>{pid}</PatientID>");
//            sb.Append("<BirthDate>18081965</BirthDate>");
//            sb.Append($"<AppUserName>{runningUserName}</AppUserName>");
//            sb.Append("</GetPatientDetails>");
//            sb.Append("</s:Body>");
//            sb.Append("</s:Envelope>");




//            return sb.ToString();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="CustumerId"></param>
//        /// <returns></returns>
//        [Obsolete("Do Not Use - Will Throw An Exception when used")]
//        public CompletePatientInformation GetPatientInfo(string CustumerId)
//        {
//            throw new NotImplementedException();
//        }

//        private void LogEvent(LogLevel level, int eventId, string message)
//        {
//            LogEventInfo logEventInfo = new LogEventInfo();
//            logEventInfo.Level = level;
//            logEventInfo.Properties.Add("EventID", eventId);
//            logEventInfo.Message = message;
//            logger.Log(logEventInfo);

//        }
//        #endregion
//    }
//}
