using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Interfaces
{
    public interface IApplicationLogger
    {
         IApplicationLogger GetInstance(string path, string fileName);
         void MakeLogEntry(string data, string source);
         void LogExecption(Exception ex, string source);
         void LogMessage(string eMessage, string source);
    } 
}
