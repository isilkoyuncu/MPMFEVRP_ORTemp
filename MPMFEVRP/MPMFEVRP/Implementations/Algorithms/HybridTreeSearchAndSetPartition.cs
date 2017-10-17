﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;

namespace MPMFEVRP.Implementations.Algorithms
{
    class HybridTreeSearchAndSetPartition : AlgorithmBase
    {
        DateTime startTime;

        Vehicle theGDV;
        XCPlexParameters xCplexParam;

        //These are the important characteristics that will have to be tied to the form
        int beamWidth;
        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList allCustomerSets;
        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents;

        XCPlex_SetCovering_wCustomerSets SetPartitionSolver;
        Dictionary<string, int> setPartitionCounterByStatus;
        Dictionary<string, double> setPartitionCumulativeTimeAccount;

//        Forms.HybridTreeSearchAndSetPartitionCharts charts;
        CustomerSetTreeSearchListener customerSetTreeSearchListener;
        UpperBoundListener upperBoundListener;
        TimeSpentAccountListener timeSpentAccountListener;

        public override void AddSpecializedParameters()
        {
        }

        public override string GetName()
        {
            return "Customer Set-based Column Generation Heuristic Version 3: Back-to-back GDV- and AFV-optimization";
        }

        public override string[] GetOutputSummary()
        {
            System.Windows.Forms.MessageBox.Show("GetOutputSummary Not yet implemented!");
            return null;
        }

        public override void SpecializedConclude()
        {
            stats.RunTimeMilliSeconds = (long)(DateTime.Now - startTime).TotalMilliseconds;
            //assuming the upper bound is properly updated throughout the course of the algorithm
            if (unexploredCustomerSets.TotalCount == 0)
                stats.LowerBound = stats.UpperBound;

            stats.addNewStat("Number of unexplored Customer Sets", unexploredCustomerSets.TotalCount.ToString());

            foreach (string key in theProblemModel.EV_TSP_TimeSpentAccount.Keys)
            {
                stats.addNewStat("EV_#solved_" + key, theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus[key].ToString());
                stats.addNewStat("EV_timespent_" + key, theProblemModel.EV_TSP_TimeSpentAccount[key].ToString());
            }
            foreach (string key in theProblemModel.GDV_TSP_TimeSpentAccount.Keys)
            {
                stats.addNewStat("GDV_#solved_" + key, theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus[key].ToString());
                stats.addNewStat("GDV_timespent_" + key, theProblemModel.GDV_TSP_TimeSpentAccount[key].ToString());
            }
            foreach (string key in setPartitionCumulativeTimeAccount.Keys)
            {
                stats.addNewStat("SetPartition_" + key, setPartitionCounterByStatus[key].ToString());
                stats.addNewStat("SetPartition_" + key, setPartitionCumulativeTimeAccount[key].ToString());
            }

        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            startTime = DateTime.Now;
            theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);//Technically this returns the first GDV, but there shouldn't be more than one anyways
            xCplexParam = new XCPlexParameters();

            //These are the important characteristics that will have to be tied to the form
            beamWidth = 1;
            popStrategy = CustomerSetList.CustomerListPopStrategy.MinOFVforAnyVehicle;//This is too tightly coupled! Will cause issues in generalizing to tree search

