using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Utility
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
    }
}
