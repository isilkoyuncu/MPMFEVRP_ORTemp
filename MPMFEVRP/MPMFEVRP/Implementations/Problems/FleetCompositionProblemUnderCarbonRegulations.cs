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
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage();//Because the problem should not depend on the problem and/or its model for this, but will have to create its own as part of experimentation to draw those frontiers
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        }
        public FleetCompositionProblemUnderCarbonRegulations(ProblemDataPackage pdp) : base(pdp)
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Minimize;
            objectiveFunction = ObjectiveFunctions.MinimizeCost;
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage();//Because the problem should not depend on the problem and/or its model for this, but will have to create its own as part of experimentation to draw those frontiers
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
