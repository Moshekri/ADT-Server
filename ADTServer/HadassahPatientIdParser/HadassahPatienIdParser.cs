using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using NLog;


namespace HadassahPatientIdParser
{
    public class HadassahPatienIdParser : IPatientIdHandler
    {
        ApplicationConfiguration _config;
        Logger logger;
        public HadassahPatienIdParser(ApplicationConfiguration config)
        {
            _config = config;
            logger = LogManager.GetCurrentClassLogger();
        }
        public PatientId[] ParseID(string idToParse)
        {

            int sifratBikret = 0;
            int.TryParse(idToParse[idToParse.Length - 1].ToString(), out sifratBikret);

            PatientId id1 = new PatientId() { ID = idToParse, IsValidIsraeliId = true, SifratBikuret = sifratBikret.ToString(), SugId = idToParse[0].ToString() };
            return new PatientId[] { id1, null };
        }
    }
}
