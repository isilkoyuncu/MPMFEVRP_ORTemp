﻿using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using MPMFEVRP.Models.XCPlex;


namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EVvsGDV_MaxProfit_VRP_Model: EVvsGDV_ProblemModel
    {
        public EVvsGDV_MaxProfit_VRP_Model(EVvsGDV_MaxProfit_VRP problem, Type TSPModelType) : base(problem, TSPModelType) { }

        public EVvsGDV_MaxProfit_VRP_Model()
        {
            EVvsGDV_MaxProfit_VRP problem = new EVvsGDV_MaxProfit_VRP();
            problemName = problem.GetName();
            objectiveFunctionType = problem.ObjectiveFunctionType;
            coverConstraintType = problem.CoverConstraintType;
            rechargingDuration_status = RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full;
        }//empty constructor
        public override string GetDescription()
        {
            throw new NotImplementedException();
        }
        public override string GetName()
        {
            return "EV vs GDV Profit Maximization Problem's Model";
        }
        public override string GetNameOfProblemOfModel()
        {
            return problemName;
        }

        public override ISolution GetRandomSolution(int seed, Type solutionType)
        {
            throw new NotImplementedException();
        }

        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            Type solutionType = solution.GetType();
            if (!GetCompatibleSolutions().Contains(solutionType))
                throw new Exception("Solution plugged into EVvsGDV_MinCost_VRP_Model.CheckFeasibilityOfSolution is not listed in GetCompatibleSolutions()!");

            if (solutionType == typeof(CustomerSetBasedSolution))
            {
                CustomerSetBasedSolution csbs = (CustomerSetBasedSolution)solution;
                return CheckFeasibilityOfSolution(csbs);
            }
            else if (solutionType == typeof(RouteBasedSolution))
            {
                RouteBasedSolution rbs = (RouteBasedSolution)solution;
                return CheckFeasibilityOfSolution(rbs);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Solution plugged into EVvsGDV_MaxProfit_VRP_Model.CheckFeasibilityOfSolution is not accounted for!");
                return false;
            }
        }
        bool CheckFeasibilityOfSolution(CustomerSetBasedSolution solution)
        {
            throw new NotImplementedException();

            //bool outcome = true;
            ////TODO check for any infeasibility and return false as soon as one is found!
            //return outcome;
        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }
    }
}
