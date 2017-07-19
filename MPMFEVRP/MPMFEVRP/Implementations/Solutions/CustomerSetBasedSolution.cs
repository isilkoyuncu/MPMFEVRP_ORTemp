﻿using System;
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
            this.model = model;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
        }

        public CustomerSetBasedSolution(CustomerSetBasedSolution twinCSBasedSolution)
        {
            model = twinCSBasedSolution.model;
            random = twinCSBasedSolution.random;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
            twinCSBasedSolution.assigned2EV.ForEach((item) => { AddCustomerSet2EVList(new CustomerSet(item)); });
            twinCSBasedSolution.assigned2GDV.ForEach((item) => { AddCustomerSet2GDVList(new CustomerSet(item)); });
            isComplete = twinCSBasedSolution.isComplete;
            lowerBound = twinCSBasedSolution.lowerBound;
            objectiveFunctionValue = twinCSBasedSolution.objectiveFunctionValue;
            status = twinCSBasedSolution.status;
            upperBound = twinCSBasedSolution.upperBound;
        }

        public CustomerSetBasedSolution(ProblemModelBase model, int[,] zSetTo1, CustomerSet[] customerSetArray)
        {
            this.model = model;
            this.zSetTo1 = zSetTo1;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
            for (int i = 0; i < customerSetArray.Length; i++)
            {
                if (zSetTo1[i, 0] == 1)
                {
                    AddCustomerSet2EVList(customerSetArray[i]);
                }
                else if (zSetTo1[i, 1] == 1)
                {
                    AddCustomerSet2GDVList(customerSetArray[i]);
                }
            }
        }
        
        // TODO fill this constructor so that it'll create an initial random solution by itself (i.e. do nothing)
        public CustomerSetBasedSolution(ProblemModelBase model, Random random)
        {
            this.model = model;
            this.random = random;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
        }

        //public CustomerSetBasedSolution(ProblemModelBase model, int[,] zSetTo1, CustomerSetBasedSolution trialSolution)
        //{
        //    this.model = model;
        //    this.zSetTo1 = zSetTo1;
        //    assigned2EV = new CustomerSetList();
        //    assigned2GDV = new CustomerSetList();
        //    for (int i = 0; i < trialSolution.NumCS_assigned2EV; i++)
        //    {
        //        if (zSetTo1[i, 0] == 1)
        //        {
        //            AddCustomerSet2EVList(trialSolution.assigned2EV[i]);
        //        }
        //        else if (zSetTo1[i, 1] == 1)
        //        {
        //            AddCustomerSet2GDVList(trialSolution.assigned2EV[i]);
        //        }
        //    }
        //    for (int i = trialSolution.NumCS_assigned2EV; i < trialSolution.NumCS_total; i++)
        //    {
        //        if (zSetTo1[i, 0] == 1)
        //        {
        //            AddCustomerSet2EVList(trialSolution.assigned2GDV[i]);
        //        }
        //        else if (zSetTo1[i, 1] == 1)
        //        {
        //            AddCustomerSet2GDVList(trialSolution.assigned2GDV[i]);
        //        }
        //    }
        //}

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
