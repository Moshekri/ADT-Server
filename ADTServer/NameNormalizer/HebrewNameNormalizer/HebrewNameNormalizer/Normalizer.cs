using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;

namespace HebrewNameNormalizer
{
    public class Normalizer :INormalizer
    {
        Regex Spaces = new Regex("[ ]{1,}");

        Regex Hifens = new Regex("[-]{1,}");

        public string Normalize(string data)
        {
            string noExtraSpaces = TrimWhiteSpaces(data);
            string noExtraHifens = RemoveExtraHiyfens(noExtraSpaces);
            var satgeTwo = ReplaceMultipleSpacesWithOneHiyfen(noExtraHifens);
            string finalStep = satgeTwo.Trim(new char[] { '-' });
            return finalStep;
        }
        private string ReplaceMultipleSpacesWithOneHiyfen(string text)
        {

            var noSpaces = Spaces.Replace(text, "-");
            return noSpaces;
        }

        private string RemoveExtraHiyfens(string text)
        {
            var noHiyfens = Hifens.Replace(text, " ");
            return noHiyfens;
        }


        private string TrimWhiteSpaces(string text)
        {
            var noSpaces = Spaces.Replace(text, " ");
            return noSpaces;
        }

    }
}
