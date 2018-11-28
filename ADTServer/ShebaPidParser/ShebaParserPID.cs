using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;

namespace ShebaPidParser
{
    public class ShebaParserPID : IPatientIdHandler
    {
        public PatientId[] ParseID(string idToParse)
        {
            PatientId id = new PatientId();
            id.ID = idToParse;
            id.IsValidIsraeliId = true;
            return new PatientId[] { id, id };
        }
    }
}
