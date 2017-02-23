using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Implementations
{
    public abstract class ProblemModelBase : IProblemModel
    {
        protected string inputFileName;
        protected SiteRelatedData srd;
        protected VehicleRelatedData vrd;
        protected ContextRelatedData crd;
        public string InputFileName { get { return inputFileName; } set {; } }
        public SiteRelatedData SRD { get { return srd; } }
        public VehicleRelatedData VRD { get { return vrd; } }
        public ContextRelatedData CRD { get { return crd; } }

        public abstract string GetDescription();
        public abstract string GetName();
        public abstract ISolution GetRandomSolution(int seed);
        public abstract bool CheckFeasibilityOfSolution(ISolution solution);
        public abstract double CalculateObjectiveFunctionValue(ISolution solution);
        public abstract bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
