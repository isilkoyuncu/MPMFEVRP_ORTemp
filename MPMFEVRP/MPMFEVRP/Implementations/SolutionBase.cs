using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestRandom;

namespace MPMFEVRP.Implementations
{
    public abstract class SolutionBase : ISolution
    {
        protected List<string> ids;
        public List<string> IDs { get { return ids; } }

        protected bool isComplete;
        public bool IsComplete { get { return isComplete; } }

        protected double objectiveFunctionValue;
        public double ObjectiveFunctionValue { get { return objectiveFunctionValue; } }

        protected double lowerBound;
        public double LowerBound { get { return lowerBound; } }

        public SolutionBase()
        {
            ids = new List<string>();
        }

        public abstract string GetName();
        public abstract ISolution GenerateRandom();
        public abstract ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2);
        public abstract void View(IProblem problem);
        public abstract List<ISolution> GetAllChildren();
        public abstract void TriggerSpecification();
    }
}
