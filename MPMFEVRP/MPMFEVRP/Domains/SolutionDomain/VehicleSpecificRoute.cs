﻿using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class VehicleSpecificRoute
    {
        //Defining fields
        EVvsGDV_ProblemModel problemModel;

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
        public int NumberOfCustomersVisited { get { int outcome = 0; foreach (SiteVisit sv in siteVisits) if (sv.Site.SiteType == SiteTypes.Customer) outcome++; return outcome; } }
        //TODO check if not adding depot id to this list is the correct way.
        public List<string> ListOfVisitedSiteIncludingDepotIDs { get { List<string> listOfVisitedSiteIncludingDepotIDs = new List<string>(); foreach (SiteVisit sv in siteVisits) listOfVisitedSiteIncludingDepotIDs.Add(sv.SiteID); return listOfVisitedSiteIncludingDepotIDs; } }
        public List<string> ListOfVisitedNonDepotSiteIDs { get { List<string> listOfVisitedNonDepotSiteIDs = new List<string>(); foreach (SiteVisit sv in siteVisits) if (sv.SiteID != problemModel.SRD.GetSingleDepotID()) listOfVisitedNonDepotSiteIDs.Add(sv.SiteID); return listOfVisitedNonDepotSiteIDs; } }
        public List<string> ListOfVisitedCustomerSiteIDs { get { List<string> listOfVisitedCustomerSiteIDs = new List<string>(); foreach (SiteVisit sv in siteVisits) if (sv.Site.SiteType== SiteTypes.Customer) listOfVisitedCustomerSiteIDs.Add(sv.SiteID); return listOfVisitedCustomerSiteIDs; } }
        public bool Feasible { get { bool outcome = siteVisits.Last().GetTimeFeasible(problemModel.CRD.TMax); foreach (SiteVisit sv in siteVisits) outcome = (outcome && sv.GetSOCFeasible()); return outcome; } }
        bool rechargeAmountsCalculated = false;
        double iSTotalRechargeAmount = 0.0;
        double eSTotalRechargeAmount = 0.0;
        double eodTotalRechargeAmount = 0.0;//end-of-day
        
        //constructors
        //public VehicleSpecificRoute() { }//empty constructor, make accessible when needed, hopefully never
        public VehicleSpecificRoute(EVvsGDV_ProblemModel problemModel, Vehicle vehicle, bool alwaysClosedLoop = true)
        {
            this.problemModel = problemModel;
            this.vehicle = vehicle;
            double batteryCapacity = (vehicle.Category == VehicleCategories.EV ? vehicle.BatteryCapacity : 1.0);
            this.alwaysClosedLoop = alwaysClosedLoop;

            siteVisits = new List<SiteVisit>();
            if (alwaysClosedLoop)
                siteVisits.Add(new SiteVisit(problemModel.SRD.GetSingleDepotSite(), batteryCapacity));
        }
        public VehicleSpecificRoute(EVvsGDV_ProblemModel theProblemModel, Vehicle vehicle, List<string> nondepotSiteIDsInOrder, IndividualRouteESVisits ESVisits = null, bool alwaysClosedLoop = true) : this(theProblemModel, vehicle, alwaysClosedLoop: alwaysClosedLoop)
        {
            double batteryCapacity = (vehicle.Category == VehicleCategories.EV ? vehicle.BatteryCapacity : 1.0);
            //nondepotSiteIDsInOrder is named this way as a constant reminder that the sites are not just in an arbitrary list (like customer set) but are in fact in a route!
            //nondepotSiteIDsInOrder CANNOT contain the depot!
            if ((nondepotSiteIDsInOrder != null) && (nondepotSiteIDsInOrder.Contains(theProblemModel.SRD.GetSingleDepotID())))
                throw new Exception("VehicleSpecificRoute invoked with a nondepotSiteIDsInOrder that contains the depot!");
            //////////////////////////////////////////////////////////////
            int ESCounter = 0;
            RechargingDurationAndAllowableDepartureStatusFromES rechargePolicy = theProblemModel.ProblemCharacteristics.GetParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION).GetValue<RechargingDurationAndAllowableDepartureStatusFromES>();
            IndividualRouteESVisits ESVisitsToUse = new IndividualRouteESVisits();
            if (vehicle.Category == VehicleCategories.EV)
            {               
                List<string> visitedESsExtracted = new List<string>();
                foreach (string sID in nondepotSiteIDsInOrder)
                    if (theProblemModel.SRD.GetSiteByID(sID).SiteType == SiteTypes.ExternalStation)
                    {
                        visitedESsExtracted.Add(sID);
                        ESCounter++;
                    }
                switch (rechargePolicy)
                {
                    case RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full:
                        //In this case, don't bother to provide ESStayDurations beforehand, we'll just ignore it for robustness
                        for (int i = 0; i < ESCounter; i++)
                            ESVisitsToUse.Add(new IndividualESVisitDataPackage(visitedESsExtracted[i], Utils.Calculators.MaxStayDurationAtSite(theProblemModel.SRD.GetSiteByID(visitedESsExtracted[i]), vehicle))); //TODO unit test: all 15mins for EMH problems
                        break;
                    case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full:
                        //In this case too, don't bother to provide ESStayDurations beforehand, we'll just ignore it for robustness. We'll instead calculate them JIT right before adding that siteVisit
                        //Hence, ESStayDurationsToUse stays empty until the time of use
                        break;
                    case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial:
                        if ((ESVisits == null) || (ESVisits.Count != ESCounter))
                            throw new Exception("VehicleSpecificRoute invoked with a wrongly populated ESStayDurations list!");
                        ESVisitsToUse = ESVisits;
                        break;
                }
            }
            //////////////////////////////////
            if ((nondepotSiteIDsInOrder == null) || (nondepotSiteIDsInOrder.Count == 0))
                return;
            if (!alwaysClosedLoop)
                siteVisits.Add(new SiteVisit(theProblemModel.SRD.GetSingleDepotSite(), batteryCapacity));

            //Finally, we can start populating the route
            ESCounter = 0;
            foreach (string siteID in nondepotSiteIDsInOrder)
            {
                string previousSiteID = siteVisits.Last().SiteID;
                Site currentSite = theProblemModel.SRD.GetSiteByID(siteID);
                string currentSiteID = currentSite.ID;
                if (currentSite.SiteType == SiteTypes.Customer)
                    siteVisits.Add(new SiteVisit(siteVisits.Last(), currentSite, theProblemModel.SRD.GetDistance(previousSiteID, currentSiteID), theProblemModel.SRD.GetTravelTime(previousSiteID, currentSiteID), vehicle, energyConsumption: theProblemModel.SRD.GetEVEnergyConsumption(previousSiteID, currentSiteID), stayDuration: currentSite.ServiceDuration));
                else//site type is ES
                {
                    /////////////////
                    if (vehicle.Category == VehicleCategories.EV)
                    {
                        if (rechargePolicy == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
                            ESVisitsToUse.Add(new IndividualESVisitDataPackage(currentSite.ID, Utils.Calculators.MaxStayDurationAtSite(currentSite, vehicle)));
                        siteVisits.Add(new SiteVisit(siteVisits.Last(), currentSite, theProblemModel.SRD.GetDistance(previousSiteID, currentSiteID), theProblemModel.SRD.GetTravelTime(previousSiteID, currentSiteID), vehicle, energyConsumption: theProblemModel.SRD.GetEVEnergyConsumption(previousSiteID, currentSiteID), stayDuration: ESVisitsToUse.GetFirstUnprocessedVisitStayDurationToES(currentSiteID)));
                        ESCounter++;
                    }
                    /////////////////////////
                }
            }//foreach(string siteID in nondepotSiteIDsInOrder)
            if (alwaysClosedLoop)
                siteVisits.Add(new SiteVisit(siteVisits.Last(), theProblemModel.SRD.GetSingleDepotSite(), theProblemModel.SRD.GetDistance(siteVisits.Last().SiteID, theProblemModel.SRD.GetSingleDepotID()), theProblemModel.SRD.GetTravelTime(siteVisits.Last().SiteID, theProblemModel.SRD.GetSingleDepotID()), vehicle, energyConsumption: theProblemModel.SRD.GetEVEnergyConsumption(siteVisits.Last().SiteID, theProblemModel.SRD.GetSingleDepotID())));
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

        public VehicleSpecificRoute(VehicleSpecificRoute gdvVSR, bool rechargeAmountsNeedCalculation)
        {
            problemModel = gdvVSR.problemModel;
            vehicle = problemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
            alwaysClosedLoop = gdvVSR.alwaysClosedLoop;

            siteVisits = new List<SiteVisit>();
            foreach (SiteVisit sv in gdvVSR.siteVisits)
                siteVisits.Add(new SiteVisit(sv));
            if (rechargeAmountsNeedCalculation)
            {
                iSTotalRechargeAmount = CalculateISTotalRechargeAmount(gdvVSR);
                eSTotalRechargeAmount = 0.0;
                eodTotalRechargeAmount = CalculateEodTotalRechargeAmount(gdvVSR);
                rechargeAmountsCalculated = true;
            }

        }
        double CalculateISTotalRechargeAmount (VehicleSpecificRoute gdvVSR)
        {
            double totalISrecharge = 0.0;
            double SOC = problemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
            for (int i = 0; i < gdvVSR.ListOfVisitedSiteIncludingDepotIDs.Count - 1; i++)
            {
                SOC = SOC - problemModel.SRD.GetEVEnergyConsumption(gdvVSR.ListOfVisitedSiteIncludingDepotIDs[i], gdvVSR.ListOfVisitedSiteIncludingDepotIDs[i + 1]);
                //double tempRechargeAmount = Math.Min(problemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity - SOC)
                //totalISrecharge = totalISrecharge + 
            }
            return 0.0;
        }
        double CalculateEodTotalRechargeAmount(VehicleSpecificRoute gdvVSR)
        {

            return 0.0;
        }
        //Other methods
        public double GetVehicleMilesTraveled() { return siteVisits.Last().CumulativeTravelDistance; }

        public double GetISTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return iSTotalRechargeAmount; }
        public double GetESTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return eSTotalRechargeAmount; }
        public double GetEndOfDayTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return eodTotalRechargeAmount; }
        public double GetGrandTotalRechargeAmount() { if (!rechargeAmountsCalculated) CalculateAllTotalRechargeAmounts(); return (iSTotalRechargeAmount + eSTotalRechargeAmount + eodTotalRechargeAmount); }
        void CalculateAllTotalRechargeAmounts()
        {
            if (siteVisits == null)
                throw new Exception("CalculateAllTotalRechargeAmounts invoked before siteVisits!");
            foreach (SiteVisit sv in siteVisits)
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
        public double GetPrizeCollected()
        {
            double outcome = 0.0;
            if (siteVisits != null)//Doing this check so we never run into a null exception in runtime
                foreach (SiteVisit sv in siteVisits)
                {
                    outcome += sv.Site.GetPrize(vehicle.Category);
                }
            return outcome;
        }
        public double GetLongestArcLength()
        {
            double outcome = 0.0;
            if (siteVisits.Count > 1)
            {
                double tempDouble;
                for (int svIndex = 1; svIndex < siteVisits.Count; svIndex++)
                {
                    tempDouble = siteVisits[svIndex].CumulativeTravelDistance - siteVisits[svIndex - 1].CumulativeTravelDistance;
                    if (outcome < tempDouble)
                        outcome = tempDouble;
                }
            }
            return outcome;
        }
        public double GetLongestTwoArcsLength()
        {
            double outcome = 0.0;
            if (siteVisits.Count > 1)
            {
                List<double> tempDouble = new List<double>(); ;
                for (int svIndex = 1; svIndex < siteVisits.Count; svIndex++)
                {
                    tempDouble.Add(siteVisits[svIndex].CumulativeTravelDistance - siteVisits[svIndex - 1].CumulativeTravelDistance);
                }
                tempDouble.Sort();
                tempDouble.Reverse();
                outcome = tempDouble[0] + tempDouble[1];
            }
            return outcome;
        }
        public double GetTotalTime()
        {
            return siteVisits.Last().ArrivalTime;
        }
    }

}
