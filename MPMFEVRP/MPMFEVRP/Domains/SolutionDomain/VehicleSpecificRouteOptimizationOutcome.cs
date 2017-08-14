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

        double objectiveFunctionValue;//This is for convenience than anything else, this information is easily reproducible based on the details in optimizedRoute, given a method to calculate the objective function value
        public double ObjectiveFunctionValue { get { return objectiveFunctionValue; } }

        VehicleSpecificRoute vsOptimizedRoute;//TODO: Eliminate this, because this new architecture whould work with the new VehicleSpecificRoute only!
        public VehicleSpecificRoute VSOptimizedRoute { get { return vsOptimizedRoute; } }

        public VehicleSpecificRouteOptimizationOutcome()
        {
            status = VehicleSpecificRouteOptimizationStatus.NotYetOptimized;
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleSpecificRouteOptimizationOutcome twinVSROO)
        {
            vehicleCategory = twinVSROO.vehicleCategory;
            status = twinVSROO.status;
            objectiveFunctionValue = twinVSROO.objectiveFunctionValue;
            if (twinVSROO.vsOptimizedRoute == null)
                vsOptimizedRoute = null;
            else
            vsOptimizedRoute = new VehicleSpecificRoute(twinVSROO.vsOptimizedRoute);//A new instance is created here because we may want to extend the route manually in heuristic algorithms 
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory, VehicleSpecificRouteOptimizationStatus status, double objectiveFunctionValue = 0.0, VehicleSpecificRoute vsOptimizedRoute = null)
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
                    this.objectiveFunctionValue = objectiveFunctionValue;
                    this.vsOptimizedRoute = vsOptimizedRoute;
                    break;
            }
        }
    }
}
