using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
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

        public static double MaxSOCGainAtESSite(ProblemModelBase theProblemModel, Site ES, Vehicle vehicle)
        {
            double batteryCap = vehicle.BatteryCapacity;
            double effectiveRechargingRate = Math.Min(ES.RechargingRate, vehicle.MaxChargingRate);
            double tMax = theProblemModel.CRD.TMax;
            double minStayAndTravelDuration = double.MaxValue;
            string theDepotID = theProblemModel.SRD.GetSingleDepotID();
            foreach (Site otherSite in theProblemModel.SRD.GetAllSitesArray())
            {
                double minTotalTravel = Math.Min(theProblemModel.SRD.GetTravelTime(theDepotID, otherSite.ID) +
                                                 theProblemModel.SRD.GetTravelTime(otherSite.ID, ES.ID) +
                                                 theProblemModel.SRD.GetTravelTime(ES.ID, theDepotID),
                                                 theProblemModel.SRD.GetTravelTime(theDepotID, ES.ID) +
                                                 theProblemModel.SRD.GetTravelTime(ES.ID, otherSite.ID) +
                                                 theProblemModel.SRD.GetTravelTime(otherSite.ID, theDepotID));

                if (minStayAndTravelDuration > otherSite.ServiceDuration + minTotalTravel)
                    minStayAndTravelDuration = otherSite.ServiceDuration + minTotalTravel;
            }
            return Math.Min(batteryCap, ((tMax - minStayAndTravelDuration) * effectiveRechargingRate));
        }

        public static double MaxSOCGainAtESSite(SiteRelatedData SRD, ContextRelatedData CRD, Site ES, Vehicle vehicle)
        {
            double batteryCap = vehicle.BatteryCapacity;
            double effectiveRechargingRate = Math.Min(ES.RechargingRate, vehicle.MaxChargingRate);
            double tMax = CRD.TMax;
            double minStayAndTravelDuration = double.MaxValue;
            string theDepotID = SRD.GetSingleDepotID();
            foreach (Site otherSite in SRD.GetAllSitesArray())
            {
                double minTotalTravel = Math.Min(SRD.GetTravelTime(theDepotID, otherSite.ID) +
                                                 SRD.GetTravelTime(otherSite.ID, ES.ID) +
                                                 SRD.GetTravelTime(ES.ID, theDepotID),
                                                 SRD.GetTravelTime(theDepotID, ES.ID) +
                                                 SRD.GetTravelTime(ES.ID, otherSite.ID) +
                                                 SRD.GetTravelTime(otherSite.ID, theDepotID));

                if (minStayAndTravelDuration > otherSite.ServiceDuration + minTotalTravel)
                    minStayAndTravelDuration = otherSite.ServiceDuration + minTotalTravel;
            }
            return Math.Min(batteryCap, ((tMax - minStayAndTravelDuration) * effectiveRechargingRate));
        }

        public static double MaxSOCGainAtSite(Site site, Vehicle vehicle, double maxStayDuration)
        {
            double batteryCap = vehicle.BatteryCapacity;
            double effectiveRechargingRate = Math.Min(site.RechargingRate, vehicle.MaxChargingRate);

            if (vehicle.BatteryCapacity != 0)
                return Math.Min(batteryCap, maxStayDuration * effectiveRechargingRate);
            else
                throw new DivideByZeroException("Vehicle battery capacity is 0.");
        }

        public static double MaxStayDurationAtSite(Site s, Vehicle v, double maxSOCGainAtSite = double.MaxValue)
        {
            double effectiveRechargingRate = Math.Min(s.RechargingRate, v.MaxChargingRate);
            if (effectiveRechargingRate == 0.0)
            {
                //Cannot charge here and cannot stay longer than service duration if applicable
                if (s.SiteType == SiteTypes.Customer)
                    return s.ServiceDuration;
                else
                    return 0.0;
            }
            else
            {
                if (maxSOCGainAtSite == double.MaxValue) //TODO: (unit)test to make sure this works as intended in a variety of situations
                    return v.BatteryCapacity / effectiveRechargingRate;
                else
                    return maxSOCGainAtSite / effectiveRechargingRate;
            }
        }

        public static double[,] SetUnreachable2Inf(double[,] dist, double Dmax)
        {
            double[,] outcome = new double[dist.GetLength(0), dist.GetLength(1)];
            for (int i = 0; i < dist.GetLength(0); i++)
                for (int j = 0; j < dist.GetLength(1); j++)
                {
                    if (dist[i, j] > Dmax)
                        outcome[i, j] = Double.MaxValue;
                    else
                        outcome[i, j] = dist[i, j];
                }
            return outcome;
        }
    }
}
