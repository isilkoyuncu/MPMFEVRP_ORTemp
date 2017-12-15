using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class VehicleSpecificRouteOptimizationOutcome
    {
        VehicleCategories vehicleCategory;
        public VehicleCategories VehicleCategory { get { return vehicleCategory; } }

        double computationTime;
        public double ComputationTime { get { return computationTime; } }

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
            computationTime = twinVSROO.computationTime;
            status = twinVSROO.status;
            if (twinVSROO.vsOptimizedRoute == null)
                vsOptimizedRoute = null;
            else
            vsOptimizedRoute = new VehicleSpecificRoute(twinVSROO.vsOptimizedRoute);//A new instance is created here because we may want to extend the route manually in heuristic algorithms 
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory, double computationTime, VehicleSpecificRouteOptimizationStatus status, VehicleSpecificRoute vsOptimizedRoute = null)
        {
            this.vehicleCategory = vehicleCategory;
            this.computationTime = computationTime;
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
            if (vsOptimizedRoute != null)
                return new ObjectiveFunctionInputDataPackage(vehicleCategory,
                                                             vsOptimizedRoute.NumberOfCustomersVisited,
                                                             vsOptimizedRoute.GetPrizeCollected(),
                                                             1,
                                                             vsOptimizedRoute.GetVehicleMilesTraveled());
            else
                return new ObjectiveFunctionInputDataPackage();
        }

    }
}
