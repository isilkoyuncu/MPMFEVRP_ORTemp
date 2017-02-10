using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Implementations
{
    public abstract class ProblemModelBase : IProblemModel
    {
        protected string[] ids;
        public string[] IDs { get { return ids; } }

        protected int[] processingTimes;
        public int[] ProcessingTimes { get { return processingTimes; } }

        protected int[] dueDates;
        public int[] DueDates { get { return dueDates; } }

        protected int totalJobs;
        public int TotalJobs { get { return totalJobs; } }

        public abstract string GetDescription();
        public abstract string GetName();
        public abstract ISolution GetRandomSolution(int seed);
        public abstract bool CheckFeasibilityOfSolution(ISolution solution);
        public abstract double CalculateObjectiveFunctionValue(ISolution solution);
        public abstract bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
