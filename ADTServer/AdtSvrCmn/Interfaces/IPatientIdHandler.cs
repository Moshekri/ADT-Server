using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Objects;

namespace AdtSvrCmn.Interfaces
{
    public interface IPatientIdHandler
    {
        /// <summary>
        /// Will Parse the input string and returns an array of PatientId
        /// arr[0] = Israeli patient id
        /// arr[1] = Non - Israeli patient id
        /// </summary>
        /// <param name="idToParse"></param>
        /// <returns>Array of PatientId [0] place - israeli PID. [1] place foriegn PID</returns>
        PatientId[] ParseID(string idToParse);
    }
}
