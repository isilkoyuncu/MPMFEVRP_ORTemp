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
        public EVvsGDV_MaxProfit_VRP_Model()
        {
            EVvsGDV_MaxProfit_VRP problem = new EVvsGDV_MaxProfit_VRP();
            problemName = problem.GetName();
        }//empty constructor
        public EVvsGDV_MaxProfit_VRP_Model(EVvsGDV_MaxProfit_VRP problem)
        {
            pdp.InputFileName = problem.PDP.InputFileName;
            pdp.SRD = new SiteRelatedData(problem.PDP.SRD);
            pdp.VRD = new VehicleRelatedData(problem.PDP.VRD);
            pdp.CRD = new ContextRelatedData(problem.PDP.CRD);
            problemName = problem.GetName();
        }

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

            if (solutionType == typeof(CustomerSetBasedSolution))
            {
                CustomerSetBasedSolution csbs = (CustomerSetBasedSolution)solution;
                return CheckFeasibilityOfSolution(csbs);
            }
            //else if (solutionType == typeof(RouteBasedSolution))
            //{
            //    RouteBasedSolution rbs = (RouteBasedSolution)solution;
            //    return CheckFeasibilityOfSolution(rbs);
            //}
            else
            {
                System.Windows.Forms.MessageBox.Show("solution plugged into EVvsGDV_MaxProfit_VRP_Model.CheckFeasibilityOfSolution is incompatible!");
                return false;
            }
        }
        bool CheckFeasibilityOfSolution(CustomerSetBasedSolution solution)
        {
            bool outcome = true;
            //TODO check for any infeasibility and return false as soon as one is found!
            return outcome;
        }

        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            throw new NotImplementedException();

        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        void PopulateCompatibleSolutionTypes()
        {
            compatibleSolutions = new List<Type>()
            {
                typeof(RouteBasedSolution),
                typeof(CustomerSetBasedSolution)
            };
        }
    }
}
