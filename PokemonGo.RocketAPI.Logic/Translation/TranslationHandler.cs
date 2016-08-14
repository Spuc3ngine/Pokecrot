using System;
using System.Collections.Generic;

/**
    Written by DeltaCore (https://github.com/DeltaCore)
*/

namespace PokemonGo.RocketAPI.Logic.Translation
{
    public class TranslationHandler
    {

        private static readonly Dictionary<string, Translation> Translations = new Dictionary<string, Translation>();
        private static string _selectedLanguage;

        public static void Init()
        {
            foreach (string file in System.IO.Directory.GetFiles("Translations"))
            {
                if (file.EndsWith(".json"))
                {
                    string code = file.Substring(file.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
                    code = code.Substring(0, code.LastIndexOf(".", StringComparison.Ordinal));

                    if (!Translations.ContainsKey(code))
                    {
                        Translations[code] = new Translation(code);
                    }
                }
            }
        }

        public static void SelectLangauge(string key)
        {
            _selectedLanguage = key;
        }

        public static string GetString(string messageKey, string defaultVal)
        {
            if (_selectedLanguage == null)
            {
                return defaultVal;
            }

            if (!Translations.ContainsKey(_selectedLanguage))
            {
                return defaultVal;
            }

            return Translations[_selectedLanguage].getString(messageKey) ?? defaultVal;
        }
    }
}
