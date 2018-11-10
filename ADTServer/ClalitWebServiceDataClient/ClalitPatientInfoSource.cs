using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using ClalitWebServiceDataClient.ClalitPatientNameService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClalitWebServiceDataClient
{
    public class ClalitPatientInfoSource : IPatientInfoSoapSource
    {
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;

        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            CompletePatientInformation patientInformation = new CompletePatientInformation();

            using (var client = new TranslationPatientNamesServiceClient("TranslationPatientNamesService"))
            {
                TranslationPatientNamesRequest request = new TranslationPatientNamesRequest();
                request.MessageInfo.RequestDatetime = DateTime.Now;
                request.MessageInfo.RequestID = Guid.NewGuid().ToString();
                request.MessageInfo.RequestingApplication = 670;
                request.MessageInfo.ServingApplication = 358;
                request.MessageInfo.ServingSite = 14;
                long patientId;
                if (long.TryParse(CustumerId, out patientId))
                {
                    request.Parameters.PatientID = patientId;
                }
                else
                {
                    throw new ArgumentException($"Patient ID Not in currect Format (was {CustumerId})");
                }

                var response =client.TranslationPatientNamesQuery(request);
                patientInformation.DOB = response.Results.BirthDate.ToShortDateString();
                patientInformation.FirstName = response.Results.FirstNameEng;
                patientInformation.LastName = response.Results.LastNameEng;
               
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
