using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EVvsGDV_MaxProfit_VRP_Model: ProblemModelBase
    {
        string problemName;
        public EVvsGDV_MaxProfit_VRP_Model(){ }//empty constructor
        public EVvsGDV_MaxProfit_VRP_Model(EVvsGDV_MaxProfit_VRP problem, ProblemDataPackage pdp) //IProblem will come here
        {
            problemName = problem.GetName();
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "EV vs GDV Profit Maximization Problem";
        }

        public override string GetNameOfProblemOfModel()
        {
            return problemName;
        }

        public override ISolution GetRandomSolution(int seed)
        {
            throw new NotImplementedException();
        }

        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            throw new NotImplementedException();
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
