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
        protected string inputFileName; // This is not for reading but just for record keeping and reporting 
        public string InputFileName { get { return inputFileName; } set {; } }

        protected ProblemDataPackage pdp;
        public SiteRelatedData SRD { get { return pdp.SRD; } }
        public VehicleRelatedData VRD { get { return pdp.VRD; } }
        public ContextRelatedData CRD { get { return pdp.CRD; } }

        public abstract string GetName();
        public abstract string GetDescription();
        public abstract string GetNameOfProblemOfModel();

        public abstract ISolution GetRandomSolution(int seed);
        public abstract bool CheckFeasibilityOfSolution(ISolution solution);
        public abstract double CalculateObjectiveFunctionValue(ISolution solution);
        public abstract bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
