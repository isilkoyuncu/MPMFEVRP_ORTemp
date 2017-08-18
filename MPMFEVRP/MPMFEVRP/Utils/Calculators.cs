using MPMFEVRP.Domains.ProblemDomain;
using System;

namespace MPMFEVRP.Utils
{
    class Calculators
    {
        public static double EuclideanDistance(double x0, double x1, double y0, double y1)
        {
            return Math.Round(Math.Sqrt(Math.Pow((x0 - x1), 2) + Math.Pow((y0 - y1), 2)), 5);
        }

        public static double HaversineDistance(double LonA, double LatA, double LonB, double LatB)
        {
            return Math.Round(2.0 * 4182.44949 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(((LatA * Math.PI / 180.0) - (LatB * Math.PI / 180.0)) / 2.0), 2.0) + Math.Cos((LatB * Math.PI / 180.0)) * Math.Cos((LatA * Math.PI / 180.0)) * Math.Pow(Math.Sin(((LonA * Math.PI / 180.0) - (LonB * Math.PI / 180.0)) / 2.0), 2.0))), 5);
        }

        public static double MaxSOCGainAtSite(Site s, Vehicle v, double maxStayDuration = double.MaxValue)
        {
            if (v.BatteryCapacity != 0)
                return Math.Min(1, Math.Min(s.RechargingRate, v.MaxChargingRate) * maxStayDuration / v.BatteryCapacity);//TODO: (unit)test to make sure this works as intended in a variety of situations
            else
                throw new DivideByZeroException("Vehicle battery capacity is 0.");
        }

        public static double MaxStayDurationAtSite(Site s, Vehicle v, double maxSOCGainAtSite = 1.0)
        {
            if (Math.Min(s.RechargingRate, v.MaxChargingRate) == 0.0)
            {
                //Cannot charge here and cannot stay longer than service duration if applicable
                if (s.SiteType == SiteTypes.Customer)
                    return s.ServiceDuration;
                else
                    return 0.0;
            }
            else
            {
                return maxSOCGainAtSite * v.BatteryCapacity / Math.Min(s.RechargingRate, v.MaxChargingRate); //TODO: (unit)test to make sure this works as intended in a variety of situations
            }
        }
    }
}
