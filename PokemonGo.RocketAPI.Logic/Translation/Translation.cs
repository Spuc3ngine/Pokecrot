using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

/**
    Written by DeltaCore (https://github.com/DeltaCore)
*/

namespace PokemonGo.RocketAPI.Logic.Translation
{
    public class Translation
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

        public Translation(String languageCode)
        {
            this.countryCode = languageCode;

            if(System.IO.File.Exists("Translations/" + this.countryCode + ".json")) { 

                if(countryCode != null) { 
                    lookup = JsonConvert.DeserializeObject<Dictionary<String, String>>(System.IO.File.ReadAllText("translations/" + this.countryCode + ".json", Encoding.UTF8));
                }
            }
            else
            {
                lookup = new Dictionary<string, string>();
            }
        }
    }
}
