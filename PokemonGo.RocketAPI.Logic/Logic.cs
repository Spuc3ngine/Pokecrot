using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Logic.Utils;
using PokemonGo.RocketAPI.Exceptions;
using System.Net;
using System.IO;
using System.Device.Location;
using PokemonGo.RocketAPI.Helpers;
using System.Web.Script.Serialization;
using POGOProtos.Map.Pokemon;
using POGOProtos.Inventory.Item;
using POGOProtos.Enums;
using POGOProtos.Networking.Responses;
using POGOProtos.Map.Fort;
using POGOProtos.Data;
using System.Threading;
using POGOProtos.Inventory;
using Newtonsoft.Json;

namespace PokemonGo.RocketAPI.Logic
{
    public class Logic
    {
        public readonly Client _client;
        public readonly ISettings _clientSettings; 
        public TelegramUtil _telegram;
        public BotStats _botStats;
        private readonly Navigation _navigation;
        public const double SpeedDownTo = 10 / 3.6;
        private readonly LogicInfoObservable _infoObservable;
        private readonly PokeVisionUtil _pokevision;


        public Logic(ISettings clientSettings, LogicInfoObservable infoObservable)
        {
            _clientSettings = clientSettings;
            _client = new Client(_clientSettings);
            _client.setFailure(new ApiFailureStrat(_client));
            _botStats = new BotStats();
            _navigation = new Navigation(_client);
            _pokevision = new PokeVisionUtil();
            _infoObservable = infoObservable;
        }

        public async Task Execute()
        {

            // Check if disabled
            StringUtils.CheckKillSwitch();

            Logger.ColoredConsoleWrite(ConsoleColor.Red, "PokecrotBOT Is Free For Use");
            Logger.ColoredConsoleWrite(ConsoleColor.Yellow, "Powered By Pokecrot.com");
            Logger.ColoredConsoleWrite(ConsoleColor.Green, $"Starting Execute on login server: {_clientSettings.AuthType}", LogLevel.Info);

            _client.CurrentAltitude = _client.Settings.DefaultAltitude;
            _client.CurrentLongitude = _client.Settings.DefaultLongitude;
            _client.CurrentLatitude = _client.Settings.DefaultLatitude;

            if (_client.CurrentAltitude == 0)
            {
                _client.CurrentAltitude = LocationUtils.getAltidude(_client.CurrentLatitude, _client.CurrentLongitude); 
                Logger.Error("Altidude was 0, resolved that. New Altidude is now: " + _client.CurrentAltitude);
            }

            if (_clientSettings.UseProxyVerified)
            {
                Logger.Error("===============================================");
                Logger.Error("Proxy enabled.");
                Logger.Error("ProxyIP: " + _clientSettings.UseProxyHost + ":" + _clientSettings.UseProxyPort);
                Logger.Error("===============================================");
            } 

            while (true)
            {
                try
                {
                    await _client.Login.DoLogin();

                    
                    if (!string.IsNullOrEmpty(_clientSettings.TelegramAPIToken) && !string.IsNullOrEmpty(_clientSettings.TelegramName))
                    {
                        try
                        {
                            _telegram = new TelegramUtil(_client, new Telegram.Bot.TelegramBotClient(_clientSettings.TelegramAPIToken), _clientSettings, _client.Inventory);

                            Logger.ColoredConsoleWrite(ConsoleColor.Green, "To activate informations with Telegram, write the bot a message for more informations");
                            var me = await _telegram.getClient().GetMeAsync();
                            _telegram.getClient().OnCallbackQuery += _telegram.BotOnCallbackQueryReceived;
                            _telegram.getClient().OnMessage += _telegram.BotOnMessageReceived;
                            _telegram.getClient().OnMessageEdited += _telegram.BotOnMessageReceived;
                            Logger.ColoredConsoleWrite(ConsoleColor.Green, "Telegram Name: " + me.Username);
                            _telegram.getClient().StartReceiving();
                        }
                        catch (Exception)
                        {

                        }
                    }

                    await PostLoginExecute();
                } 
                catch (Exception ex)
                {
                    Logger.Error($"Error: " + ex.Source);
                    Logger.Error($"{ex}"); 
                    Logger.ColoredConsoleWrite(ConsoleColor.Green, "Trying to Restart."); 
                    try
                    {
                        _telegram.getClient().StopReceiving();
                    }
                    catch (Exception)
                    {

                    } 
                }

                Logger.ColoredConsoleWrite(ConsoleColor.Red, "Restarting in 10 Seconds."); 
                await Task.Delay(10000);
            }
        }

        public async Task PostLoginExecute()
        {
            while (true)
            {
                try
                {
                    var profil = await _client.Player.GetPlayer();
                    await _client.Inventory.ExportPokemonToCSV(profil.PlayerData);
                    await StatsLog(_client);

                    if (_clientSettings.EvolvePokemonsIfEnoughCandy)
                    {
                        await EvolveAllPokemonWithEnoughCandy();
                    }

                    if (_clientSettings.AutoIncubate)
                    { 
                        await StartIncubation();
                    }

                    await TransferDuplicatePokemon(_clientSettings.keepPokemonsThatCanEvolve, _clientSettings.TransferFirstLowIV);
                    await RecycleItems();
                    await ExecuteFarmingPokestopsAndPokemons(_client);

                }
                catch (AccessTokenExpiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Write($"Exception: {ex}", LogLevel.Error);
                }
                Logger.ColoredConsoleWrite(ConsoleColor.Green, "Starting again in 10 seconds...");
                await Task.Delay(10000);
            }
        }

