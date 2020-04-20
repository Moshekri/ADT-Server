using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Interfaces
{
    public interface IDbConnector
    {
        Dictionary<string, string> GetData();
        bool SaveData(Dictionary<string, string> data);
        bool SaveData(Dictionary<string, string> data, string path);
        string GetValue(string key);
        void SaveSingleEntry(string key, string value);
    }
}
