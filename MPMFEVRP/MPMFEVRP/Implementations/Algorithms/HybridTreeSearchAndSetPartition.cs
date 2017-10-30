using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Algorithms
{
    class HybridTreeSearchAndSetPartition : AlgorithmBase
    {
        DateTime startTime;

        Vehicle theGDV;
        XCPlexParameters xCplexParam;

        //These are the important characteristics that will have to be tied to the form
        int beamWidth;
        int filterWidth;
        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList allCustomerSets;
        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents;

        XCPlex_SetCovering_wCustomerSets SetPartitionSolver;
        Dictionary<string, int> setPartitionCounterByStatus;
        Dictionary<string, double> setPartitionCumulativeTimeAccount;
        Dictionary<string, double> timeSpentByXCplexSolutionStatus;

//        Forms.HybridTreeSearchAndSetPartitionCharts charts;
        CustomerSetTreeSearchListener customerSetTreeSearchListener;
        UpperBoundListener upperBoundListener;
        TimeSpentAccountListener timeSpentAccountListener;

        public override void AddSpecializedParameters()
        {
        }

        public override string GetName()
        {
            return "CS Generation Heuristic";
        }

        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>{
                 //Algorithm Name has to be the first entry for output file name purposes
                "Algorithm Name: " + GetName(),
                //Run time limit has to be the second entry for output file name purposes
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
                
                //Optional
                //algorithmParameters.GetAllParameters();
                //var asString = string.Join(";", algorithmParameters.GetAllParameters());
                //list.Add(asString);
                
                //Necessary statistics
                "CPU Run Time(sec): " + stats.RunTimeMilliSeconds.ToString(),
                "Solution Status: " + status.ToString()
            };
            switch (status)
            {
                case AlgorithmSolutionStatus.NotYetSolved:
                    {
                        break;
                    }
                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
                    {
                        break;
                    }
                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
                    {
                        break;
                    }
                default:
                    {
                        list.Add("UB(Best Int): " + stats.UpperBound.ToString());
                        list.Add("LB(Relaxed): " + stats.LowerBound.ToString());
                        break;
                    }
            }

            foreach(string s in stats.getOtherStats().Keys)
            {
                list.Add(s+"\t"+stats.getOtherStats()[s]);
            }

            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
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
            if (setPartitionCumulativeTimeAccount != null)
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
            beamWidth = 3;
            filterWidth = 7;
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
            stats = new AlgorithmStatistics();
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
                    parents = unexploredCustomerSets.Pop(currentLevel, ((currentLevel<=3)&&(currentLevel == unexploredCustomerSets.GetHighestNonemptyLevel()))?100: beamWidth);

                    //populate children from parents
                    PopulateAndPlaceChildren();

                    InformCustomerSetTreeSearchListener();

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
                List<string> remainingCustomers = FilterRemainingCustomers(cs);

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
        List<string> FilterRemainingCustomers(CustomerSet cs)
        {
            List<string> remainingCustomers_list = theProblemModel.GetAllCustomerIDs();
            foreach (string customerID in cs.Customers)
                remainingCustomers_list.Remove(customerID);
            string[] remainingCustomers_array = remainingCustomers_list.ToArray();
            double[] minDistances = new double[remainingCustomers_array.Length];

            string unvisited;
            double minTempDist, tempDist;
            for (int i=0;i<remainingCustomers_array.Length;i++)
            {
                unvisited = remainingCustomers_array[i];
               minTempDist = double.MaxValue;
                foreach(string visited in cs.Customers)
                {
                    tempDist = Math.Min(theProblemModel.SRD.GetDistance(unvisited, visited), theProblemModel.SRD.GetDistance(unvisited, visited));
                    if (tempDist < minTempDist)
                    {
                        minTempDist = tempDist;
                    }
                }
                minDistances[i] = minTempDist;
            }
            Array.Sort(minDistances, remainingCustomers_array);

            List<string> outcome = new List<string>();
            for (int i = 0; i < filterWidth; i++)
                outcome.Add(remainingCustomers_array[i]);
            return outcome;
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
            if (timeSpentAccountListener == null)
                return;
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

        public override bool setListener(IListener listener)
        {
            bool outcome = false;
            if (listener is CustomerSetTreeSearchListener)
            {
                outcome = true;
                customerSetTreeSearchListener = (CustomerSetTreeSearchListener)listener;
            }
            if (listener is UpperBoundListener)
            {
                outcome = true;
                upperBoundListener = (UpperBoundListener)listener;
            }
            if (listener is TimeSpentAccountListener)
            {
                outcome = true;
                timeSpentAccountListener = (TimeSpentAccountListener)listener;
            }
            return outcome;
        }
    }
}
