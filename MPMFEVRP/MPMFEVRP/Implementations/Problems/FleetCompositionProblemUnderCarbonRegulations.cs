using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Problems
{
    public class FleetCompositionProblemUnderCarbonRegulations : EVvsGDV_Problem
    {

        public FleetCompositionProblemUnderCarbonRegulations()
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Minimize;
            objectiveFunction = ObjectiveFunctions.MinimizeCost;
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        }
        public override string GetName()
        {
            return "Fleet Composition with Carbon Regulations";
        }

        public override string ToString()
        {
            return "Fleet Composition Problem Under Carbon Regulations";
        }
    }
}
