using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizationOutcome//TODO: This class will completely replace RouteOptimizerOutput
    {
        List<VehicleSpecificRouteOptimizationOutcome> theList;
        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } } //TODO: Retrieved from archive relates to CustomerSet, not RouteOptimizationOutcome!!! It should be deleted from here.
        RouteOptimizationStatus overallStatus;

        public RouteOptimizationOutcome()
        {
            theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            retrievedFromArchive = false;
            overallStatus = RouteOptimizationStatus.NotYetOptimized;
        }
        public RouteOptimizationOutcome(bool retrievedFromArchive, RouteOptimizationOutcome twinROO)
        {
            if (retrievedFromArchive)
                this.retrievedFromArchive = true;
            else
                this.retrievedFromArchive = twinROO.retrievedFromArchive;
            theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in twinROO.theList)
                theList.Add(new VehicleSpecificRouteOptimizationOutcome(vsroo));
            overallStatus = twinROO.overallStatus;
        }
        public RouteOptimizationOutcome(RouteOptimizationStatus status, List<VehicleSpecificRouteOptimizationOutcome> theList = null)
        {
            if ((status == RouteOptimizationStatus.NotYetOptimized) || (status == RouteOptimizationStatus.InfeasibleForBothGDVandEV))
            {
                this.theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            }
            else
            {
                this.theList = theList;
            }
            retrievedFromArchive = false;
            overallStatus = status;
        }
        public RouteOptimizationOutcome(List<VehicleSpecificRouteOptimizationOutcome> theList)
        {
            this.theList = theList;
            retrievedFromArchive = false;
            UpdateOverallStatus();
        }
        public RouteOptimizationOutcome(ProblemModelBase problemModel, CustomerSet customerSet, List<Vehicle> vehicles)
        {
            if ((vehicles == null) || (vehicles.Count == 0))
                throw new Exception("RouteOptimizationOutcome constructor invoked with no vehicles!");
            List<Vehicle> GDVs = new List<Vehicle>();
            List<Vehicle> EVs = new List<Vehicle>();
            foreach(Vehicle v in vehicles)
                switch (v.Category)
                {
                    case VehicleCategories.GDV:
                        GDVs.Add(v);
                        break;
                    case VehicleCategories.EV:
                        EVs.Add(v);
                        break;
                }
            if (GDVs.Count > 1)
                throw new Exception("There can't be more than 1 GDVs in this project, because a GDV is essentially an unconstrained vehicle!");
            if (EVs.Count > 1)
                throw new Exception("Capability to work with multiple EVs is a possible feature to add later, but is not included as of now!");

            //At this time, if we get to here, we know for sure that there are one or two vehicles. If there are two, one is a GDV and the other an EV.
            //If there is only an EV, we want to GDV-optimize it first because we believe that would save time overall
            if (GDVs.Count == 0)
                GDVs.Add(problemModel.VRD.VehicleArray[1]);//Because [1] is the index of the GDV
            //the previous line just assured that there is always a GDV to optimize for
            Vehicle theGDV = GDVs[0];
            VehicleSpecificRouteOptimizationOutcome vsroo_GDV = problemModel.OptimizeRoute(customerSet, theGDV);
            theList = new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV };
            if (vsroo_GDV.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
            {
                overallStatus = RouteOptimizationStatus.InfeasibleForBothGDVandEV;
            }
            else
            {
                theList.Add(vsroo_GDV);
                Vehicle theEV = EVs[0];
                VehicleSpecificRouteOptimizationOutcome vsroo_EV = problemModel.OptimizeRoute(customerSet, theEV, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                theList.Add(vsroo_EV);
                switch (vsroo_EV.Status)
                {
                    case VehicleSpecificRouteOptimizationStatus.Infeasible:
                        overallStatus = RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV;
                        break;
                    case VehicleSpecificRouteOptimizationStatus.Optimized:
                        overallStatus = RouteOptimizationStatus.OptimizedForBothGDVandEV;
                        break;
                    default:
                        throw new Exception("We just optimized for the EV, the status of it should have been either optimal or infeasible!");
                }

            }
        }

        public VehicleSpecificRouteOptimizationOutcome GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory)
        {
            return theList.Find(x => x.VehicleCategory == vehicleCategory);
        }

        public VehicleSpecificRouteOptimizationStatus GetRouteOptimizationStatus(VehicleCategories vehicleCategory)
        {
            VehicleSpecificRouteOptimizationOutcome theItem = theList.Find(x => x.VehicleCategory == vehicleCategory);
            return (theItem == null ? VehicleSpecificRouteOptimizationStatus.NotYetOptimized : theItem.Status);
        }
        public RouteOptimizationStatus GetRouteOptimizationStatus()
        {
            if (overallStatus == RouteOptimizationStatus.NotYetOptimized)
                if (theList != null)
                    if (theList.Count > 0)
                    {
                        //Need to update overallStatus
                        UpdateOverallStatus();
                    }
            return overallStatus;
        }
        void UpdateOverallStatus()
        {
            VehicleSpecificRouteOptimizationStatus GDVStatus = GetRouteOptimizationStatus(VehicleCategories.GDV);
            VehicleSpecificRouteOptimizationStatus EVStatus = GetRouteOptimizationStatus(VehicleCategories.EV);
            if (GDVStatus == VehicleSpecificRouteOptimizationStatus.Infeasible)
            {
                overallStatus = RouteOptimizationStatus.InfeasibleForBothGDVandEV;
                return;
            }
            if (GDVStatus == VehicleSpecificRouteOptimizationStatus.Optimized)
            {
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
        }
        public double GetEVMilesTraveled()
        {
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in theList)
            {
                if (vsroo.VehicleCategory == VehicleCategories.EV)
                    return vsroo.VSOptimizedRoute.GetVehicleMilesTraveled();
            }
            throw new Exception("There is no EV in theList hence we couldn't retrieve it's miles traveled");
        }

        public double GetGDVMilesTraveled()
        {
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in theList)
            {
                if(vsroo.VehicleCategory==VehicleCategories.GDV)
                    return vsroo.VSOptimizedRoute.GetVehicleMilesTraveled();
            }
             throw new Exception("There is no GDV in theList hence we couldn't retrieve it's miles traveled");
        }
        public bool IsEvFeasible()
        {
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in theList)
            {
                if (vsroo.VehicleCategory == VehicleCategories.EV)
                    return vsroo.VSOptimizedRoute.Feasible;
            }
            throw new Exception("There is no EV in theList hence we couldn't retrieve it's miles traveled");
        }

        public bool IsGdvFeasible()
        {
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in theList)
            {
                if (vsroo.VehicleCategory == VehicleCategories.GDV)
                    return vsroo.VSOptimizedRoute.Feasible;
            }
            throw new Exception("There is no GDV in theList hence we couldn't retrieve it's miles traveled");
        }

    }
}
