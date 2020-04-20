//Converted to using NLOG


using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using NLog;
using AdtSvrCmn;

namespace TranslationManager
{
    internal class GoogleTranslator
    {
        GoogleCredential credential = null;
        TranslationClient googleTranslationClient;
        ApplicationConfiguration _config;
        Logger logger;

        public GoogleTranslator(string credFilePath, NLog.Logger logger, ApplicationConfiguration configuration)
        {
            this.logger = LogManager.GetCurrentClassLogger();
            _config = configuration;
            credential = GoogleCredential.FromStream(File.OpenRead(credFilePath));
            googleTranslationClient = TranslationClient.Create(credential, TranslationModel.NeuralMachineTranslation);
        }

        public string GetEnglishLasttNameFromGoogle(string hebrewLastName)
        {
            logger.Trace("inside GetEnglishLAsttNameFromGoogle");
            TranslationResult res = null;
            //if (CheckGoogleTranslateAccess())
            //{
                if (hebrewLastName == string.Empty || hebrewLastName == " ")
                {
                    logger.Debug("No hebrew last name provided - nothing to translate");
                    return string.Empty;
                }
                string lastName = string.Empty;
                logger.Debug($"Triyng to translate {hebrewLastName} to english using google API");
                try
                {
                    res = googleTranslationClient.TranslateText($"ליאת {hebrewLastName}", "en");
                    var temp = res.TranslatedText.Split(' ');
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].ToLower() != "liat")
                        {
                            lastName += temp[i] + "-";
                        }
                    }
                    lastName = lastName.TrimEnd(new char[] { '-' });
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "Error when trying to access google translate API");
                    return string.Empty;
                }

                logger.Debug($"Translation Completed {hebrewLastName} to {lastName}");
                logger.Trace("exiting GetEnglishLAsttNameFromGoogle");
                return lastName;
            


        }

        public string GetEnglishFirstNameFromGoogle(string hebrewFirstName)
        {
            if (CheckGoogleTranslateAccess())
            {
                if (hebrewFirstName == string.Empty || hebrewFirstName == " ")
                {
                    return string.Empty;
                }
                string translation = string.Empty;
                var res = googleTranslationClient.TranslateText($"{hebrewFirstName} כהן", "en");
                var temp = res.TranslatedText.Split(' ');
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].ToLower() != "cohen")
                    {
                        translation += temp[i] + "-";
                    }
                }
                translation = translation.Substring(0, translation.Length - 1);
                return translation;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool CheckGoogleTranslateAccess()
        {
            logger.Trace("entering CheckGoogleTranslateAccess ");
            WebRequest req = WebRequest.Create("https://translation.googleapis.com/language/translate/v2");

            try
            {

                var setting = ConfigurationManager.AppSettings;
                req.Timeout = int.Parse(_config.WebTimeOut);
                req.GetResponse();
                return true;
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("403"))
                {
                    logger.Debug("Network access to  translation API - O.K", "translation manager");
                    return true;
                }
                else
                {
                    NlogHelper.CreateLogEntry("Some Problem Accessing translation api","1000",LogLevel.Debug,logger);
                    logger.Warn(ex);
                    return false;
                }

            }
        }

       
    }
}
