using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsraeliIdTools
{
    public static class IDTools
    {
        static int[] temp = new int[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        static int[] sum = new int[10];

        public static string CalculateSifratBikuret(string id)
        {
            int res;
            if (int.TryParse(id , out res))
            {
                id = id.TrimStart('0').PadLeft(8, '0');
                int total = 0;
                if (id.Length < 8)
                {
                    id = id.PadLeft(8, '0');
                }
                else if (id.Length == 9)
                {
                    id = id.Substring(0, 8);
                }
                else if (id.Length > 9)
                {
                    id = id.TrimStart('0');
                    if (id.Length > 9)
                    {
                        return "";
                    }
                }

                id = id.PadLeft(10, '0');
                for (int i = 0; i < temp.Length; i++)
                {

                    sum[i] = temp[i] * int.Parse(id[i].ToString());
                    sum[i] = AddDigits(sum[i]);
                    total += sum[i];
                }

                if (total != 0)
                {
                    if (total % 10 == 0)
                    {
                        return "0";
                    }
                    return (10 - (total % 10)).ToString();
                }
                else
                {
                    return "0";
                }
                
            }
            else
            {
                return string.Empty;
            }

            


        }
        /// <summary>
        /// Checks if input string is avalid israeli Id.
        /// input should contain the check digit at the last position
        /// </summary>
        /// <param name="idToCheck">The Id to be checked - check digit sould be last</param>
        /// <returns></returns>
        public static bool IsVaildPatientID(string idToCheck)
        {
            if ((idToCheck.StartsWith("1") || idToCheck.StartsWith("9")) && idToCheck.Length >1)
            {
                idToCheck = idToCheck.Substring(1).PadLeft(9, '0');
            }

            idToCheck =  idToCheck.TrimStart('0').PadLeft(9, '0');
            

            var sifratBikoret = CalculateSifratBikuret(idToCheck.Substring(0, idToCheck.Length - 1));
            if (sifratBikoret == idToCheck.Substring(idToCheck.Length - 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static int AddDigits(int num)
        {
            if (num < 10)
            {
                return num;
            }
            return num % 10 + num / 10;
        }
    }
}
