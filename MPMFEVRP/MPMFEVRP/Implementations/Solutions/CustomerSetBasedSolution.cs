using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestRandom;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Implementations.Solutions
{
    public class CustomerSetBasedSolution : SolutionBase
    {
        private ProblemModelBase model;
        private Random random;
        List<CustomerSet> cs_List;
        private int[,] ZSetTo1;

        public List<CustomerSet> CS_List { get { return cs_List; } }

        public CustomerSetBasedSolution()
        {
        }

        public CustomerSetBasedSolution(CustomerSetBasedSolution twinCSBasedSolution)
        {
            // TODO check if this copies everything from the twin CS based solution
            model = twinCSBasedSolution.model;
            random = twinCSBasedSolution.random;
            ids = twinCSBasedSolution.ids;
            isComplete = twinCSBasedSolution.isComplete;
            lowerBound = twinCSBasedSolution.lowerBound;
            objectiveFunctionValue = twinCSBasedSolution.objectiveFunctionValue;
            routes = twinCSBasedSolution.routes;
            status = twinCSBasedSolution.status;
            upperBound = twinCSBasedSolution.upperBound;
            cs_List = twinCSBasedSolution.cs_List;
        }

        // TODO fill this constructor so that it'll create an initial random solution by itself (i.e. do nothing)
        public CustomerSetBasedSolution(ProblemModelBase model, Random random)
        {
            this.model = model;
            this.random = random;
        }

        public CustomerSetBasedSolution(ProblemModelBase model, int[,] ZSetTo1, CustomerSetBasedSolution trialSolution)
        {
            this.model = model;
            this.ZSetTo1 = ZSetTo1;
            
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

        public void AddCustomerSet(CustomerSet currentCS)
        {
            cs_List.Add(currentCS);
        }
    }
}
