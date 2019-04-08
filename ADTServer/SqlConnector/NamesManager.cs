using AdtSvrCmn.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
namespace SqlConnector
{
    public class NamesManager :IDbConnector
    {
        NamesModel db;
        Dictionary<string, string> namesDict;
        Logger logger;
        public NamesManager()
        {
            db = new NamesModel();
            logger = LogManager.GetCurrentClassLogger();
            namesDict = new Dictionary<string, string>();
        }

        public Dictionary<string, string> GetData()
        {
            var data = db.MyNamesEntities.ToList();
            foreach (var item in data)
            {
                if (!namesDict.ContainsKey(item.HebrewName))
                {
                    namesDict.Add(item.HebrewName, item.EnglishName);
                }
            }
            return namesDict;

        }

        public bool SaveData(Dictionary<string, string> data)
        {
            List<Names> dataFromDb = db.MyNamesEntities.ToList();
            foreach (var item in data)
            {
                if (dataFromDb.FirstOrDefault(e=>e.HebrewName == item.Key) == null)
                {
                    db.MyNamesEntities.Add(new Names() { HebrewName = item.Key, EnglishName = item.Value });
                }
            }
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Error saving to sql database {ex.Message}");
                return false;
            }
            

        }

        public bool SaveData(Dictionary<string, string> data, string path)
        {
            throw new NotImplementedException();
        }
    }
}
