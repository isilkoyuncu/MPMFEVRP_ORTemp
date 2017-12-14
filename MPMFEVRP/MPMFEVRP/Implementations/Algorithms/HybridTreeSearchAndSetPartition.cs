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
        bool partialWideBranching;
        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList allCustomerSets;
        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents;

        XCPlex_SetCovering_wCustomerSets RelaxedSetPartitionSolver;
        Dictionary<string, double> ShadowPrices;
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
            return "Hybrid tree search & set partition";
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
                    stats.addNewStat("SetPartition_#solved_" + key, setPartitionCounterByStatus[key].ToString());
                    stats.addNewStat("SetPartition_timespent" + key, setPartitionCumulativeTimeAccount[key].ToString());
                }
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            startTime = DateTime.Now;
            theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);//Technically this returns the first GDV, but there shouldn't be more than one anyways
            xCplexParam = new XCPlexParameters();

            //These are the important characteristics that will have to be tied to the form
            beamWidth = 10;
            filterWidth = 1;
            partialWideBranching = true;
            popStrategy = CustomerSetList.CustomerListPopStrategy.MinOFVforAnyVehicle;//This is too tightly coupled! Will cause issues in generalizing to tree search

            allCustomerSets = new PartitionedCustomerSetList();
            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);

            timeSpentByXCplexSolutionStatus = new Dictionary<string, double>();

            ShadowPrices = new Dictionary<string, double>();
            foreach (string customerID in theProblemModel.SRD.GetCustomerIDs())
                ShadowPrices.Add(customerID, 0.0);
        }
        void PopulateAndPlaceInitialCustomerSets()
        {
            int nCustomers = theProblemModel.SRD.NumCustomers;
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            int level = 0;
            int maxProbingLevel = 15;
            foreach (string customerID in allCustomers)
            {
                CustomerSet candidate = new CustomerSet(customerID, allCustomers);//The customer set is not TSP-optimized
                OptimizeCustomerSetAndEvaluateForLists(candidate);
                level = 1;
                do
                {
                    level++;
                    List<string> possibleExtendingCustomers = FilterRemainingCustomers(candidate, candidate.NumberOfCustomers, false);
                    if (possibleExtendingCustomers.Count > 0)
                    {
                        string extendingCustomerID = possibleExtendingCustomers[0];
                        candidate = new CustomerSet(candidate);
                        candidate.Extend(extendingCustomerID);
                        if (!OptimizeCustomerSetAndEvaluateForLists(candidate))
                            candidate = null;
                    }
                    else
                        candidate = null;
                } while ((candidate != null) && (level < maxProbingLevel));
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

                //This is new, it focuses only on the basic variables (customer sets)
                int nColumns_before = allCustomerSets.TotalCount;
                List<CustomerSet> basicCustomerSetsInBestSolution = new List<CustomerSet>();
                CustomerSetBasedSolution csbsBestSolutionFound = (CustomerSetBasedSolution)bestSolutionFound;
                foreach (CustomerSet cs in csbsBestSolutionFound.Assigned2EV)
                    basicCustomerSetsInBestSolution.Add(cs);
                foreach (CustomerSet cs in csbsBestSolutionFound.Assigned2GDV)
                    basicCustomerSetsInBestSolution.Add(cs);
                parents = unexploredCustomerSets.Pop(basicCustomerSetsInBestSolution);
                PopulateAndPlaceChildren(-1,true);
                InformCustomerSetTreeSearchListener();
                //if new columns have been added, we need to bypass the IBS column generation phase
                if (allCustomerSets.TotalCount == nColumns_before)
                {
                    //The following is the regular IBS node selection and branching
                    currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();
                    while (currentLevel <= deepestPossibleLevel)
                    {
                        //Take the parents from the current level
                        if (currentLevel > unexploredCustomerSets.GetDeepestNonemptyLevel())
                            break;
                        // parents = unexploredCustomerSets.Pop(currentLevel, ((currentLevel<=3)&&(currentLevel == unexploredCustomerSets.GetHighestNonemptyLevel()))?100: beamWidth);
                        parents = unexploredCustomerSets.Pop(currentLevel, beamWidth, VehicleCategories.EV, ShadowPrices);//ISSUE (#5): This is hardcoded with the EMH problem in mind, will need to flex vehicleCategory
                                                                                                                          //populate children from parents
                        PopulateAndPlaceChildren();

                        InformCustomerSetTreeSearchListener();

                        //end of the level, moving on to the next level
                        currentLevel++;
                    }
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
            PopulateAndPlaceChildren(filterWidth, true);
        }
        void PopulateAndPlaceChildren(int specialFilterWidth, bool useShadowPrices)
        {
            foreach (CustomerSet cs in parents)
            {
                foreach (string customerID in FilterRemainingCustomers(cs, specialFilterWidth, useShadowPrices))
                {
                    CustomerSet candidate = new CustomerSet(cs);
                    candidate.Extend(customerID);
                    cs.MakeCustomerImpossible(customerID);
                    OptimizeCustomerSetAndEvaluateForLists(candidate);
                }//foreach (string customerID in remainingCustomers)
            }//foreach (CustomerSet cs in parents)
            if (partialWideBranching)
            {
                foreach (CustomerSet cs in parents)
                    if (cs.PossibleOtherCustomers.Count > 0)
                        unexploredCustomerSets.Add(cs);
            }
            parents.Clear();
            InformCustomerSetTreeSearchListener();
        }
        List<string> FilterRemainingCustomers(CustomerSet cs, int specialFilterWidth, bool useShadowPrices)
        {
            List<string> remainingCustomers_list = cs.PossibleOtherCustomers;
            string[] remainingCustomers_array = remainingCustomers_list.ToArray();
            double[] bestEstimates = new double[remainingCustomers_array.Length];

            string unvisited;
            for (int i=0;i<remainingCustomers_array.Length;i++)
            {
                unvisited = remainingCustomers_array[i];
                double minAddlDist = cs.MinAdditionalDistanceForPossibleOtherCustomer[unvisited];
                bestEstimates[i] = cs.MinAdditionalDistanceForPossibleOtherCustomer[unvisited]-ShadowPrices[unvisited];
            }
            Array.Sort(bestEstimates, remainingCustomers_array);

            List<string> outcome = new List<string>();
            if (specialFilterWidth < 0)
            {
                int bestEstIndex = 0;
                while ((bestEstIndex < cs.NumberOfCustomers)&&(bestEstIndex < remainingCustomers_array.Length))
                {
                    if ((!useShadowPrices) || ((useShadowPrices) && (bestEstimates[bestEstIndex] <= 0)))
                        outcome.Add(remainingCustomers_array[bestEstIndex]);
                    bestEstIndex++;
                }
            }
            else
            {
                for (int i = 0; i < specialFilterWidth; i++)
                    if (remainingCustomers_array.Length > i)
                        if ((!useShadowPrices) || ((useShadowPrices) && (bestEstimates[i] <= 0)))
                            outcome.Add(remainingCustomers_array[i]);
            }
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
            /*Dictionary<string, double> *///timeSpentByXCplexSolutionStatus = new Dictionary<string, double>();
            foreach (string key in theProblemModel.EV_TSP_TimeSpentAccount.Keys)
            {
                if (timeSpentByXCplexSolutionStatus.ContainsKey("EV_" + key))
                {
                    timeSpentByXCplexSolutionStatus["EV_" + key] = theProblemModel.EV_TSP_TimeSpentAccount[key];
                }
                else
                {
                    timeSpentByXCplexSolutionStatus.Add("EV_" + key, theProblemModel.EV_TSP_TimeSpentAccount[key]);
                }
            }
            foreach (string key in theProblemModel.GDV_TSP_TimeSpentAccount.Keys)
            {
                if (timeSpentByXCplexSolutionStatus.ContainsKey("GDV_" + key))
                {
                    timeSpentByXCplexSolutionStatus["GDV_" + key] = theProblemModel.GDV_TSP_TimeSpentAccount[key];
                }
                else
                {
                    timeSpentByXCplexSolutionStatus.Add("GDV_" + key, theProblemModel.GDV_TSP_TimeSpentAccount[key]);
                }
            }
            if (setPartitionCumulativeTimeAccount != null)
                foreach (string key in setPartitionCumulativeTimeAccount.Keys)
                {
                    if (timeSpentByXCplexSolutionStatus.ContainsKey("SetPartition_" + key))
                    {
                        timeSpentByXCplexSolutionStatus["SetPartition_" + key] = setPartitionCumulativeTimeAccount[key];
                    }
                    else
                    {
                        timeSpentByXCplexSolutionStatus.Add("SetPartition_" + key, setPartitionCumulativeTimeAccount[key]);
                    }
                }

            //TODO: Set partition data is obtained and stored in not an elegant way. Turning it into a singleton just like the EV and GDV TSP solvers may be a good idea.
            //TODO: Also pass the number of times!
            //The rest is good
            if (timeSpentByXCplexSolutionStatus.Count > 0)
                timeSpentAccountListener.OnChangeOfTimeSpentAccount(timeSpentByXCplexSolutionStatus);
        }

        bool OptimizeCustomerSetAndEvaluateForLists(CustomerSet candidate)
        {
            //First we check equivalency of the candidate to previously evaluated customer sets
            if (allCustomerSets.ContainsAnIdenticalCustomerSet(candidate))
                return false;

            
            Console.WriteLine("Now solving " + Utils.StringOperations.CombineAndSpaceSeparateArray(candidate.Customers.ToArray()));
            candidate.Optimize(theProblemModel);
            allCustomerSets.Add(candidate);
            //ISSUE (#5): the following check to make sure the candidate is worth keeping is only for all-AFV version, thus we need to have a checkpoint to determine of the problem is mixed-fleet or all-AFV
            if (candidate.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                return false;
            if (candidate.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).Status == VehicleSpecificRouteOptimizationStatus.Optimized)
            {
                unexploredCustomerSets.Add(candidate);
            }
            InformCustomerSetTreeSearchListener();
            InformTimeSpentAccountListener();
            return true;
        }

        void RunSetPartition()
        {
            RelaxedSetPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, new XCPlexParameters(relaxation: XCPlexRelaxation.LinearProgramming), cs_List: allCustomerSets.GetAFVFeasibles(), noGDVUnlimitedEV: true);
            RelaxedSetPartitionSolver.Solve_and_PostProcess();
            ShadowPrices = RelaxedSetPartitionSolver.GetCustomerCoverageConstraintShadowPrices();
            SetPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, xCplexParam, cs_List: allCustomerSets.GetAFVFeasibles(), noGDVUnlimitedEV: true);
            SetPartitionSolver.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)SetPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
            stats.UpperBound = theProblemModel.CalculateObjectiveFunctionValue(bestSolutionFound.OFIDP);
            //stats.UpperBound = unexploredCustomerSets.TotalCount * 10;//
            updateSetCoverTimeStatistics();
            InformUpperBoundListener();
            InformTimeSpentAccountListener();
        }
        void updateSetCoverTimeStatistics()
        {
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
            }
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
