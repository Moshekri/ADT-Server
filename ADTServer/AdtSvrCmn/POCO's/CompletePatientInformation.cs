using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Objects
{
    public class CompletePatientInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Age { get; set; }
        public string DOB { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Gender { get; set; }
        public string GenderDesc { get; set; }
        public string PatientId { get; set; }
        public string ResponseStatus { get; set; }
        public string ResponseStatusMessage { get; set; }
        public string Severity { get; set; }
        public string CompleteResponseStatusMessage { get; set; }
        public string HebrewFirstName { get; set; }
        public string  HebrewLastName { get; set; }



        public CompletePatientInformation()
        {
            CompleteResponseStatusMessage =ResponseStatus = FirstName = LastName = Age = DOB = Height = Gender = GenderDesc = PatientId = string.Empty;

        }
        public override string ToString()
        {
            return ($"{PatientId} : {FirstName} , {LastName} Age:{Age} Gender {Gender} ");
        }
    }
}
