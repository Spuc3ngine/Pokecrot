using System;
using System.Device.Location;

namespace PokemonGo.RocketAPI.Logic.Utils
{
    internal class GeoUtils
    {
        public static GeoCoordinate GetNextPoint(GeoCoordinate current, double bearing, double range)
        {
            double latA = current.Latitude * DegreesToRadians;
            double lonA = current.Longitude * DegreesToRadians;
            double angularDistance = range / EarthRadius;
            double trueCourse = bearing * DegreesToRadians;

            double lat = Math.Asin(
                Math.Sin(latA) * Math.Cos(angularDistance) +
                Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));

            double dlon = Math.Atan2(
                Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA),
                Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));

            double lon = ((lonA + dlon + Math.PI) % TwoPi) - Math.PI;
            return new GeoCoordinate(lat * RadiansToDegrees, lon * RadiansToDegrees);
        }

        public static Double GetBearing(GeoCoordinate p1, GeoCoordinate p2)
        {
            return DegreeBearing(p1.Latitude, p1.Longitude, p2.Latitude, p2.Longitude);
        }


        static double DegreeBearing(double lat1, double lon1, double lat2, double lon2)
        {
            //const double r = 6371; //earth’s radius (mean radius = 6,371km)
            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
            {
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            }

            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        public static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        //private const double EARTH_RADIUS_IN_MILES = 3964.037911746;
        //private const double EARTH_RADIUS_IN_METERS = 6378137.0;
        private const double EarthRadius = 6378137.0;
        private const double TwoPi = Math.PI * 2;
        private const double DegreesToRadians = 0.0174532925;
        private const double RadiansToDegrees = 57.2957795;
    }
}
