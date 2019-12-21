using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HadassahWebServiceDataClient.HadassahWebClient;
using System.Configuration;

namespace HadassahWebServiceDataClient
{
    class HadassahPatientDataSource : IPatientInfoSoapSource
    {
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;

        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            demogByPatientIDResponseType response;
            using (var client = new HadassahWebClient.DWHDemogPortTypeClient("DWHDemogPortTypeEndpoint0SSL"))
            {

                demogByPatientIDRequestType req = new demogByPatientIDRequestType();
                req.RequestorID = ConfigurationManager.AppSettings["ReqSysID"];
                req.PatientID = CustumerId;
                response = client.GetDemogByPatientID(req);
            }
            var dateOfBirth = response.PatientDemography.BirthDate.Split('/');
            
            string age = CalculateAge(response.PatientDemography.BirthDate);
            string gender = GetGender(response.PatientDemography.Sex);
            string dob = dateOfBirth[2] + dateOfBirth[1] + dateOfBirth[0];

            return new CompletePatientInformation()
            {
                Age = age,
                Gender = gender,
                DOB = dob,
                FirstName = response.PatientDemography.FirstName,
                LastName = response.PatientDemography.LastName ,
                Height = "",
                PatientId = CustumerId,
                GenderDesc = response.PatientDemography.Sex,
                ResponseStatusMessage = response.Result,
                CompleteResponseStatusMessage = response.ReturnedException.Exception.ExecptionText,
                Severity = response.ReturnedException.Exception.SevirityLevel,
                ResponseStatus = response.Result
               
            };
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
            var dateElements = birthDate.Split('/');
            DateTime bDate = new DateTime(int.Parse(dateElements[2]), int.Parse(dateElements[1]), int.Parse(dateElements[0]));
            int years = (int)((DateTime.Now - bDate).TotalDays / 365);
            return years.ToString();
        }

        public CompletePatientInformation GetPatientInfo(string CustumerId, string pidType)
        {
            return GetPatientInfo(CustumerId);
        }

        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            return GetPatientInfo(patientId.ID);

        }
    }
}
