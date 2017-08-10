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
        ProblemModelBase problemModel;

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

        //public VehicleSpecificRoute() { }//empty constructor, make accessible when needed, hopefully never
        public VehicleSpecificRoute(ProblemModelBase problemModel, Vehicle vehicle, bool alwaysClosedLoop = true)
        {
            this.problemModel = problemModel;
            this.vehicle = vehicle;
            this.alwaysClosedLoop = alwaysClosedLoop;

            siteVisits = new List<SiteVisit>();
            if (alwaysClosedLoop)
                siteVisits.Add(new SiteVisit(problemModel.SRD.GetSingleDepotSite()));
        }
        public VehicleSpecificRoute(ProblemModelBase problemModel, Vehicle vehicle, List<string> nondepotSiteIDsInOrder, List<double> ESStayDurations = null, bool alwaysClosedLoop = true) : this(problemModel, vehicle, alwaysClosedLoop: alwaysClosedLoop)
        {
            //nondepotSiteIDsInOrder is named this way as a constant reminder that the sites are not just in an arbitrary list (like customer set) but are in fact in a route!
            //nondepotSiteIDsInOrder CANNOT contain the depot!
            if ((nondepotSiteIDsInOrder != null) && (nondepotSiteIDsInOrder.Contains(problemModel.SRD.GetSingleDepotID())))
                throw new Exception("VehicleSpecificRoute invoked with a nondepotSiteIDsInOrder that contains the depot!");
            int ESCounter = 0;
            List<string> visitedESsExtracted = new List<string>();
            foreach (string sID in nondepotSiteIDsInOrder)
                if (problemModel.SRD.GetSiteByID(sID).SiteType == SiteTypes.ExternalStation)
                {
                    visitedESsExtracted.Add(sID);
                    ESCounter++;
                }
            RechargingDurationAndAllowableDepartureStatusFromES rechargePolicy = problemModel.ProblemCharacteristics.GetParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION).GetValue<RechargingDurationAndAllowableDepartureStatusFromES>();
            List<double> ESStayDurationsToUse = new List<double>();
            switch (rechargePolicy)
            {
                case RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full:
                    //In this case, don't bother to provide ESStayDurations beforehand, we'll just ignore it for robustness
                    for (int i = 0; i < ESCounter; i++)
                        ESStayDurationsToUse.Add(Utils.Calculators.MaxStayDurationAtSite(problemModel.SRD.GetSiteByID(visitedESsExtracted[i]), vehicle)); //TODO unit test: all 30mins for EMH problems
                    break;
                case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full:
                    //In this case too, don't bother to provide ESStayDurations beforehand, we'll just ignore it for robustness. We'll instead calculate them JIT right before adding that siteVisit
                    //Hence, ESStayDurationsToUse stays empty until the time of use
                    break;
                case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial:
                    if ((ESStayDurations == null) || (ESStayDurations.Count != ESCounter))
                        throw new Exception("VehicleSpecificRoute invoked with a wrongly populated ESStayDurations list!");
                    ESStayDurationsToUse = ESStayDurations;
                    break;
            }
            if ((nondepotSiteIDsInOrder == null) || (nondepotSiteIDsInOrder.Count == 0))
                return;
            if (!alwaysClosedLoop)
                siteVisits.Add(new SiteVisit(problemModel.SRD.GetSingleDepotSite()));

            //Finally, we can start populating the route
            ESCounter = 0;
            foreach (string siteID in nondepotSiteIDsInOrder)
            {
                string previousSiteID = siteVisits.Last().SiteID;
                Site currentSite = problemModel.SRD.GetSiteByID(siteID);
                string currentSiteID = currentSite.ID;
                if (currentSite.SiteType == SiteTypes.Customer)
                    siteVisits.Add(new SiteVisit(siteVisits.Last(), currentSite, problemModel.SRD.GetDistance(previousSiteID, currentSiteID), problemModel.SRD.GetTravelTime(previousSiteID, currentSiteID), vehicle, energyConsumption: problemModel.SRD.GetEVEnergyConsumption(previousSiteID, currentSiteID), stayDuration: currentSite.ServiceDuration));
                else//site type is ES
                {
                    if (rechargePolicy == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
                        ESStayDurationsToUse.Add(Utils.Calculators.MaxStayDurationAtSite(currentSite, vehicle));
                    siteVisits.Add(new SiteVisit(siteVisits.Last(), currentSite, problemModel.SRD.GetDistance(previousSiteID, currentSiteID), problemModel.SRD.GetTravelTime(previousSiteID, currentSiteID), vehicle, energyConsumption: problemModel.SRD.GetEVEnergyConsumption(previousSiteID, currentSiteID), stayDuration: ESStayDurationsToUse[ESCounter]));
                    ESCounter++;
                }
            }//foreach(string siteID in nondepotSiteIDsInOrder)
            if (alwaysClosedLoop)
                siteVisits.Add(new SiteVisit(siteVisits.Last(), problemModel.SRD.GetSingleDepotSite(), problemModel.SRD.GetDistance(siteVisits.Last().SiteID, problemModel.SRD.GetSingleDepotID()), problemModel.SRD.GetTravelTime(siteVisits.Last().SiteID, problemModel.SRD.GetSingleDepotID()), vehicle, energyConsumption: problemModel.SRD.GetEVEnergyConsumption(siteVisits.Last().SiteID, problemModel.SRD.GetSingleDepotID())));
        }

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
            departureTime = arrivalTime + stayDuration;
            if (vehicle.Category == VehicleCategories.GDV)
            {
                arrivalSOC = 1.0;
                SOCGain = 0.0;
                departureSOC = 1.0;
            }
            else
            {
                arrivalSOC = previousSV.departureSOC - energyConsumption;
                SOCGain = Utils.Calculators.MaxSOCGainAtSite(currentSite, vehicle, stayDuration);
                departureSOC = arrivalSOC + SOCGain;
            }
            cumulativeTravelDistance = previousSV.cumulativeTravelDistance + travelDistance;
        }
        //public SiteVisit(SiteVisit previousSV, Site currentSite, double travelDistance, double travelTime, Vehicle vehicle, double energyConsumption = 0.0, double stayDuration = double.MaxValue, double maxSOCGain = 1.0)
        //{
        //    // travelDistance, travelTime and energyConsumption are all about the travel between the previous site and this
        //    //Obviously, if there is no previous site, this constructor cannot be used!
        //    //A limitation is that we must know the stay duration or maximum SOC gain beforehand, can't come back to optimize it!

        //    siteID = currentSite.ID;

        //    arrivalTime = previousSV.departureTime + travelTime;
        //    arrivalSOC = previousSV.departureSOC - energyConsumption;
        //    SOCGain = Utils.Calculators.MaxSOCGainAtSite(currentSite, vehicle, stayDuration);
        //    departureTime = arrivalTime + stayDuration;
        //    departureSOC = arrivalSOC + SOCGain;

        //    cumulativeTravelDistance = previousSV.cumulativeTravelDistance + travelDistance;
        //}

    }
}
