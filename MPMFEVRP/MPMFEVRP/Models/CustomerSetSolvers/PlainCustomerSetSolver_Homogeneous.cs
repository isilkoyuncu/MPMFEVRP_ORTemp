using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.CustomerSetSolvers.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Models.CustomerSetSolvers
{
    /// <summary>
    /// This class solves each customer set with only AFV solver, does not exploit GDV optimal routes.
    /// </summary>
    public class PlainCustomerSetSolver_Homogeneous : ICustomerSetSolver
    {
        readonly CustomerSetSolverWithOnlyAFV AFV_Solver;
        readonly Vehicle theAFV;

        public PlainCustomerSetSolver_Homogeneous(EVvsGDV_ProblemModel theProblemModel)
        {
            AFV_Solver = new CustomerSetSolverWithOnlyAFV(theProblemModel);
            theAFV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
        }
        public RouteOptimizationOutcome Solve(CustomerSet customerSet, bool PreserveCustomerVisitSequence = false)
        {
            VehicleSpecificRouteOptimizationOutcome vsroo_AFV;
            AFV_Solver.Solve(customerSet, PreserveCustomerVisitSequence);
            if (AFV_Solver.SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                VehicleSpecificRoute vsr_AFV = AFV_Solver.GetVehicleSpecificRoutes(theAFV.Category).First();
                vsroo_AFV = new VehicleSpecificRouteOptimizationOutcome(theAFV.Category, AFV_Solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsr_AFV);
                return new RouteOptimizationOutcome(RouteOptimizationStatus.OptimizedForBothGDVandEV, new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_AFV });
            }
            else
            {
                vsroo_AFV = new VehicleSpecificRouteOptimizationOutcome(theAFV.Category, AFV_Solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
                return new RouteOptimizationOutcome(RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV, new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_AFV });
            }
        }
    }
}
