using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizationOutcome
    {
        List<VehicleSpecificRouteOptimizationOutcome> theListofVSROOs;
        RouteOptimizationStatus overallStatus; public RouteOptimizationStatus Status;
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
        //TODO: Delete after making sure this is not needed: The following constructor appears to have been coded as a duplicate
        //public RouteOptimizationOutcome(ProblemModelBase problemModel, CustomerSet customerSet, List<Vehicle> vehicles)
        //{
        //    if ((vehicles == null) || (vehicles.Count == 0))
        //        throw new Exception("RouteOptimizationOutcome constructor invoked with no vehicles!");
        //    List<Vehicle> GDVs = new List<Vehicle>();
        //    List<Vehicle> EVs = new List<Vehicle>();
        //    foreach (Vehicle v in vehicles)
        //        switch (v.Category)
        //        {
        //            case VehicleCategories.GDV:
        //                GDVs.Add(v);
        //                break;
        //            case VehicleCategories.EV:
        //                EVs.Add(v);
        //                break;
        //        }
        //    if (GDVs.Count > 1)
        //        throw new Exception("There can't be more than 1 GDVs in this project, because a GDV is essentially an unconstrained vehicle!");
        //    if (EVs.Count > 1)
        //        throw new Exception("Capability to work with multiple EVs is a possible feature to add later, but is not included as of now!");

        //    //At this time, if we get to here, we know for sure that there are one or two vehicles. If there are two, one is a GDV and the other an EV.
        //    //If there is only an EV, we want to GDV-optimize it first because we believe that would save time overall
        //    if (GDVs.Count == 0)
        //        GDVs.Add(problemModel.VRD.VehicleArray[1]);//Because [1] is the index of the GDV
        //    //the previous line just assured that there is always a GDV to optimize for
        //    Vehicle theGDV = GDVs[0];
        //    VehicleSpecificRouteOptimizationOutcome vsroo_GDV = problemModel.OptimizeRoute(customerSet, theGDV);
        //    theListofVSROOs = new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV };
        //    if (vsroo_GDV.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
        //    {
        //        overallStatus = RouteOptimizationStatus.InfeasibleForBothGDVandEV;
        //    }
        //    else
        //    {
        //        theListofVSROOs.Add(vsroo_GDV);
        //        Vehicle theEV = EVs[0];
        //        VehicleSpecificRouteOptimizationOutcome vsroo_EV = problemModel.OptimizeRoute(customerSet, theEV, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
        //        theListofVSROOs.Add(vsroo_EV);
        //        switch (vsroo_EV.Status)
        //        {
        //            case VehicleSpecificRouteOptimizationStatus.Infeasible:
        //                {
        //                    overallStatus = RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV;
        //                    ofidp = GetObjectiveFunctionInputDataPackage(VehicleCategories.GDV);
        //                }
        //                break;
        //            case VehicleSpecificRouteOptimizationStatus.Optimized:
        //                {
        //                    overallStatus = RouteOptimizationStatus.OptimizedForBothGDVandEV;
        //                    ofidp = new ObjectiveFunctionInputDataPackage(GetObjectiveFunctionInputDataPackage(VehicleCategories.EV), GetObjectiveFunctionInputDataPackage(VehicleCategories.GDV));
        //                }
        //                break;
        //            default:
        //                throw new Exception("We just optimized for the EV, the status of it should have been either optimal or infeasible!");
        //        }

        //    }
        //}

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
                    return vsroo.VSOptimizedRoute.Feasible;
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
