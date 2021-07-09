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
        public CustomerSetSolverWithOnlyAFV(EVvsGDV_ProblemModel theProblemModel) : base(theProblemModel, new XCPlexParameters(tSP: true,limitComputationTime:false,runtimeLimit_Seconds:36000.0), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
        {

        }
        //Other Methods
        public VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool preserveCustomerVisitSequence, VehicleSpecificRoute vsr_GDV, bool useTilim = false, double tilim = Double.MaxValue)
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
            
            double varCostPerMile = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).VariableCostPerMile;
            double fuelCost = -1;
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeFuelCost)
            {
                if (GetBestObjValue() == GetObjValue())
                    fuelCost = GetObjValue();
                else
                    fuelCost = GetBestObjValue();
            }
            //Return the desired outcome
            if (SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile,VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First(), fuelCost);
            }
            //else if (SolutionStatus == XCPlexSolutionStatus.Feasible)
            //    return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First(), fuelCost);
            //else if (SolutionStatus == XCPlexSolutionStatus.NoFeasibleSolutionFound)
            //    return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Infeasible);
            else
                throw new System.Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + VehicleCategory.ToString());
        }

        public VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool preserveCustomerVisitSequence = false, bool useTilim = false, double tilim = Double.MaxValue)
        {
            //Implementation
            //Pre-process
            RefineDecisionVariables(customerSet, preserveCustomerVisitSequence);

            //Solve & Post-process
            Solve_and_PostProcess();

            double varCostPerMile = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).VariableCostPerMile;
            double fuelCost = -1;
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeFuelCost)
            {
                if (GetBestObjValue() == GetObjValue())
                    fuelCost = GetObjValue();
                else
                    fuelCost = GetBestObjValue();
            }

            //Return the desired outcome
            if (SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First(), fuelCost);
            }
            //else if (useTilim)
            //{
            //    if (cpuTime >= tilim + 1.0)
            //        return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Infeasible);
            //    else
            //        throw new System.Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + VehicleCategory.ToString());
            //}
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
            double varCostPerMile = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).VariableCostPerMile;
            double fuelCost = -1;
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeFuelCost)
            {
                if (GetBestObjValue() == GetObjValue())
                    fuelCost = GetObjValue();
                else
                    fuelCost = GetBestObjValue();
            }
            //Return the desired outcome
            if (SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile,VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategory, CPUtime, varCostPerMile, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: GetVehicleSpecificRoutes(VehicleCategory).First(), fuelCost);
            }
            else
                throw new System.Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + VehicleCategory.ToString());
        }


        public override string GetModelName()
        {
            return "CustomerSetSolverWithOnlyAFV";
        }
    }
}
