﻿using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class AssignedRoute//TODO Rethink this class now that there is VehicleSpecificRoute
    {
        /* Each route starts at the depot (0) and ends at the depot (0)
         * */

        // Input related fields
        EVvsGDV_ProblemModel theProblemModel;
        VehicleCategories vehicleCategory;//TODO: vehicleCategory shouldn't be an integer! We should use the VehicleCategories enum here

        public VehicleCategories VehicleCategoryIndex { get { return vehicleCategory; } }
        // Output related fields
        List<int> sitesVisited;
        public List<int> SitesVisited { get { return sitesVisited; } }

        double totalDistance;
        public double TotalDistance { get { return totalDistance; } }

        double totalCollectedPrize;
        public double TotalCollectedPrize { get { return totalCollectedPrize; } }

        double fixedCost;
        public double FixedCost { get { return fixedCost; } }

        double totalVariableTravelCost;
        public double TotalVariableTravelCost { get { return totalVariableTravelCost; } }

        double totalProfit;
        public double TotalProfit { get { return totalProfit; } }

        List<double> arrivalTime;
        public List<double> ArrivalTime { get { return arrivalTime; } }

        List<double> departureTime;
        public List<double> DepartureTime { get { return departureTime; } }

        List<double> arrivalSOC;
        public List<double> ArrivalSOC { get { return arrivalSOC; } }

        List<double> departureSOC;
        public List<double> DepartureSOC { get { return departureSOC; } }

        List<bool> feasible;//[numVehicleCategories]
        public List<bool> Feasible { get { return feasible; } }

        // Intermediate steps and validity
        public int LastVisitedSite { get { return sitesVisited.Last(); } }
        public bool Complete { get { return sitesVisited.Last() == 0; } }

        public AssignedRoute() { } //An empty constructor
        public AssignedRoute(AssignedRoute twinAR)// twin copy constructor
        {
            theProblemModel = twinAR.theProblemModel;
            vehicleCategory = twinAR.vehicleCategory;
            sitesVisited = twinAR.sitesVisited;
            totalDistance = twinAR.totalDistance;
            totalCollectedPrize = twinAR.totalCollectedPrize;
            fixedCost = twinAR.fixedCost;
            totalVariableTravelCost = twinAR.totalVariableTravelCost;
            totalProfit = twinAR.totalProfit;
            arrivalTime = twinAR.arrivalTime;
            departureTime = twinAR.departureTime;
            arrivalSOC = twinAR.arrivalSOC;
            departureSOC = twinAR.departureSOC;
            feasible = twinAR.feasible;
        }

        public AssignedRoute(EVvsGDV_ProblemModel theProblemModel, VehicleCategories vehicleCategory)
        {
            this.theProblemModel = theProblemModel;
            this.vehicleCategory = vehicleCategory;
            sitesVisited = new List<int>{ 0 };//The depot is the only visited site, this is how we get the route going
            totalDistance = 0.0;
            totalCollectedPrize = 0.0;
            fixedCost = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).FixedCost;
            totalVariableTravelCost = 0.0;
            totalProfit = -fixedCost;
            arrivalTime = new List<double> { 0.0 };//Starting at the depot at time 0
            departureTime = new List<double> { 0.0 };//Starting at the depot at time 0
            arrivalSOC = new List<double>{1.0};//Starting at the depot with full charge
            departureSOC = new List<double>{1.0};//Starting at the depot with full charge
            feasible = new List<bool>{true};//Starting at the depot with full charge
        }
        public void Extend(int nextSite)
        {
            int lastSite = sitesVisited.Last();
            sitesVisited.Add(nextSite);
            totalDistance += theProblemModel.SRD.Distance[lastSite, nextSite];
            totalCollectedPrize += theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).GetPrize(vehicleCategory);
            totalVariableTravelCost += theProblemModel.SRD.Distance[lastSite, nextSite] * theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).VariableCostPerMile;
            totalProfit += theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).GetPrize(vehicleCategory) - theProblemModel.SRD.Distance[lastSite, nextSite] * theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).VariableCostPerMile;
            double nextArrivalTime;
            double nextDepartureTime;
            double nextArrivalSOC;
            double nextDepartureSOC;
            bool nextFeasible;

            nextArrivalTime = departureTime.Last() + theProblemModel.SRD.TimeConsumption[lastSite, nextSite];
            nextArrivalSOC = departureSOC.Last() - theProblemModel.SRD.GetEVEnergyConsumption(theProblemModel.SRD.GetSiteID(lastSite), theProblemModel.SRD.GetSiteID(nextSite));
            double batteryCapacity = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).BatteryCapacity;
            double rechargingRate = theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).RechargingRate;
            double serviceDuration = theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).ServiceDuration;
            switch (theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).SiteType)
            {
                case SiteTypes.Depot:
                    nextDepartureTime = nextArrivalTime;
                    nextDepartureSOC = nextArrivalSOC;
                    break;
                case SiteTypes.Customer:
                    nextDepartureTime = nextArrivalTime + theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).ServiceDuration;
                    if (batteryCapacity != 0)
                        nextDepartureSOC = Math.Min(1, nextArrivalSOC + serviceDuration * rechargingRate / batteryCapacity);//Math.Min(1.0, nextArrivalSOC + problemModel.SRD.SiteArray[nextSite].RechargingRate * problemModel.SRD.SiteArray[nextSite].ServiceDuration);
                    else
                        nextDepartureSOC = 1.0;
                    break;
                case SiteTypes.ExternalStation:
                    nextDepartureTime = nextArrivalTime + ((1.0 - nextArrivalSOC) / theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetSiteID(nextSite)).RechargingRate);
                    nextDepartureSOC = 1.0;
                    break;
                default:
                    throw new Exception("Not all cases of SiteType accounted for in Route.Extend!");
            }//switch (fromProblem.SiteArray[nextSite].SiteType)
            nextFeasible = feasible.Last();
            if (nextFeasible)
            {
                if ((nextArrivalSOC < -1.0 * ProblemConstants.ERROR_TOLERANCE) || (nextDepartureTime > theProblemModel.CRD.TMax + ProblemConstants.ERROR_TOLERANCE))
                    nextFeasible = false;
            }//if (nextFeasible[vc])

            arrivalTime.Add(nextArrivalTime);
            departureTime.Add(nextDepartureTime);
            arrivalSOC.Add(nextArrivalSOC);
            departureSOC.Add(nextDepartureSOC);
            feasible.Add(nextFeasible);
        }

        public static AssignedRoute EvaluateOrderedListOfCustomers(EVvsGDV_ProblemModel theProblemModel, List<string> customers)
        {
            AssignedRoute outcome = new AssignedRoute(theProblemModel, VehicleCategories.GDV);
            foreach (string c in customers)
                outcome.Extend(theProblemModel.SRD.GetSiteIndex(c));
            outcome.Extend(0);//I hope this is the index of the depot
            return outcome;
        }

        public bool IsSame(AssignedRoute otherAR)
        {
            if (Math.Abs(totalDistance - otherAR.TotalDistance)>0.00001)
                return false;
            if (Math.Abs(totalCollectedPrize -otherAR.TotalCollectedPrize)>0.00001)
                return false;
            if (fixedCost != otherAR.FixedCost)
                return false;
            if (Math.Abs(totalVariableTravelCost - otherAR.TotalVariableTravelCost) > 0.00001)
                return false;
            if (Math.Abs(totalProfit - otherAR.TotalProfit) > 0.00001)
                return false;

            int nSites = sitesVisited.Count;
            if (Math.Abs(arrivalTime[nSites - 1] - otherAR.ArrivalTime[nSites - 1])>0.00001)
                return false;

            if (otherAR.SitesVisited.Count != nSites)
                return false;
            for (int i = 0; i < nSites; i++)
            {
                if ((sitesVisited[i] != otherAR.SitesVisited[i]) && (sitesVisited[i] != otherAR.SitesVisited[nSites - 1 - i]))
                    return false;
                if (feasible[i] != otherAR.Feasible[i])
                    return false;
            }

            return true;
        }
    }
}
