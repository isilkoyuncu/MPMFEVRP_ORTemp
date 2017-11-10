using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Models;


namespace MPMFEVRP.Implementations.Problems
{
    public class EVvsGDV_MaxProfit_VRP: EVvsGDV_Problem
    {
        /* This problem is a heteregoneus VRP which allows any number of vehicle types, but they belong to two categories: EV and GDV
         * The objective is to maximize profit = revenue - cost (fixed per day & variable per mile)
         * Uncapacitated problem: customer demands are all 0, vehicle capacities are undefined
         * No time windows
         * Each customer must be visited at most once
         * */

        public EVvsGDV_MaxProfit_VRP()
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Maximize;
            objectiveFunction = ObjectiveFunctions.MaximizeProfit;
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage();
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce;
        }

        public EVvsGDV_MaxProfit_VRP(ProblemDataPackage PDP) : base(PDP) 
        {
            objectiveFunctionType = ObjectiveFunctionTypes.Maximize;
            objectiveFunction = ObjectiveFunctions.MaximizeProfit;
            objectiveFunctionCoefficientsPackage = new ObjectiveFunctionCoefficientsPackage(0, 0, 1, 1, pdp.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).FixedCost, pdp.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV).FixedCost, pdp.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).VariableCostPerMile, pdp.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV).VariableCostPerMile);
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce;
        }

        public override string GetName()
        {
            return "EV vs GDV Maximum Profit VRP";
        }

        //public new EVvsGDV_MaxProfit_VRP_Model GetProblemModel()
        //{
        //    return new EVvsGDV_MaxProfit_VRP_Model( numCustomers,  numES,  numNodes,  siteArray,  numVehicleCategories,  numVehicles, vehicleArray,  travelSpeed,  tMax,  lambda,  distance,  energyConsumption, timeConsumption);
        //}

        public override string ToString() //CONSULT ask about deleting this since base of this contains this fnc
        {
            return PDP.InputFileName;
        }
    }
}
