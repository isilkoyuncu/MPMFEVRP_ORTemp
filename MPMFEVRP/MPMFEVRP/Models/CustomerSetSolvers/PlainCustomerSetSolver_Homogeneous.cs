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
using MPMFEVRP.Utils;
using System.Diagnostics;

namespace MPMFEVRP.Models.CustomerSetSolvers
{
    /// <summary>
    /// This class solves each customer set with only AFV solver, does not exploit GDV optimal routes.
    /// </summary>
    public class PlainCustomerSetSolver_Homogeneous : ICustomerSetSolver
    {
        readonly CustomerSetSolverWithOnlyAFV AFV_Solver;
        readonly Vehicle theAFV;
        Stopwatch stopwatch = new Stopwatch();
        public OptimizationStatistics optimizationStatstics;
        double t5AFVSoln = 0.0; //public double T5AFVSoln => t5AFVSoln;

        public PlainCustomerSetSolver_Homogeneous(EVvsGDV_ProblemModel theProblemModel)
        {
            AFV_Solver = new CustomerSetSolverWithOnlyAFV(theProblemModel);
            theAFV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
        }
        public RouteOptimizationOutcome Solve(CustomerSet customerSet, bool PreserveCustomerVisitSequence = false, bool feasibleEnough = false, bool performSwap =false)
        {
            RouteOptimizationOutcome outcome;
            int nCustomers = customerSet.Customers.Count;
            List<string> customers = customerSet.Customers;
            t5AFVSoln = 0.0;

            VehicleSpecificRouteOptimizationOutcome vsroo_AFV;
            stopwatch.Start();
            AFV_Solver.Solve(customerSet, PreserveCustomerVisitSequence);
            t5AFVSoln = stopwatch.Elapsed.TotalSeconds;
            if (AFV_Solver.SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                VehicleSpecificRoute vsr_AFV = AFV_Solver.GetVehicleSpecificRoutes(theAFV.Category).First();
                vsroo_AFV = new VehicleSpecificRouteOptimizationOutcome(theAFV.Category, AFV_Solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsr_AFV);
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV = new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV, 0.0, VehicleSpecificRouteOptimizationStatus.Optimized, vsr_AFV);
                outcome = new RouteOptimizationOutcome(RouteOptimizationStatus.OptimizedForBothGDVandEV, new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_AFV });
                optimizationStatstics = new OptimizationStatistics(nCustomers, outcome, customers, 0.0, 0.0, 0.0, 0.0, 0.0, t5AFVSoln,0);
                return outcome;
            }
            else
            {
                vsroo_AFV = new VehicleSpecificRouteOptimizationOutcome(theAFV.Category, AFV_Solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV = new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV, 0.0, VehicleSpecificRouteOptimizationStatus.Infeasible);
                outcome = new RouteOptimizationOutcome(RouteOptimizationStatus.InfeasibleForBothGDVandEV, new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_AFV });
                optimizationStatstics = new OptimizationStatistics(nCustomers, outcome, customers, 0.0, 0.0, 0.0, 0.0, 0.0, t5AFVSoln,0);
                return outcome;
            }
        }
    }
}
