using AdtSvrCmn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AdtSvrCmn.Interfaces
{
    public interface IPatientInfoSoapSource
    {
        event EventHandler ErrorGettingPatientInfo;
        event EventHandler RecevingPatientInfo;
        event EventHandler RequestingPatientInfo;
        CompletePatientInformation GetPatientInfo(string CustumerId);
        CompletePatientInformation GetPatientInfo(string CustumerId, string pidType);
        CompletePatientInformation GetPatientInfo(PatientId patientId);
    }
}
