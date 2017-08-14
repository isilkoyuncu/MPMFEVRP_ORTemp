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
        public int NumberOfSitesVisited { get { return siteVisits.Count; } }
        //TODO check if not adding depot id to this list is the correct way.
        public List<string> ListOfVisitedSiteIDs { get { List<string> listOfVisitedSiteIDs = new List<string>(); foreach (SiteVisit sv in siteVisits) if(sv.SiteID!=problemModel.SRD.GetSingleDepotID()) listOfVisitedSiteIDs.Add(sv.SiteID); return listOfVisitedSiteIDs; } }
        public bool Feasible { get { return siteVisits.Last().GetFeasible(problemModel.CRD.TMax); } }
        bool rechargeAmountsCalculated = false;
        double iSTotalRechargeAmount = 0.0;
        double eSTotalRechargeAmount = 0.0;
        double eodTotalRechargeAmount = 0.0;//end-of-day

        //constructors
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
        public VehicleSpecificRoute(VehicleSpecificRoute twinVSR)
        {
            problemModel = twinVSR.problemModel;
            vehicle = twinVSR.vehicle;
            alwaysClosedLoop = twinVSR.alwaysClosedLoop;

            siteVisits = new List<SiteVisit>();
            foreach (SiteVisit sv in twinVSR.siteVisits)
                siteVisits.Add(new SiteVisit(sv));
            rechargeAmountsCalculated = twinVSR.rechargeAmountsCalculated;
            iSTotalRechargeAmount = twinVSR.iSTotalRechargeAmount;
            eSTotalRechargeAmount = twinVSR.eSTotalRechargeAmount;
            eodTotalRechargeAmount = twinVSR.eodTotalRechargeAmount;
        }

        //Other methods
        public double GetVehicleMilesTraveled() { return siteVisits.Last().CumulativeTravelDistance; }

        public double GetISTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return iSTotalRechargeAmount; }
        public double GetESTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return eSTotalRechargeAmount; }
        public double GetEndOfDayTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return eodTotalRechargeAmount; }
        public double GetGrandTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return (iSTotalRechargeAmount+ eSTotalRechargeAmount+ eodTotalRechargeAmount); }
        void CalculateAllTotalRechargeAmounts()
        {
            if (siteVisits == null)
                throw new Exception("CalculateAllTotalRechargeAmounts invoked before siteVisits!");
            foreach(SiteVisit sv in siteVisits)
            {
                switch (sv.Site.SiteType)
                {
                    case SiteTypes.Customer:
                        iSTotalRechargeAmount += sv.SOCGain;
                        break;
                    case SiteTypes.ExternalStation:
                        eSTotalRechargeAmount += sv.SOCGain;
                        break;
                    case SiteTypes.Depot:
                        eodTotalRechargeAmount += sv.SOCGain;
                        break;
                }
            }
            rechargeAmountsCalculated = true;
        }
    }

}
