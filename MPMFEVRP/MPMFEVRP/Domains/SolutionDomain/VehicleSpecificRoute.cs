using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class VehicleSpecificRoute
    {
        //Defining fields
        IProblemModel problemModel;

        Vehicle vehicle;
        public Vehicle Vehicle { get { return vehicle; } }
        public VehicleCategories VehicleCategory { get { return vehicle.Category; } }

        bool alwaysClosedLoop;//
        public bool AlwaysClosedLoop { get { return alwaysClosedLoop; } }
        public bool AtDepot { get { return (siteVisits.Last().SiteID == problemModel.SRD.GetSingleDepotID()); } }
        public string LastSiteID { get { return siteVisits.Last().SiteID; } }

        //Other fields
        List<SiteVisit> siteVisits;
        //public List<string> SitesVisited_ID { get { return sitesVisited_ID; } }//Keep unaccessible until needed, which hopefully won't be needed at all

    }

    public class SiteVisit
    {
        //site visited
        string siteID; public string SiteID { get { return siteID; } }

        //time and SOC at the site
        double arrivalTime;
        double arrivalSOC;
        double SOCGain;
        double departureTime;
        double departureSOC;

        //the cumulatives (for statistics)
        double cumulativeTravelDistance; //(by arrival at this site)
        public double CumulativeTravelDistance { get { return cumulativeTravelDistance; } }

        //feasibility
        public bool GetTimeFeasible(double Tmax)  { return arrivalTime <= Tmax; }
        public bool GetSOCFeasible() { return arrivalSOC >= 0; }
        public bool GetFeasible(double Tmax) { return (GetSOCFeasible() && GetTimeFeasible(Tmax)); }

        //Constructors
        //public SiteVisit() { }//empty constructor, make accessible when needed, hopefully never
        public SiteVisit(Site depot)
        {
            if (depot.SiteType != SiteTypes.Depot)
                throw new Exception("SiteVisit special constructor for depot invoked for a non-depot site!");
            siteID = depot.ID;
            arrivalTime = 0.0;
            arrivalSOC = 1.0;
            SOCGain = 0.0;
            departureTime = 0.0;
            departureSOC = 1.0;
            cumulativeTravelDistance = 0.0;
        }
        public SiteVisit(SiteVisit previousSV, Site currentSite, double travelDistance, double travelTime, Vehicle vehicle, double energyConsumption = 0.0, double stayDuration = double.MaxValue)
        {
            // travelDistance, travelTime and energyConsumption are all about the travel between the previous site and this
            //Obviously, if there is no previous site, this constructor cannot be used!
            //A limitation is that we must know the stay duration gain beforehand, can't come back to optimize it!

            siteID = currentSite.ID;

            arrivalTime = previousSV.departureTime + travelTime;
            arrivalSOC = previousSV.departureSOC - energyConsumption;
            SOCGain = Utils.Calculators.MaxSOCGainAtSite(currentSite,vehicle,stayDuration);
            departureTime = arrivalTime + stayDuration;
            departureSOC = arrivalSOC + SOCGain;

            cumulativeTravelDistance = previousSV.cumulativeTravelDistance + travelDistance;
        }
        public SiteVisit(SiteVisit previousSV, Site currentSite, double travelDistance, double travelTime, Vehicle vehicle, double energyConsumption = 0.0, double stayDuration = double.MaxValue, double maxSOCGain = 1.0)
        {
            // travelDistance, travelTime and energyConsumption are all about the travel between the previous site and this
            //Obviously, if there is no previous site, this constructor cannot be used!
            //A limitation is that we must know the stay duration or maximum SOC gain beforehand, can't come back to optimize it!

            siteID = currentSite.ID;

            arrivalTime = previousSV.departureTime + travelTime;
            arrivalSOC = previousSV.departureSOC - energyConsumption;
            SOCGain = Utils.Calculators.MaxSOCGainAtSite(currentSite, vehicle, stayDuration);
            departureTime = arrivalTime + stayDuration;
            departureSOC = arrivalSOC + SOCGain;

            cumulativeTravelDistance = previousSV.cumulativeTravelDistance + travelDistance;
        }

    }
}
