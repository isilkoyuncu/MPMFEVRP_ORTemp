using MPMFEVRP.Domains.ProblemDomain;
using System;

namespace MPMFEVRP.Utils
{
    class Calculators
    {
        public static double[,] EuclideanDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new Exception("Calculators.EuclideanDistance invoked with different lengths of x and y coordinates.");

            double[,] distance = new double[x.Length, y.Length];
            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < x.Length; j++)
                {
                    distance[i, j] = EuclideanDistance(x[i], x[j], y[i], y[j]);
                }
            }
            return distance;
        }
        public static double EuclideanDistance(double x0, double x1, double y0, double y1)
        {
            return Math.Round(Math.Sqrt(Math.Pow((x0 - x1), 2) + Math.Pow((y0 - y1), 2)), 5);
        }

        public static double[,] HaversineDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new Exception("Calculators.HaversineDistance invoked with different lengths of x and y coordinates.");

            double[,] distance = new double[x.Length, x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < x.Length; j++)
                {
                    distance[i, j] = HaversineDistance(x[i], y[i], x[j], y[j]);
                }
            }
            return distance;
        }
        public static double HaversineDistance(double LonA, double LatA, double LonB, double LatB)
        {
            return Math.Round(2.0 * 4182.44949 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(((LatA * Math.PI / 180.0) - (LatB * Math.PI / 180.0)) / 2.0), 2.0) + Math.Cos((LatB * Math.PI / 180.0)) * Math.Cos((LatA * Math.PI / 180.0)) * Math.Pow(Math.Sin(((LonA * Math.PI / 180.0) - (LonB * Math.PI / 180.0)) / 2.0), 2.0))), 5);
        }

        public static double MaxSOCGainAtSite(Site s, Vehicle v, double maxStayDuration = double.MaxValue)
        {
            if (v.BatteryCapacity != 0)
                return Math.Min(v.BatteryCapacity, Math.Min(s.RechargingRate, v.MaxChargingRate) * maxStayDuration);//TODO: (unit)test to make sure this works as intended in a variety of situations
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
