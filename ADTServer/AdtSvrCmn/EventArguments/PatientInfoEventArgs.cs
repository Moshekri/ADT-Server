using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.EventArguments
{
    public class PatientInfoEventArgs :EventArgs
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public Exception Exception { get { return Exception; } set { value = null; } }
        public string SiteId { get; set; }
        public string  MessageControlId { get; set; }
    }
}
