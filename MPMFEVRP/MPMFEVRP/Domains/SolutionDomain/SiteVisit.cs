using MPMFEVRP.Domains.ProblemDomain;
using System;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class SiteVisit
    {
        double mipErrorTime = 0.01;
        double mipErrorSOE = 0.0005;
        
        //site visited
        Site site;
        public Site Site { get { return site; } }
        public string SiteID { get { return site.ID; } }

        //time and SOC at the site
        double arrivalTime; public double ArrivalTime { get { return arrivalTime; } }
        double arrivalSOC;
        double socGain; public double SOCGain { get { return socGain; } }
        double departureTime;
        double departureSOC;

        //the cumulatives (for statistics)
        double cumulativeTravelDistance; //(by arrival at this site)
        public double CumulativeTravelDistance { get { return cumulativeTravelDistance; } }

        //feasibility
        public bool GetTimeFeasible(double Tmax) { return arrivalTime <= Tmax+ mipErrorTime; }
        public bool GetSOCFeasible() { return arrivalSOC + mipErrorSOE >= 0.0; }
        public bool GetFeasible(double Tmax) { return (GetSOCFeasible() && GetTimeFeasible(Tmax)); }

        //Constructors
        //public SiteVisit() { }//empty constructor, make accessible when needed, hopefully never
        public SiteVisit(Site depot, double batteryCapacity)
        {
            if (depot.SiteType != SiteTypes.Depot)
                throw new Exception("SiteVisit special constructor for depot invoked for a non-depot site!");
            site = depot;
            arrivalTime = 0.0;
            arrivalSOC = batteryCapacity;
            socGain = 0.0;
            departureTime = 0.0;
            departureSOC = batteryCapacity;
            cumulativeTravelDistance = 0.0;
        }
        public SiteVisit(SiteVisit previousSV, Site currentSite, double travelDistance, double travelTime, Vehicle vehicle, double energyConsumption = 0.0, double stayDuration = double.MaxValue)
        {
            // travelDistance, travelTime and energyConsumption are all about the travel between the previous site and this
            //Obviously, if there is no previous site, this constructor cannot be used!
            //A limitation is that we must know the stay duration gain beforehand, can't come back to optimize it!

            site = currentSite;

            arrivalTime = previousSV.departureTime + travelTime;
            departureTime = arrivalTime + stayDuration;
            if (vehicle.Category == VehicleCategories.GDV)
            {
                arrivalSOC = previousSV.departureSOC;
                socGain = 0.0;
                departureSOC = arrivalSOC;
            }
            else
            {
                arrivalSOC = previousSV.departureSOC - energyConsumption;
                double maxSOCGain = Utils.Calculators.MaxSOCGainAtSite(currentSite, vehicle, stayDuration);
                departureSOC = Math.Min(arrivalSOC + maxSOCGain, vehicle.BatteryCapacity);
                socGain = departureSOC - arrivalSOC;
            }
            cumulativeTravelDistance = previousSV.cumulativeTravelDistance + travelDistance;
        }
        public SiteVisit(SiteVisit twinSiteVisit)
        {
            site = twinSiteVisit.site;

            arrivalTime = twinSiteVisit.arrivalTime;
            arrivalSOC = twinSiteVisit.arrivalSOC;
            socGain = twinSiteVisit.socGain;
            departureTime = twinSiteVisit.departureTime;
            departureSOC = twinSiteVisit.departureSOC;

            cumulativeTravelDistance = twinSiteVisit.cumulativeTravelDistance;
        }
    }
}
