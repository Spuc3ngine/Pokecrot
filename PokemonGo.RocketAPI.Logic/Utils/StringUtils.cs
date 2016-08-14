using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Logic.Utils
{
    public static class StringUtils
    {
        public static string getPokemonNameGer(PokemonId b)
        {
            string GerName = string.Empty;
            switch (b)
            {
                case PokemonId.Missingno:
                    GerName = "Missingno";
                    break;
                case PokemonId.Bulbasaur:
                    GerName = "Bisasam";
                    break;
                case PokemonId.Ivysaur:
                    GerName = "Bisaknosp";
                    break;
                case PokemonId.Venusaur:
                    GerName = "Bisaflor";
                    break;
                case PokemonId.Charmander:
                    GerName = "Glumanda";
                    break;
                case PokemonId.Charmeleon:
                    GerName = "Glutexo";
                    break;
                case PokemonId.Charizard:
                    GerName = "Glurak";
                    break;
                case PokemonId.Squirtle:
                    GerName = "Schiggy";
                    break;
                case PokemonId.Wartortle:
                    GerName = "Schillok";
                    break;
                case PokemonId.Blastoise:
                    GerName = "Turtok";
                    break;
                case PokemonId.Caterpie:
                    GerName = "Raupy";
                    break;
                case PokemonId.Metapod:
                    GerName = "Safcon";
                    break;
                case PokemonId.Butterfree:
                    GerName = "Smettbo";
                    break;
                case PokemonId.Weedle:
                    GerName = "Hornliu";
                    break;
                case PokemonId.Kakuna:
                    GerName = "Kokuna";
                    break;
                case PokemonId.Beedrill:
                    GerName = "Bibor";
                    break;
                case PokemonId.Pidgey:
                    GerName = "Taubsi";
                    break;
                case PokemonId.Pidgeotto:
                    GerName = "Tauboga";
                    break;
                case PokemonId.Pidgeot:
                    GerName = "Tauboss";
                    break;
                case PokemonId.Rattata:
                    GerName = "Rattfratz";
                    break;
                case PokemonId.Raticate:
                    GerName = "Rattikarl";
                    break;
                case PokemonId.Spearow:
                    GerName = "Habitak";
                    break;
                case PokemonId.Fearow:
                    GerName = "Ibitak";
                    break;
                case PokemonId.Ekans:
                    GerName = "Rettan";
                    break;
                case PokemonId.Arbok:
                    GerName = "Arbok";
                    break;
                case PokemonId.Pikachu:
                    GerName = "Pikachu";
                    break;
                case PokemonId.Raichu:
                    GerName = "Raichu";
                    break;
                case PokemonId.Sandshrew:
                    GerName = "Sandan";
                    break;
                case PokemonId.Sandslash:
                    GerName = "Sandamer";
                    break;
                case PokemonId.NidoranFemale:
                    GerName = "Weibliches Nidoran";
                    break;
                case PokemonId.Nidorina:
                    GerName = "Nidorina";
                    break;
                case PokemonId.Nidoqueen:
                    GerName = "Nidoqueen";
                    break;
                case PokemonId.NidoranMale:
                    GerName = "Maennliches Nidoran";
                    break;
                case PokemonId.Nidorino:
                    GerName = "Nidorino";
                    break;
                case PokemonId.Nidoking:
                    GerName = "Nidoking";
                    break;
                case PokemonId.Clefairy:
                    GerName = "Piepi";
                    break;
                case PokemonId.Clefable:
                    GerName = "Pixi";
                    break;
                case PokemonId.Vulpix:
                    GerName = "Vulpix";
                    break;
                case PokemonId.Ninetales:
                    GerName = "Vulnona";
                    break;
                case PokemonId.Jigglypuff:
                    GerName = "Pummeluff";
                    break;
                case PokemonId.Wigglytuff:
                    GerName = "Knuddeluff";
                    break;
                case PokemonId.Zubat:
                    GerName = "Zubat";
                    break;
                case PokemonId.Golbat:
                    GerName = "Golbat";
                    break;
                case PokemonId.Oddish:
                    GerName = "Myrapla";
                    break;
                case PokemonId.Gloom:
                    GerName = "Duflor";
                    break;
                case PokemonId.Vileplume:
                    GerName = "Giflor";
                    break;
                case PokemonId.Paras:
                    GerName = "Paras";
                    break;
                case PokemonId.Parasect:
                    GerName = "Parasek";
                    break;
                case PokemonId.Venonat:
                    GerName = "Bluzuk";
                    break;
                case PokemonId.Venomoth:
                    GerName = "Omot";
                    break;
                case PokemonId.Diglett:
                    GerName = "Digda";
                    break;
                case PokemonId.Dugtrio:
                    GerName = "Digdri";
                    break;
                case PokemonId.Meowth:
                    GerName = "Mauzi";
                    break;
                case PokemonId.Persian:
                    GerName = "Snobilikat";
                    break;
                case PokemonId.Psyduck:
                    GerName = "Enton";
                    break;
                case PokemonId.Golduck:
                    GerName = "Entoron";
                    break;
                case PokemonId.Mankey:
                    GerName = "Menki";
                    break;
                case PokemonId.Primeape:
                    GerName = "Rasaff";
                    break;
                case PokemonId.Growlithe:
                    GerName = "Fukano";
                    break;
                case PokemonId.Arcanine:
                    GerName = "Arkani";
                    break;
                case PokemonId.Poliwag:
                    GerName = "Quapsel";
                    break;
                case PokemonId.Poliwhirl:
                    GerName = "Quaputzi";
                    break;
                case PokemonId.Poliwrath:
                    GerName = "Quappo";
                    break;
                case PokemonId.Abra:
                    GerName = "Abra";
                    break;
                case PokemonId.Kadabra:
                    GerName = "Kadabra";
                    break;
                case PokemonId.Alakazam:
                    GerName = "Simsala";
                    break;
                case PokemonId.Machop:
                    GerName = "Machollo";
                    break;
                case PokemonId.Machoke:
                    GerName = "Maschock";
                    break;
                case PokemonId.Machamp:
                    GerName = "Machomei";
                    break;
                case PokemonId.Bellsprout:
                    GerName = "Knofensa";
                    break;
                case PokemonId.Weepinbell:
                    GerName = "Ultrigaria";
                    break;
                case PokemonId.Victreebel:
                    GerName = "Sarzenia";
                    break;
                case PokemonId.Tentacool:
                    GerName = "Tentacha";
                    break;
                case PokemonId.Tentacruel:
                    GerName = "Tentoxa";
                    break;
                case PokemonId.Geodude:
                    GerName = "Kleinstein";
                    break;
                case PokemonId.Graveler:
                    GerName = "Georok";
                    break;
                case PokemonId.Golem:
                    GerName = "Geowaz";
                    break;
                case PokemonId.Ponyta:
                    GerName = "Ponita";
                    break;
                case PokemonId.Rapidash:
                    GerName = "Gallopa";
                    break;
                case PokemonId.Slowpoke:
                    GerName = "Flegmon";
                    break;
                case PokemonId.Slowbro:
                    GerName = "Lahmus";
                    break;
                case PokemonId.Magnemite:
                    GerName = "Magnetilo";
                    break;
                case PokemonId.Magneton:
                    GerName = "Magneton";
                    break;
                case PokemonId.Farfetchd:
                    GerName = "Porenta";
                    break;
                case PokemonId.Doduo:
                    GerName = "Dodu";
                    break;
                case PokemonId.Dodrio:
                    GerName = "Dodri";
                    break;
                case PokemonId.Seel:
                    GerName = "Jurob";
                    break;
                case PokemonId.Dewgong:
                    GerName = "Jugong";
                    break;
                case PokemonId.Grimer:
                    GerName = "Sleima";
                    break;
                case PokemonId.Muk:
                    GerName = "Sleimok";
                    break;
                case PokemonId.Shellder:
                    GerName = "Muschas";
                    break;
                case PokemonId.Cloyster:
                    GerName = "Austos";
                    break;
                case PokemonId.Gastly:
                    GerName = "Nebulak";
                    break;
                case PokemonId.Haunter:
                    GerName = "Alpollo";
                    break;
                case PokemonId.Gengar:
                    GerName = "Gengar";
                    break;
                case PokemonId.Onix:
                    GerName = "Onix";
                    break;
                case PokemonId.Drowzee:
                    GerName = "Traumato";
                    break;
                case PokemonId.Hypno:
                    GerName = "Hypno";
                    break;
                case PokemonId.Krabby:
                    GerName = "Krabby";
                    break;
                case PokemonId.Kingler:
                    GerName = "Kingler";
                    break;
                case PokemonId.Voltorb:
                    GerName = "Voltobal";
                    break;
                case PokemonId.Electrode:
                    GerName = "Lektrobal";
                    break;
                case PokemonId.Exeggcute:
                    GerName = "Owei";
                    break;
                case PokemonId.Exeggutor:
                    GerName = "Kokowei";
                    break;
                case PokemonId.Cubone:
                    GerName = "Tragosso";
                    break;
                case PokemonId.Marowak:
                    GerName = "Knogga";
                    break;
                case PokemonId.Hitmonlee:
                    GerName = "Kicklee";
                    break;
                case PokemonId.Hitmonchan:
                    GerName = "Nockchan";
                    break;
                case PokemonId.Lickitung:
                    GerName = "Schlurp";
                    break;
                case PokemonId.Koffing:
                    GerName = "Smogon";
                    break;
                case PokemonId.Weezing:
                    GerName = "Smogmog";
                    break;
                case PokemonId.Rhyhorn:
                    GerName = "Rihorn";
                    break;
                case PokemonId.Rhydon:
                    GerName = "Rizeros";
                    break;
                case PokemonId.Chansey:
                    GerName = "Chaneira";
                    break;
                case PokemonId.Tangela:
                    GerName = "Tangela";
                    break;
                case PokemonId.Kangaskhan:
                    GerName = "Kangama";
                    break;
                case PokemonId.Horsea:
                    GerName = "Seeper";
                    break;
                case PokemonId.Seadra:
                    GerName = "Seemon";
                    break;
                case PokemonId.Goldeen:
                    GerName = "Goldini";
                    break;
                case PokemonId.Seaking:
                    GerName = "Golking";
                    break;
                case PokemonId.Staryu:
                    GerName = "Sterndu";
                    break;
                case PokemonId.Starmie:
                    GerName = "Starmie";
                    break;
                case PokemonId.MrMime:
                    GerName = "Pantimos";
                    break;
                case PokemonId.Scyther:
                    GerName = "Sichlor";
                    break;
                case PokemonId.Jynx:
                    GerName = "Rossana";
                    break;
                case PokemonId.Electabuzz:
                    GerName = "Elektek";
                    break;
                case PokemonId.Magmar:
                    GerName = "Magmar";
                    break;
                case PokemonId.Pinsir:
                    GerName = "Pinsir";
                    break;
                case PokemonId.Tauros:
                    GerName = "Tauros";
                    break;
                case PokemonId.Magikarp:
                    GerName = "Karpador";
                    break;
                case PokemonId.Gyarados:
                    GerName = "Garados";
                    break;
                case PokemonId.Lapras:
                    GerName = "Lapras";
                    break;
                case PokemonId.Ditto:
                    GerName = "Ditto";
                    break;
                case PokemonId.Eevee:
                    GerName = "Evoli";
                    break;
                case PokemonId.Vaporeon:
                    GerName = "Aquana";
                    break;
                case PokemonId.Jolteon:
                    GerName = "Blitza";
                    break;
                case PokemonId.Flareon:
                    GerName = "Flamara";
                    break;
                case PokemonId.Porygon:
                    GerName = "Porygon";
                    break;
                case PokemonId.Omanyte:
                    GerName = "Amonitas";
                    break;
                case PokemonId.Omastar:
                    GerName = "Amoroso";
                    break;
                case PokemonId.Kabuto:
                    GerName = "Kabuto";
                    break;
                case PokemonId.Kabutops:
                    GerName = "Kabutops";
                    break;
                case PokemonId.Aerodactyl:
                    GerName = "Aerodactyl";
                    break;
                case PokemonId.Snorlax:
                    GerName = "Relaxo";
                    break;
                case PokemonId.Articuno:
                    GerName = "Arktos";
                    break;
                case PokemonId.Zapdos:
                    GerName = "Zapdos";
                    break;
                case PokemonId.Moltres:
                    GerName = "Lavados";
                    break;
                case PokemonId.Dratini:
                    GerName = "Dratini";
                    break;
                case PokemonId.Dragonair:
                    GerName = "Dragonir";
                    break;
                case PokemonId.Dragonite:
                    GerName = "Dragoran";
                    break;
                case PokemonId.Mewtwo:
                    GerName = "Mewtu";
                    break;
                case PokemonId.Mew:
                    GerName = "Mew";
                    break;
                default:
                    GerName = "Unknown Pokemon";
                    break;

            }
            return GerName;
        }

        public static string getPokemonNameByLanguage(ISettings clientSettings, PokemonId b)
        {
            if (clientSettings.Language)
            {
                return getPokemonNameGer(b);
                        
            }
            else
            {
                return b.ToString();
            }
        }

        public static int getExpDiff(int level)
        {
            switch (level)
            {
                case 1:
                    return 0;
                case 2:
                    return 1000;
                case 3:
                    return 2000;
                case 4:
                    return 3000;
                case 5:
                    return 4000;
                case 6:
                    return 5000;
                case 7:
                    return 6000;
                case 8:
                    return 7000;
                case 9:
                    return 8000;
                case 10:
                    return 9000;
                case 11:
                    return 10000;
                case 12:
                    return 10000;
                case 13:
                    return 10000;
                case 14:
                    return 10000;
                case 15:
                    return 15000;
                case 16:
                    return 20000;
                case 17:
                    return 20000;
                case 18:
                    return 20000;
                case 19:
                    return 25000;
                case 20:
                    return 25000;
                case 21:
                    return 50000;
                case 22:
                    return 75000;
                case 23:
                    return 100000;
                case 24:
                    return 125000;
                case 25:
                    return 150000;
                case 26:
                    return 190000;
                case 27:
                    return 200000;
                case 28:
                    return 250000;
                case 29:
                    return 300000;
                case 30:
                    return 350000;
                case 31:
                    return 500000;
                case 32:
                    return 500000;
                case 33:
                    return 750000;
                case 34:
                    return 1000000;
                case 35:
                    return 1250000;
                case 36:
                    return 1500000;
                case 37:
                    return 2000000;
                case 38:
                    return 2500000;
                case 39:
                    return 5000000;
                case 40:
                    return 1000000;
            }
            return 0;
        }

        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<ItemAward> items)
        {
            var enumerable = items as IList<ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                          .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }

        public static bool CheckKillSwitch()
        {
            using (var wc = new WebClient())
            {
                try
                {
                    string strResponse = wc.DownloadString("https://raw.githubusercontent.com/hartmantam/PokemonGo-Bot-Ar1i/master/switch.txt");

                    if (strResponse == null)
                        return false;

                    string[] strSplit = strResponse.Split(';');

                    if (strSplit.Length > 1)
                    {
                        string strStatus = strSplit[0];
                        string strReason = strSplit[1];

                        if (strStatus.ToLower().Contains("false"))
                        {
                            Logger.Error(strReason + "\n");

                            Logger.Error("The bot will now close, please press enter to continue");
                            Console.ReadLine();
                            return true;
                        }
                    }
                    else
                        return false;
                }
                catch (WebException)
                {
                }
            }

            return false;
        }

    }
}
