using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestRandom;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Implementations.Solutions
{
    public class CustomerSetBasedSolution : SolutionBase
    {
        private ProblemModelBase model;
        private Random random;

        public CustomerSetBasedSolution()
        {
        }
        // TODO fill this constructor so that it'll create an initial random solution by itself (i.e. do nothing)
        public CustomerSetBasedSolution(ProblemModelBase model, Random random)
        {
            this.model = model;
            this.random = random;
        }

        public override ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        public override ISolution GenerateRandom()
        {
            throw new NotImplementedException();
        }

        public override List<ISolution> GetAllChildren()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Customer Set Based";
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }

        public override string[] GetWritableSolution()
        {
            throw new NotImplementedException();
        }

        public override void TriggerSpecification()
        {
            throw new NotImplementedException();
        }

        public override void View(IProblem problem)
        {
            throw new NotImplementedException();
        }
    }
}
