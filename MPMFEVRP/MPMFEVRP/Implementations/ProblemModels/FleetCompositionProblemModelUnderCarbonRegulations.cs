using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class FleetCompositionProblemModelUnderCarbonRegulations : EVvsGDV_ProblemModel
    {
        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            throw new NotImplementedException();
        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Model for Fleet Composition with Carbon Regulations";
        }

        public override string GetNameOfProblemOfModel()
        {
            return "Fleet Composition with Carbon Regulations";
        }

        public override ISolution GetRandomSolution(int seed, Type SolutionType)
        {
            throw new NotImplementedException();
        }
    }
}
