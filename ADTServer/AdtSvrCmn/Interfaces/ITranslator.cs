using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Objects;

namespace AdtSvrCmn.Interfaces
{
    public interface ITranslator
    {
      PatientTranslationObject GetEnglishName(string firstName , string lastName);
    }
}
