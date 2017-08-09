using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRPTests.TestUtility_DefaultGetters
{
    class SolutionRelated
    {
        public static CustomerSet GetDefaultCustomerSet(MPMFEVRP.Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel)
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            CustomerSet outcome = new CustomerSet(allCustomers[5], theProblemModel);
            outcome.Extend(allCustomers[2], theProblemModel);
            return outcome;
        }

        public static PartitionedCustomerSetList GetDefaultPartitionedCustomerSetList(MPMFEVRP.Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel)
        {
            PartitionedCustomerSetList outcome = new PartitionedCustomerSetList();
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            List<string> lc1 = new List<string>() { allCustomers[5], allCustomers[2] };
            CustomerSet cs1 = new CustomerSet(theProblemModel, lc1);
            VehicleSpecificRouteOptimizationOutcome vsroo1 = theProblemModel.OptimizeRoute(cs1, theProblemModel.VRD.VehicleArray[1]);
            cs1.RouteOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo1 });
            outcome.Add(cs1);
            List<string> lc2 = new List<string>() { allCustomers[1], allCustomers[11], allCustomers[15] };
            CustomerSet cs2 = new CustomerSet(theProblemModel, lc2);
            VehicleSpecificRouteOptimizationOutcome vsroo2 = theProblemModel.OptimizeRoute(cs2, theProblemModel.VRD.VehicleArray[1]);
            cs2.RouteOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo2 });
            outcome.Add(cs2);
            return outcome;
        }
    }
}
