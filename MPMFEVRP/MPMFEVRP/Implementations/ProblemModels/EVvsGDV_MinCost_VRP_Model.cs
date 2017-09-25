using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using MPMFEVRP.Models.XCPlex;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EVvsGDV_MinCost_VRP_Model : EVvsGDV_ProblemModel
    {
        public EVvsGDV_MinCost_VRP_Model()
        {
            EVvsGDV_MinCost_VRP problem = new EVvsGDV_MinCost_VRP();
            problemName = problem.GetName();
            objectiveFunctionType = problem.ObjectiveFunctionType;
            coverConstraintType = problem.CoverConstraintType;
            rechargingDuration_status = RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full; //TODO delete these because these are unnecessary. Without data, this problem model is useless and we have this empty constructor just to show model on the form
        }//empty constructor
        public EVvsGDV_MinCost_VRP_Model(EVvsGDV_MinCost_VRP problem, XCPlexBase TSPModel) : base(problem, TSPModel) { }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }
        public override string GetName()
        {
            return "EV vs GDV Cost Minimization Problem's Model";
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
            if(!GetCompatibleSolutions().Contains(solutionType))
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
                System.Windows.Forms.MessageBox.Show("Solution type plugged into EVvsGDV_MinCost_VRP_Model.CheckFeasibilityOfSolution is not accounted for!");
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

        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            throw new NotImplementedException();

        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }
    }
}
