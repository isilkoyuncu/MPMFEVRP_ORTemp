using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Interfaces
{
    public interface IProblemModel
    {
        string[] IDs { get; }
        int[] ProcessingTimes { get; }
        int[] DueDates { get; }
        int TotalJobs { get; }

        string GetName();
        string GetDescription();

        ISolution GetRandomSolution(int seed);
        bool CheckFeasibilityOfSolution(ISolution solution);
        double CalculateObjectiveFunctionValue(ISolution solution);
        bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
