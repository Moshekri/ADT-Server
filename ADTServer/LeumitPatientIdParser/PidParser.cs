//   this will try to parse an id recived from the ecg device :

//   three options are available:
//      1. A magnetic card was swiped with an israeli customer details
//      2. A magnetic card was swiped with a foriegn customer details
//      3. The tech/nurse entered  manually a correct id for an israeli patient without sug id
//      4. The tech/nurse entered  manually a correct id for a porigen patient without sug id
//      5. the entered pid is not correct

// for option 1 - 



using System.Text.RegularExpressions;
using AdtSvrCmn.Objects;
using IsraeliIdTools;
using AdtSvrCmn.Interfaces;
using System;
using NLog;

namespace LeumitPatientIdParser
{
    public class PidParser : IPatientIdHandler
    {
        bool continueToParsdeForiegnID = false;
        Logger logger;
        private static object locker = new object();
        public PidParser()
        {
            logger = LogManager.GetCurrentClassLogger();
        }



        /// <summary>
        /// Will Parse the input string and returns an array of PatientId
        /// arr[0] = Israeli patient id
        /// arr[1] = Non - Israeli patient id
        /// </summary>
        /// <param name="idToParse"></param>
        /// <returns></returns>
        public PatientId[] ParseID(string idToParse)
        {
            PatientId[] results = new PatientId[2];
            double res;
            if (!double.TryParse(idToParse, out res))
            {
                results[0] = results[1] = null;
                return results;
            }

            if (idToParse.Length > 10)
            {
                return results;
            }

            results[0] = ParseAsIsraeliPatientID(idToParse);
            if (!results[0].IsValidIsraeliId)
            {
                results[1] = ParseAsNonIsraeliPatientId(idToParse);
            }
            else
            {
                results[1] = null;
                continueToParsdeForiegnID = false;
            }
            if (null != results[0] && results[0].ID.Length > 8)
            {
                logger.Trace($"Can not validate pid {idToParse} as israeli ID");
                results[0] = null;
            }
            if (null != results[1] && results[1].ID.Length > 8)
            {

                results[1].ID = results[1].ID.TrimStart('0');

                switch (results[1].ID.Length)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        results[1].ID = results[1].ID.PadLeft(8, '0');
                        break;
                    default:
                        results[1].ID = results[1].ID.Substring(0, 8);
                        break;
                }

                results[1].SugId = "9";
            }
            return results;

        }

        private PatientId ParseAsIsraeliPatientID(string idToParse)
        {
            logger.Trace("Entered :ParseAsIsraeliPatientID ");
            lock (locker)
            {
                logger.Trace($"Attempting to parse patient id {idToParse}");
                PatientId patientId = new PatientId();


                // Patients Magnetic card was swiped 
                patientId = CardSwiped(idToParse);
                if (patientId != null)
                {
                    return patientId;
                }

                // patient id was entered manually without the sugId digit at the start
                // so first 8 digits are the id digits and the last is the sifrat bikoret
                else if (idToParse.Length <= 10 && IDTools.IsVaildPatientID(idToParse))
                {
                    logger.Trace($"{idToParse} was entered manually  but last digit is sifrat bikoret and PID is valid");
                    patientId = new PatientId();
                    logger.Trace($"setting sugId to \"1\" for patient ID : {idToParse}");
                    patientId.SugId = "1";

                    if ((idToParse.StartsWith("1") || idToParse.StartsWith("9")) && idToParse.Length > 1)
                    {
                        idToParse = idToParse.Substring(1).TrimStart('0').PadLeft(9, '0');
                    }
                    else
                    {
                        idToParse = idToParse.TrimStart('0').PadLeft(9, '0');
                    }

                    if (idToParse.TrimStart('0').Length != 1)
                    {
                        patientId.ID = idToParse.Substring(0, idToParse.Length - 1);
                        patientId.SifratBikuret = idToParse.Substring(idToParse.Length - 1);
                        logger.Trace($"for {idToParse} the 8 digit id is {patientId.ID} and sifrat bikoret is {patientId.SifratBikuret} sug id is {patientId.SugId}");
                        patientId.IsValidIsraeliId = true;
                        return patientId;
                    }
                    else
                    {
                        patientId.ID = idToParse.Substring(1, 8);
                        patientId.SifratBikuret = IDTools.CalculateSifratBikuret(patientId.ID);
                        logger.Trace($"for {idToParse} the 8 digit id is {patientId.ID} and sifrat bikoret is {patientId.SifratBikuret} sug id is {patientId.SugId}");
                        patientId.IsValidIsraeliId = true;
                        return patientId;
                    }

                }
                // patient id was enterd manually without sifrat bikoret
                // we assume the id is correct and add 1 as sugId and calculate sifrat bikoret from the given ID
                else
                {
                    logger.Trace($"Patient id {idToParse} was NOT swiped and is not a valid israeli Patient Id {Environment.NewLine}" +
                        $" however we will try to treat it as an id without sifrat bikoret.");
                    logger.Trace($"Setting sugid for {idToParse} as 1 ....");
                    continueToParsdeForiegnID = true;
                    patientId = new PatientId();
                    patientId.SugId = "1";
                    patientId.ID = idToParse.TrimStart('0').PadLeft(8, '0');
                    patientId.SifratBikuret = IDTools.CalculateSifratBikuret(idToParse);
                    logger.Trace($"Setting Sifrat bikoret for {idToParse} as {patientId.SifratBikuret}");
                    logger.Trace("Exiting : ParseAsIsraeliPatientID ");
                    patientId.IsValidIsraeliId = false;
                    return patientId;
                }

            }


        }
        private PatientId ParseAsNonIsraeliPatientId(string idToParse)
        {
            logger.Trace("Entered ParseAsNonIsraeliPatientId");
            lock (locker)
            {
                PatientId patientId = new PatientId();

                patientId = CardSwiped(idToParse);

                // if not null  - a card was swiped 
                if (patientId != null)
                {

                    return patientId;
                }
                // in case of non israeli customers we always add 9 as sugId
                //  also no need to calculate sifrat bikoret
                else
                {

                    idToParse = idToParse.TrimStart('0').PadLeft(9, '0');

                    patientId = new PatientId();
                    patientId.ID = idToParse;
                    patientId.SugId = "9";
                    patientId.SifratBikuret = "";
                    logger.Trace($"for {idToParse} setting sugId as 9 not adding sifrat bikoret");
                    logger.Trace("Exiting ParseAsNonIsraeliPatientId");
                    return patientId;
                }
            }

        }
        private PatientId CardSwiped(string idToParse)
        {
            Regex tenDigits = new Regex("[0-9]{10}");

            if (tenDigits.IsMatch(idToParse) && (idToParse[0] == '1' || idToParse[0] == '9'))
            {
                var tempId = idToParse.Substring(1, idToParse.Length - 1);
                if (IDTools.IsVaildPatientID(tempId))
                {
                    PatientId patientId = new PatientId();
                    patientId.SugId = idToParse.Substring(0, 1);
                    patientId.ID = idToParse.Substring(1, 8);
                    patientId.SifratBikuret = idToParse.Substring(9, 1);
                    patientId.IsValidIsraeliId = true;
                    return patientId;
                }
                else
                {
                    PatientId patientId = new PatientId();
                    patientId.SugId = idToParse.Substring(0, 1);
                    patientId.ID = idToParse.Substring(1).TrimStart('0').PadLeft(8, '0');
                    patientId.IsValidIsraeliId = false;
                    return patientId;
                }

            }
            else
                return null;
        }
    }
}
