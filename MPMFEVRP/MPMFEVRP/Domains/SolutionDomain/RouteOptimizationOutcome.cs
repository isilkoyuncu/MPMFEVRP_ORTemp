using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizationOutcome//TODO: This class will completely replace RouteOptimizerOutput
    {
        List<VehicleSpecificRouteOptimizationOutcome> theList;
        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } }
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
    }
}
