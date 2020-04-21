using System;
using System.Collections.Generic;
using System.IO;
using DbLayer;
using AdtSvrCmn.Objects;
using Google.Cloud.Translation.V2;
using AdtSvrCmn.Interfaces;
using NLog;


namespace TranslationManager
{
    public class TranslationManger : ITranslator, IDisposable
    {
        #region Private Fields
        string dbFilePath;
        IDbConnector dbCon;
        string cerdFilePath;
        private readonly object locker = new object();
        private ApplicationConfiguration _config;
        INormalizer normalizer;
        GoogleTranslator googleTranslator;
        Logger logger;
        #endregion

        //constructor
        internal TranslationManger(TranslationManagerData translationManagerData)
        {
            this.normalizer = translationManagerData.NameNormalyzer;
            _config = translationManagerData.Config;
            this.logger = LogManager.GetCurrentClassLogger();
            try
            {
                cerdFilePath = Path.Combine(_config.GoogleCredentialFilePath, _config.GoogleCredentialFileName);
                dbFilePath = Path.Combine(_config.DataBaseFolder, _config.DataBaseFileName);
                dbCon = DbFactory.GetDbConnector(_config);
                googleTranslator = new GoogleTranslator(cerdFilePath, logger, _config);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception("From Translation manager constructor", ex);
            }


        }
        public PatientTranslationObject GetEnglishName(string firstName, string lastName)
        {
            //initialize local variables
            firstName = normalizer.Normalize(firstName);
            lastName = normalizer.Normalize(lastName);
            string googleEnglishFirstName = string.Empty;
            string googleEnglishLastName = string.Empty;
            PatientTranslationObject pat = new PatientTranslationObject()
            {
                HebrewFirstName = firstName,
                HebrewLastName = lastName
            };
            string phoneticEnglishFirstName;
            string phoneticEnglishLastName;

            // Do we go to local Db for the names ??
            if (_config.UseLocalDb)
            {
                pat.EnglishFirstName = dbCon.GetValue(firstName);
                pat.EnglishLastName = dbCon.GetValue(lastName);
                if (pat.EnglishLastName != string.Empty && pat.EnglishFirstName != string.Empty)
                {
                    return pat;
                }
            }

            // do we try to get google translation ??? if NOT only phonetic translations will be used !!
            if (_config.MustGetTranslation)
            {
                try
                {
                    googleEnglishFirstName = googleTranslator.GetEnglishFirstNameFromGoogle(pat.HebrewFirstName);
                    googleEnglishLastName = googleTranslator.GetEnglishLasttNameFromGoogle(pat.HebrewLastName);
                }
                catch (Exception ex)
                {
                    logger.Debug("Problem getting google translation - using phonetic translation see MUSEADT log for more details");
                    logger.Error(ex.Message);
                }

            }

            phoneticEnglishFirstName = pat.EnglishFirstName == string.Empty ? PhoneticTranslator.GetPhoneticTranslation(pat.HebrewFirstName) : pat.EnglishFirstName;
            phoneticEnglishLastName = pat.EnglishFirstName == string.Empty ? PhoneticTranslator.GetPhoneticTranslation(pat.HebrewFirstName) : pat.EnglishFirstName;

            pat.EnglishFirstName = GetFinalName(firstName, phoneticEnglishFirstName, googleEnglishFirstName);
            pat.EnglishLastName = GetFinalName(lastName, phoneticEnglishLastName, googleEnglishLastName);

            logger.Debug($"Hebrew First Name : {pat.HebrewFirstName} , Google : {googleEnglishFirstName} Phonetic : {phoneticEnglishFirstName}");
            logger.Debug($"Hebrew First Name : {pat.HebrewLastName} , Google : {googleEnglishLastName} Phonetic : {phoneticEnglishLastName}");

            return pat;
        }

        #region Private helper Methods
        private string GetFinalName(string hebrew, string phonetic, string google)
        {

            double googleTranslationQuality = GetGoogleTranslationQuality(google, phonetic);
            var q1 = LevenshteinDistance.Compute(google, phonetic);

            if (googleTranslationQuality < 50 || (google.Length > 8 ? q1 >= 6 : q1 > 4))
            {
                return phonetic;
            }
            else
            {
                dbCon.SaveSingleEntry(hebrew, google);
                return google;
            }

        }
        private double GetGoogleTranslationQuality(string googleTranslation, string phoneticTranslation)
        {
            if (googleTranslation.Length == 0 && phoneticTranslation.Length != 0 || googleTranslation.Length == 0 && phoneticTranslation.Length == 0)
            {
                return 0;
            }


            googleTranslation = googleTranslation.ToLower();
            phoneticTranslation = phoneticTranslation.ToLower();


            double factor = 1;
            double hitCount = 0;

            if (googleTranslation.Length > phoneticTranslation.Length)
            {
                factor = 1.15;
            }
            else if (googleTranslation.Length == phoneticTranslation.Length)
            {
                factor = 1;
            }
            else
            {
                factor = 1;
            }
            double lettersInGoogleTranslation = googleTranslation.Length;
            foreach (char letter in phoneticTranslation)
            {
                if (letter != '-')
                {
                    if (letter == 'c' || letter == 'C' || letter == 'k' || letter == 'K')
                    {
                        if (googleTranslation.Contains("k") || googleTranslation.Contains("K") || googleTranslation.Contains("c") || googleTranslation.Contains("C"))
                        {
                            hitCount++;
                        }

                    }
                    else if (googleTranslation.Contains(letter.ToString()))
                    {
                        hitCount = hitCount + (1 * factor);
                    }
                }
            }
            if (googleTranslation.Length > phoneticTranslation.Length)
            {
                return ((hitCount * 100) / lettersInGoogleTranslation) * factor;
            }
            else
            {
                return ((hitCount * 100) / phoneticTranslation.Length) * factor;
            }

        }
        public void Dispose()
        {
            
        }

        #endregion
    }
}
