using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

/**
    Written by DeltaCore (https://github.com/DeltaCore)
*/

namespace PokemonGo.RocketAPI.Console.Translation
{
    class Translation
    {

        private Dictionary<String, String> lookup;
        private String countryCode;

        public String getString(String messageKey)
        {
            if (this.lookup.ContainsKey(messageKey))
            {
                return lookup[messageKey];
            }
            return null;
        }

        public void addString(String messageKey, String value)
        {
            this.lookup[messageKey] = value;
        }

        public Translation(String languageCode)
        {
            this.countryCode = languageCode;

            if(System.IO.File.Exists("Translations/" + this.countryCode + ".json")) { 

                if(countryCode != null) { 
                    lookup = JsonConvert.DeserializeObject<Dictionary<String, String>>(System.IO.File.ReadAllText("Translations/" + this.countryCode + ".json"));
                }
            }
            else
            {
                lookup = new Dictionary<string, string>();
            }
        }

        public void save()
        {
            System.IO.File.WriteAllText("Translations/" + this.countryCode + ".json", JsonConvert.SerializeObject(lookup).Replace("\",", "\",\n").Replace("{", "{\n").Replace("}", "\n}"));
        }

    }
}
