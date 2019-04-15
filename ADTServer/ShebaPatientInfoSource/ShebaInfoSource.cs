
using AdtSvrCmn;
using AdtSvrCmn.EventArguments;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using DbLayer;
using NLog;
using SqlConnector;
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
            {
                var data = this.GetPatientInfo(patientId.ID, patientId.SugId);
                SaveDataToSql(data);
                return data;
            }

            return (CompletePatientInformation)null;
        }

        private void SaveDataToSql(CompletePatientInformation data)
        {
            NamesModel db = new NamesModel();
            var ent =  db.MyNamesEntities.Find(data.HebrewFirstName);
            if (ent == null)
            {
                db.MyNamesEntities.Add(new Names() { EnglishName = data.FirstName, HebrewName = data.HebrewFirstName });
            }
            var en1t = db.MyNamesEntities.Find(data.HebrewLastName);
            if (en1t == null)
            {
                db.MyNamesEntities.Add(new Names() { EnglishName = data.LastName, HebrewName = data.HebrewLastName });
            }
            db.SaveChanges();


        }

        private CompletePatientInformation ParseResponse(string responseMessage)
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
            patientInformation.HebrewFirstName = GetDataFromMW300D(responseMessage, "familyname");
            patientInformation.HebrewLastName = GetDataFromMW300D(responseMessage, "firstname");
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


    }
}



