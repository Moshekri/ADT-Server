using AdtSvrCmn;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using System;
using System.Text;
using AdtSvrCmn.EventArguments;
using System.Net;
using System.IO;
using IsraeliIdTools;
using NLog;

namespace LeumitWebServiceDataClient
{
    public class LeumitPatientInfoClient : IPatientInfoSoapSource
    {
        Logger logger;
        ApplicationConfiguration _config;


        public LeumitPatientInfoClient()
        {

        }
        public LeumitPatientInfoClient(ApplicationConfiguration config)
        {
            _config = config;
            logger = LogManager.GetLogger("LeumitWebServiceDataClient");
        }

        public event EventHandler RequestingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler ErrorGettingPatientInfo;


        public CompletePatientInformation GetPatientInfo(string patientId, string pidType)
        {
            logger.Trace("Inside GetPatientInfo");
            // prepare objects
            CompletePatientInformation patientInfo = new CompletePatientInformation();
            byte[] buffer = new byte[10000];
            string responseMessage = string.Empty;

            // Create request
            string requestMessage = BuildRequestMessage(patientId, pidType);
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
                NlogHelper.CreateLogEntry( ex.Message,"100", LogLevel.Error, logger);
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
                NlogHelper.CreateLogEntry(ex.Message,"200", LogLevel.Error, logger);
                return null;
            }
            var sevirity = GetDataFromMW300D(responseMessage, "PGL_SEVERITY");
            if (sevirity != "0")
            {
                var errorMessage = GetDataFromMW300D(responseMessage, "wsrcmsg");
                if (errorMessage.ToLower().Contains("exception") || errorMessage.ToLower().Contains("error"))
                {
                    NlogHelper.CreateLogEntry(pidType + patientId + ":" + errorMessage ,"300",LogLevel.Error, logger );
                    return null;
                }
                string errMsg = GetDataFromMW300D(responseMessage, "PGL_MESSAGE");
                if (errMsg.ToUpper().Contains("DVL0011"))
                {
                    LogEvent(LogLevel.Error, 400, pidType + patientId + ":" + "Patient does not exist on file" + Environment.NewLine + "המבוטח אינו קיים בקובץ מבוטחים");
                    return null;
                }
                else if (errMsg.ToUpper().Contains("DVL0012"))
                {
                    LogEvent(LogLevel.Error, 500, pidType + patientId + ":" + "Patient is not eligible to recieve treatment" + Environment.NewLine + "מבוטח לא זכאי לטיפול");
                    return null;
                }
                else if (errMsg.ToUpper().Contains("DVL"))
                {
                    LogEvent(LogLevel.Error, 600, pidType + patientId + ":" + errMsg);
                    return null;
                }
                //Construct CompletePatientInformation object
            }

            patientInfo = ParseResponse(responseMessage);


            return patientInfo;
        }
        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
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

            var statusMessage = GetDataFromMW300D(responseMessage, "PGL_MESSAGE");
            var severity = GetDataFromMW300D(responseMessage, "PGL_SEVERITY");

            var status = GetDataFromMW300D(responseMessage, "STATUS");
            var firstName = GetDataFromMW300D(responseMessage, "FNAME").TrimEnd(' ').TrimStart(' ');
            var lastName = GetDataFromMW300D(responseMessage, "SNAME").TrimEnd(' ').TrimStart(' ');
            var gender = GetDataFromMW300D(responseMessage, "GENDER");
            var patientId = GetDataFromMW300D(responseMessage, "NUMBID");
            var weight = GetDataFromMW300D(responseMessage, "WEIGHT");
            var height = GetDataFromMW300D(responseMessage, "HEIGHT");

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
            var age = GetDataFromMW300D(responseMessage, "AGE");
            //var pidType = GetDataFromMW300D(completeMessage, "SUGID");

            //TODO : get the full respons message and add it to the object for logging

            patientInfo.Age = Convert.ToInt32(Convert.ToDecimal(age)).ToString();
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
            patientInfo.DOB = GetPatientDateOfBirth(age);
         // patientInfo.DOB = (new DateTime(DateTime.Now.Year - ageNumber, 01, 01)).ToString();

            // web service returned an error
            if (patientInfo.Severity != "0" || patientInfo.ResponseStatus != _config.GoodSoapResponseErrorCode)
            {
                string errorMsg = $"Patient Id : {patientId} " +
                    $"{Environment.NewLine}Status Code : {patientInfo.ResponseStatus}" +
                    $"{Environment.NewLine}Severity : {patientInfo.Severity}" +
                    $"{Environment.NewLine}Response Message : {patientInfo.ResponseStatusMessage}";
                NlogHelper.CreateLogEntry(errorMsg,"200",LogLevel.Error,logger);
            }



            return patientInfo;
        }

        private string GetPatientDateOfBirth(string age)
        {
            var ageElements = age.Split('.');
            var years = int.Parse(ageElements[0].TrimStart('0'));
            var months = int.Parse(ageElements[1].TrimStart('0'));

            var yearOfBirth = DateTime.Now.Year - years;
            if (DateTime.Now.Month < months)
            {
                yearOfBirth -= 1;
            }
            int monthsCorrected = 12 - (months - DateTime.Now.Month);
            return new DateTime(yearOfBirth, monthsCorrected, 01).ToString();
        }

        private WebRequest GetWebRequest(string target, int contentLength)
        {
            var request = WebRequest.Create(_config.SoapRequestTarget);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.ContentLength = contentLength;
            request.Headers.Add("SOAPAction: \"MW300IR\"");
            return request;
        }

        private string GetDataFromMW300D(string message, string itemToFind)
        {

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

        public string BuildRequestMessage(string pid, string pidType)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sb.Append("<s:Header>");
            sb.Append("<h:ShellWizHeader xmlns:h=\"http://www.shellWiz.com/shellWizHeader/v1.0\" xmlns=\"http://www.shellWiz.com/shellWizHeader/v1.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"/>");
            sb.Append("</s:Header>");
            sb.Append("<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
            sb.Append("<mw300irMessageRequest xmlns=\"http://www.mw300ir.org/mw300ir/\">");
            sb.Append("<PRMGLOBAL>");
            sb.Append("<PGL_RC>0</PGL_RC>");
            sb.Append("<PGL_SEVERITY>0</PGL_SEVERITY>");
            sb.Append("</PRMGLOBAL>");
            sb.Append("<MW300D>");
            sb.Append($"<SUGID>{pidType}</SUGID>");
            sb.Append($"<NUMBID>{pid}</NUMBID>");
            sb.Append("<STATUS>0</STATUS>");
            sb.Append("<BTDATE>0</BTDATE>");
            sb.Append("<AGE>0</AGE>");
            sb.Append("<TYPEINSUR>0</TYPEINSUR>");
            sb.Append("</MW300D>");
            sb.Append("</mw300irMessageRequest>");
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
