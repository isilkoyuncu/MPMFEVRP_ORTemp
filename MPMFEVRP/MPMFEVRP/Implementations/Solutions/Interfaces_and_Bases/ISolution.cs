using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases
{
    public interface ISolution : RandomGenerator<ISolution>
    {
        List<string> IDs { get; }
        ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2);
        void View(IProblem problem);
        void TriggerSpecification();

        bool IsComplete { get; }
        ObjectiveFunctionInputDataPackage OFIDP { get; }
        List<ISolution> GetAllChildren();
        double LowerBound { get; set; }
        double UpperBound { get; set; }
        AlgorithmSolutionStatus Status { get; set; }
        string GetName();
        string[] GetOutputSummary();
        string[] GetWritableSolution();
    }
}
