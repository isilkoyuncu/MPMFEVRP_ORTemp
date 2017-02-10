using BestRandom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Interfaces
{
    public interface ISolution : RandomGenerator<ISolution>
    {
        List<string> IDs { get; }
        ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2);
        void View(IProblem problem);
        void TriggerSpecification();

        bool IsComplete { get; }
        double ObjectiveFunctionValue { get; }
        List<ISolution> GetAllChildren();

        double LowerBound { get; }

        string GetName();
    }
}
