using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HadassahWebServiceDataClient.HadassahWebClient;
using System.Configuration;
using NLog;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Web;
using SoapClientMessageInspector;
namespace HadassahWebServiceDataClient
{
    public class HadassahPatientDataSource : IPatientInfoSoapSource
    {
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;
        ApplicationConfiguration _config;
        Logger logger;
        public HadassahPatientDataSource(ApplicationConfiguration config)
        {
            logger = LogManager.GetCurrentClassLogger();
            _config = config;
        }
        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            
            logger.Trace("Inside GetPatientInfo - start ");
            demogByPatientIDResponseType response = new demogByPatientIDResponseType();
            logger.Debug("Trying to create web client for hadassah web service");
            var SEC = ConfigurationManager.GetSection("startup");

            using (var client = new DWHDemogPortTypeClient("DWHDemogPortTypeEndpoint0SSL"))
            {
                client.Endpoint.EndpointBehaviors.Add(new SoapClientMessageInspector.MessageEndPointBehavior());
                logger.Info(client.InnerChannel.Via);


                logger.Debug($"client to hadassaah web service  created successfuly  target address : {client.Endpoint.Address}");
                logger.Debug("Creating request ...");
                demogByPatientIDRequestType req = new demogByPatientIDRequestType();
                logger.Debug($"Setting requester ID to {ConfigurationManager.AppSettings["ReqSysID"]}");
                req.RequestorID = ConfigurationManager.AppSettings["ReqSysID"];
                logger.Debug($"Setting patient id to : {CustumerId} ");
                req.PatientID = CustumerId;
                logger.Debug($"Attempting to retrieve patient Demographics for PID : {CustumerId} ...");
                try
                {
                                        logger.Trace("Request message : ");
                    string msg = GetXml(typeof(demogByPatientIDRequestType), req);
                    logger.Trace(msg);
                    response = client.GetDemogByPatientID(req);
                    string responeMsg = GetXml(typeof(demogByPatientIDResponseType), response);
                    logger.Trace(responeMsg);

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while trying to get patient demograpgics");
                    LogException(ex);
                    return null;
                }
                logger.Debug("Successefully got response from web service");
                logger.Debug($"Status returned from web service : {response.Result}");
                logger.Debug($"Execption returned from web service : {response.ReturnedException.Exception.ExecptionText}");
                logger.Debug($"Execption severity level returned from web service : { response.ReturnedException.Exception.SevirityLevel}");
            }
            try
            {
                var dateOfBirth = response.PatientDemography.BirthDate.Split('/');
                logger.Debug("Calculating age...");
                string age = CalculateAge(response.PatientDemography.BirthDate);
                string gender = GetGender(response.PatientDemography.Sex);
                string dob = dateOfBirth[2] + dateOfBirth[1] + dateOfBirth[0];
                logger.Debug($"age : {age} , gender : {gender} , date of birth : {dob}");
                logger.Info("Returning patient information object");
                return new CompletePatientInformation()
                {
                    Age = age,
                    Gender = gender,
                    DOB = dob,
                    FirstName = response.PatientDemography.FFirstName,
                    LastName = response.PatientDemography.FLastName,
                    Height = "",
                    PatientId = CustumerId,
                    GenderDesc = response.PatientDemography.Sex,
                    ResponseStatusMessage = response.Result,
                    CompleteResponseStatusMessage = response.ReturnedException.Exception.ExecptionText,
                    Severity = response.ReturnedException.Exception.SevirityLevel,
                    ResponseStatus = response.Result

                };
            }
            catch (Exception ex)
            {

                LogException(ex);
                return null;
            }

        }

        private void LogException(Exception ex)
        {
            logger.Error("Exception Message : "  + ex.Message);
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                logger.Error("Inner Exception : " + ex.Message);

            }
        }

        private string GetGender(string sex)
        {
            sex = sex.ToLower();
            switch (sex)
            {
                case "ז":
                    return "M";
                case "נ":
                    return "F";
                case "ב":
                    return "";
                default:
                    return "";
            }
        }

        private string CalculateAge(string birthDate)
        {
            logger.Debug("Calculate patient age from date of birth");
            var dateElements = birthDate.Split('/');
            try
            {
                DateTime bDate = new DateTime(int.Parse(dateElements[2]), int.Parse(dateElements[1]), int.Parse(dateElements[0]));
                double years = (double)((DateTime.Now - bDate).TotalDays / 365);
                int intyears = (int)years;
                return intyears.ToString();
            }
            catch (Exception ex)
            {
                logger.Error($"Error calculating age from d.o.b {birthDate}");
                LogException(ex);
                return string.Empty;
            }

        }

        public CompletePatientInformation GetPatientInfo(string CustumerId, string pidType)
        {
            return GetPatientInfo(CustumerId);
        }

        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            return GetPatientInfo(patientId.ID);
        }

        private string GetXml(Type T, object obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer ser = new XmlSerializer(T);
                ser.Serialize(sw, obj);
                return sw.ToString();
            }
        }
    }
}
