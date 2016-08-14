using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI.Console
{ 
    public class Settings : ISettings
    {
        public bool UseLastCords => true; // Only disable this if your sure what you're doing!


        public string GoogleUsername => Globals.username;

        public string GooglePassword => Globals.password;

        public bool UseIncense => Globals.useincense;

        AuthType ISettings.AuthType => Globals.acc;

        public string PtcUsername => Globals.username;
        public string PtcPassword => Globals.password;
        public double DefaultLatitude => Globals.latitute;
        public double DefaultLongitude => Globals.longitude;
        public double DefaultAltitude => Globals.altitude;

        public bool WalkBackToDefaultLocation => Globals.defLoc;
        public int MaxWalkingRadiusInMeters => Globals.radius;

        public int HoldMaxDoublePokemons => Globals.duplicate;
        public int TelegramLiveStatsDelay => Globals.telDelay;

        public bool sleepatpokemons => Globals.sleepatpokemons;

        public double WalkingSpeedInKilometerPerHour => Globals.speed;

        public bool TransferDoublePokemons => Globals.transfer;
        public int DontTransferWithCPOver => Globals.maxCp;
        public int ivmaxpercent => Globals.ivmaxpercent;

        public bool EvolvePokemonsIfEnoughCandy => Globals.evolve;

        public string TelegramAPIToken => Globals.telAPI;
        public string TelegramName => Globals.telName;

        public int navigation_option => Globals.navigation_option;

        public bool UseLuckyEgg => Globals.useluckyegg;
        public bool UseRazzBerry => Globals.userazzberry;
        public double razzberry_chance => Globals.razzberry_chance;
        public bool keepPokemonsThatCanEvolve => Globals.keepPokemonsThatCanEvolve;
        public bool TransferFirstLowIV => Globals.TransferFirstLowIV;
 
        public bool UseBasicIncubators => Globals.useBasicIncubators;
        public bool pokevision => Globals.pokevision;
         
        public bool AutoIncubate => Globals.autoIncubate;
        public bool UseLuckyEggIfNotRunning => Globals.useLuckyEggIfNotRunning;

        public bool Language => Globals.gerNames;

        public string SelectedLanguage => Globals.settingsLanguage;
        /*
                 string UseProxyHost { get; set; }
                int UseProxyPort { get; set; }
                string UseProxyUsername { get; set; }
                string UseProxyPassword { get; set; }

                bool UseProxyVerified { get; set; }
                bool UseProxyAuthentication { get; set; }
                */
        public string UseProxyHost
        {
            get
            {
                return UserSettings.Default.UseProxyHost;
            }

            set
            {
                UserSettings.Default.UseProxyHost = value;
                UserSettings.Default.Save();
            }
        }

        public int UseProxyPort
        {
            get
            {
                return UserSettings.Default.UseProxyPort;
            }

            set
            {
                UserSettings.Default.UseProxyPort = value;
                UserSettings.Default.Save();
            }
        }

        public string UseProxyUsername
        {
            get
            {
                return UserSettings.Default.UseProxyUsername;
            }

            set
            {
                UserSettings.Default.UseProxyUsername = value;
                UserSettings.Default.Save();
            }
        }

        public string UseProxyPassword
        {
            get
            {
                return UserSettings.Default.UseProxyPassword;
            }

            set
            {
                UserSettings.Default.UseProxyPassword = value;
                UserSettings.Default.Save();
            }
        }

        public bool UseProxyVerified
        {
            get
            {
                return UserSettings.Default.UseProxyVerified;
            }

            set
            {
                UserSettings.Default.UseProxyVerified = value;
                UserSettings.Default.Save();
            }
        }

        public bool UseProxyAuthentication
        {
            get
            {
                return UserSettings.Default.UseProxyAuthentication;
            }

            set
            {
                UserSettings.Default.UseProxyAuthentication = value;
                UserSettings.Default.Save();
            }
        }

        List<PokemonId> ISettings.catchPokemonSkipList
        {
            get
            {
                var catchPokemonSkipList = new List<PokemonId>();
                foreach (PokemonId pokemon in Globals.noCatch)
                {
                    catchPokemonSkipList.Add(pokemon);
                }

                return catchPokemonSkipList;
            }
        }

        List<PokemonId> ISettings.pokemonsToHold
        {
            get
            {
                //Type and amount to keep
                List<PokemonId> pokemonsToHold = new List<PokemonId>();

                foreach (PokemonId pokemon in Globals.noTransfer)
                {
                    pokemonsToHold.Add(pokemon);
                }

                return pokemonsToHold;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        List<PokemonId> ISettings.pokemonsToEvolve
        {
            get
            {
                List<PokemonId> pokemonsToEvolve = new List<PokemonId>();
                foreach (PokemonId pokemon in Globals.doEvolve)
                {
                    pokemonsToEvolve.Add(pokemon);
                }

                return pokemonsToEvolve;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        ICollection<KeyValuePair<ItemId, int>> ISettings.itemRecycleFilter
        {
            get
            {
                //Type and amount to keep
                return new[]
                {
                    new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, Globals.pokeball),
                    new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, Globals.greatball),
                    new KeyValuePair<ItemId, int>(ItemId.ItemUltraBall, Globals.ultraball),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMasterBall, Globals.masterball),
                    new KeyValuePair<ItemId, int>(ItemId.ItemRevive, Globals.revive),
                    new KeyValuePair<ItemId, int>(ItemId.ItemPotion, Globals.potion),
                    new KeyValuePair<ItemId, int>(ItemId.ItemSuperPotion, Globals.superpotion),
                    new KeyValuePair<ItemId, int>(ItemId.ItemHyperPotion, Globals.hyperpotion),
                    new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, Globals.berry),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxPotion, Globals.toppotion),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxRevive, Globals.toprevive)

                };
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public string GoogleRefreshToken
        {
            get { return UserSettings.Default.GoogleRefreshToken; }
            set
            {
                UserSettings.Default.GoogleRefreshToken = value;
                UserSettings.Default.Save();
            }
        }
    }
}
