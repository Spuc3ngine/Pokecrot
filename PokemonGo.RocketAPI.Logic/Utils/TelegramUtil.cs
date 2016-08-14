using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Logic.Utils;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Logic;
using PokemonGo.RocketAPI.Helpers;

using PokemonGo.RocketAPI.Logic.Translation;
using PokemonGo.RocketAPI.Rpc;
using POGOProtos.Data;

namespace PokemonGo.RocketAPI.Logic.Utils
{
    public class TelegramUtil
    {

        private static TelegramUtil instance;

        public static TelegramUtil getInstance()
        {
            return instance;
        }

        public enum TelegramUtilInformationTopics
        {
            Pokestop,
            Catch,
            Evolve,
            Transfer,
            Levelup
        }
        private Dictionary<TelegramUtilInformationTopics, Boolean> _information = new Dictionary<TelegramUtilInformationTopics, Boolean>();
        private Dictionary<TelegramUtilInformationTopics, String> _informationDescription = new Dictionary<TelegramUtilInformationTopics, String>();

        private Dictionary<TelegramUtilInformationTopics, String> _informationDescriptionDefault = new Dictionary<TelegramUtilInformationTopics, String>() {
            { TelegramUtilInformationTopics.Pokestop, @"Notifies you when a pokestop was visited" },
            { TelegramUtilInformationTopics.Catch, @"Notifies you when a pokemon is caught" },
            { TelegramUtilInformationTopics.Evolve, @"Notifies you when a pokemon was evolved" },
            { TelegramUtilInformationTopics.Transfer, @"Notifies you when a pokemon is transfered" },
            { TelegramUtilInformationTopics.Levelup, @"Notifies you when you got a level up" },
        };
        private Dictionary<TelegramUtilInformationTopics, String> _informationDescriptionIDs = new Dictionary<TelegramUtilInformationTopics, String>() {
            { TelegramUtilInformationTopics.Pokestop, @"telegram_pokestop_description" },
            { TelegramUtilInformationTopics.Catch, @"telegram_catch_description" },
            { TelegramUtilInformationTopics.Evolve, @"telegram_evolve_description" },
            { TelegramUtilInformationTopics.Transfer, @"telegram_transfer_description" },
            { TelegramUtilInformationTopics.Levelup, @"telegram_levelup_description" },
        };


        public Dictionary<TelegramUtilInformationTopics, String> _informationTopicDefaultTexts = new Dictionary<TelegramUtilInformationTopics, String>() {
            { TelegramUtilInformationTopics.Pokestop, "Visited a pokestop {0}\nXP: {1}, Eggs: {2}, Gems:{3}, Items: {4}" },
            { TelegramUtilInformationTopics.Catch, "Caught {0} CP {1} IV {2}% using {3} got {4} XP." },
            { TelegramUtilInformationTopics.Evolve, "Evolved {0} CP {1} {2}%  to {3} CP: {4} for {5}xp" },
            { TelegramUtilInformationTopics.Transfer, "Transfer {0} CP {1} IV {2}% (Best: {4} CP)" },
            { TelegramUtilInformationTopics.Levelup, "You ({0}) got Level Up! Your new Level is now {1}!" },
        };

        public Dictionary<TelegramUtilInformationTopics, String> _informationTopicDefaultTextIDs = new Dictionary<TelegramUtilInformationTopics, String>() {
            { TelegramUtilInformationTopics.Pokestop, @"telegram_pokestop" },
            { TelegramUtilInformationTopics.Catch, @"telegram_catch" },
            { TelegramUtilInformationTopics.Evolve, @"telegram_evolve" },
            { TelegramUtilInformationTopics.Transfer, @"telegram_transfer" },
            { TelegramUtilInformationTopics.Levelup, @"telegram_levelup" },
        };

        public void sendInformationText(TelegramUtilInformationTopics topic, params object[] args)
        {
            if(_information.ContainsKey(topic) && _information[topic] == true && _informationTopicDefaultTexts.ContainsKey(topic) && _informationTopicDefaultTextIDs.ContainsKey(topic))
            {
                String unformatted = TranslationHandler.GetString(_informationTopicDefaultTextIDs[topic], _informationTopicDefaultTexts[topic]);
                String formatted = string.Format(unformatted, args);
                sendMessage(formatted);
            }
        }

        public void sendMessage(String msg)
        {
            if(_chatid != -1)
            {
                _telegram.SendTextMessageAsync(_chatid, msg, replyMarkup: new ReplyKeyboardHide());
            }
        }

        #region private properties
        private Client _client;
        private Inventory _inventory;

        private Telegram.Bot.TelegramBotClient _telegram;
        private readonly ISettings _clientSettings;

        private long _chatid = -1;
        private bool _livestats = false;
        private bool _informations = false;

