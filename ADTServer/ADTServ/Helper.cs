using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn;
using AdtSvrCmn.Objects;
using NLog;

namespace ADTServ
{
    public static class Helper
    {
       private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns patients age calculated from date of birth
        /// </summary>
        /// <param name="customer">customer information object</param>
        /// <returns>age as string</returns>
        public static string CalculateAge(CompletePatientInformation customer)
        {
            try
            {
                logger.Trace("entered CalculateAge");

                logger.Debug("Calculating patients age..");
                string age;
                DateTime dateOfBirth = new DateTime();
                if (DateTime.TryParse(customer.DOB.ToString(), out dateOfBirth))
                {
                    age = (DateTime.Now.Year - dateOfBirth.Year).ToString();
                }
                else
                {
                    age = "";
                }
                logger.Debug($"Age Calculated : {age}");
                logger.Trace("left CalculateAge");
                return age;
            }
            catch(Exception ex)
            {
                logger.Debug(ex);
                return string.Empty;
            }
          
        }

        /// <summary>
        /// Helper methos to format the datetime to yyyymmddhhmmss ( one string no spaces or other chars)
        /// this is the format that MUSE understands
        /// </summary>
        /// <param name="dOB">The date string to format - MUST be</param>
        /// <returns></returns>
        public static string FormatDateOfBirth(string dOB)
        {
            logger.Trace("entered FormatDateOfBirth");
            DateTime dob = new DateTime();
            var isParseOk = DateTime.TryParse(dOB, out dob);
            if (isParseOk)
            {
                var year = dob.Year.ToString();
                var month = dob.Month.ToString();
                if (month.Length == 1)
                {
                    month = "0" + month;
                }
                var day = dob.Day.ToString();
                if (day.Length == 1)
                {
                    day = "0" + day;
                }
                return year + month + day + "000000";
            }
            logger.Trace("left FormatDateOfBirth");
            return new DateTime().ToString();




        }
    }
}
