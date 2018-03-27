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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Implementations.Algorithms
{
    class RandomizedCustomerSetExplorer : AlgorithmBase
    {
        //        XCPlexParameters xCplexParam;
        Vehicle theGDV;
        Vehicle theEV;

        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList unexploredCustomerSets;
        PartitionedCustomerSetList exploredFeasibleCustomerSets;
        PartitionedCustomerSetList exploredInfeasibleCustomerSets;

        PartitionedCustomerSetList InfeasibleCustomerSets, OnlyGDVFeasibleCustomerSets, EVFeasibleCustomerSets;

        double avgTimePerGDVOptimalTSPSolution, avgTimePerGDVInfeasibleTSPSolution, avgTimePerEVOptimalTSPSolution, avgTimePerEVInfeasibleTSPSolution, avgTimePerTheOtherEVOptimalTSPSolution, avgTimePerTheOtherEVInfeasibleTSPSolution;
        TripleSolveOutComeStatistics tripleSolveOutcomeStats;
        string allStats_formatted;

        public RandomizedCustomerSetExplorer() : base()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_NUM_CUSTOMER_SETS_TO_REPORT_PER_CATEGORY, "Minimum # of infeasible customer sets", new List<object>() { 1, 5, 10, 50, 100, 500, 1000 }, 50, UserInputObjectType.TextBox));
            //algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_BEAM_WIDTH, "Beam width", 1));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_MIN_NUM_CUSTOMERS_IN_A_SET, "Minimum # of customers in a set", new List<object>() { 3, 4, 5 }, 4, UserInputObjectType.ComboBox));
        }

        public override string GetName()
        {
            return "Randomized Customer Set Explorer via Tree Search";
        }

        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>{
                //Algorithm Name has to be the first entry for output file name purposes
                //"Algorithm Name: " + GetName()+ "-" +algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value.ToString(),
                //Run time limit has to be the second entry for output file name purposes
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
                
                //Optional
                //"Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value.ToString(),
                //algorithmParameters.GetAllParameters();
                //var asString = string.Join(";", algorithmParameters.GetAllParameters());
                //list.Add(asString);
                
                //Necessary statistics
                "CPU Run Time(sec): " + stats.RunTimeMilliSeconds.ToString(),
                "avgTimePerGDVOptimalTSPSolution: " + avgTimePerGDVOptimalTSPSolution,
                "avgTimePerGDVInfeasibleTSPSolution" + avgTimePerGDVInfeasibleTSPSolution,
                "avgTimePerEVOptimalTSPSolution" + avgTimePerEVOptimalTSPSolution,
                "avgTimePerEVInfeasibleTSPSolution" + avgTimePerEVInfeasibleTSPSolution,
                "Total GDV time spent on optimal: "+ theProblemModel.GDV_TSP_TimeSpentAccount["Optimal"],
                "Total GDV time spent on infeasible: "+ theProblemModel.GDV_TSP_TimeSpentAccount["Infeasible"],
                "Total EV time spent on optimal (NDF): " + theProblemModel.EV_TSP_TimeSpentAccount["Optimal"],
                "Total EV time spent on infeasible (NDF): " + theProblemModel.EV_TSP_TimeSpentAccount["Infeasible"],
                "Total EV time spent on optimal (ADF): " + theProblemModel.TheOtherEV_TSP_TimeSpentAccount["Optimal"],
                "Total EV time spent on infeasible (ADF): " + theProblemModel.TheOtherEV_TSP_TimeSpentAccount["Infeasible"],
                "Total # of GDV optimal: "+ theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus["Optimal"],
                "Total # of GDV infeasible: "+ theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus["Infeasible"],
                "Total # of EV optimal (NDF): "+ theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus["Optimal"],
                "Total # of EV infeasible (NDF): "+ theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus["Infeasible"],
                "Total # of EV optimal (ADF): "+ theProblemModel.TheOtherEV_TSP_NumberOfCustomerSetsByStatus["Optimal"],
                "Total # of EV infeasible (ADF): "+ theProblemModel.TheOtherEV_TSP_NumberOfCustomerSetsByStatus["Infeasible"],
                "Solution Status: " + status.ToString()
            };
            //list.Add("Optimal Comp Time Avgs by Level");
            //Dictionary<int, Tuple<int, double>> optimalCompTimeAvgsByLevel = exploredFeasibleCustomerSets.GetSolutionTimeStatisticsByLevel(VehicleCategories.EV);
            //foreach (int l in optimalCompTimeAvgsByLevel.Keys)
            //{
            //    list.Add(l.ToString());
            //    list.Add(optimalCompTimeAvgsByLevel[l].Item1.ToString());
            //    list.Add(optimalCompTimeAvgsByLevel[l].Item2.ToString());
            //}
            //list.Add("Infeasible Comp Time Avgs by Level");
            //Dictionary<int, Tuple<int, double>> infeasibleCompTimeAvgsByLevel = exploredInfeasibleCustomerSets.GetSolutionTimeStatisticsByLevel(VehicleCategories.EV);
            //foreach (int l in infeasibleCompTimeAvgsByLevel.Keys)
            //{
            //    list.Add(l.ToString());
            //    list.Add(infeasibleCompTimeAvgsByLevel[l].Item1.ToString());
            //    list.Add(infeasibleCompTimeAvgsByLevel[l].Item2.ToString());
            //}

            list.Add(Environment.NewLine);
            list.Add(tripleSolveOutcomeStats.GetSummaryData(theProblemModel.InputFileName));

            list.Add(Environment.NewLine);
            list.Add(allStats_formatted);

            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
        }

        public override bool setListener(IListener listener)
        {
            //TODO: Add later
            System.Windows.Forms.MessageBox.Show("RandomizedCustomerSetExplorerViaTreeSearch.setListener called for some reason; this is not ready yet!");
            throw new NotImplementedException();
        }

        public override void SpecializedConclude()
        {
            //System.Windows.Forms.MessageBox.Show("RandomizedCustomerSetExplorerViaTreeSearch algorithm run successfully, now it's time to conclude by reporting some statistics!");

            //The following assumes no div/zero error is possible, will have to come back to make this code robust
            avgTimePerGDVOptimalTSPSolution = theProblemModel.GDV_TSP_TimeSpentAccount["Optimal"] / theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus["Optimal"];
            avgTimePerGDVInfeasibleTSPSolution = theProblemModel.GDV_TSP_TimeSpentAccount["Infeasible"] / theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus["Infeasible"];

            avgTimePerEVOptimalTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Optimal"] / theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus["Optimal"];
            avgTimePerEVInfeasibleTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Infeasible"] / theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus["Infeasible"];

            avgTimePerTheOtherEVOptimalTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Optimal"] / theProblemModel.TheOtherEV_TSP_NumberOfCustomerSetsByStatus["Optimal"];
            avgTimePerTheOtherEVInfeasibleTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Infeasible"] / theProblemModel.TheOtherEV_TSP_NumberOfCustomerSetsByStatus["Infeasible"];
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //            xCplexParam = new XCPlexParameters();
            theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);
            theEV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);

            popStrategy = CustomerSetList.CustomerListPopStrategy.First;

            exploredFeasibleCustomerSets = new PartitionedCustomerSetList();
            exploredInfeasibleCustomerSets = new PartitionedCustomerSetList();

            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);
            PopulateAndPlaceInitialUnexploredCustomerSets();

            InfeasibleCustomerSets = new PartitionedCustomerSetList();
            OnlyGDVFeasibleCustomerSets = new PartitionedCustomerSetList();
            EVFeasibleCustomerSets = new PartitionedCustomerSetList();

            tripleSolveOutcomeStats = new TripleSolveOutComeStatistics();
            allStats_formatted = "instance\t# Customers\tCustomers\tCombined Status\tGDV time\tEV NDF time\tEV ADF time";
        }
        void PopulateAndPlaceInitialUnexploredCustomerSets()
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            int nCustomers = allCustomers.Count;
            foreach (string customerID in allCustomers)
            {
                CustomerSet candidate = new CustomerSet(customerID, allCustomers);//The customer set is not TSP-optimized

                if ((exploredFeasibleCustomerSets.ContainsAnIdenticalCustomerSet(candidate)) || (exploredInfeasibleCustomerSets.ContainsAnIdenticalCustomerSet(candidate)))
                    continue;

                OptimizeAndEvaluateForLists(candidate);
            }
        }
        void OptimizeAndEvaluateForLists(CustomerSet cs)
        {
            if ((exploredFeasibleCustomerSets.ContainsAnIdenticalCustomerSet(cs)) || (exploredInfeasibleCustomerSets.ContainsAnIdenticalCustomerSet(cs)))
                return;

            cs.Optimize(theProblemModel, theGDV);//TODO: This is necessary only because the current infrastructure assumes GDV-optimization must precede EV-Optimization
            cs.Optimize(theProblemModel, theEV, vsroo_GDV: cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV), requireGDVSolutionBeforeEV: false);
            bool feasible = (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).Status == VehicleSpecificRouteOptimizationStatus.Optimized);
            bool infeasible = (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).Status == VehicleSpecificRouteOptimizationStatus.Infeasible);
            bool largeEnough = (cs.NumberOfCustomers > 3);
            if (largeEnough)
            {
                int numCustomerSetsToReport = algorithmParameters.GetParameter(ParameterID.ALG_NUM_CUSTOMER_SETS_TO_REPORT_PER_CATEGORY).GetIntValue();

                if (feasible)
                {
                    if((numCustomerSetsToReport<0)||(exploredFeasibleCustomerSets.TotalCount< numCustomerSetsToReport))
                    exploredFeasibleCustomerSets.Add(cs);
                }
                else if (infeasible)
                {
                    if ((numCustomerSetsToReport < 0) || (exploredInfeasibleCustomerSets.TotalCount < numCustomerSetsToReport))
                        exploredInfeasibleCustomerSets.Add(cs);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("We should have shown the customer set feasible or infeasible!");
                }
            }
            if (feasible)
                unexploredCustomerSets.Add(cs);
        }


        public override void SpecializedReset()
        {
            //throw new NotImplementedException();
        }

        public override void SpecializedRun()
        {
            int numCustomerSetsToReport = algorithmParameters.GetParameter(ParameterID.ALG_NUM_CUSTOMER_SETS_TO_REPORT_PER_CATEGORY).GetIntValue();
            Random random = new Random(1);
            string[] csTripleSolveOutcome;

            //for(int i=0; i< numCustomerSetsToReport; i++)
            //{
            //    //Randomly generate a customer set
            //    CustomerSet cs = new CustomerSet(RandomlySelectASetOfCustomers(random));
            //    //Triple solve
            //    csTripleSolveOutcome = theProblemModel.TripleSolve(cs);
            //    //Place in the proper list
            //    allStats_formatted += Environment.NewLine + cs.NumberOfCustomers.ToString()
            //        + "\t" + Utils.StringOperations.CombineAndSpaceSeparateArray(cs.Customers.ToArray())
            //        + "\t" + csTripleSolveOutcome[0]
            //        + "\t" + csTripleSolveOutcome[1]
            //        + "\t" + csTripleSolveOutcome[2]
            //        + "\t" + csTripleSolveOutcome[3];
            //}


            int beamWidth = 1;// algorithmParameters.GetParameter(ParameterID.ALG_BEAM_WIDTH).GetIntValue();
            int currentLevel;
            int deepestPossibleLevel = theProblemModel.SRD.NumCustomers - 1;
            int nOptimizedCustomerSets = 0;
            while ((unexploredCustomerSets.TotalCount > 0) && (nOptimizedCustomerSets < numCustomerSetsToReport))
            {
                currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();

                while (currentLevel <= deepestPossibleLevel)
                {
                    //Take the parents from the current level
                    if (currentLevel > unexploredCustomerSets.GetDeepestNonemptyLevel())
                        break;
                    if (unexploredCustomerSets.CountByLevel()[currentLevel] == 0)
                        continue;
                    CustomerSet theParent = unexploredCustomerSets.Pop(currentLevel, beamWidth)[0];
                    List<string> possibleOtherCustomers = new List<string>();
                    foreach (string customerID in theParent.PossibleOtherCustomers)
                        possibleOtherCustomers.Add(customerID);

                    foreach (string customerID in possibleOtherCustomers)
                    {
                        CustomerSet candidate = new CustomerSet(theParent);
                        candidate.Extend(customerID);
                        theParent.MakeCustomerImpossible(customerID);

                        //OptimizeAndEvaluateForLists(candidate);
                        //Triple solve
                        csTripleSolveOutcome = theProblemModel.TripleSolve(candidate);
                        //Place in the proper list
                        if (csTripleSolveOutcome[0] != "Infeasible")
                            unexploredCustomerSets.Add(candidate);
                        allStats_formatted += Environment.NewLine + theProblemModel.InputFileName
                            + "\t" + candidate.NumberOfCustomers.ToString()
                            + "\t" + Utils.StringOperations.CombineAndSpaceSeparateArray(candidate.Customers.ToArray())
                            + "\t" + csTripleSolveOutcome[0]
                            + "\t" + csTripleSolveOutcome[1]
                            + "\t" + csTripleSolveOutcome[2]
                            + "\t" + csTripleSolveOutcome[3];

                        if (candidate.NumberOfCustomers >= algorithmParameters.GetParameter(ParameterID.ALG_MIN_NUM_CUSTOMERS_IN_A_SET).GetValue<int>())
                            nOptimizedCustomerSets++;

                        tripleSolveOutcomeStats.AddToData(candidate.NumberOfCustomers, csTripleSolveOutcome);
                    }//foreach (string customerID in remainingCustomers)

                    //end of the level, moving on to the next level
                    currentLevel++;
                }
            }//while(exploredInfeasibleCustomerSets.TotalCount< minNumInfeasibles)
        }

        List<string> RandomlySelectASetOfCustomers(Random rnd)
        {
            int n = rnd.Next(algorithmParameters.GetParameter(ParameterID.ALG_MIN_NUM_CUSTOMERS_IN_A_SET).GetValue<int>(), algorithmParameters.GetParameter(ParameterID.ALG_MAX_NUM_CUSTOMERS_IN_A_SET).GetValue<int>() + 1);

            List<string> allCustomerIDs = theProblemModel.SRD.GetCustomerIDs();
            List<string> outcome = new List<string>();
            while (outcome.Count < n)
            {
                string newCustID = allCustomerIDs[rnd.Next(0, allCustomerIDs.Count)];
                if (!outcome.Contains(newCustID))
                    outcome.Add(newCustID);
            }
            return outcome;
        }
    }

    class TripleSolveOutComeStatistics
    {
        SortedDictionary<Tuple<string, int>, List<double>> theData_GDV;
        SortedDictionary<Tuple<string, int>, List<double>> theData_EV_NDF;
        SortedDictionary<Tuple<string, int>, List<double>> theData_EV_ADF;

        internal TripleSolveOutComeStatistics()
        {
            theData_GDV = new SortedDictionary<Tuple<string, int>, List<double>>();
            theData_EV_NDF = new SortedDictionary<Tuple<string, int>, List<double>>();
            theData_EV_ADF = new SortedDictionary<Tuple<string, int>, List<double>>();
        }

        internal void AddToData(int nCustomers, string[] csTripleSolveOutcome)
        {
            Tuple<string, int> key = new Tuple<string, int>(csTripleSolveOutcome[0], nCustomers);
            if (!theData_GDV.ContainsKey(key))
            {
                theData_GDV.Add(key, new List<double>());
                theData_EV_NDF.Add(key, new List<double>());
                theData_EV_ADF.Add(key, new List<double>());
            }
            theData_GDV[key].Add(double.Parse(csTripleSolveOutcome[1]));
            theData_EV_NDF[key].Add(double.Parse(csTripleSolveOutcome[2]));
            theData_EV_ADF[key].Add(double.Parse(csTripleSolveOutcome[3]));
        }

        internal string GetSummaryData(string instance)
        {
            string outcome = instance+"\tStatus\t# Customers\t# Customer Sets (Sample Size)\tAvg. GDV Time\tAvg. EV NDF Time\tAvg. EV ADF";

            foreach(Tuple<string, int> key in theData_GDV.Keys)
            {
                outcome += Environment.NewLine + instance
                    + "\t" + key.Item1
                    + "\t" + key.Item2
                    + "\t" + theData_GDV[key].Count
                    + "\t" + theData_GDV[key].Average().ToString()
                    + "\t" + theData_EV_NDF[key].Average().ToString()
                    + "\t" + theData_EV_ADF[key].Average().ToString();
            }

            return outcome;
        }
    }
}
