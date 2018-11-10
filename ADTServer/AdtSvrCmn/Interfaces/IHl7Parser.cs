using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Interfaces
{
    public interface IHl7Parser
    {
          string GetPatientId(string message);
          string GetSiteID(string message);
          string GetMessageDateTime(string message);
          string GetMessageControlId(string message);
          string GetMessageType(string message);
    }
}
