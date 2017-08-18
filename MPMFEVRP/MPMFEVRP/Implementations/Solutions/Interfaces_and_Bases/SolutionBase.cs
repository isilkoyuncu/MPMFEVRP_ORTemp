using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases
{
    public abstract class SolutionBase : ISolution
    {
        protected List<string> ids;
        public List<string> IDs { get { return ids; } }

        protected bool isComplete;
        public bool IsComplete { get { return isComplete; } }

        protected ObjectiveFunctionInputDataPackage ofidp;
        public ObjectiveFunctionInputDataPackage OFIDP { get { return ofidp; } }

        protected double lowerBound;
        public double LowerBound { get { return lowerBound; } set { lowerBound = value; } }

        protected double upperBound;
        public double UpperBound { get { return upperBound; } set { upperBound = value; } }

        protected AlgorithmSolutionStatus status;
        public AlgorithmSolutionStatus Status { get { return status; } set { status = value; } }

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
        public abstract string[] GetOutputSummary();
        public abstract string[] GetWritableSolution();
    }
}
