using System;
using System.Collections.Generic;
using System.IO;
using DbLayer;
using AdtSvrCmn.Objects;
using Google.Cloud.Translation.V2;
using AdtSvrCmn.Interfaces;
using NLog;
using SqlConnector;
using System.Configuration;

namespace TranslationManager
{
    public class TranslationManger : ITranslator, IDisposable
    {
        #region Private Fields
        Dictionary<string, string> data;
        string dbFilePath;
        IDbConnector dbCon;
        string cerdFilePath;
        TranslationResult res;
        private object locker = new object();
        private ApplicationConfiguration _config;
        //private IApplicationLogger logger;
        System.Timers.Timer loadDataTimer = new System.Timers.Timer(3600000);
        INormalizer normalizer;
        GoogleTranslator googleTranslator;
        Logger logger;
        #endregion
        NamesManager man;


        internal TranslationManger(TranslationManagerData translationSetupdata)
        {
            this.normalizer = translationSetupdata.NameNormalyzer;
            loadDataTimer.Elapsed += LoadDataTimer_Elapsed;
            loadDataTimer.Start();
            _config = translationSetupdata.Config;
            this.logger = LogManager.GetCurrentClassLogger();
            try
            {
                cerdFilePath = Path.Combine(_config.GoogleCredentialFilePath, _config.GoogleCredentialFileName);
                dbFilePath = Path.Combine(_config.DataBaseFolder, _config.DataBaseFileName);


                if(ConfigurationManager.AppSettings["UseSql"] == "true")
                {
                    dbCon = DbFactory.GetSqlConnector();
                }
                else
                {
                    dbCon = DbFactory.GetDbConnector(_config);
                }


                data = dbCon.GetData();

                //data = dbCon.GetData();
                googleTranslator = new GoogleTranslator(cerdFilePath, logger, _config);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw new Exception("From Translation manager constructor", ex);
            }


        }

        // load new data from local database at specific interval 3600000 msec = 60 minutes
        private void LoadDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            data = dbCon.GetData();
        }

        public PatientTranslationObject GetEnglishName(string firstName, string lastName)
        {
            firstName = normalizer.Normalize(firstName);
            lastName = normalizer.Normalize(lastName);
            string googleEnglishFirstName = string.Empty;
            string googleEnglishLastName = string.Empty;
            double googleFirstNameTranslationQuality = 0;
            double googleLastNameTranslationQuality = 0;

            string finalEnglishFirstName = string.Empty;
            string finalEnglishLastName = string.Empty;

            string textToTranslate = firstName + " " + lastName;

            PatientTranslationObject pat = new PatientTranslationObject()
            {
                HebrewFirstName = firstName,
                HebrewLastName = lastName
            };


            // Do we go to local Db for the names ??
            if (_config.UseLocalDb)
            {
                pat = GetNamesFromLocalDb(textToTranslate);
                if (pat.EnglishFirstName != string.Empty && pat.EnglishLastName != string.Empty &&
                    pat.EnglishFirstName != null && pat.EnglishLastName != null)
                {
                    return pat;
                }
            }

            // do we try to get google translation ???
            // if NOT only phonetic translations will be used !!
            if (_config.MustGetTranslation)
            {
                try
                {
                    googleEnglishFirstName = googleTranslator.GetEnglishFirstNameFromGoogle(pat.HebrewFirstName);
                    googleEnglishLastName = googleTranslator.GetEnglishLAsttNameFromGoogle(pat.HebrewLastName);
                }
                catch (Exception ex)
                {
                    logger.Debug("Problem getting google translation - using phonetic translation see MUSEADT log for more details");
                    logger.Error(ex.Message);
                }

            }

            string phoneticEnglishFirstName = PhoneticTranslator.GetPhoneticTranslation(pat.HebrewFirstName);
            string phoneticEnglishLastName = PhoneticTranslator.GetPhoneticTranslation(pat.HebrewLastName);

            googleFirstNameTranslationQuality = GetGoogleTranslationQuality(googleEnglishFirstName, phoneticEnglishFirstName);
            googleLastNameTranslationQuality = GetGoogleTranslationQuality(googleEnglishLastName, phoneticEnglishLastName);
            var q1 = LevenshteinDistance.Compute(googleEnglishFirstName, phoneticEnglishFirstName);
            var q2 = LevenshteinDistance.Compute(googleEnglishLastName, phoneticEnglishLastName);

          

            if (googleFirstNameTranslationQuality < 50 || (googleEnglishFirstName.Length > 8 ? q1 >= 6 : q1 > 4))
            {
                finalEnglishFirstName = phoneticEnglishFirstName;
            }
            else
            {
                finalEnglishFirstName = googleEnglishFirstName;
                pat.IsEnglishFirstNameByGoogle = true;
                UpdateNamesData(pat.HebrewFirstName, finalEnglishFirstName);
                //todo: find and update the entry in the local database
            }
            if (googleLastNameTranslationQuality < 50 || (googleEnglishLastName.Length > 8 ? q2 >= 6 : q2 > 4))
            {
                finalEnglishLastName = phoneticEnglishLastName;
            
            }
            else
            {
                finalEnglishLastName = googleEnglishLastName;
                pat.IsEnglishLastNameByGoogle = true;
                UpdateNamesData(pat.HebrewLastName, finalEnglishLastName);
                //todo: find and update the entry in the local database
            }

            res = new TranslationResult(textToTranslate, finalEnglishFirstName + " " + finalEnglishLastName, "iw", "hebrew", "en", TranslationModel.ServiceDefault);
            pat.EnglishFirstName = finalEnglishFirstName;
            pat.EnglishLastName = finalEnglishLastName;

            string translatedText = res.TranslatedText;
            //man.SaveData(data);
            AddTranslationsToDictionay(pat, translatedText);
            SaveData(data);



            logger.Debug($"Hebrew First Name : {pat.HebrewFirstName} , Google : {googleEnglishFirstName} Phonetic : {phoneticEnglishFirstName}");
            logger.Debug($"Hebrew First Name : {pat.HebrewLastName} , Google : {googleEnglishLastName} Phonetic : {phoneticEnglishLastName}");

            return pat;

        }

