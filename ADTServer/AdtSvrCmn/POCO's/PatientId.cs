using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Objects
{
    public class PatientId
    {
        public string SugId { get; set; }
        public string  SifratBikuret { get; set; }
        public string  ID { get; set; }
        public bool IsValidIsraeliId { get; set; }
        public override string ToString()
        {
            return $"{SugId}{ID}{SifratBikuret}";
        }
    }
}
