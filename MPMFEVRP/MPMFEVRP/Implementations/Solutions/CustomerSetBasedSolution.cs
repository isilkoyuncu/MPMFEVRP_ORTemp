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

        CustomerSetList assigned2EV; public CustomerSetList Assigned2EV { get { return assigned2EV; } }
        CustomerSetList assigned2GDV; public CustomerSetList Assigned2GDV { get { return assigned2GDV; } }

        public int NumCS_assigned2EV { get { return (assigned2EV == null) ? 0 : assigned2EV.Count; } }
        public int NumCS_assigned2GDV { get { return (assigned2GDV == null) ? 0 : assigned2GDV.Count; } }
        public int NumCS_total { get { return (assigned2EV == null && assigned2GDV == null) ? 0 : (assigned2EV.Count + assigned2GDV.Count); } }


        private int[,] zSetTo1;

        public CustomerSetBasedSolution() 
        {
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
        }

        public CustomerSetBasedSolution(ProblemModelBase model)
        {
            this.model = model; //TODO pirimitive olmayan seyleri de boyle kopyalayabiliyor muyuz test et.
            //lowerBound = 0.0;
            //for(int cs=0;cs< assigned2EV.Count; cs++)
            //{
            //    lowerBound += assigned2EV[cs].RouteOptimizerOutcome.OFV[0];
            //}
            //for (int cs = 0; cs < assigned2GDV.Count; cs++)
            //{
            //    lowerBound += assigned2GDV[cs].RouteOptimizerOutcome.OFV[1];
            //}
        }

        public CustomerSetBasedSolution(CustomerSetBasedSolution twinCSBasedSolution)
        {
            model = twinCSBasedSolution.model;
            random = twinCSBasedSolution.random;
            isComplete = twinCSBasedSolution.isComplete;
            lowerBound = twinCSBasedSolution.lowerBound;
            objectiveFunctionValue = twinCSBasedSolution.objectiveFunctionValue;
            status = twinCSBasedSolution.status;
            upperBound = twinCSBasedSolution.upperBound;
        }

        // TODO fill this constructor so that it'll create an initial random solution by itself (i.e. do nothing)
        public CustomerSetBasedSolution(ProblemModelBase model, Random random)
        {
            this.model = model;
            this.random = random;
        }

        public CustomerSetBasedSolution(ProblemModelBase model, int[,] zSetTo1, CustomerSetBasedSolution trialSolution)
        {
            this.model = model;
            this.zSetTo1 = zSetTo1;
            //cs_List = trialSolution.CS_List; // TODO if there wont be a seperate cs list than assigned vehicles then change this method as well
            //for (int i = 0; i < cs_List.Count; i++)
            //{
            //    for (int j = 0; j < 2; j++)//TODO parametrize for the num of veh categories (2 for now)
            //    {
            //        if (zSetTo1[i, j] == 1)
            //        {
            //            lowerBound += cs_List[i].RouteOptimizerOutcome.OFV[j];
            //        }
            //    }
            //}
        }

        public void AddCustomerSet2EVList(CustomerSet currentCS)
        {
            assigned2EV.Add(currentCS);
        }

        public void AddCustomerSet2GDVList(CustomerSet currentCS)
        {
            assigned2GDV.Add(currentCS);
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
