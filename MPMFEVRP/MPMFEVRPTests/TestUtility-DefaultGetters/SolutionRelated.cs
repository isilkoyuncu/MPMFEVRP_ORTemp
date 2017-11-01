using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using System.Collections.Generic;

namespace MPMFEVRPTests.TestUtility_DefaultGetters
{
    class SolutionRelated
    {
        public static CustomerSet GetDefaultCustomerSet(MPMFEVRP.Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel)
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            CustomerSet outcome = new CustomerSet(allCustomers[5],allCustomers);
            outcome.ExtendAndOptimize(allCustomers[2], theProblemModel);
            return outcome;
        }

        public static PartitionedCustomerSetList GetDefaultPartitionedCustomerSetList(MPMFEVRP.Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel)
        {
            PartitionedCustomerSetList outcome = new PartitionedCustomerSetList();
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            List<string> lc1 = new List<string>() { allCustomers[5], allCustomers[2] };
            //TODO we need to solve this ambiguity between customer set constructors: customerSet(csList, vsros...) and customerSet(csList, roo...) 
            //I had to correct the following by adding vsros as not yet optimized
            CustomerSet cs1 = new CustomerSet(lc1, vsros:VehicleSpecificRouteOptimizationStatus.NotYetOptimized);
            VehicleSpecificRouteOptimizationOutcome vsroo1 = theProblemModel.RouteOptimize(cs1, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV));
            cs1.RouteOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo1 });
            outcome.Add(cs1);
            List<string> lc2 = new List<string>() { allCustomers[1], allCustomers[11], allCustomers[15] };
            CustomerSet cs2 = new CustomerSet(lc2, VehicleSpecificRouteOptimizationStatus.NotYetOptimized);
            VehicleSpecificRouteOptimizationOutcome vsroo2 = theProblemModel.RouteOptimize(cs2, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV));
            cs2.RouteOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo2 });
            outcome.Add(cs2);
            return outcome;
        }
    }
}
