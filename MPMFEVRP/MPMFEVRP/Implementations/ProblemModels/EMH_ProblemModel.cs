using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EMH_ProblemModel : EVvsGDV_ProblemModel
    {
        public EMH_ProblemModel()
        {
            EMH_Problem problem = new EMH_Problem();
            problemName = problem.GetName();
            objectiveFunctionType = problem.ObjectiveFunctionType;
            coverConstraintType = problem.CoverConstraintType;
            rechargingDuration_status = RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full; //TODO delete these because these are unnecessary. Without data, this problem model is useless and we have this empty constructor just to show model on the form
        }//empty constructor
        public EMH_ProblemModel(EMH_Problem problem) : base(problem) { }
        public void ModifyNumVehicles()
        {
            NumVehicles[1] = 0;
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }
        public override string GetName()
        {
            return "Erdogan & Miller - Hooks Problem Model";
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
                throw new Exception("Solution plugged into EMH_ProblemModel.CheckFeasibilityOfSolution is not listed in GetCompatibleSolutions()!");

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

        public override double CalculateObjectiveFunctionValue(ISolution solution) //TODO unit test this also check the structure
        {
            return solution.OFIDP.GetTotalVMT();
        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }
    }
}
