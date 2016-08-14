using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using POGOProtos.Enums;

namespace PokemonGo.RocketAPI.Logic.Utils
{
    class PokeVisionUtil
    {

        private List<spottedPoke> _newSpotted;
        private List<spottedPoke> _alreadySpotted;
        HttpClient.PokemonHttpClient _httpClient;

        public PokeVisionUtil()
        {
            _newSpotted = new List<spottedPoke>();
            _alreadySpotted = new List<spottedPoke>();
            _httpClient = new HttpClient.PokemonHttpClient();
        }


        public async Task<List<spottedPoke>> GetNearPokemons(double lat, double lng)
        {
            _newSpotted.Clear();
            ClearAlreadySpottedByTime();

            HttpResponseMessage response = await _httpClient.GetAsync("https://pokevision.com/map/data/" + lat.ToString().Replace(",", ".") + "/" + lng.ToString().Replace(",", "."));
            HttpContent content = response.Content;
            string result = await content.ReadAsStringAsync();

            dynamic stuff = JsonConvert.DeserializeObject(result);
            if (stuff.status == "success")
            {
                try
                {
                    for (int i = 0; stuff.pokemon[i].id > 0; i++)
                    {
                        if (CatchThisPokemon((Int32)stuff.pokemon[i].id) &&
                            inRange(lat, lng, (Double)(stuff.pokemon[i].latitude), (Double)(stuff.pokemon[i].longitude)) &&
                            !AlreadyCatched((Int32)(stuff.pokemon[i].id)))
                        {
                            _newSpotted.Add(new spottedPoke((Int32)stuff.pokemon[i].pokemonId, (Double)stuff.pokemon[i].latitude, (Double)stuff.pokemon[i].longitude, (Int32)stuff.pokemon[i].expiration_time, (Int32)stuff.pokemon[i].id));
                            _alreadySpotted.Add(new spottedPoke((Int32)stuff.pokemon[i].pokemonId, (Double)stuff.pokemon[i].latitude, (Double)stuff.pokemon[i].longitude, (Int32)stuff.pokemon[i].expiration_time, (Int32)stuff.pokemon[i].id));

                        }

                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return _newSpotted;
        }

        bool CatchThisPokemon(int id)
        {
            return true;
        }

        bool inRange(double curLat, double curLng, double pokeLat, double pokeLng)
        {
            return LocationUtils.CalculateDistanceInMeters(curLat, curLng, pokeLat, pokeLng) < 125;
        }

        void ClearAlreadySpottedByTime()
        {
            int actUnixTime = UnixTimeNow();
            for (int i = 0; i < _alreadySpotted.Count; ++i)
            {
                if (_alreadySpotted[i]._expTime < actUnixTime)
                {
                    _alreadySpotted.RemoveAt(i);
                    --i;
                }
            }
        }

        bool AlreadyCatched(int id)
        {
            foreach (spottedPoke poke in _alreadySpotted)
            {
                if (poke._visionId == id)
                    return true;
            }
            return false;
        }

        public int UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (int)timeSpan.TotalSeconds;
        }

    }

    class spottedPoke
    {
        public PokemonId _pokeId = 0;
        public int _visionId;
        public double _lat;
        public double _lng;
        public int _expTime;

        public spottedPoke(int id, double lat, double lng, int expTime, int visionid)
        {
            _pokeId = (PokemonId)id;
            _lat = lat;
            _lng = lng;
            _expTime = expTime;
            _visionId = visionid;
        }


    }
}
