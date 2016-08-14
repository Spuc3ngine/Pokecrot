using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
    Written by DeltaCore (https://github.com/DeltaCore)
*/

namespace PokemonGo.RocketAPI.Console.Translation
{
    class TranslationHandler
    {

        private static Dictionary<String, Translation> translations = new Dictionary<string, Translation>();
        private static String selectedLanguage = "";

        public static void init()
        {
            foreach(String file in System.IO.Directory.GetFiles("Translations"))
            {
                if (file.EndsWith(".json"))
                {
                    String code = file.Substring(file.LastIndexOf(@"\") + 1);
                    code = code.Substring(0, code.LastIndexOf("."));

                    if (!translations.ContainsKey(code))
                    {
                        translations[code] = new Translation(code);
                    }
                    else
                    {
                        throw new Exception("Language already defined");
                    }
                }
            }
        }

        public static void selectLangauge(String key)
        {
            selectedLanguage = key;
        }

        public static String getString(String messageKey, String defaultVal)
        {

            if(selectedLanguage == null) {
                return defaultVal;
            }


            if (!translations.ContainsKey(selectedLanguage))
            {
                translations[selectedLanguage] = new Translation(selectedLanguage);
            }

            if(translations[selectedLanguage].getString(messageKey) == null)
            {
                translations[selectedLanguage].addString(messageKey, defaultVal);
                translations[selectedLanguage].save();
            }
            return (translations[selectedLanguage].getString(messageKey) == null ? defaultVal : translations[selectedLanguage].getString(messageKey));
        }

    }
}
