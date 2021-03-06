﻿using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;


namespace MPMFEVRP.Implementations.Solutions
{
    public class CustomerSetBasedSolution : SolutionBase 
    {
        private EVvsGDV_ProblemModel model;
        private Random random;

        //For now we assume we have only EV and GDV lists, if you add another vehicle category, you should update the objective function calculations.
        CustomerSetList assigned2EV; public CustomerSetList Assigned2EV { get { return assigned2EV; } }
        CustomerSetList assigned2GDV; public CustomerSetList Assigned2GDV { get { return assigned2GDV; } }

        public int NumCS_assigned2EV { get { return (assigned2EV == null) ? 0 : assigned2EV.Count; } }
        public int NumCS_assigned2GDV { get { return (assigned2GDV == null) ? 0 : assigned2GDV.Count; } }
        public int NumCS_total { get { return (assigned2EV == null && assigned2GDV == null) ? 0 : (assigned2EV.Count + assigned2GDV.Count); } }

        private int[,] zSetTo1;

        public CustomerSetBasedSolution() //ISSUE (#7) check if we really need these to be created in the empty constructor
        {
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
            ofidp = new ObjectiveFunctionInputDataPackage();
            upperBound = double.MaxValue;
            lowerBound = double.MinValue;
        }

        public CustomerSetBasedSolution(EVvsGDV_ProblemModel theProblemModel)
        {
            this.model = theProblemModel;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
            ofidp = new ObjectiveFunctionInputDataPackage();
            upperBound = double.MaxValue;
            lowerBound = double.MinValue;
        }

        public CustomerSetBasedSolution(CustomerSetBasedSolution twinCSBasedSolution)
        {
            model = twinCSBasedSolution.model;
            random = twinCSBasedSolution.random;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
            twinCSBasedSolution.assigned2EV.ForEach((item) => { AddCustomerSet2EVList(new CustomerSet(item)); });
            twinCSBasedSolution.assigned2GDV.ForEach((item) => { AddCustomerSet2GDVList(new CustomerSet(item)); });
            ofidp = new ObjectiveFunctionInputDataPackage(twinCSBasedSolution.OFIDP);
            isComplete = twinCSBasedSolution.isComplete;
            lowerBound = twinCSBasedSolution.lowerBound;
            status = twinCSBasedSolution.status;
            upperBound = twinCSBasedSolution.upperBound;
        }

        public CustomerSetBasedSolution(EVvsGDV_ProblemModel theProblemModel, int[,] zSetTo1, CustomerSet[] customerSetArray)
        {
            ofidp = new ObjectiveFunctionInputDataPackage();
            this.model = theProblemModel;
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

        // TODO fill this constructor so that it'll create an initial random solution by itself (i.e. do nothing) This is needed for the BestOfRandom algorithm, so tie this to it
        public CustomerSetBasedSolution(EVvsGDV_ProblemModel theProblemModel, Random random)
        {
            this.model = theProblemModel;
            this.random = random;
            assigned2EV = new CustomerSetList();
            assigned2GDV = new CustomerSetList();
            ofidp = new ObjectiveFunctionInputDataPackage();
        }

        public void AddCustomerSet2EVList(CustomerSet currentCS)
        {
            assigned2EV.Add(currentCS);
            ofidp.Add(currentCS.OFIDP, VehicleCategories.EV);
        }

        public void AddCustomerSet2GDVList(CustomerSet currentCS)
        {
            assigned2GDV.Add(currentCS);
            ofidp.Add(currentCS.OFIDP, VehicleCategories.GDV);
        }

        public void UpdateUpperLowerBoundsAndStatus()
        {
            if (model.ObjectiveFunctionType == Models.ObjectiveFunctionTypes.Maximize)//If it is a maximization problem, LB is the incumbent solution's objective value
                lowerBound = model.CalculateObjectiveFunctionValue(this);
            else //If it is a minimization problem, UB is the incumbent solution's objective value
                upperBound = model.CalculateObjectiveFunctionValue(this);
            status = Domains.AlgorithmDomain.AlgorithmSolutionStatus.Feasible;
        }

        public void UpdateUpperLowerBoundsAndStatusForInfeasible()
        {
            if (model.ObjectiveFunctionType == Models.ObjectiveFunctionTypes.Maximize)//If it is a maximization problem, LB is the incumbent solution's objective value
                lowerBound = double.MinValue;
            else //If it is a minimization problem, UB is the incumbent solution's objective value
                upperBound = double.MaxValue;
            status = Domains.AlgorithmDomain.AlgorithmSolutionStatus.Infeasible;
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
            return GetWritableSolution();//throw new NotImplementedException();
        }

        public override string[] GetWritableSolution()
        {
            List<string> list = new List<string>
            {
                "Route\tVehicle\tPrizeCollected\tCostIncurred\tProfit\tTime2ReturnDepot",
            };
            if (status == AlgorithmSolutionStatus.Feasible || status == AlgorithmSolutionStatus.Optimal)
            {
                for (int r = 0; r < assigned2EV.Count; r++)
                {
                    //TODO this is not necessary to fix, we need to code a converter that will convert all the solutions into meaningful text files.
                    list.Add(String.Join("-", assigned2EV[r].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs) + "\t" + VehicleCategories.EV.ToString() + "\t" + "-" + "\t" + assigned2EV[r].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetVehicleMilesTraveled().ToString() + "\t" + "-" + "\t" + "-");
                }
                for (int r = 0; r < assigned2GDV.Count; r++)
                {
                    //TODO this is not necessary to fix, we need to code a converter that will convert all the solutions into meaningful text files.
                    list.Add(String.Join("-", assigned2GDV[r].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs) + "\t" + VehicleCategories.GDV.ToString() + "\t" + "-" + "\t" + assigned2GDV[r].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled().ToString() + "\t" + "-" + "\t" + "-");
                }
            }
            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
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
