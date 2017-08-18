﻿using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;



namespace MPMFEVRP.Implementations.Problems
{
    public class EVvsGDV_MinCost_VRP:EVvsGDV_Problem
    {
        /* This problem is a heteregoneus VRP which allows any number of vehicle types, but they belong to two categories: EV and GDV
         * The objective is to minimize cost = cost (fixed per day & variable per mile)
         * Uncapacitated problem: customer demands are all 0, vehicle capacities are undefined
         * No time windows
         * Each customer must be visited exactly once
         * */

        public EVvsGDV_MinCost_VRP()
        {
            objectiveFunctionType = Models.ObjectiveFunctionTypes.Minimize;
            objectiveFunction = Models.ObjectiveFunctions.MinimizeCost;
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        }

        public EVvsGDV_MinCost_VRP(ProblemDataPackage PDP) : base(PDP)
        {
            objectiveFunctionType = Models.ObjectiveFunctionTypes.Minimize;
            objectiveFunction = Models.ObjectiveFunctions.MinimizeCost;
            coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        }

        public override string GetName()
        {
            return "EV vs GDV Minimum Cost VRP";
        }

        //public new EVvsGDV_MaxProfit_VRP_Model GetProblemModel()
        //{
        //    return new EVvsGDV_MaxProfit_VRP_Model( numCustomers,  numES,  numNodes,  siteArray,  numVehicleCategories,  numVehicles, vehicleArray,  travelSpeed,  tMax,  lambda,  distance,  energyConsumption, timeConsumption);
        //}

        public override string ToString() //TODO ask about deleting this since base of this contains this fnc
        {
            return PDP.InputFileName;
        }
    }
}
