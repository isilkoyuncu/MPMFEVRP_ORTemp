using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Problems
{
    public class EMH_Problem : EVvsGDV_Problem
    {
        /* This problem is a homogeneous GVRP which allows any number of vehicles, but they belong to one category: AFV
         * The objective is to minimize total vehicle miles travelled
         * Uncapacitated problem: customer demands are all 0, vehicle capacities are undefined
         * No time windows
         * Each customer must be visited exactly once
         * */

        public EMH_Problem()
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Minimize;
            objectiveFunction = ObjectiveFunctions.MinimizeVMT;
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage();
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        }

        public EMH_Problem(ProblemDataPackage PDP) : base(PDP)
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Minimize;
            objectiveFunction = ObjectiveFunctions.MinimizeVMT;
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, pdp.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).VariableCostPerMile, 0.0);
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        }

        public EMH_Problem(ProblemDataPackage PDP, int numberOfEVs) : base(PDP)
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Minimize;
            objectiveFunction = ObjectiveFunctions.MinimizeVMT;
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, pdp.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).VariableCostPerMile, 0.0);
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
            problemCharacteristics.UpdateParameter(ParameterID.PRB_NUM_EV, numberOfEVs);
        }

        public override string GetName()
        {
            return "Erdogan & Miller-Hooks Problem";
        }
    }
}
