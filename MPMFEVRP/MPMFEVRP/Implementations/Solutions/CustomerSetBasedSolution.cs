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
        public List<CustomerSet> CS_List { get { return cs_List; } }

        CustomerSetVehicleAssignment csVehAssignment;
        public CustomerSetVehicleAssignment CSVehAssignment{ get { return csVehAssignment; }}


private int[,] zSetTo1;

        public CustomerSetBasedSolution(ProblemModelBase model, CustomerSetVehicleAssignment csVehAssignment)
        {
            this.model = model;
            this.csVehAssignment = csVehAssignment;//TODO pirimitive olmayan seyleri de boyle kopyalayabiliyor muyuz test et.
            lowerBound = 0.0;
            for(int cs=0;cs< csVehAssignment.Assigned2EV.Count; cs++)
            {
                lowerBound += csVehAssignment.Assigned2EV[cs].RouteOptimizerOutcome.OFV[0];
            }
            for (int cs = 0; cs < csVehAssignment.Assigned2GDV.Count; cs++)
            {
                lowerBound += csVehAssignment.Assigned2GDV[cs].RouteOptimizerOutcome.OFV[1];
            }
        }

        public CustomerSetBasedSolution()// TODO do initialization 
        {
            CustomerSetVehicleAssignment csVehAssignment = new CustomerSetVehicleAssignment();
        }


        public CustomerSetBasedSolution(CustomerSetBasedSolution twinCSBasedSolution)
        {
            model = twinCSBasedSolution.model;
            random = twinCSBasedSolution.random;
            ids = twinCSBasedSolution.ids; // TODO convert this to deep copy
            isComplete = twinCSBasedSolution.isComplete;
            lowerBound = twinCSBasedSolution.lowerBound;
            objectiveFunctionValue = twinCSBasedSolution.objectiveFunctionValue;
            status = twinCSBasedSolution.status;
            upperBound = twinCSBasedSolution.upperBound;
            cs_List = twinCSBasedSolution.cs_List;// TODO convert this to deep copy
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
            cs_List = trialSolution.CS_List; // TODO if there wont be a seperate cs list than assigned vehicles then change this method as well
            for (int i = 0; i < cs_List.Count; i++)
            {
                for (int j = 0; j < 2; j++)//TODO parametrize for the num of veh categories (2 for now)
                {
                    if (zSetTo1[i, j] == 1)
                    {
                        lowerBound += cs_List[i].RouteOptimizerOutcome.OFV[j];
                    }
                }
            }
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