        public async Task RepeatAction(int repeat, Func<Task> action)
        {
            for (int i = 0; i < repeat; i++)
            {
                await action();
            }
        }

        //class PokeService
        //{
        //    public bool go_online; //Online oder nicht?
        //    public double go_response; //Wie lange der Server zum Responden braucht
        //    public double go_idle; //Wie lange die go server online sind (in minuten)
        //    public double go_uptime_hour; //Uptime in Prozent die letzte Stunde
        //    public double go_uptime_day; //Uptime in Prozent die letzten 24h

        //    public bool ptc_online; //Online oder nicht?
        //    public double ptc_response; //Wie lange PTC zum responden braucht
        //    public double ptc_idle; //Wie lange ptc schon läuft
        //    public double ptc_uptime_hour; //Prozent von PTC uptime letzte stunde
        //    public double ptc_uptime_day; //Prozent von PTC uptime letzten tag
        //}

        int dontspam = 3;
        int level = -1;
        private async Task StatsLog(Client client)
        {
            // Check if disabled
            StringUtils.CheckKillSwitch();

            dontspam++;
            var profile = await _client.Player.GetPlayer();
            var playerStats = await _client.Inventory.GetPlayerStats();
            var stats = playerStats.First();


            var expneeded = stats.NextLevelXp - stats.PrevLevelXp - StringUtils.getExpDiff(stats.Level);
            var curexp = stats.Experience - stats.PrevLevelXp - StringUtils.getExpDiff(stats.Level);
            var curexppercent = Convert.ToDouble(curexp) / Convert.ToDouble(expneeded) * 100;
            var pokemonToEvolve = (await _client.Inventory.GetPokemonToEvolve()).Count();
            var pokedexpercentraw = Convert.ToDouble(stats.UniquePokedexEntries) / Convert.ToDouble(150) * 100;
            var pokedexpercent = Math.Floor(pokedexpercentraw);

            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "-----------------------[PLAYER STATS]-----------------------");
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, $"Level/EXP: {stats.Level} | {curexp.ToString("N0")}/{expneeded.ToString("N0")} ({Math.Round(curexppercent, 2)}%)");
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "EXP to Level up: " + (stats.NextLevelXp - stats.Experience)); ;
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "PokeStops visited: " + stats.PokeStopVisits);
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "KM Walked: " + Math.Round(stats.KmWalked, 2));
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "Pokemon: " + await _client.Inventory.getPokemonCount() + " + " + await _client.Inventory.GetEggsCount() + " Eggs /" + profile.PlayerData.MaxPokemonStorage + " (" + pokemonToEvolve + " Evolvable)");
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "Pokedex Completion: " + stats.UniquePokedexEntries + "/150 " + "[" + pokedexpercent + "%]");
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "Items: " + await _client.Inventory.getInventoryCount() + "/" + profile.PlayerData.MaxItemStorage);
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "Stardust: " + profile.PlayerData.Currencies.ToArray()[1].Amount.ToString("N0"));
            Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "------------------------------------------------------------");

            if (level == -1)
            {
                level = stats.Level;
            } else if (stats.Level > level)
            {
                level = stats.Level;
                Logger.ColoredConsoleWrite(ConsoleColor.Magenta, "Got the level up reward from your level up.");
                var lvlup = await _client.Player.GetLevelUpRewards(stats.Level);
                List<ItemId> alreadygot = new List<ItemId>();
                foreach (var i in lvlup.ItemsAwarded)
                {
                    if (!alreadygot.Contains(i.ItemId))
                    {
                        Logger.ColoredConsoleWrite(ConsoleColor.Magenta, "Got Item: " + i.ItemId + " " + i.ItemCount + "x");
                        alreadygot.Add(i.ItemId);
                    }
                }
                alreadygot.Clear();
            }


            //if (dontspam >= 3)
            //{
            //    dontspam = 0;
            //    PokeService data = null;
            //    try
            //    {
            //        var clientx = new WebClient();
            //        clientx.Headers.Add("user-agent", "PokegoBot-Ar1i-Github");
            //        var jsonString = clientx.DownloadString("https://go.jooas.com/status");
            //        data = new JavaScriptSerializer().Deserialize<PokeService>(jsonString);
            //    }
            //    catch (Exception)
            //    {

            //    }

            //    if (data != null)
            //    {
            //        Logger.ColoredConsoleWrite(ConsoleColor.White, "");
            //        Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "PokemonGO Server Status:");
            //        if (data.go_online)
            //        {
            //            if (data.go_idle > 60)
            //            {
            //                int gohour = Convert.ToInt32(data.go_idle / 60);

            //                if (gohour > 24)
            //                {
            //                    int goday = gohour / 24;

            //                    Logger.ColoredConsoleWrite(ConsoleColor.Green, "Online since ~" + goday + " day(s).");
            //                }
            //                else
            //                {
            //                    Logger.ColoredConsoleWrite(ConsoleColor.Green, "Online since ~" + gohour + "h.");
            //                }
            //            }
            //            else
            //            {
            //                Logger.ColoredConsoleWrite(ConsoleColor.Green, "Online since ~" + data.go_idle + " min.");
            //            }

            //            Logger.ColoredConsoleWrite(ConsoleColor.Green, "Server anwsers in ~" + data.go_response + " seconds.");
            //        }
            //        else
            //        {
            //            Logger.ColoredConsoleWrite(ConsoleColor.Red, "Pokemon GO Servers: Offline.");
            //        }
            //        Logger.ColoredConsoleWrite(ConsoleColor.Cyan, "Pokemon Trainer Club Server Status:");
            //        if (data.ptc_online)
            //        {
            //            if (data.ptc_idle > 60)
            //            {
            //                int ptchour = Convert.ToInt32(data.ptc_idle / 60);

            //                if (ptchour > 24)
            //                {
            //                    int ptcday = ptchour / 24;
            //                    Logger.ColoredConsoleWrite(ConsoleColor.Green, "Online since ~" + ptcday + " day(s).");

            //                }
            //                else
            //                {
            //                    Logger.ColoredConsoleWrite(ConsoleColor.Green, "Online since ~" + ptchour + "h.");
            //                }
            //            }
            //            else
            //            {
            //                Logger.ColoredConsoleWrite(ConsoleColor.Green, "Online since ~" + data.ptc_idle + " min.");
            //            }

            //            Logger.ColoredConsoleWrite(ConsoleColor.Green, "Server anwsers in ~" + data.ptc_response + " seconds.");
            //        }
            //        else
            //        {
            //            Logger.ColoredConsoleWrite(ConsoleColor.Red, "Pokemon Trainer Club: Offline.");
            //        }
            //    } 
            //    else
            //    {
            //        Logger.ColoredConsoleWrite(ConsoleColor.Red, "Cant get Server Status from https://go.jooas.com/status");
            //    }
            //}

            Console.Title = profile.PlayerData.Username + " lvl" + stats.Level + "-(" + (stats.Experience - stats.PrevLevelXp -
                StringUtils.getExpDiff(stats.Level)).ToString("N0") + "/" + (stats.NextLevelXp - stats.PrevLevelXp - StringUtils.getExpDiff(stats.Level)).ToString("N0") + "|" + Math.Round(curexppercent, 2) + "%)| Stardust: " + profile.PlayerData.Currencies.ToArray()[1].Amount + "| " + _botStats.ToString();
            
        }


        private int count = 0;

        public static int failed_softban = 0;

        private async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {

            var distanceFromStart = LocationUtils.CalculateDistanceInMeters(_clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude, _client.CurrentLatitude, _client.CurrentLongitude);
            
            if (_clientSettings.MaxWalkingRadiusInMeters != 0 && distanceFromStart > _clientSettings.MaxWalkingRadiusInMeters)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Green, "You're outside of the defined max. walking radius. Walking back!");
                var update = await _navigation.HumanLikeWalking(new GeoCoordinate(_clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude), _clientSettings.WalkingSpeedInKilometerPerHour, ExecuteCatchAllNearbyPokemons);
                var start = await _navigation.HumanLikeWalking(new GeoCoordinate(_clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude), _clientSettings.WalkingSpeedInKilometerPerHour, ExecuteCatchAllNearbyPokemons);
            }

            //Resources.OutPutWalking = true;
            var mapObjects = await _client.Map.GetMapObjects();

            //var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts).Where(i => i.Type == FortType.Checkpoint && i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());
            var pokeStops =
            _navigation.pathByNearestNeighbour(
            mapObjects.Item1.MapCells.SelectMany(i => i.Forts)
            .Where(
                i =>
                i.Type == FortType.Checkpoint &&
                i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                .OrderBy(
                i =>
                LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, i.Latitude, i.Longitude)).ToArray(), _clientSettings.WalkingSpeedInKilometerPerHour);


            if (_clientSettings.MaxWalkingRadiusInMeters != 0)
            {
                pokeStops = pokeStops.Where(i => LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, i.Latitude, i.Longitude) <= _clientSettings.MaxWalkingRadiusInMeters).ToArray();
                if (pokeStops.Count() == 0)
                {
                    Logger.ColoredConsoleWrite(ConsoleColor.Red, "We can't find any PokeStops in a range of " + _clientSettings.MaxWalkingRadiusInMeters + "m!");
                    await ExecuteCatchAllNearbyPokemons();
                }
            }


            if (pokeStops.Count() == 0)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, "We can't find any PokeStops, which are unused! Probably the server are unstable, or you visted them all. Retrying..");
                await ExecuteCatchAllNearbyPokemons();

            }
            else
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Yellow, "We found " + pokeStops.Count() + " usable PokeStops near your current location.");
            }

            _infoObservable.PushPokeStopLocations(pokeStops);

            foreach (var pokeStop in pokeStops)
            {
                await UseIncense();

                await ExecuteCatchAllNearbyPokemons();

                if (count >= 3)
                {
                    count = 0;
                    await StatsLog(client);

                    if (_clientSettings.EvolvePokemonsIfEnoughCandy)
                    {
                        await EvolveAllPokemonWithEnoughCandy();
                    }

                    if (_clientSettings.AutoIncubate)
                    {
                        await StartIncubation();
                    }

                    await TransferDuplicatePokemon(_clientSettings.keepPokemonsThatCanEvolve, _clientSettings.TransferFirstLowIV);
                    //await RecycleItems();

                    ////
                    if (_clientSettings.UseLuckyEggIfNotRunning)
                    {
                        await _client.Inventory.UseLuckyEgg(_client);
                    }
                }
                //if (_clientSettings.pokevision)
                //{
                //    foreach (spottedPoke p in await _pokevision.GetNearPokemons(_client.CurrentLat, _client.CurrentLng))
                //    {
                //        var dist = LocationUtils.CalculateDistanceInMeters(_client.CurrentLat, _client.CurrentLng, p._lat, p._lng);
                //        Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"PokeVision: A {StringUtils.getPokemonNameByLanguage(_clientSettings, p._pokeId)} in {dist:0.##}m distance. Trying to catch.");
                //        var upd = await _navigation.HumanLikeWalking(new GeoCoordinate(p._lat, p._lng), _clientSettings.WalkingSpeedInKilometerPerHour, ExecuteCatchAllNearbyPokemons);
                //    }
                //}

                _infoObservable.PushNewGeoLocations(new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude));

                var distance = LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                if (fortInfo == null)
                {
                    continue;
                }
                Logger.ColoredConsoleWrite(ConsoleColor.Green, $"Next Pokestop: {fortInfo.Name} in {distance:0.##}m distance.");
                var update = await _navigation.HumanLikeWalking(new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude), _clientSettings.WalkingSpeedInKilometerPerHour, ExecuteCatchAllNearbyPokemons);

                ////var fortInfo = await client.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await _client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                count++;

                if (fortSearch.ExperienceAwarded > 0)
                {
                    failed_softban = 0;
                    _botStats.AddExperience(fortSearch.ExperienceAwarded);
                    var egg = "";
                    if (fortSearch.PokemonDataEgg != null)
                    { 
                        egg = ", Egg " + fortSearch.PokemonDataEgg.EggKmWalkedTarget;
                    }
                    else  
                    {
                        egg = "";
                    }
                    var gems = "";
                    if (fortSearch.GemsAwarded > 0)
                    {
                        gems = ", Gems:" + fortSearch.GemsAwarded;
                    }
                    else
                    {
                        gems = "";
                    }

                    var items = "";
                    if (StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded) != "")
                    {
                        items = StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded);
                    }
                    else
                    {
                        items = "Nothing (Inventory Full?)";
                        await RecycleItems();
                    }

                    Logger.ColoredConsoleWrite(ConsoleColor.Green, $"Farmed XP: {fortSearch.ExperienceAwarded}{gems}{egg}, Items: {items}", LogLevel.Info);
                    


                    double eggs = 0;
                    if (fortSearch.PokemonDataEgg != null)
                    {
                        eggs = fortSearch.PokemonDataEgg.EggKmWalkedTarget;
                    }
                    try
                    {
                        TelegramUtil.getInstance().sendInformationText(TelegramUtil.TelegramUtilInformationTopics.Pokestop, fortInfo.Name, fortSearch.ExperienceAwarded, eggs, fortSearch.GemsAwarded, items);
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    failed_softban++;
                    if (failed_softban >= 6)
                    {  
                        Logger.Error("Detected a softban. Trying to use our special 1337 unban methode."); 
                        for (int i = 0; i < 60; i++)
                        {
                            var unban = await client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                            if (unban.ExperienceAwarded > 0)
                            {
                                break;
                            }
                        }
                        failed_softban = 0;
                        Logger.ColoredConsoleWrite(ConsoleColor.Green, "Probably unbanned now.");
                    }
                }

                await RandomHelper.RandomDelay(50, 200);
            }
            if (_clientSettings.WalkBackToDefaultLocation)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Green, "Walking back to default location.");
                await _navigation.HumanLikeWalking(new GeoCoordinate(_clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude), _clientSettings.WalkingSpeedInKilometerPerHour, ExecuteCatchAllNearbyPokemons);
            }
        }

        private async Task ExecuteCatchAllNearbyPokemons()
        {
            _infoObservable.PushNewGeoLocations(new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude));
            var client = _client;
            var mapObjects = await client.Map.GetMapObjects();

            //var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);
            var pokemons =
               mapObjects.Item1.MapCells.SelectMany(i => i.CatchablePokemons)
               .OrderBy(
                   i =>
                   LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, i.Latitude, i.Longitude));

            if (pokemons != null && pokemons.Any())
                Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"Found {pokemons.Count()} catchable Pokemon(s).");

            foreach (var pokemon in pokemons)
            {
                count++;
                if (count >= 6)
                {
                    count = 0;
                    await StatsLog(client);

                    if (_clientSettings.EvolvePokemonsIfEnoughCandy)
                    {
                        await EvolveAllPokemonWithEnoughCandy();
                    }

                    if (_clientSettings.AutoIncubate)
                    {
                        await StartIncubation();
                    }

                    await TransferDuplicatePokemon(_clientSettings.keepPokemonsThatCanEvolve, _clientSettings.TransferFirstLowIV);
                    //await RecycleItems();
                }

                if (_clientSettings.catchPokemonSkipList.Contains(pokemon.PokemonId))
                {
                    Logger.ColoredConsoleWrite(ConsoleColor.Green, "Skipped Pokemon: " + pokemon.PokemonId);
                    continue;
                }

                var distance = LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                await Task.Delay(distance > 100 ? 1000 : 100);
                var encounterPokemonResponse = await _client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);
                
                if (encounterPokemonResponse.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    var bestPokeball = await GetBestBall(encounterPokemonResponse?.WildPokemon);
                    if (bestPokeball == ItemId.ItemUnknown)
                    { 
                        Logger.ColoredConsoleWrite(ConsoleColor.Red, $"No Pokeballs! - missed {pokemon.PokemonId} CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} IV {PokemonInfo.CalculatePokemonPerfection(encounterPokemonResponse.WildPokemon.PokemonData).ToString("0.00")}%");
  
                        return;
                    }
                    var inventoryBerries = await _client.Inventory.GetItems();
                    var probability = encounterPokemonResponse?.CaptureProbability?.CaptureProbability_?.FirstOrDefault();
                    CatchPokemonResponse caughtPokemonResponse;
                    Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"Encountered {StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp}  IV {PokemonInfo.CalculatePokemonPerfection(encounterPokemonResponse.WildPokemon.PokemonData).ToString("0.00")}% Probability {Math.Round(probability.Value * 100)}%");
                    do
                    {
                        if (probability.HasValue && probability.Value < _clientSettings.razzberry_chance && _clientSettings.UseRazzBerry)
                        { 
                            var bestBerry = await GetBestBerry(encounterPokemonResponse?.WildPokemon);
                            var berries = inventoryBerries.Where(p => (ItemId)p.ItemId == bestBerry).FirstOrDefault();
                            if (bestBerry != ItemId.ItemUnknown)
                            {
                                //Throw berry if we can
                                var useRaspberry = await _client.Encounter.UseCaptureItem(pokemon.EncounterId, bestBerry, pokemon.SpawnPointId);
                                Logger.ColoredConsoleWrite(ConsoleColor.Green, $"Thrown {bestBerry}. Remaining: {berries.Count}.", LogLevel.Info);
                                await RandomHelper.RandomDelay(50, 200);
                            } 
                        }

                        caughtPokemonResponse = await _client.Encounter.CatchPokemon(pokemon.EncounterId, pokemon.SpawnPointId, bestPokeball);
                        if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed)
                        {
                            Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"Missed {StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} while using {bestPokeball}");
                        }
                        else if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape)
                        {
                            Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"{StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} escaped while using {bestPokeball}");
                        }
                    }
                    while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);

                    if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                    {
                        foreach (int xp in caughtPokemonResponse.CaptureAward.Xp)
                            _botStats.AddExperience(xp);

                        DateTime curDate = DateTime.Now;
                        _infoObservable.PushNewHuntStats(String.Format("{0}/{1};{2};{3};{4}", pokemon.Latitude, pokemon.Longitude, pokemon.PokemonId, curDate.Ticks, curDate.ToString()) + Environment.NewLine);

                        if (caughtPokemonResponse.CaptureAward.Xp.Sum() >= 500)
                        {
                            Logger.ColoredConsoleWrite(ConsoleColor.DarkMagenta,
                                $"Caught New {StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} IV {PokemonInfo.CalculatePokemonPerfection(encounterPokemonResponse.WildPokemon.PokemonData).ToString("0.00")}% using {bestPokeball} got {caughtPokemonResponse.CaptureAward.Xp.Sum()} XP.");
                        }
                        else
                        {
                            Logger.ColoredConsoleWrite(ConsoleColor.Magenta,
                                $"Caught {StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} IV {PokemonInfo.CalculatePokemonPerfection(encounterPokemonResponse.WildPokemon.PokemonData).ToString("0.00")}% using {bestPokeball} got {caughtPokemonResponse.CaptureAward.Xp.Sum()} XP.");
                        }

                        try
                        {
                            TelegramUtil.getInstance().sendInformationText(TelegramUtil.TelegramUtilInformationTopics.Catch, StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId), encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp, PokemonInfo.CalculatePokemonPerfection(encounterPokemonResponse.WildPokemon.PokemonData).ToString("0.00"), bestPokeball, caughtPokemonResponse.CaptureAward.Xp.Sum());
                        }
                        catch (Exception)
                        {

                        }
                        //try
                        //{
                        //    var r = (HttpWebRequest)WebRequest.Create("http://pokemon.becher.xyz/index.php?pokeName=" + pokemon.PokemonId);
                        //    var rp = (HttpWebResponse)r.GetResponse();
                        //    var rps = new StreamReader(rp.GetResponseStream()).ReadToEnd();
                        //    Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"We caught a {pokemon.PokemonId} ({rps}) with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} using a {bestPokeball}");
                        //} catch (Exception)
                        //{
                        //    Logger.ColoredConsoleWrite(ConsoleColor.Magenta, $"We caught a {pokemon.PokemonId} (Language Server Offline) with CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} using a {bestPokeball}");
                        //}

                        _botStats.AddPokemon(1);
                    }
                    else
                    {
                        Logger.ColoredConsoleWrite(ConsoleColor.DarkYellow, $"{StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} CP {encounterPokemonResponse?.WildPokemon?.PokemonData?.Cp} IV {PokemonInfo.CalculatePokemonPerfection(encounterPokemonResponse.WildPokemon.PokemonData).ToString("0.00")}% got away while using {bestPokeball}..");
                        failed_softban++;
                    }
                }
                else
                {
                    Logger.ColoredConsoleWrite(ConsoleColor.Red, $"Error catching Pokemon: {encounterPokemonResponse?.Status}");
                }
                if (_clientSettings.sleepatpokemons)
                {
                    await RandomHelper.RandomDelay(1000, 3000);
                }
                await RandomHelper.RandomDelay(200, 300);
            }
        }

        private async Task EvolveAllPokemonWithEnoughCandy(IEnumerable<PokemonId> filter = null)
        {
            var pokemonToEvolve = await _client.Inventory.GetPokemonToEvolve(filter);
            if (pokemonToEvolve.Count() != 0)
            {
                if (_clientSettings.UseLuckyEgg)
                {
                    await _client.Inventory.UseLuckyEgg(_client);
                }
            }
            foreach (var pokemon in pokemonToEvolve)
            {

                if (!_clientSettings.pokemonsToEvolve.Contains(pokemon.PokemonId))
                {
                    continue;
                }

                count++;
                if (count == 6)
                {
                    count = 0;
                    await StatsLog(_client);
                }

                var evolvePokemonOutProto = await _client.Inventory.EvolvePokemon((ulong)pokemon.Id);
                
                if (evolvePokemonOutProto.Result == EvolvePokemonResponse.Types.Result.Success)
                { 
                    Logger.ColoredConsoleWrite(ConsoleColor.Green, $"Evolved {StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId)} CP {pokemon.Cp} {PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}%  to {StringUtils.getPokemonNameByLanguage(_clientSettings, evolvePokemonOutProto.EvolvedPokemonData.PokemonId)} CP: {evolvePokemonOutProto.EvolvedPokemonData.Cp} for {evolvePokemonOutProto.ExperienceAwarded.ToString("N0")}xp", LogLevel.Info);
                    _botStats.AddExperience(evolvePokemonOutProto.ExperienceAwarded);

                    try
                    {
                        TelegramUtil.getInstance().sendInformationText(TelegramUtil.TelegramUtilInformationTopics.Evolve, StringUtils.getPokemonNameByLanguage(_clientSettings, pokemon.PokemonId), pokemon.Cp, PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00"), StringUtils.getPokemonNameByLanguage(_clientSettings, evolvePokemonOutProto.EvolvedPokemonData.PokemonId), evolvePokemonOutProto.EvolvedPokemonData.Cp, evolvePokemonOutProto.ExperienceAwarded.ToString("N0"));
                    }
                    catch (Exception) { }
                }
                else
                {
                    if (evolvePokemonOutProto.Result != EvolvePokemonResponse.Types.Result.Success)
                    {
                        Logger.ColoredConsoleWrite(ConsoleColor.Red, $"Failed to evolve {pokemon.PokemonId}. EvolvePokemonOutProto.Result was {evolvePokemonOutProto.Result}", LogLevel.Info);
                    }
                }

                await RandomHelper.RandomDelay(1000, 2000);
            }
        }
       
        private async Task StartIncubation()
        {
            try
            {
                await _client.Inventory.RefreshCachedInventory(); // REFRESH
                var incubators = (await _client.Inventory.GetEggIncubators()).ToList();
                var unusedEggs = (await _client.Inventory.GetEggs()).Where(x => string.IsNullOrEmpty(x.EggIncubatorId)).OrderBy(x => x.EggKmWalkedTarget - x.EggKmWalkedStart).ToList();
                var pokemons = (await _client.Inventory.GetPokemons()).ToList();

                var playerStats = await _client.Inventory.GetPlayerStats();
                var stats = playerStats.First();

                var kmWalked = stats.KmWalked;

                var rememberedIncubatorsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Configs", "incubators.json");
                var rememberedIncubators = GetRememberedIncubators(rememberedIncubatorsFilePath);

                foreach (var incubator in rememberedIncubators)
                {
                    var hatched = pokemons.FirstOrDefault(x => !x.IsEgg && x.Id == incubator.PokemonId);
                    if (hatched == null) continue;

                    Logger.ColoredConsoleWrite(ConsoleColor.DarkYellow, "Egg hatched and we got a " + hatched.PokemonId + " CP: " + hatched.Cp + " MaxCP: " + PokemonInfo.CalculateMaxCP(hatched) + " Level: " + PokemonInfo.GetLevel(hatched) + " IV: " + PokemonInfo.CalculatePokemonPerfection(hatched).ToString("0.00") + "%");
                }

                var newRememberedIncubators = new List<IncubatorUsage>();

                foreach (var incubator in incubators)
                {
                    if (incubator.PokemonId == 0)
                    {
                        // Unlimited incubators prefer short eggs, limited incubators prefer long eggs
                        // Special case: If only one incubator is available at all, it will prefer long eggs
                        var egg = (incubator.ItemId == ItemId.ItemIncubatorBasicUnlimited && incubators.Count > 1)
                            ? unusedEggs.FirstOrDefault()
                            : unusedEggs.LastOrDefault();

                        if (egg == null)
                            continue;

                        if (egg.EggKmWalkedTarget < 5 && incubator.ItemId != ItemId.ItemIncubatorBasicUnlimited)
                            continue;

                        var response = await _client.Inventory.UseItemEggIncubator(incubator.Id, egg.Id);
                        unusedEggs.Remove(egg);

                        newRememberedIncubators.Add(new IncubatorUsage { IncubatorId = incubator.Id, PokemonId = egg.Id });

                        Logger.ColoredConsoleWrite(ConsoleColor.DarkYellow, "Added Egg which needs " + egg.EggKmWalkedTarget + "km");
                        // We need some sleep here or this shit explodes
                        await RandomHelper.RandomDelay(100, 200);
                    }
                    else
                    {
                        newRememberedIncubators.Add(new IncubatorUsage
                        {
                            IncubatorId = incubator.Id,
                            PokemonId = incubator.PokemonId
                        });

                        Logger.ColoredConsoleWrite(ConsoleColor.DarkYellow, "Egg (" + (incubator.TargetKmWalked - incubator.StartKmWalked) + "km) need to walk " + Math.Round(incubator.TargetKmWalked - kmWalked, 2) + " km.");
                    }
                }

                if (!newRememberedIncubators.SequenceEqual(rememberedIncubators))
                    SaveRememberedIncubators(newRememberedIncubators, rememberedIncubatorsFilePath);
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace.ToString()); 
            }
        }

        private async Task TransferDuplicatePokemon(bool keepPokemonsThatCanEvolve = false, bool TransferFirstLowIV = false)
        {
            if (_clientSettings.TransferDoublePokemons)
            { 
                var duplicatePokemons = await _client.Inventory.GetDuplicatePokemonToTransfer(keepPokemonsThatCanEvolve, TransferFirstLowIV); 
                //var duplicatePokemons = await _client.Inventory.GetDuplicatePokemonToTransfer(keepPokemonsThatCanEvolve); Is doch retarded
 

                foreach (var duplicatePokemon in duplicatePokemons)
                {
                    if (!_clientSettings.pokemonsToHold.Contains(duplicatePokemon.PokemonId))
                    {
                        if (duplicatePokemon.Cp >= _clientSettings.DontTransferWithCPOver || PokemonInfo.CalculatePokemonPerfection(duplicatePokemon) >= _client.Settings.ivmaxpercent)
                        {
                            continue;
                        }

                        var bestPokemonOfType = await _client.Inventory.GetHighestCPofType(duplicatePokemon);
                        var bestPokemonsCPOfType = await _client.Inventory.GetHighestCPofType2(duplicatePokemon);
                        var bestPokemonsIVOfType = await _client.Inventory.GetHighestIVofType(duplicatePokemon);

                        var transfer = await _client.Inventory.TransferPokemon(duplicatePokemon.Id);
                        if (TransferFirstLowIV)
                        {
                            Logger.ColoredConsoleWrite(ConsoleColor.Yellow, $"Transfer {StringUtils.getPokemonNameByLanguage(_clientSettings, duplicatePokemon.PokemonId)} CP {duplicatePokemon.Cp} IV {PokemonInfo.CalculatePokemonPerfection(duplicatePokemon).ToString("0.00")} % (Best IV: {PokemonInfo.CalculatePokemonPerfection(bestPokemonsIVOfType.First()).ToString("0.00")} %)", LogLevel.Info);
                        }
                        else
                        {
                            Logger.ColoredConsoleWrite(ConsoleColor.Yellow, $"Transfer {StringUtils.getPokemonNameByLanguage(_clientSettings, duplicatePokemon.PokemonId)} CP {duplicatePokemon.Cp} IV {PokemonInfo.CalculatePokemonPerfection(duplicatePokemon).ToString("0.00")} % (Best: {bestPokemonsCPOfType.First().Cp} CP)", LogLevel.Info);
                        }
                        
                        try
                        {
                            TelegramUtil.getInstance().sendInformationText(TelegramUtil.TelegramUtilInformationTopics.Transfer, 
                                StringUtils.getPokemonNameByLanguage(_clientSettings, duplicatePokemon.PokemonId), duplicatePokemon.Cp, 
                                PokemonInfo.CalculatePokemonPerfection(duplicatePokemon).ToString("0.00"), bestPokemonOfType);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        await RandomHelper.RandomDelay(500, 700);
                    }
                }
            }
        }

        private async Task RecycleItems(bool forcerefresh = false)
        {
            var items = await _client.Inventory.GetItemsToRecycle(_clientSettings);

            foreach (var item in items)
            {  
                var transfer = await _client.Inventory.RecycleItem((ItemId)item.ItemId, item.Count);
                Logger.ColoredConsoleWrite(ConsoleColor.Yellow, $"Recycled {item.Count}x {(ItemId)item.ItemId}", LogLevel.Info); 
                await RandomHelper.RandomDelay(500, 700);
            }
        }

        private async Task<ItemId> GetBestBall(WildPokemon pokemon)
        {
            var pokemonCp = pokemon?.PokemonData?.Cp;


            //await RecycleItems(true);
            var items = await _client.Inventory.GetItems();
            
            var balls = items.Where(i => ((ItemId)i.ItemId == ItemId.ItemPokeBall
                                      || (ItemId)i.ItemId == ItemId.ItemGreatBall
                                      || (ItemId)i.ItemId == ItemId.ItemUltraBall
                                      || (ItemId)i.ItemId == ItemId.ItemMasterBall) && i.ItemId > 0).GroupBy(i => ((ItemId)i.ItemId)).ToList();
            if (balls.Count() == 0) return ItemId.ItemUnknown;




            var pokeBalls = balls.Any(g => g.Key == ItemId.ItemPokeBall);
            var greatBalls = balls.Any(g => g.Key == ItemId.ItemGreatBall);
            var ultraBalls = balls.Any(g => g.Key == ItemId.ItemUltraBall);
            var masterBalls = balls.Any(g => g.Key == ItemId.ItemMasterBall);

            var coll = _clientSettings.itemRecycleFilter;
            foreach (KeyValuePair<ItemId, int> v in coll )
            {
                if (v.Key == ItemId.ItemPokeBall && v.Value == 0)
                {
                    pokeBalls = false;
                } else if (v.Key == ItemId.ItemGreatBall && v.Value == 0)
                {
                    greatBalls = false;
                } else if (v.Key == ItemId.ItemUltraBall && v.Value == 0)
                {
                    ultraBalls = false;
                } else if (v.Key == ItemId.ItemMasterBall && v.Value == 0)
                {
                    masterBalls = false;
                }
            }

            if (masterBalls && pokemonCp >= 2000)
                return ItemId.ItemMasterBall;
            else if (ultraBalls && pokemonCp >= 2000)
                return ItemId.ItemUltraBall;
            else if (greatBalls && pokemonCp >= 2000)
                return ItemId.ItemGreatBall;

            if (ultraBalls && pokemonCp >= 1000)
                return ItemId.ItemUltraBall;
            else if (greatBalls && pokemonCp >= 1000)
                return ItemId.ItemGreatBall;

            if (greatBalls && pokemonCp >= 500)
                return ItemId.ItemGreatBall;

            if (pokeBalls)
                return ItemId.ItemPokeBall;

            if (greatBalls)
                return ItemId.ItemGreatBall;

            if (ultraBalls)
                return ItemId.ItemUltraBall;

            if (masterBalls)
                return ItemId.ItemMasterBall;

            return balls.OrderBy(c => c.Key).First().Key;
        }

        private async Task<ItemId> GetBestBerry(WildPokemon pokemon)
        {
            var pokemonCp = pokemon?.PokemonData?.Cp;

            var items = await _client.Inventory.GetItems();
            var berries = items.Where(i => (ItemId)i.ItemId == ItemId.ItemRazzBerry
                                        || (ItemId)i.ItemId == ItemId.ItemBlukBerry
                                        || (ItemId)i.ItemId == ItemId.ItemNanabBerry
                                        || (ItemId)i.ItemId == ItemId.ItemWeparBerry
                                        || (ItemId)i.ItemId == ItemId.ItemPinapBerry).GroupBy(i => (ItemId)i.ItemId).ToList();
            if (berries.Count() == 0)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, $"No Berrys to select!", LogLevel.Info);
                return ItemId.ItemUnknown;
            }

            var razzBerryCount = await _client.Inventory.GetItemAmountByType(ItemId.ItemRazzBerry);
            var blukBerryCount = await _client.Inventory.GetItemAmountByType(ItemId.ItemBlukBerry);
            var nanabBerryCount = await _client.Inventory.GetItemAmountByType(ItemId.ItemNanabBerry);
            var weparBerryCount = await _client.Inventory.GetItemAmountByType(ItemId.ItemWeparBerry);
            var pinapBerryCount = await _client.Inventory.GetItemAmountByType(ItemId.ItemPinapBerry);

            if (pinapBerryCount > 0 && pokemonCp >= 2000)
                return ItemId.ItemPinapBerry;
            else if (weparBerryCount > 0 && pokemonCp >= 2000)
                return ItemId.ItemWeparBerry;
            else if (nanabBerryCount > 0 && pokemonCp >= 2000)
                return ItemId.ItemNanabBerry;
            else if (nanabBerryCount > 0 && pokemonCp >= 2000)
                return ItemId.ItemBlukBerry;

            if (weparBerryCount > 0 && pokemonCp >= 1500)
                return ItemId.ItemWeparBerry;
            else if (nanabBerryCount > 0 && pokemonCp >= 1500)
                return ItemId.ItemNanabBerry;
            else if (blukBerryCount > 0 && pokemonCp >= 1500)
                return ItemId.ItemBlukBerry;

            if (nanabBerryCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemNanabBerry;
            else if (blukBerryCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemBlukBerry;

            if (blukBerryCount > 0 && pokemonCp >= 500)
                return ItemId.ItemBlukBerry;

            return berries.OrderBy(g => g.Key).First().Key;
        }

        DateTime lastincenseuse;
        public async Task UseIncense()
        {
            if (_clientSettings.UseIncense)
            {
                var inventory = await _client.Inventory.GetItems();
                var incsense = inventory.Where(p => (ItemId)p.ItemId == ItemId.ItemIncenseOrdinary).FirstOrDefault();

                if (lastincenseuse > DateTime.Now.AddSeconds(5))
                {
                    TimeSpan duration = lastincenseuse - DateTime.Now;
                    Logger.ColoredConsoleWrite(ConsoleColor.Cyan, $"Incense still running: {duration.Minutes}m{duration.Seconds}s");
                    return;
                }
                if (incsense == null || incsense.Count <= 0) { return; }

                await _client.Inventory.UseIncense(ItemId.ItemIncenseOrdinary);
                Logger.ColoredConsoleWrite(ConsoleColor.Cyan, $"Used Incsense, remaining: {incsense.Count - 1}");
                lastincenseuse = DateTime.Now.AddMinutes(30);
                await Task.Delay(3000);
            }
        }

        private double _distance(double Lat1, double Lng1, double Lat2, double Lng2)
        {
            double r_earth = 6378137;
            double d_lat = (Lat2 - Lat1) * Math.PI / 180;
            double d_lon = (Lng2 - Lng1) * Math.PI / 180;
            double alpha = Math.Sin(d_lat / 2) * Math.Sin(d_lat / 2)
                + Math.Cos(Lat1 * Math.PI / 180) * Math.Cos(Lat2 * Math.PI / 180)
                * Math.Sin(d_lon / 2) * Math.Sin(d_lon / 2);
            double d = 2 * r_earth * Math.Atan2(Math.Sqrt(alpha), Math.Sqrt(1 - alpha));
            return d;
        }
        private static List<IncubatorUsage> GetRememberedIncubators(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<List<IncubatorUsage>>(File.ReadAllText(filePath, Encoding.UTF8));

            return new List<IncubatorUsage>(0);
        }

        private static void SaveRememberedIncubators(List<IncubatorUsage> incubators, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            File.WriteAllText(filePath, JsonConvert.SerializeObject(incubators), Encoding.UTF8);
        }

        private class IncubatorUsage : IEquatable<IncubatorUsage>
        {
            public string IncubatorId;
            public ulong PokemonId;

            public bool Equals(IncubatorUsage other)
            {
                return other != null && other.IncubatorId == IncubatorId && other.PokemonId == PokemonId;
            }
        }
    }
}
