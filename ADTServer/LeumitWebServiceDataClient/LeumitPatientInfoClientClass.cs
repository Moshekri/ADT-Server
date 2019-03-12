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
         
            CompletePatientInformation patientInfo = new CompletePatientInformation();
            string responseMessage = string.Empty;
          
            responseMessage = GetDataFromWebService(patientId, pidType);

            var sevirity = GetDataFromMW300D(responseMessage, "PGL_SEVERITY");
            if (sevirity != "0")
            {
                LogAS400Erorrs(sevirity, responseMessage, pidType, patientId);
                return null;
            }

            patientInfo = ParseResponse(responseMessage);
            return patientInfo;
        }

        private void LogAS400Erorrs(string sevirity, string responseMessage, string pidType, string patientId)
        {
            var errorMessage = GetDataFromMW300D(responseMessage, "wsrcmsg");
            if (errorMessage.ToLower().Contains("exception") || errorMessage.ToLower().Contains("error"))
            {
                NlogHelper.CreateLogEntry(pidType + patientId + ":" + errorMessage, "300", LogLevel.Error, logger);

            }
            string errMsg = GetDataFromMW300D(responseMessage, "PGL_MESSAGE");
            if (errMsg.ToUpper().Contains("DVL0011"))
            {
                LogEvent(LogLevel.Error, 400, pidType + patientId + ":" + "Patient does not exist on file" + Environment.NewLine + "המבוטח אינו קיים בקובץ מבוטחים");

            }
            else if (errMsg.ToUpper().Contains("DVL0012"))
            {
                LogEvent(LogLevel.Error, 500, pidType + patientId + ":" + "Patient is not eligible to recieve treatment" + Environment.NewLine + "מבוטח לא זכאי לטיפול");

            }
            else if (errMsg.ToUpper().Contains("DVL"))
            {
                LogEvent(LogLevel.Error, 600, pidType + patientId + ":" + errMsg);
            }
        }

        private string GetDataFromWebService(string patientId,string pidType)
        {
            string responseMessage;
            string requestMessage = BuildRequestMessage(patientId, pidType);
            WebRequest request = GetWebRequest(_config.SoapRequestTarget, requestMessage.Length);
            logger.Info($"Sending request for patient Data :  patient id {pidType}{patientId}");
            logger.Debug(requestMessage);
            byte[] buffer = new byte[10000];
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
            return responseMessage;
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

            string statusMessage = GetDataFromMW300D(responseMessage, "PGL_MESSAGE");
            string severity = GetDataFromMW300D(responseMessage, "PGL_SEVERITY");
            string status = GetDataFromMW300D(responseMessage, "STATUS");
            string lastName = GetDataFromMW300D(responseMessage, "FNAME").TrimEnd(' ').TrimStart(' ');
            string firstName = GetDataFromMW300D(responseMessage, "SNAME").TrimEnd(' ').TrimStart(' ');
            string gender = GetDataFromMW300D(responseMessage, "GENDER");
            string patientId = GetDataFromMW300D(responseMessage, "NUMBID");
            string weight = GetDataFromMW300D(responseMessage, "WEIGHT");
            string height = GetDataFromMW300D(responseMessage, "HEIGHT");

            SetGender(gender, patientInfo);
            SetPatientDateOfBirth(responseMessage, patientInfo);
            SetAge(responseMessage, patientInfo);
            SetHeight(height, patientInfo);
            SetWeight(weight, patientInfo);
            patientInfo.ResponseStatusMessage = statusMessage;
            patientInfo.Severity = severity.Trim();
            patientInfo.FirstName = firstName;
            patientInfo.LastName = lastName;
            patientInfo.Gender = gender;
            patientInfo.GenderDesc = gender == "M" ? "Male" : "Female";
            patientInfo.PatientId = patientId;
            patientInfo.ResponseStatus = status;

           
            // web service returned an error
            if (patientInfo.Severity != "0" || patientInfo.ResponseStatus != _config.GoodSoapResponseErrorCode)
            {
                string errorMsg = $"Patient Id : {patientId} " +
                    $"{Environment.NewLine}Status Code : {patientInfo.ResponseStatus}" +
                    $"{Environment.NewLine}Severity : {patientInfo.Severity}" +
                    $"{Environment.NewLine}Response Message : {patientInfo.ResponseStatusMessage}";
                NlogHelper.CreateLogEntry(errorMsg, "200", LogLevel.Error, logger);
            }



            return patientInfo;
        }

        private void SetWeight(string weight, CompletePatientInformation patientInfo)
        {
            try
            {

                weight = ((int)(double.Parse(weight))).ToString();
            }
            catch (Exception ex)
            {
                LogEvent(LogLevel.Error, 702, $"Error parsing weight {Environment.NewLine}{ex.Message}");
                weight = "0";
            }
            patientInfo.Weight = weight;
        }

        private void SetHeight(string height, CompletePatientInformation patientInfo)
        {
            try
            {
                height = ((int)(double.Parse(height))).ToString();
            }
            catch (Exception ex)
            {
                LogEvent(LogLevel.Error, 701, $"Error parsing height :{Environment.NewLine}{ex.Message}");
                height = "0";
            }
            patientInfo.Height = height;
        }

        private void SetGender(string gender, CompletePatientInformation patientInfo)
        {
            switch (gender)
            {
                case "ז":
                    patientInfo.Gender = "M";
                    break;
                case "נ":
                    patientInfo.Gender = "F";
                    break;
                default:
                    patientInfo.Gender = "";
                    break;
            }
        }

        private void SetAge(string responseMessage, CompletePatientInformation patientInfo)
        {
            var age = GetDataFromMW300D(responseMessage, "AGE");
            try
            {
                patientInfo.Age = Convert.ToInt32(Convert.ToDecimal(age)).ToString();
            }
            catch (Exception)
            {
                NlogHelper.CreateLogEntry($"Error parsing age , input was {age}", "703", LogLevel.Error, logger);
                patientInfo.Age = "0";
            }
        }

        private void SetPatientDateOfBirth(string responseMessage, CompletePatientInformation patientInfo)
        {
            string fullDateOfBirth = GetDataFromMW300D(responseMessage, "BTDATE");
            try
            {
                fullDateOfBirth = FormatDateOfBirth(fullDateOfBirth);
            }
            catch (Exception ex)
            {
                LogEvent(LogLevel.Error, 700, $"Error parsing date of birth from \'BDATE\' field  {Environment.NewLine}{ex.Message}");
                logger.Debug($"Error parsing date of birth {fullDateOfBirth} , exception message : {Environment.NewLine}{ex.Message}");
                fullDateOfBirth = "-1";
            }

            var age = GetDataFromMW300D(responseMessage, "AGE");
            if (fullDateOfBirth == "-1")
            {
                patientInfo.DOB = GetPatientDateOfBirth(age);
            }
            else
            {
                patientInfo.DOB = fullDateOfBirth;
            }
        }

        private string FormatDateOfBirth(string dateOfBirth)
        {
            if (dateOfBirth == string.Empty || dateOfBirth == "0")
            {
                return "-1";
            }
            int year = int.Parse(dateOfBirth.Substring(0, 4));
            if (year > DateTime.Now.Year || year < DateTime.Now.Year - 120)
            {
                return "-1";
            }
            int month = int.Parse(dateOfBirth.Substring(4, 2));
            if (month > 12 || month <= 0)
            {
                return "-1";
            }
            int day = int.Parse(dateOfBirth.Substring(6, 2));
            if (day > 31 || day <= 0)
            {
                return "-1";
            }
            return new DateTime(year, month, day).ToString();
        }

        private string GetPatientDateOfBirth(string age)
        {
            var today = DateTime.Now;
            decimal ageNumber = decimal.Parse(age);
            int years = (int)ageNumber;
            int months = (int)((ageNumber - years) * 100);
            today = today.AddYears(-years);
            today = today.AddMonths(-months);
            today = today.AddDays(-1 * today.Day + 1);

            return today.ToString();







            //int months = 0;
            //var ageElements = age.Split('.');
            //var years = int.Parse(ageElements[0].TrimStart('0'));
            //if (ageElements.Length > 1 )
            //{
            //    ageElements[1] = ageElements[1].TrimStart('0');
            //    if (ageElements[1]!= string.Empty)
            //    {
            //        months = int.Parse(ageElements[1].TrimStart('0'));
            //    }

            //}

            //var yearOfBirth = DateTime.Now.Year - years;
            //if (DateTime.Now.Month < months)
            //{
            //    yearOfBirth -= 1;
            //}

            //int monthsCorrected = DateTime.Now.Month  - months;
            //return new DateTime(yearOfBirth, monthsCorrected, 01).ToString();
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
