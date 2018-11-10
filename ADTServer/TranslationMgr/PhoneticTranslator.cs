using System.Collections.Generic;
using NLog;

namespace TranslationManager
{
    internal static class PhoneticTranslator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string GetPhoneticTranslation(string hebrewInput)
        {
            logger.Trace("Inside GetPhoneticTranslation ");
            if (hebrewInput == "דוד")
            {
                return "David";
            }
            string lastLetter = string.Empty;
            string FirstLetter = string.Empty;

            Dictionary<string, string> letters = new Dictionary<string, string>()
            {
                {"א","a"},{"ב","v"},{"ג","g"},{"ד","d"},{"ה","h"},{"ו","o"},{"ז","z"},
                {"ח","ch"},{"ט","t"},{"י","i"},{"כ","ch"},{"ל","l"},{"מ","m"},{"נ","n"},
                {"ס","s"},{"ע","a"},{"פ","f"},{"צ","tz"},{"ק","k"},{"ר","r"},{"ש","sh"},{"ת","t"},{"ם","m"},{"ן","n"},
                {"ץ","tz"},{"ך","ch"},{" "," "},{"-","-"}
            };

            string englishTemp = string.Empty;

            char currentLetter = 'Z';
            char nextLetter = 'Z';
            int currentIndex = 0;

            for (currentIndex = 0; currentIndex < hebrewInput.Length; currentIndex++)
            {
                currentLetter = hebrewInput[currentIndex];
                if (currentIndex + 1 <= hebrewInput.Length - 1)
                {
                    nextLetter = hebrewInput[currentIndex + 1];
                }
                else
                {
                    nextLetter = 'Z';
                }

                string temp = string.Empty;
                if (letters.TryGetValue(currentLetter.ToString(), out temp))
                {
                    // Set first (and second) Letters
                    if (currentIndex == 0)
                    {

                        if (currentLetter == 'ו')
                        {
                            temp = "V";
                        }
                        else if (currentLetter == 'ח')
                        {
                            temp = "H";
                        }
                        else if (hebrewInput[currentIndex] == 'ב')
                        {
                            temp = "B";
                        }
                        else if (currentLetter == 'ח')
                        {
                            temp = "H";
                        }
                        else if (currentLetter == 'י')
                        {
                            temp = "Y";
                        }
                        else if (currentLetter == 'פ')
                        {
                            temp = "P";
                        }
                        else if (currentLetter == 'כ')
                        {
                            temp = "K";
                        }
                        else if (currentLetter == 'צ')
                        {
                            temp = "Tz";
                            englishTemp += temp;
                            continue;
                        }
                        if (temp.Length > 1)
                        {
                            temp = Capitalize(temp);
                        }
                        else
                        {
                            temp = temp.ToUpper();
                        }
                        englishTemp += temp;
                    }

                    // handle last letter
                    else if (currentIndex == hebrewInput.Length - 1)
                    {
                        if (currentLetter == 'ו' || currentLetter == 'ב')
                        {
                            temp = "v";
                        }
                        if (currentLetter == 'י')
                        {
                            temp = "y";
                        }
                        if (currentLetter == 'ה' || currentLetter == 'א')
                        {
                            temp = "a";
                        }
                        englishTemp += temp;
                    }

                    // handle mid letters
                    else
                    {
                        if (currentLetter == 'ב' && nextLetter == 'ו')
                        {
                            temp = "bu";
                            currentIndex++;
                        }
                        else if ((currentLetter == 'ו' && nextLetter == 'ו') && currentIndex != hebrewInput.Length - 1)
                        {
                            temp = "v";
                            currentIndex++;
                        }

                        englishTemp += temp;

                    }


                }
            }
            logger.Trace("exiting GetPhoneticTranslation ");
            logger.Debug($"Translated {hebrewInput} to {englishTemp}");
            return englishTemp;


        }

        private static string Capitalize(string temp)
        {
            string data = string.Empty;
            for (int i = 0; i < temp.Length; i++)
            {
                if (i == 0)
                {
                    data += temp[i].ToString().ToUpper();
                }
                else
                {
                    data += temp[i];
                }

            }
            return data;
        }
    }

}
