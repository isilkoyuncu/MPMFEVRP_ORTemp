using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class VehicleSpecificRouteOptimizationOutcome
    {
        VehicleCategories vehicleCategory;
        public VehicleCategories VehicleCategory { get { return vehicleCategory; } }

        VehicleSpecificRouteOptimizationStatus status;
        public VehicleSpecificRouteOptimizationStatus Status { get { return status; } }

        VehicleSpecificRoute vsOptimizedRoute;
        public VehicleSpecificRoute VSOptimizedRoute { get { return vsOptimizedRoute; } }

        public VehicleSpecificRouteOptimizationOutcome()
        {
            status = VehicleSpecificRouteOptimizationStatus.NotYetOptimized;
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleSpecificRouteOptimizationOutcome twinVSROO)
        {
            vehicleCategory = twinVSROO.vehicleCategory;
            status = twinVSROO.status;
            if (twinVSROO.vsOptimizedRoute == null)
                vsOptimizedRoute = null;
            else
            vsOptimizedRoute = new VehicleSpecificRoute(twinVSROO.vsOptimizedRoute);//A new instance is created here because we may want to extend the route manually in heuristic algorithms 
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory, VehicleSpecificRouteOptimizationStatus status, VehicleSpecificRoute vsOptimizedRoute = null)
        {
            this.vehicleCategory = vehicleCategory;
            this.status = status;

            switch (status)
            {
                case VehicleSpecificRouteOptimizationStatus.NotYetOptimized:
                    break;
                case VehicleSpecificRouteOptimizationStatus.Infeasible:
                    break;
                case VehicleSpecificRouteOptimizationStatus.Optimized:
                    this.vsOptimizedRoute = vsOptimizedRoute;
                    break;
            }
        }

        public ObjectiveFunctionInputDataPackage GetObjectiveFunctionInputDataPackage()
        {
            return new ObjectiveFunctionInputDataPackage(vehicleCategory,
                                                         vsOptimizedRoute.NumberOfCustomersVisited,
                                                         vsOptimizedRoute.GetPrizeCollected(),
                                                         1,
                                                         vsOptimizedRoute.GetVehicleMilesTraveled());
        }

    }
}
