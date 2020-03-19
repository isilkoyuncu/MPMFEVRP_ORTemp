using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.CustomerSetSolvers.Interfaces_and_Bases;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Models.CustomerSetSolvers
{
    public class CustomerSetSolverWithOnlyAFV : XCPlex_Model_AFV_SingleCustomerSet, ICustomerSetSolverForASingleVehicleCategory
    {
        //Properties
        public VehicleCategories VehicleCategory => VehicleCategories.EV;

        //Constructors
        public CustomerSetSolverWithOnlyAFV() : base() { }
        public CustomerSetSolverWithOnlyAFV(EVvsGDV_ProblemModel theProblemModel) : base(theProblemModel, new XCPlexParameters(tSP: true,limitComputationTime:true,runtimeLimit_Seconds:90000.0), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
        {

        }
        //Other Methods
        public VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool preserveCustomerVisitSequence, VehicleSpecificRoute vsr_GDV)
        {
            //Verification
            if (preserveCustomerVisitSequence) //If we want to preserce cs visit sequence, we must provide a CS that has been optimized with a GDV solver
                if (!vsr_GDV.Feasible)
                    throw new ArgumentException("CustomerSetSolverWithOnlyAFV Solve method is invoked with a wrong set of arguments. We want to keep the sequence but we do not provide an optimal GDV specific route.");

            //Implementation
            //Pre-process
            RefineDecisionVariables(customerSet, preserveCustomerVisitSequence, vsr_GDV);

            //Solve & Post-process
            Solve_and_PostProcess();

            //Return the desired outcome
            if (SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First());
            }
            else
                throw new System.Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + VehicleCategory.ToString());
        }

        public VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool preserveCustomerVisitSequence=false)
        {
            //Implementation
            //Pre-process
            RefineDecisionVariables(customerSet, preserveCustomerVisitSequence);

            //Solve & Post-process
            Solve_and_PostProcess();

            //Return the desired outcome
            if (SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First());
            }
            else
                throw new System.Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + VehicleCategory.ToString());
        }

        public VehicleSpecificRouteOptimizationOutcome SolveWithSubOptSolution(CustomerSet customerSet, VehicleSpecificRoute vsr_AFV)
        {
            //Implementation
            //Pre-process
            RefineDecisionVariables(customerSet, vsr_AFV.GetVehicleMilesTraveled());

            //Solve & Post-process
            Solve_and_PostProcess();

            //Return the desired outcome
            if (SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First());
            }
            else
                throw new System.Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + VehicleCategory.ToString());
        }
    }
}
