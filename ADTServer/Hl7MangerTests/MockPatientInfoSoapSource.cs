using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;

namespace Hl7MangerTests
{
    class MockPatientInfoSoapSource :IPatientInfoSoapSource
    {
        ApplicationConfiguration _config;
        public MockPatientInfoSoapSource(ApplicationConfiguration config) 
        {
            _config = config;
        }

       public  event EventHandler RequestingPatientInfo;
       public  event EventHandler RecevingPatientInfo;
        public  event EventHandler ErrorGettingPatientInfo;

        public  CompletePatientInformation GetPatientInfo(string CustumerId)
        {
           return  new CompletePatientInformation()
           {
               Age = "55",
               DOB = "01011995",
               FirstName = "Test",
               LastName = "mock",
               Gender = "Male",
               GenderDesc = "male",
               PatientId = CustumerId,
               Height = "181"

               
           };
        }

        public  CompletePatientInformation GetPatientInfo(string CustumerId, string pidType)
        {
            return new CompletePatientInformation()
            {
                Age = "55",
                DOB = "01011995",
                FirstName = "Test",
                LastName = "mock",
                Gender = "Male",
                GenderDesc = "male",
                PatientId = CustumerId,
                Height = "181"


            };
        }

        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            return new CompletePatientInformation()
            {
                Age = "55",
                DOB = "01011995",
                FirstName = "Test",
                LastName = "mock",
                Gender = "Male",
                GenderDesc = "male",
                PatientId = patientId.ID,
                Height = "181"


            };
        }
    }
}
