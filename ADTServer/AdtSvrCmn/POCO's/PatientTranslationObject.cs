using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn.Objects
{
    public class PatientTranslationObject
    {
        public string HebrewFirstName { get; set; }
        public string HebrewLastName { get; set; }
        public string EnglishFirstName { get; set; }
        public string EnglishLastName { get; set; }
        public bool IsEnglishFirstNameByGoogle { get; set; }
        public bool IsEnglishLastNameByGoogle { get; set; }

        public PatientTranslationObject()
        {
            HebrewFirstName = string.Empty;
            HebrewLastName = string.Empty;
            EnglishFirstName = string.Empty;
            EnglishLastName = string.Empty;
            IsEnglishFirstNameByGoogle = false;
            IsEnglishLastNameByGoogle = false;
                        
        }
    }
}
