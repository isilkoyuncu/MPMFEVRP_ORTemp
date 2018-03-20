using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizationOutcome
    {
        List<VehicleSpecificRouteOptimizationOutcome> theListofVSROOs; public List<VehicleSpecificRouteOptimizationOutcome> TheListofVSROOs { get => theListofVSROOs; }
        RouteOptimizationStatus overallStatus; public RouteOptimizationStatus Status { get { return overallStatus; } }
        ObjectiveFunctionInputDataPackage ofidp;
        public ObjectiveFunctionInputDataPackage OFIDP { get { return ofidp; } }

        public RouteOptimizationOutcome()
        {
            theListofVSROOs = new List<VehicleSpecificRouteOptimizationOutcome>();
            overallStatus = RouteOptimizationStatus.NotYetOptimized;
        }
        public RouteOptimizationOutcome(RouteOptimizationOutcome twinROO)
        {
            theListofVSROOs = new List<VehicleSpecificRouteOptimizationOutcome>();
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in twinROO.theListofVSROOs)
                theListofVSROOs.Add(new VehicleSpecificRouteOptimizationOutcome(vsroo));
            overallStatus = twinROO.overallStatus;
            ofidp = new ObjectiveFunctionInputDataPackage(twinROO.ofidp);
        }
        public RouteOptimizationOutcome(RouteOptimizationStatus status, List<VehicleSpecificRouteOptimizationOutcome> theList = null)
        {
            if ((status == RouteOptimizationStatus.NotYetOptimized) || (status == RouteOptimizationStatus.InfeasibleForBothGDVandEV))
            {
                theListofVSROOs = new List<VehicleSpecificRouteOptimizationOutcome>();
            }
            else
            {
                if ((theList == null)
                    || (((status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV) || (status == RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV)) && (theList.Count != 1))
                    || ((status == RouteOptimizationStatus.OptimizedForBothGDVandEV) && (theList.Count != 2)))
                    throw new Exception("RouteOptimizationOutcome constructor(status,theList) invoked with inconsistent arguments!");
                theListofVSROOs = theList;
            }
            overallStatus = status;
            ofidp = GetObjectiveFunctionInputDataPackage();//Note that this is invoked after setting the list as well as the status 
        }
        public RouteOptimizationOutcome(List<VehicleSpecificRouteOptimizationOutcome> theList)
        {
            theListofVSROOs = theList;
            UpdateOverallStatus();
            ofidp = GetObjectiveFunctionInputDataPackage();//Note that this is invoked after setting the list as well as the status
        }

        public VehicleSpecificRouteOptimizationOutcome GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory)
        {
            return theListofVSROOs.Find(x => x.VehicleCategory == vehicleCategory);
        }

        public VehicleSpecificRouteOptimizationStatus GetRouteOptimizationStatus(VehicleCategories vehicleCategory)
        {
            VehicleSpecificRouteOptimizationOutcome theItem = theListofVSROOs.Find(x => x.VehicleCategory == vehicleCategory);
            return (theItem == null ? VehicleSpecificRouteOptimizationStatus.NotYetOptimized : theItem.Status);
        }
        public RouteOptimizationStatus GetRouteOptimizationStatus()
        {
            if (overallStatus == RouteOptimizationStatus.NotYetOptimized)
                if (theListofVSROOs != null)
                    if (theListofVSROOs.Count > 0)
                    {
                        //Need to update overallStatus because one or more VSROO's have been added since the last status update
                        UpdateOverallStatus();
                    }
            if(overallStatus == RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV)
                if(theListofVSROOs!=null)
                    if(theListofVSROOs.Count>1)
                    {
                        //Need to update overallStatus because the EV VSROO has been added since the last status update
                        UpdateOverallStatus();
                    }
            return overallStatus;
        }
        void UpdateOverallStatus()
        {
            VehicleSpecificRouteOptimizationStatus GDVStatus = GetRouteOptimizationStatus(VehicleCategories.GDV);
            if (GDVStatus == VehicleSpecificRouteOptimizationStatus.NotYetOptimized)
            {
                overallStatus = RouteOptimizationStatus.NotYetOptimized;
                return;
            }
            if (GDVStatus == VehicleSpecificRouteOptimizationStatus.Infeasible)
            {
                overallStatus = RouteOptimizationStatus.InfeasibleForBothGDVandEV;
                return;
            }
            //if we're here, GDV must be optimized
            VehicleSpecificRouteOptimizationStatus EVStatus = GetRouteOptimizationStatus(VehicleCategories.EV);
            if (EVStatus == VehicleSpecificRouteOptimizationStatus.NotYetOptimized)
            {
                overallStatus = RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV;
                return;
            }
            if (EVStatus == VehicleSpecificRouteOptimizationStatus.Infeasible)
            {
                overallStatus = RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV;
                return;
            }
            //if we're here, EV must be optimized
            overallStatus = RouteOptimizationStatus.OptimizedForBothGDVandEV;
            return;
        }

        public bool IsFeasible(VehicleCategories vehCategory)
        {
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in theListofVSROOs)
            {
                if (vsroo.VehicleCategory == vehCategory)
                {
                    if (vsroo.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
                        return false;
                    else
                        return vsroo.VSOptimizedRoute.Feasible;
                }
            }
            return false; //TODO this is not correct, false means it is not feasible; we cannot distinguish between not yet optimized and feasible.
            throw new Exception("There is no" + vehCategory.ToString() + "in theList hence we couldn't retrieve it's feasibility status");
        }

        ObjectiveFunctionInputDataPackage GetObjectiveFunctionInputDataPackage()
        {
            switch (overallStatus)
            {
                case RouteOptimizationStatus.NotYetOptimized:
                    return new ObjectiveFunctionInputDataPackage();
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV://This is by design, if it's ever called and the null exception is gotten, we must consider using a better check mechanism there!
                    return null;
                case RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV:
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                    return GetObjectiveFunctionInputDataPackage(VehicleCategories.GDV);//This must not return null because we have optimized for the GDV
                case RouteOptimizationStatus.OptimizedForBothGDVandEV:
                    return new ObjectiveFunctionInputDataPackage(GetObjectiveFunctionInputDataPackage(VehicleCategories.EV), GetObjectiveFunctionInputDataPackage(VehicleCategories.GDV));
                default:
                    throw new NotImplementedException("RouteOptimizationOutcome.overallStatus didn't include this case at time of coding this method!");
            }
        }
        ObjectiveFunctionInputDataPackage GetObjectiveFunctionInputDataPackage(VehicleCategories vehCategory)
        {
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in theListofVSROOs)//This code would work as long as there is no more than one vehicle of a certain category, and not beyond!
            {
                if (vsroo.VehicleCategory == vehCategory)
                    return vsroo.GetObjectiveFunctionInputDataPackage();
            }
            throw new Exception("There is no" + vehCategory.ToString() + "in theList hence we couldn't construct a OFDP.");
        }

    }
}