        /// <summary>
        /// defines what to do
        /// </summary>
        private enum TelegramUtilTask
        {
            UNKNOWN,
            /// <summary>
            /// Outputs Current Stats
            /// </summary>
            GET_STATS,
            /// <summary>
            /// Outputs Top (?) Pokemons
            /// </summary>
            GET_TOPLIST,
            /// <summary>
            /// Enable/Disable Live Stats
            /// </summary>
            SWITCH_LIVESTATS,
            /// <summary>
            /// Enable/Disable Informations
            /// </summary>
            SWITCH_INFORMATION,
            /// <summary>
            /// Forces Evolve
            /// </summary>
            RUN_FORCEEVOLVE
        }

        /// <summary>
        /// defined telegram commandos
        /// </summary>
        private Dictionary<string, TelegramUtilTask> _telegramCommandos = new Dictionary<string, TelegramUtilTask>()
        {
            { @"/stats", TelegramUtilTask.GET_STATS },
            { @"/top", TelegramUtilTask.GET_TOPLIST },
            { @"/livestats", TelegramUtilTask.SWITCH_LIVESTATS },
            { @"/information", TelegramUtilTask.SWITCH_INFORMATION },
            { @"/forceevolve", TelegramUtilTask.RUN_FORCEEVOLVE },
        };
        #endregion

        public TelegramUtil(Client client, Telegram.Bot.TelegramBotClient telegram, ISettings settings, Inventory inv)
        {
            instance = this;
            _client = client;
            _telegram = telegram;
            _clientSettings = settings;
            _inventory = inv;

            Array values = Enum.GetValues(typeof(TelegramUtilInformationTopics));
            foreach (TelegramUtilInformationTopics topic in values)
            {
                _informationDescription[topic] = TranslationHandler.GetString(_informationDescriptionIDs[topic], _informationDescriptionDefault[topic]);
                _information[topic] = false;
            }

            DoLiveStats(settings);
            DoInformation();
        }

        public Telegram.Bot.TelegramBotClient getClient()
        {
            return _telegram;
        }

        public async void DoLiveStats(ISettings settings)
        {
            try
            {

                if (_chatid != -1 && _livestats)
                {
                    var usage = "";
                    var inventory = await _client.Inventory.GetInventory();
                    var profil = await _client.Player.GetPlayer();
                    var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                    foreach (var c in stats)
                    {
                        if (c != null)
                        {
                            int l = c.Level;

                            var expneeded = ((c.NextLevelXp - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level));
                            var curexp = ((c.Experience - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level));
                            var curexppercent = (Convert.ToDouble(curexp) / Convert.ToDouble(expneeded)) * 100;

                            usage += "\nNickname: " + profil.PlayerData.Username +
                                "\nLevel: " + c.Level
                                + "\nEXP Needed: " + ((c.NextLevelXp - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level))
                                + $"\nCurrent EXP: {curexp} ({Math.Round(curexppercent)}%)"
                                + "\nEXP to Level up: " + ((c.NextLevelXp) - (c.Experience))
                                + "\nKM walked: " + c.KmWalked
                                + "\nPokeStops visited: " + c.PokeStopVisits
                                + "\nStardust: " + profil.PlayerData.Currencies.ToArray()[1].Amount;
                        }
                    }

                    await _telegram.SendTextMessageAsync(_chatid, usage, replyMarkup: new ReplyKeyboardHide());
                }
                await System.Threading.Tasks.Task.Delay(settings.TelegramLiveStatsDelay);
                DoLiveStats(settings);
            } catch (Exception)
            {

            }
        }

         int level;
        
        public async void DoInformation()
        {
            try
            {
                if (_chatid != -1 && _informations)
                {
                    int current = 0;
                    var inventory = await _client.Inventory.GetInventory();
                    var profil = await _client.Player.GetPlayer();
                    var stats = await _client.Inventory.GetPlayerStats();

                    current = stats.First().Level;
                    
                    if (current != level)
                    {
                        level = current;
                        string nick = profil.PlayerData.Username;

                        sendInformationText(TelegramUtilInformationTopics.Levelup, nick, level);

                    }
                }
                await System.Threading.Tasks.Task.Delay(5000);
                DoInformation();
            } catch (Exception)
            {

            }
        }
        