            allCustomerSets = new PartitionedCustomerSetList();
            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);
        }
        void PopulateAndPlaceInitialCustomerSets()
        {
            int nCustomers = theProblemModel.SRD.NumCustomers;
            foreach (string customerID in theProblemModel.SRD.GetCustomerIDs())
            {
                CustomerSet candidate = new CustomerSet(customerID);//The customer set is not TSP-optimized
                OptimizeCustomerSetAndEvaluateForLists(candidate);
            }
        }

        public override void SpecializedReset()
        {
            SetPartitionSolver.Dispose();
        }

        public override void SpecializedRun()
        {
            //Iteration 0:
            PopulateAndPlaceInitialCustomerSets();
            InformCustomerSetTreeSearchListener();
            InformTimeSpentAccountListener();
            RunSetPartition();

            int currentLevel;//, highestNonemptyLevel, deepestNonemptyLevel;
            int deepestPossibleLevel = theProblemModel.SRD.NumCustomers - 1;//This is for the unexplored, when explored its children will be at the next level which is the number of customers; thus, a CS visiting all customers will be created, TSP-optimized and hence added to the repository for the set cover model but it won't ever be added to the unexplored list
            int iter = 0;

            while (!Terminate())
            {
                //The core of the iteration:
                iter++;
                currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();
                while (currentLevel <= deepestPossibleLevel)
                {
                    //Take the parents from the current level
                    if (currentLevel > unexploredCustomerSets.GetDeepestNonemptyLevel())
                        break;
                    parents = unexploredCustomerSets.Pop(currentLevel, ((currentLevel<=-3)&&(currentLevel == unexploredCustomerSets.GetHighestNonemptyLevel()))?100: beamWidth);

                    //populate children from parents
                    PopulateAndPlaceChildren();

                //    InformCustomerSetTreeSearchListener();

                    //end of the level, moving on to the next level
                    currentLevel++;
                }

                //The end of the iteration, solving the SetPartition and updating the best solution if/when possible
                RunSetPartition();
            }//while (!Terminate())
        }
        bool Terminate()
        {
            if (unexploredCustomerSets.TotalCount == 0)
                return true;
            if ((DateTime.Now - startTime).TotalSeconds >= algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue())
                return true;
            //If none of the rules tried above works, we can't terminate
            return false;
        }
        void PopulateAndPlaceChildren()
        {
            foreach (CustomerSet cs in parents)
            {
                List<string> remainingCustomers = theProblemModel.GetAllCustomerIDs();
                foreach (string customerID in cs.Customers)
                    remainingCustomers.Remove(customerID);
                foreach (string customerID in remainingCustomers)
                {
                    CustomerSet candidate = new CustomerSet(cs);
                    candidate.Extend(customerID);
                    OptimizeCustomerSetAndEvaluateForLists(candidate);
                }//foreach (string customerID in remainingCustomers)
            }//foreach (CustomerSet cs in parents)
            parents.Clear();
            InformCustomerSetTreeSearchListener();
        }
        void InformCustomerSetTreeSearchListener()
        {
            if (customerSetTreeSearchListener != null)
                //customerSetTreeSearchListener.OnChangeOfNumberOfUnexploredCustomerSets(unexploredCustomerSets.CountByLevel());
                customerSetTreeSearchListener.OnChangeOfNumbersOfUnexploredAndExploredCustomerSets(unexploredCustomerSets.CountByLevel(), allCustomerSets.CountByLevel());
        }
        void InformUpperBoundListener()
        {
            if (upperBoundListener != null)
                upperBoundListener.OnUpperBoundUpdate(stats.UpperBound);
        }
        void InformTimeSpentAccountListener()
        {
            Dictionary<string, double> timeSpentByXCplexSolutionStatus = new Dictionary<string, double>();
            foreach (string key in theProblemModel.EV_TSP_TimeSpentAccount.Keys)
                timeSpentByXCplexSolutionStatus.Add("EV_" + key, theProblemModel.EV_TSP_TimeSpentAccount[key]);
            foreach (string key in theProblemModel.GDV_TSP_TimeSpentAccount.Keys)
                timeSpentByXCplexSolutionStatus.Add("GDV_" + key, theProblemModel.GDV_TSP_TimeSpentAccount[key]);
            if (SetPartitionSolver != null)
            {
                if (setPartitionCumulativeTimeAccount == null)
                {
                    setPartitionCounterByStatus = new Dictionary<string, int>();
                    setPartitionCumulativeTimeAccount = new Dictionary<string, double>();
                }
                if (setPartitionCumulativeTimeAccount != null)
                {
                    foreach (string key in SetPartitionSolver.TotalTimeInSolveOnStatus.Keys)
                        if (setPartitionCumulativeTimeAccount.ContainsKey(key))
                        {
                            setPartitionCounterByStatus[key]++;
                            setPartitionCumulativeTimeAccount[key] += SetPartitionSolver.TotalTimeInSolveOnStatus[key];
                        }
                        else
                        {
                            setPartitionCounterByStatus.Add(key, 1);
                            setPartitionCumulativeTimeAccount.Add(key, SetPartitionSolver.TotalTimeInSolveOnStatus[key]);
                        }
                }
                foreach(string key in setPartitionCumulativeTimeAccount.Keys)
                    timeSpentByXCplexSolutionStatus.Add("SetPartition_" + key, setPartitionCumulativeTimeAccount[key]);
            }
            //TODO: Set partition data is obtained and stored in not an elegant way. Turning it into a singleton just like the EV and GDV TSP solvers may be a good idea.
            //TODO: Also pass the number of times!
            //The rest is good
            if (timeSpentByXCplexSolutionStatus.Count > 0)
                timeSpentAccountListener.OnChangeOfTimeSpentAccount(timeSpentByXCplexSolutionStatus);
        }

        void OptimizeCustomerSetAndEvaluateForLists(CustomerSet candidate)
        {
            //First we check equivalency of the candidate to previously evaluated customer sets
            if (allCustomerSets.ContainsAnIdenticalCustomerSet(candidate))
                return;

            candidate.Optimize(theProblemModel);
            allCustomerSets.Add(candidate);
            //TODO: #2-the following check to make sure the candidate is worth keeping is only for all-AFV version, thus we need to have a checkpoint to detrmine of the problem is mixed-fleet or all-AFV
            if (candidate.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                return;
            if (candidate.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).Status == VehicleSpecificRouteOptimizationStatus.Optimized)
            {
                unexploredCustomerSets.Add(candidate);
            }
            InformCustomerSetTreeSearchListener();
            InformTimeSpentAccountListener();
        }

        void RunSetPartition()
        {
            SetPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, xCplexParam, cs_List: allCustomerSets.GetAFVFeasibles(), noGDVUnlimitedEV: true);
            SetPartitionSolver.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)SetPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
            stats.UpperBound = theProblemModel.CalculateObjectiveFunctionValue(bestSolutionFound.OFIDP);
            //stats.UpperBound = unexploredCustomerSets.TotalCount * 10;//
            InformUpperBoundListener();
            InformTimeSpentAccountListener();
        }

        public override void setListener(IListener listener)
        {
            if (listener is CustomerSetTreeSearchListener)
                customerSetTreeSearchListener = (CustomerSetTreeSearchListener)listener;
            if (listener is UpperBoundListener)
                upperBoundListener = (UpperBoundListener)listener;
            if (listener is TimeSpentAccountListener)
                timeSpentAccountListener = (TimeSpentAccountListener)listener;
        }
    }
}
