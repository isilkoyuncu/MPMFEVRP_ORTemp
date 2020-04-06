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
    public class CustomerSetSolverWithOnlyGDV : XCPlex_Model_GDV_SingleCustomerSet, ICustomerSetSolverForASingleVehicleCategory
    {
        //Properties
        public VehicleCategories VehicleCategory => VehicleCategories.GDV;

        //Constructors
        public CustomerSetSolverWithOnlyGDV() : base() { }
        public CustomerSetSolverWithOnlyGDV(EVvsGDV_ProblemModel theProblemModel) :base(theProblemModel, new XCPlexParameters(tSP:true), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
        {

        }

        //Other methods
        public VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool preserveCustomerVisitSequence = false, bool useTilim = false, double tilim = double.MaxValue)
        {
            //Verificiation
            if (preserveCustomerVisitSequence)
                throw new ArgumentException("CustomerSetSolverWithOnlyGDV.Solve method invoked with preserveCustomerVisitSequence=true, which cannot happen, because this solver is concerned only with the GDV anyways!");

            //Implementation
            //Pre-process
            RefineDecisionVariables(customerSet);

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
