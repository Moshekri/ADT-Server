using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Objects
{
   public class Hl7ManagerData
    {
        public ApplicationConfiguration Config { get; set; }
        public IPatientInfoSoapSource PatientInformationSource { get; set; }
        public IHl7Parser IHl7Parser { get; set; }

        public IPatientIdHandler PidHandler{get;set;}

    }
}