        public async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage)
                return;
            _chatid = message.Chat.Id;
            try
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, "[TelegramAPI] Got Request from " + message.From.Username + " | " + message.Text);
                string username = _clientSettings.TelegramName;
                string telegramAnswer = string.Empty;
                
                if (username != message.From.Username)
                {
                    using (System.IO.Stream stream = new System.IO.MemoryStream())
                    {
                        Properties.Resources.norights.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        stream.Position = 0;
                        await _telegram.SendPhotoAsync(_chatid, new FileToSend("norights.jpg", stream), replyMarkup: new ReplyKeyboardHide());
                    }
                    return;
                }

                // [0]-Commando; [1+]-Argument
                string[] textCMD = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                TelegramUtilTask cmd = getTask(textCMD[0]);
                switch (cmd)
                {
                    case TelegramUtilTask.UNKNOWN:
                        telegramAnswer = string.Format("Usage:\r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}",
                            @"/stats - Get Current Stats",
                            @"/livestats - Enable/Disable Live Stats",
                            @"/information <topic> - Enable/Disable Information topics",
                            @"/top <HowMany?> - Outputs Top (?) Pokemons",
                            @"/forceevolve - Forces Evolve");
                        break;
                    case TelegramUtilTask.GET_STATS:
                        var inventory = await _client.Inventory.GetInventory();
                        var profil = await _client.Player.GetPlayer();
                        var ps = await _client.Inventory.GetPlayerStats(); 

                        int l = ps.First().Level; 
                        long expneeded = ((ps.First().NextLevelXp - ps.First().PrevLevelXp) - StringUtils.getExpDiff(ps.First().Level));
                        long curexp = ((ps.First().Experience - ps.First().PrevLevelXp) - StringUtils.getExpDiff(ps.First().Level));
                        double curexppercent = (Convert.ToDouble(curexp) / Convert.ToDouble(expneeded)) * 100;
                        string curloc = _client.CurrentLatitude + "%20" + _client.CurrentLongitude;
                        curloc = curloc.Replace(",", ".");
                        string curlochtml = "https://www.google.com/maps/search/" + curloc + "/";
                        double shortenLng = Math.Round(_client.CurrentLongitude, 3);
                        double shortenLat = Math.Round(_client.CurrentLatitude, 3);
                        string pokemap = shortenLat + ";" + shortenLng;
                        pokemap = pokemap.Replace(",", ".").Replace(";", ",");
                        string pokevishtml = "https://skiplagged.com/pokemon/#" + pokemap +",14";
                        telegramAnswer +=
                            "\nNickname: " + profil.PlayerData.Username 
                            + "\nLevel: " + ps.First().Level
                            + "\nEXP Needed: " + ((ps.First().NextLevelXp - ps.First().PrevLevelXp) - StringUtils.getExpDiff(ps.First().Level))
                            + $"\nCurrent EXP: {curexp} ({Math.Round(curexppercent)}%)"
                            + "\nEXP to Level up: " + ((ps.First().NextLevelXp) - (ps.First().Experience))
                            + "\nKM walked: " + ps.First().KmWalked
                            + "\nPokeStops visited: " + ps.First().PokeStopVisits
                            + "\nStardust: " + profil.PlayerData.Currencies.ToArray()[1].Amount
                            + "\nPokemons: " + await _client.Inventory.getPokemonCount() + "/" + profil.PlayerData.MaxPokemonStorage
                            + "\nItems: " + await _client.Inventory.getInventoryCount() + " / " + profil.PlayerData.MaxItemStorage
                            + "\nCurentLocation:\n" + curlochtml
                            + "\nPokevision:\n" + pokevishtml; 
                        break;
                    case TelegramUtilTask.GET_TOPLIST:
                        int shows = 10;
                        if (textCMD.Length > 1 && !int.TryParse(textCMD[1], out shows))
                        {
                            telegramAnswer += $"Error! This is not a Number: {textCMD[1]}\nNevermind...\n";
                            shows = 10; //TryParse error will reset to 0
                        }
                        telegramAnswer += "Showing " + shows + " Pokemons...\nSorting...";
                        await _telegram.SendTextMessageAsync(_chatid, telegramAnswer, replyMarkup: new ReplyKeyboardHide());

                        var myPokemons = await _inventory.GetPokemons();
                        myPokemons = myPokemons.OrderByDescending(x => x.Cp);
                        var profile = await _client.Player.GetPlayer();
                        telegramAnswer = $"Top {shows} Pokemons of {profile.PlayerData.Username}:";

                        IEnumerable<PokemonData> topPokemon = myPokemons.Take(shows);
                        foreach (PokemonData pokemon in topPokemon)
                        {
                            telegramAnswer += string.Format("\n{0} ({1})  |  CP: {2} ({3}% perfect)",
                                pokemon.PokemonId,
                                StringUtils.getPokemonNameGer(pokemon.PokemonId),
                                pokemon.Cp,
                                Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon), 2));
                        }
                        break;
                    case TelegramUtilTask.SWITCH_LIVESTATS:
                        _livestats = SwitchAndGetAnswer(_livestats, out telegramAnswer, "Live Stats");
                        break;
                    case TelegramUtilTask.SWITCH_INFORMATION:
                        //_informations = SwitchAndGetAnswer(_informations, out telegramAnswer, "Information");
                        Array topics = Enum.GetValues(typeof(TelegramUtilInformationTopics));
                        if (textCMD.Length > 1)
                        {
                            if(textCMD[1] == "all-enable")
                            {
                                foreach (TelegramUtilInformationTopics topic in topics)
                                {
                                    String niceName = topic.ToString().Substring(0, 1).ToUpper() + topic.ToString().Substring(1).ToLower();
                                    telegramAnswer += "Enabled information topic " + niceName + "\n";
                                    _information[topic] = true;
                                }
                                break;
                            }
                            else if(textCMD[1] == "all-disable")
                            {
                                foreach (TelegramUtilInformationTopics topic in topics)
                                {
                                    String niceName = topic.ToString().Substring(0, 1).ToUpper() + topic.ToString().Substring(1).ToLower();
                                    telegramAnswer += "Disabled information topic " + niceName + "\n";
                                    _information[topic] = false;
                                }
                                break;
                            }
                            else { 
                                foreach (TelegramUtilInformationTopics topic in topics)
                                {
                                    if (textCMD[1].ToLower() == topic.ToString().ToLower()) {
                                        String niceName = topic.ToString().Substring(0, 1).ToUpper() + topic.ToString().Substring(1).ToLower();
                                        _information[topic] = !_information[topic];
                                        telegramAnswer = (_information[topic] ? "Dis" : "En") + "abled information topic " + niceName + "\n";
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (TelegramUtilInformationTopics topic in topics)
                            {
                                String niceName = topic.ToString().Substring(0, 1).ToUpper() + topic.ToString().Substring(1).ToLower();
                                telegramAnswer += " - " + niceName + "\n";
                                telegramAnswer += " -     " + _informationDescription[topic] + "\n";
                                telegramAnswer += " -     Currently " + (_information[topic] ? "enabled" : "disabled") + "\n";
                                telegramAnswer += "\n";
                            }

                            telegramAnswer += " - all-disable\n";
                            telegramAnswer += " -     " + TranslationHandler.GetString("telegram-disable-all", "Disable all topics") + "\n";
                            telegramAnswer += "\n";

                            telegramAnswer += " - all-enable\n";
                            telegramAnswer += " -     " + TranslationHandler.GetString("telegram-enable-all", "Enable all topics") + "\n";
                            telegramAnswer += "\n";
                            break;
                        }

                        break;
                    case TelegramUtilTask.RUN_FORCEEVOLVE:
                        IEnumerable<PokemonData> pokemonToEvolve = await _inventory.GetPokemonToEvolve(null);
                        if (pokemonToEvolve.Count() > 3)
                        {
                            await _inventory.UseLuckyEgg(_client);
                        }
                        foreach (PokemonData pokemon in pokemonToEvolve)
                        {
                            if (_clientSettings.pokemonsToEvolve.Contains(pokemon.PokemonId))
                            {
                                var evolvePokemonOutProto = await _client.Inventory.EvolvePokemon((ulong)pokemon.Id);
                                if (evolvePokemonOutProto.Result == POGOProtos.Networking.Responses.EvolvePokemonResponse.Types.Result.Success)
                                {
                                    await _telegram.SendTextMessageAsync(_chatid, $"Evolved {pokemon.PokemonId} successfully for {evolvePokemonOutProto.ExperienceAwarded}xp", replyMarkup: new ReplyKeyboardHide());
                                }
                                else
                                {
                                    await _telegram.SendTextMessageAsync(_chatid, $"Failed to evolve {pokemon.PokemonId}. EvolvePokemonOutProto.Result was {evolvePokemonOutProto.Result}, stopping evolving {pokemon.PokemonId}", replyMarkup: new ReplyKeyboardHide());
                                }
                                await RandomHelper.RandomDelay(1000, 2000);
                            }
                        }
                        telegramAnswer = "Done.";
                        break;
                }

                await _telegram.SendTextMessageAsync(_chatid, telegramAnswer, replyMarkup: new ReplyKeyboardHide());
            }
            catch (Exception ex)
            {
                if (ex is ApiRequestException)
                    await _telegram.SendTextMessageAsync(_chatid, (ex as ApiRequestException).Message, replyMarkup: new ReplyKeyboardHide());
            }
        }

        private bool SwitchAndGetAnswer(bool oldSwitchState, out string answerText, string text)
        {
            answerText = string.Format("{0} {1}.", oldSwitchState ? "Disabled" : "Enabled", text);
            return !oldSwitchState;
        }

        private TelegramUtilTask getTask(string cmdString)
        {
            if(_telegramCommandos.ContainsKey(cmdString))
            {
                TelegramUtilTask task;
                if (_telegramCommandos.TryGetValue(cmdString, out task))
                    return task;
            }
            return TelegramUtilTask.UNKNOWN;
        }

        public async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _telegram.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
