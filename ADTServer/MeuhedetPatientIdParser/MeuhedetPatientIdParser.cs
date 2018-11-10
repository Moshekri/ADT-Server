using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using IsraeliIdTools;
using NLog;


namespace MeuhedetPatientIdParser
{
    public class MeuhedetIdParser : IPatientIdHandler
    {
        private Logger logger;
        public MeuhedetIdParser()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        public PatientId[] ParseID(string idToParse)
        {
            PatientId[] ids = new PatientId[2];
            logger.Debug("Parsing Meuhedet Patient Id");
            if (idToParse.Length > 10)
            {
                throw new ArgumentOutOfRangeException($"Id cannot be more than 10 digits long, {idToParse} was {idToParse.Length} digits long");
            }
            if (idToParse.Length == 10)
            {
                ids[0] = new PatientId() { ID = idToParse.Substring(1,9), SugId = idToParse[0].ToString() ,SifratBikuret = idToParse.Substring(idToParse.Length-1)};
                ids[1] = new PatientId() { ID = idToParse.Substring(1,9), SugId = idToParse[0].ToString() , SifratBikuret = idToParse.Substring(idToParse.Length-1)};
                return ids;
            }
            
            else if(idToParse.Length== 9)
            {
                
                ids[0] = new PatientId() { ID = idToParse, SugId = "1" };
                ids[1] = new PatientId() { ID = idToParse, SugId = "9" };
                return ids;
            }
            else  
            {
                string paddedID = idToParse.PadLeft(9,'0');
                var sb = IDTools.CalculateSifratBikuret(paddedID); 

               
                ids[0] = new PatientId() { ID = paddedID, SugId = "1" ,SifratBikuret = sb};
                ids[1] = new PatientId() { ID = paddedID, SugId = "9"};
                return ids;
            }
           
        }
    }
}
