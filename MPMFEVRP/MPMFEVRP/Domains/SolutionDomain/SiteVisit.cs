using System;
using MPMFEVRP.Domains.ProblemDomain;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class SiteVisit
    {
        //site visited
        Site site;
        public Site Site { get { return site; } }
        public string SiteID { get { return site.ID; } }

        //time and SOC at the site
        double arrivalTime;
        double arrivalSOC;
        double socGain; public double SOCGain { get { return socGain; } }
        double departureTime;
        double departureSOC;

        //the cumulatives (for statistics)
        double cumulativeTravelDistance; //(by arrival at this site)
        public double CumulativeTravelDistance { get { return cumulativeTravelDistance; } }

        //feasibility
        public bool GetTimeFeasible(double Tmax) { return arrivalTime <= Tmax; }
        public bool GetSOCFeasible() { return arrivalSOC >= 0; }
        public bool GetFeasible(double Tmax) { return (GetSOCFeasible() && GetTimeFeasible(Tmax)); }

        //Constructors
        //public SiteVisit() { }//empty constructor, make accessible when needed, hopefully never
        public SiteVisit(Site depot)
        {
            if (depot.SiteType != SiteTypes.Depot)
                throw new Exception("SiteVisit special constructor for depot invoked for a non-depot site!");
            site = depot;
            arrivalTime = 0.0;
            arrivalSOC = 1.0;
            socGain = 0.0;
            departureTime = 0.0;
            departureSOC = 1.0;
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
                arrivalSOC = 1.0;
                socGain = 0.0;
                departureSOC = 1.0;
            }
            else
            {
                arrivalSOC = previousSV.departureSOC - energyConsumption;
                socGain = Utils.Calculators.MaxSOCGainAtSite(currentSite, vehicle, stayDuration);
                departureSOC = arrivalSOC + socGain;
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
