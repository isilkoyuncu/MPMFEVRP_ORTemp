﻿using System;
using System.Collections.Generic;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class PartitionedCustomerSetList
    {
        CustomerSetList.CustomerListPopStrategy popStrategy;

        int deepestLevel;
        public int DeepestLevel { get { return deepestLevel; } }

        List<CustomerSetList> CSLList;

        public PartitionedCustomerSetList()
        {
            deepestLevel = -1;
            CSLList = new List<CustomerSetList>();
        }
        public PartitionedCustomerSetList(CustomerSetList.CustomerListPopStrategy popStrategy)
        {
            this.popStrategy = popStrategy;
            deepestLevel = -1;
            CSLList = new List<CustomerSetList>();
        }

        public void ConsiderForAddition(CustomerSetList CSL)
        {
            foreach (CustomerSet cs in CSL)
                ConsiderForAddition(cs);
        }
        public void ConsiderForAddition(CustomerSet CS)
        {
            int level = CS.NumberOfCustomers;
            if(level > deepestLevel)//this is the first ever customer set seen at this level, the list at the level has not been needed so far, this is the time to create it
            {
                for (int l = deepestLevel + 1; l <= level; l++)
                    CSLList.Add(new CustomerSetList(popStrategy));
                deepestLevel = level;
            }

            if (!CSLList[level].Includes(CS))
                CSLList[level].Add(CS);
        }
        
        public CustomerSetList Pop(int level, int numberToPop)
        {
            return CSLList[level].Pop(numberToPop);
        }
        public CustomerSetList Pop(int level, int numberToPop, VehicleCategories vehicleCategory, Dictionary<string,double> shadowPrices)
        {
            return CSLList[level].Pop(numberToPop, vehicleCategory, shadowPrices);
        }
        public CustomerSetList Pop(List<CustomerSet> theList)
        {
            CustomerSetList outcome = new CustomerSetList();
            foreach(CustomerSet cs in theList)
            {
                int l = cs.NumberOfCustomers;
                if (CSLList[l].Contains(cs))
                {
                    CSLList[l].Remove(cs);
                    outcome.Add(cs);
                }
                else
                {
                    //do nothing
                }
            }
            return outcome;
        }

        public int GetHighestNonemptyLevel()
        {
            for (int level = 0; level <= deepestLevel; level++)
                if (CSLList[level].Count > 0)
                    return level;
            return -1;
        }
        public int GetDeepestNonemptyLevel()
        {
            for (int level = deepestLevel; level >= 0; level--)
                if (CSLList[level].Count > 0)
                    return level;
            return -1;
        }
        public int[] CountByLevel()
        {
            int[] outcome = new int[deepestLevel + 1];
            for (int l = 0; l < outcome.Length; l++)
                outcome[l] = CSLList[l].Count;
            return outcome;
        }
        public int TotalCount
        {
            get
            {
                int outcome = 0;
                foreach (CustomerSetList csl in CSLList)
                    outcome += csl.Count;
                return outcome;
            }
        }

        public bool ContainsAnIdenticalCustomerSet(CustomerSet cs)
        {
            return (CSLList.Count == 0 ? false : 
                (cs.NumberOfCustomers >= CSLList.Count ? false : 
                (CSLList[cs.NumberOfCustomers] == null ? false : CSLList[cs.NumberOfCustomers].ContainsAnIdenticalCustomerSet(cs))));
        }
        public void Add(CustomerSet cs)
        {
            if ((CSLList.Count==0)||(cs.NumberOfCustomers>deepestLevel))
                ConsiderForAddition(cs);
            else
                CSLList[cs.NumberOfCustomers].Add(cs);
        }

        public CustomerSetList ToCustomerSetList()
        {
            CustomerSetList outcome = new CustomerSetList();
            for (int l = 0; l <= deepestLevel; l++)
                foreach (CustomerSet cs in CSLList[l])
                    outcome.Add(cs);
            return outcome;
        }
        public CustomerSetList GetAFVFeasibles()
        {
            List<RouteOptimizationStatus> desiredStatuses = new List<RouteOptimizationStatus>() { RouteOptimizationStatus.OptimizedForBothGDVandEV };
            CustomerSetList outcome = new CustomerSetList();
            for (int l = 0; l <= deepestLevel; l++)
                foreach (CustomerSet cs in CSLList[l])
                    if (desiredStatuses.Contains(cs.RouteOptimizationOutcome.Status))
                        outcome.Add(cs);
            return outcome;
        }
        public CustomerSetList GetGDVFeasibles()
        {
            List<RouteOptimizationStatus> desiredStatuses = new List<RouteOptimizationStatus>() { RouteOptimizationStatus.OptimizedForBothGDVandEV, RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV, RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV };
            CustomerSetList outcome = new CustomerSetList();
            for (int l = 0; l <= deepestLevel; l++)
                foreach (CustomerSet cs in CSLList[l])
                    if (desiredStatuses.Contains(cs.RouteOptimizationOutcome.Status))
                        outcome.Add(cs);
            return outcome;
        }

        /// <summary>
        /// Returns Dictionary<level,Tuple<Number at Level, Avg. Comp. Time>>
        /// </summary>
        /// <param name="vc"></param>
        /// <returns></returns>
        public Dictionary<int, Tuple<int,double>> GetSolutionTimeStatisticsByLevel(VehicleCategories vc)
        {
            if (TotalCount == 0)
                return null;

            Dictionary<int, Tuple<int, double>> outcome = new Dictionary<int, Tuple<int, double>>();

            int lMin = GetHighestNonemptyLevel();
            int lMax = GetDeepestNonemptyLevel();
            int[] countsByLevel = CountByLevel();

            for (int l = lMin; l <= lMax; l++)
            {
                int numCS = 0;
                double totalCompTime = 0.0;
                if (CSLList[l].Count > 0)
                {
                    foreach (CustomerSet cs in CSLList[l])
                        if (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vc) != null)
                        {
                            numCS++;
                            totalCompTime += cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vc).ComputationTime;
                        }
                    outcome.Add(l, new Tuple<int, double>(numCS, totalCompTime / numCS));
                }
            }
            return outcome;
        }
    }
}