        private void UpdateNamesData(string HebrewName, string finalEnglishName)
        {
            string value;
            if (data.ContainsKey(HebrewName))
            {
               var result =  data.TryGetValue(HebrewName, out value);
                if (result && value != finalEnglishName)
                {
                    data[HebrewName] = finalEnglishName;
                }
            }
            else
            {
                data.Add(HebrewName, finalEnglishName);
            }

        }


        #region Private Methods

        private void SaveData(Dictionary<string, string> data)
        {
            try
            {
                dbCon.SaveData(data);
            }
            catch (Exception ex)
            {
                throw new Exception("Error Saving to data base", ex);
            }
        }

        private void AddTranslationsToDictionay(PatientTranslationObject pat, string text)
        {
            if (pat.IsEnglishFirstNameByGoogle)
            {
                AddToDictionery(pat.HebrewFirstName, pat.EnglishFirstName,pat.IsEnglishFirstNameByGoogle);
            }
            if (pat.IsEnglishLastNameByGoogle)
            {
                AddToDictionery(pat.HebrewLastName, pat.EnglishLastName,pat.IsEnglishLastNameByGoogle);
            }
           

            //var resultArray = text.Split(' ');
            //if (resultArray[0].ToUpper() == "null".ToUpper())
            //{
            //    pat.EnglishFirstName = "";
            //}
            //else
            //{
            //    pat.EnglishFirstName = resultArray[0];
            //    AddToDictionery(pat.HebrewFirstName, pat.EnglishFirstName);
            //}

            //if (resultArray[1].ToUpper() == "null".ToUpper())
            //{
            //    pat.EnglishLastName = "";
            //}
            //else
            //{
            //    pat.EnglishLastName = resultArray[1];
            //    AddToDictionery(pat.HebrewLastName, pat.EnglishLastName);
            //}
        }

        private PatientTranslationObject GetNamesFromLocalDb(string textToTranslate)
        {
            PatientTranslationObject pat = new PatientTranslationObject();

            var tempArr = textToTranslate.Split(' ');

            for (int i = 0; i < tempArr.Length; i++)
            {
                if (i == 0)
                {
                    pat.HebrewFirstName = tempArr[i];
                }
                if (i == 1)
                {
                    pat.HebrewLastName = tempArr[i];

                }
            }

            pat.EnglishFirstName = GetEnglishNameFromDb(pat.HebrewFirstName);
            pat.EnglishLastName = GetEnglishNameFromDb(pat.HebrewLastName);

            return pat;

        }

        private void AddToDictionery(string hebrewFirstName, string englishFirstName,bool isGoogle)
        {

            string name="";
            if (data.TryGetValue(hebrewFirstName, out name))
            {
                //db name and translation name are the same
                if (name==englishFirstName)
                {
                    return;
                }
                else if (isGoogle)
                {
                    //db and translation names are different but we have a better google translation
                    //update tge recird with google translation
                    data[hebrewFirstName] = englishFirstName;
                    return;
                }
            }
            else
            {
             //   just add a new entry
                data.Add(hebrewFirstName, englishFirstName);
            }
           
        }

        private string GetEnglishNameFromDb(string hebrewFirstName)
        {
            string temp = null;
            data.TryGetValue(hebrewFirstName, out temp);
            return temp;
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
            loadDataTimer.Dispose();
        }

        #endregion
    }
}
