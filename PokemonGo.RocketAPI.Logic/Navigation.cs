using PokemonGo.RocketAPI.Logic.Utils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Helpers;
using POGOProtos.Networking.Responses;
using POGOProtos.Map.Fort;

namespace PokemonGo.RocketAPI.Logic
{
    public class Navigation
    {
        public static double DistanceBetween2Coordinates(double Lat1, double Lng1, double Lat2, double Lng2)
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
        
        private readonly Client _client;
        private const double SpeedDownTo = 10 / 3.6;
        private static readonly Random RandomDevice = new Random();

        public Navigation(Client client)
        {
            _client = client;
        }

        public async Task<PlayerUpdateResponse> HumanLikeWalking(GeoCoordinate targetLocation,
        double walkingSpeedInKilometersPerHour, Func<Task> functionExecutedWhileWalking)
        { 
            var randomFactor = 0.5f;
            var randomMin = (int)(walkingSpeedInKilometersPerHour * (1 - randomFactor));
            var randomMax = (int)(walkingSpeedInKilometersPerHour * (1 + randomFactor));
            var RandomWalkSpeed = RandomDevice.Next(randomMin, randomMax);
            walkingSpeedInKilometersPerHour = RandomWalkSpeed;

            var speedInMetersPerSecond = walkingSpeedInKilometersPerHour / 3.6;
        
            var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
            var distanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
            Logger.ColoredConsoleWrite(ConsoleColor.DarkCyan, $"Distance to target location: {distanceToTarget:0.##} meters. Will take {distanceToTarget / speedInMetersPerSecond:0.##} seconds!");

            var nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
            var nextWaypointDistance = speedInMetersPerSecond;
            var waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);
             
            //Initial walking
            var requestSendDateTime = DateTime.Now;
            var result =
                await
                    _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude, waypoint.Altitude);

            if (functionExecutedWhileWalking != null)
                await functionExecutedWhileWalking();

            var locatePokemonWhileWalkingDateTime = DateTime.Now;
            do
            {
                var millisecondsUntilGetUpdatePlayerLocationResponse =
                    (DateTime.Now - requestSendDateTime).TotalMilliseconds;

                sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);

                if (currentDistanceToTarget < 40)
                {
                    if (speedInMetersPerSecond > SpeedDownTo)
                    {
                        Logger.ColoredConsoleWrite(ConsoleColor.DarkCyan, $"We are within 40 meters of the target. Slowing down to ~10 km/h to not pass the target.");
                        speedInMetersPerSecond = SpeedDownTo;
                    }
                }

                nextWaypointDistance = Math.Min(currentDistanceToTarget, millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
                nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
                waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);
   
                requestSendDateTime = DateTime.Now;

                result =
                    await
                        _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
                            waypoint.Altitude);
                if (functionExecutedWhileWalking != null)
                    await functionExecutedWhileWalking();// look for pokemon 

                await RandomHelper.RandomDelay(500, 600);
            } while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 30);
            return result;
        }

        private double calcTime(ref FortData[] pokeStops, List<int> _chromosome, double walkingSpeedInKilometersPerHour)
        {
            double time = 0.0;
            for (int i = 0; i < _chromosome.Count - 1; ++i)
            {
                double distance = DistanceBetween2Coordinates(pokeStops[_chromosome[i]].Latitude, pokeStops[_chromosome[i]].Longitude, pokeStops[_chromosome[i + 1]].Latitude, pokeStops[_chromosome[i + 1]].Longitude);
                if (distance <= 40)
                {
                    time += distance / Logic.SpeedDownTo;
                }
                else
                {
                    time += distance * 3.6 / walkingSpeedInKilometersPerHour;
                }
            }
            return time;
        }
        private int calcFitness(ref FortData[] pokeStops, List<int> _chromosome, double walkingSpeedInKilometersPerHour)
        {
            if (_chromosome.Count <= 2) return 0;

            double time = 0.0;
            double length = 0.0;
            for (int i = 0; i < _chromosome.Count - 1; ++i)
            {
                double distance = DistanceBetween2Coordinates(pokeStops[_chromosome[i]].Latitude, pokeStops[_chromosome[i]].Longitude, pokeStops[_chromosome[i + 1]].Latitude, pokeStops[_chromosome[i + 1]].Longitude);
                if (distance <= 40)
                {
                    time += distance / Logic.SpeedDownTo;
                }
                else
                {
                    time += distance * 3.6 / walkingSpeedInKilometersPerHour;
                }
                length += distance;
            }

            if (time <= 380 || !(time > 0.0)) return 0;

            if (_client.Settings.navigation_option == 1)
            {
                return Convert.ToInt32((_chromosome.Count * 10000) / time);
            } else
            {
                return Convert.ToInt32(_chromosome.Count * length / time);
            }
        }
        private List<int> calcCrossing(List<int> _chromosome1, List<int> _chromosome2)
        {
            List<int> child = new List<int>(_chromosome1);

            if (child.Count <= 3) return child;

            Random rnd = new Random();
            int p = rnd.Next(1, child.Count - 2);
            for (; p < child.Count - 1; ++p)
            {
                for (int i = 0; i < _chromosome2.Count; ++i)
                {
                    var tempIndex = child.FindIndex(x => x == _chromosome2[i]);
                    if (tempIndex > p || tempIndex < 0)
                    {
                        child[p] = _chromosome2[i];
                        break;
                    }
                }
            }

            return child;
        }
        private void mutate(ref List<int> _chromosome)
        {
            Random rnd = new Random();
            int i1 = rnd.Next(1, _chromosome.Count - 2), i2 = rnd.Next(1, _chromosome.Count - 2);
            int temp = _chromosome[i1];
            _chromosome[i1] = _chromosome[i2];
            _chromosome[i2] = temp;
        }

        private List<List<int>> selection(FortData[] pokeStops, List<List<int>> population, double walkingSpeedInKilometersPerHour)
        {
            List<List<int>> listSelection = new List<List<int>>();
            int sumPop = 0;
            List<int> fittnes = new List<int>();

            for (int c = 0; c < population.Count; ++c)
            {
                var temp = calcFitness(ref pokeStops, population[c], walkingSpeedInKilometersPerHour);
                sumPop += temp;
                fittnes.Add(temp);
            }
            List<int> fittnesSorted = new List<int>(fittnes);
            fittnesSorted.Sort();

            if (sumPop < 2) return listSelection;

            Random rnd = new Random();
            int selcetedChr = -1;
            do
            {
                var tempIndex = rnd.Next(0, sumPop);
                int tempSumPop = 0;
                for (int c = fittnesSorted.Count - 1; c > 0; --c)
                {
                    tempSumPop += fittnesSorted[c];
                    if (tempSumPop > tempIndex)
                    {
                        var tempSelcetedChr = fittnes.FindIndex(x => x == fittnesSorted[c]);
                        if (tempSelcetedChr != selcetedChr && !(tempSelcetedChr < 0))
                        {
                            selcetedChr = tempSelcetedChr;
                            listSelection.Add(population[selcetedChr]);
                            break;
                        }
                    }

                }
            } while (listSelection.Count < 2);



            return listSelection;
        }
        

        public FortData[] pathByNearestNeighbour(FortData[] pokeStops, double walkingSpeedInKilometersPerHour)
        {
            ////Start Gen. alg.
            //if (pokeStops.Length > 15)
            //{
            //    //Config
            //    int ITERATIONS = 100000;
            //    int POPSIZE = pokeStops.Length * 60;
            //    double CROSSPROP = 99;
            //    double MUTPROP = 20;


            //    List<List<int>> population = new List<List<int>>();
            //    Random rnd = new Random();
            //    //Create Population
            //    for (var i = POPSIZE; i > 0; --i)
            //    {
            //        List<int> tempChromosome = new List<int>();
            //        int items = rnd.Next(2, pokeStops.Length * 3 / 4);
            //        do
            //        {
            //            int tempIndex = rnd.Next(0, pokeStops.Length - 1);
            //            //Add only if new Index
            //            while (tempChromosome.Exists(x => x == tempIndex))
            //            {
            //                tempIndex = rnd.Next(0, pokeStops.Length - 1);
            //            }
            //            tempChromosome.Add(tempIndex);
            //        } while (--items > 0);

            //        if (calcFitness(ref pokeStops, tempChromosome, walkingSpeedInKilometersPerHour) > 0.0)
            //        {
            //            tempChromosome.Add(tempChromosome[0]);
            //            population.Add(tempChromosome);
            //        }
            //    }

            //    if (population.Count > 10)
            //    {
            //        for (int i = 0; i < ITERATIONS; ++i)
            //        {
            //            //Selection
            //            var parents = selection(pokeStops, population, walkingSpeedInKilometersPerHour);
            //            List<int> child1 = parents[0], child2 = parents[1];
            //            //Crossing
            //            if (rnd.Next(0, 100) < CROSSPROP)
            //            {
            //                child1 = calcCrossing(parents[0], parents[1]);
            //                child2 = calcCrossing(parents[1], parents[0]);
            //            }
            //            //Mutation
            //            if (rnd.Next(0, 100) < MUTPROP)
            //            {
            //                mutate(ref child1);
            //            }
            //            if (rnd.Next(0, 100) < MUTPROP)
            //            {
            //                mutate(ref child2);
            //            }

            //            //Replace
            //            List<int> fittnes = new List<int>();
            //            int sumPop = 0;
            //            for (int c = 0; c < population.Count; ++c)
            //            {
            //                var temp = calcFitness(ref pokeStops, population[c], walkingSpeedInKilometersPerHour);
            //                sumPop += temp;
            //                fittnes.Add(temp);
            //            }
            //            List<int> fittnesSorted = new List<int>(fittnes);
            //            fittnesSorted.Sort();

            //            if (fittnesSorted[0] <= calcFitness(ref pokeStops, child1, walkingSpeedInKilometersPerHour))
            //            {
            //                var tempSelcetedChr = fittnes.FindIndex(x => x == fittnesSorted[0]);
            //                population[tempSelcetedChr] = child1;
            //            }
            //            if (fittnesSorted[1] <= calcFitness(ref pokeStops, child2, walkingSpeedInKilometersPerHour))
            //            {
            //                var tempSelcetedChr = fittnes.FindIndex(x => x == fittnesSorted[1]);
            //                population[tempSelcetedChr] = child2;
            //            }

            //            //get best Generation
            //            List<int> fittnes2 = new List<int>();
            //            for (int c = 0; c < population.Count; ++c)
            //            {
            //                var temp = calcFitness(ref pokeStops, population[c], walkingSpeedInKilometersPerHour);
            //                fittnes2.Add(temp);
            //            }
            //            List<int> fittnesSorted2 = new List<int>(fittnes2);
            //            fittnesSorted2.Sort();
            //            var tempSelcetedChr2 = fittnes2.FindIndex(x => x == fittnesSorted2[fittnesSorted2.Count - 1]);

            //            List<FortData> newPokeStops = new List<FortData>();
            //            foreach (var element in population[tempSelcetedChr2])
            //            {
            //                newPokeStops.Add(pokeStops[element]);
            //            }
            //            Logger.ColoredConsoleWrite(ConsoleColor.Yellow, $"{Math.Round(newPokeStops.Count * 3600 / calcTime(ref pokeStops, population[tempSelcetedChr2], walkingSpeedInKilometersPerHour))} PokeStops per Hour.");
            //            return newPokeStops.ToArray();
            //        }
            //    }
            //}
            //End gen. alg

            //Normal calculation
            for (var i = 1; i < pokeStops.Length - 1; i++)
            {
                var closest = i + 1;
                var cloestDist = LocationUtils.CalculateDistanceInMeters(pokeStops[i].Latitude, pokeStops[i].Longitude, pokeStops[closest].Latitude, pokeStops[closest].Longitude);
                for (var j = closest; j < pokeStops.Length; j++)
                {
                    var initialDist = cloestDist;
                    var newDist = LocationUtils.CalculateDistanceInMeters(pokeStops[i].Latitude, pokeStops[i].Longitude, pokeStops[j].Latitude, pokeStops[j].Longitude);
                    if (initialDist > newDist)
                    {
                        cloestDist = newDist;
                        closest = j;
                    }
                }
                var tmpPok = pokeStops[closest];
                pokeStops[closest] = pokeStops[i + 1];
                pokeStops[i + 1] = tmpPok;
            }
            return pokeStops;
        }
    }
}