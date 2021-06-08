using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Utils;
using System.Diagnostics;


namespace MPMFEVRP.Implementations.Algorithms
{
    class RandomizedCustomerSetExplorer : AlgorithmBase
    {
        Vehicle theGDV;
        Vehicle theEV;

        CustomerSetList.CustomerListPopStrategy popStrategy;

        double runtimeLimit = 0.0;
        Stopwatch sw = new Stopwatch();

        PartitionedCustomerSetList unexploredCustomerSets;
        PartitionedCustomerSetList exploredFeasibleCustomerSets;
        PartitionedCustomerSetList exploredInfeasibleCustomerSets;

        PartitionedCustomerSetList InfeasibleCustomerSets, OnlyGDVFeasibleCustomerSets, EVFeasibleCustomerSets;

        double avgTimePerGDVOptimalTSPSolution, avgTimePerGDVInfeasibleTSPSolution, avgTimePerEVOptimalTSPSolution, avgTimePerEVInfeasibleTSPSolution, avgTimePerTheOtherEVOptimalTSPSolution, avgTimePerTheOtherEVInfeasibleTSPSolution;
        TripleSolveOutComeStatistics tripleSolveOutcomeStats;
        string allStats_formatted;
        string orienteeringresults_formatted;

        List<CustomerSetWithVMTs> customerSetsWithVMTs;
        List<CustomerSetWithVMTs> CustomerSetsWithVMTs { get => customerSetsWithVMTs; }

        RandomSubsetOfCustomerSetsWithVMTs randomSubsetOfCustomerSetsWithVMTs;

        XCPlex_SetCovering_wSetOfCustomerSetswVMTs setCoveringModel;
        XCPlexParameters setCoverXCplexParameters;

        double customerSetSelectionProbability;
        bool usePricingRuntimeLimit;
        double pricingRuntimeLimit;
        bool compareToGDV_CG;
        bool compareToEV_NDF_CG;

        GDVvsAFV_OptimizationComparisonStatistics _OptimizationComparisonStatistics;

        public RandomizedCustomerSetExplorer() : base()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_NUM_CUSTOMER_SETS_TO_REPORT_PER_CATEGORY, "Minimum # of infeasible customer sets", new List<object>() { 1, 5, 10, 50, 100, 500, 1000 }, 1000, UserInputObjectType.TextBox));
            //algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_BEAM_WIDTH, "Beam width", 1));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_MIN_NUM_CUSTOMERS_IN_A_SET, "Minimum # of customers in a set", new List<object>() { 3, 4, 5 }, 4, UserInputObjectType.ComboBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_NUM_RANDOM_SUBSETS_OF_CUSTOMER_SETS, "Number of random subsets", new List<object>() { 1, 5, 10, 50, 100, 500, 1000 }, 100, UserInputObjectType.TextBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PROB_SELECTING_A_CUSTOMER_SET, "CS Selection Probability", new List<object>() { 0.01, 0.1, 0.5, 0.9, 0.99 },0.5, UserInputObjectType.ComboBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PROB_PRICING_USE_RUNTIME_LIMIT,"Use pricing problem runtime limit", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRICING_RUNTIME_LIMIT, "Runtime limit for the pricing problem solver(s)", new List<object>() { 1.0, 10.0, 30.0, 60.0, 600.0, 3600.0, 36000.0 }, 1.0, UserInputObjectType.ComboBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_COMPARE_TO_GDV_PRICINGPROBLEM, "Compare to GDV pricing (CG) problem?", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_COMPARE_TO_EV_NDF_PRICINGPROBLEM, "Compare to EV pricing (CG) problem w/ NDF model?", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));

        }

        public override string GetName()
        {
            return "Randomized Customer Set Explorer via Tree Search";
        }

        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>{
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
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

            list.Add(Environment.NewLine);
            list.Add(tripleSolveOutcomeStats.GetSummaryData(theProblemModel.InputFileName));

            list.Add(Environment.NewLine);
            list.Add(allStats_formatted);

            list.Add(Environment.NewLine);
            list.Add(orienteeringresults_formatted);

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
            //The following assumes no div/zero error is possible, will have to come back to make this code robust
            avgTimePerGDVOptimalTSPSolution = theProblemModel.GDV_TSP_TimeSpentAccount["Optimal"] / theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus["Optimal"];
            avgTimePerGDVInfeasibleTSPSolution = theProblemModel.GDV_TSP_TimeSpentAccount["Infeasible"] / theProblemModel.GDV_TSP_NumberOfCustomerSetsByStatus["Infeasible"];

            avgTimePerEVOptimalTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Optimal"] / theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus["Optimal"];
            avgTimePerEVInfeasibleTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Infeasible"] / theProblemModel.EV_TSP_NumberOfCustomerSetsByStatus["Infeasible"];

            avgTimePerTheOtherEVOptimalTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Optimal"] / theProblemModel.TheOtherEV_TSP_NumberOfCustomerSetsByStatus["Optimal"];
            avgTimePerTheOtherEVInfeasibleTSPSolution = theProblemModel.EV_TSP_TimeSpentAccount["Infeasible"] / theProblemModel.TheOtherEV_TSP_NumberOfCustomerSetsByStatus["Infeasible"];

            _OptimizationComparisonStatistics.WriteToFile(StringOperations.AppendToFilename(theProblemModel.InputFileName,"_OptimizationComparisonStatistics"));
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);
            theEV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);

            popStrategy = CustomerSetList.CustomerListPopStrategy.First;

            exploredFeasibleCustomerSets = new PartitionedCustomerSetList();
            exploredInfeasibleCustomerSets = new PartitionedCustomerSetList();

            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);
            PopulateAndPlaceInitialUnexploredCustomerSets();

            _OptimizationComparisonStatistics = new GDVvsAFV_OptimizationComparisonStatistics();
            foreach (CustomerSet cs in unexploredCustomerSets.ToCustomerSetList())
                _OptimizationComparisonStatistics.RecordObservation(cs.Customers.Count, cs.RouteOptimizationOutcome, cs.Customers);

            InfeasibleCustomerSets = new PartitionedCustomerSetList();
            OnlyGDVFeasibleCustomerSets = new PartitionedCustomerSetList();
            EVFeasibleCustomerSets = new PartitionedCustomerSetList();

            tripleSolveOutcomeStats = new TripleSolveOutComeStatistics();
            allStats_formatted = "instance\t# Customers\tCustomers\tCombined Status\tGDV time\tEV NDF time\tEV ADF time";

            customerSetsWithVMTs = new List<CustomerSetWithVMTs>();
            //The members of the "unexploredCustomerSets" will only be used as parents below, hence, we need to include them in customerSetsWithVMTs here
            foreach (CustomerSet unexpCS in unexploredCustomerSets.ToCustomerSetList())
                customerSetsWithVMTs.Add(new CustomerSetWithVMTs(unexpCS, unexpCS.RouteOptimizationOutcome.Status, vmt_GDV: unexpCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled(), vmt_EV: unexpCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetVehicleMilesTraveled()));//TODO This code is not robust, it assumes that each customer can be visited directly by either type of vehicles.

            setCoverXCplexParameters = new XCPlexParameters(relaxation: XCPlexRelaxation.LinearProgramming);

            runtimeLimit = algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();

            customerSetSelectionProbability = algorithmParameters.GetParameter(ParameterID.ALG_PROB_SELECTING_A_CUSTOMER_SET).GetDoubleValue();
            usePricingRuntimeLimit = algorithmParameters.GetParameter(ParameterID.ALG_PROB_PRICING_USE_RUNTIME_LIMIT).GetBoolValue();
            pricingRuntimeLimit = algorithmParameters.GetParameter(ParameterID.ALG_PRICING_RUNTIME_LIMIT).GetDoubleValue();
            compareToGDV_CG = algorithmParameters.GetParameter(ParameterID.ALG_COMPARE_TO_GDV_PRICINGPROBLEM).GetBoolValue();
            compareToEV_NDF_CG = algorithmParameters.GetParameter(ParameterID.ALG_COMPARE_TO_EV_NDF_PRICINGPROBLEM).GetBoolValue();

            orienteeringresults_formatted = Initialize_orienteeringresults_formatted();
        }
        string Initialize_orienteeringresults_formatted()
        {
            List<string> theList = new List<string>();
            theList.Add("instance");
            theList.Add("Observation #");
            if (compareToGDV_CG)
            {
                theList.Add("GDV:Status");
                theList.Add("GDV:CPUTime");
                theList.Add("GDV:ObjValue");
            }
            if (compareToEV_NDF_CG)
            {
                theList.Add("EV_NDF:Status");
                theList.Add("EV_NDF:CPUTime");
                theList.Add("EV_NDF:ObjValue");
            }
            theList.Add("EV_ADF:Status");
            theList.Add("EV_ADF:CPUTime");
            theList.Add("EV_ADF:ObjValue");

            return String.Join("\t", theList.ToArray());
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
            int beamWidth = 1;
            int currentLevel;
            int deepestPossibleLevel = theProblemModel.SRD.NumCustomers - 1;
            int nOptimizedCustomerSets = 0;
            sw = Stopwatch.StartNew();
            while ((unexploredCustomerSets.TotalCount > 0) && (nOptimizedCustomerSets < numCustomerSetsToReport) && (sw.Elapsed.TotalSeconds < runtimeLimit))
            {
                currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();

                while ((currentLevel <= deepestPossibleLevel) && (nOptimizedCustomerSets < numCustomerSetsToReport) && (sw.Elapsed.TotalSeconds < runtimeLimit))
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
                        if (sw.Elapsed.TotalSeconds > runtimeLimit)
                            break;
                        CustomerSet candidate = new CustomerSet(theParent);
                        candidate.Extend(customerID);
                        theParent.MakeCustomerImpossible(customerID);

                        OptimizeAndEvaluateForLists(candidate);
                        if (candidate.RouteOptimizationOutcome.Status == RouteOptimizationStatus.NotYetOptimized)
                            continue;

                        _OptimizationComparisonStatistics.RecordObservation(candidate.Customers.Count, candidate.RouteOptimizationOutcome, candidate.Customers);


                        if (candidate.NumberOfCustomers >= algorithmParameters.GetParameter(ParameterID.ALG_MIN_NUM_CUSTOMERS_IN_A_SET).GetValue<int>())
                        {
                            nOptimizedCustomerSets++;
                            if (nOptimizedCustomerSets >= numCustomerSetsToReport)
                                break;
                        }

                    }//foreach (string customerID in remainingCustomers)

                    //end of the level, moving on to the next level
                    currentLevel++;
                }
            }
        }

        RouteOptimizationStatus InterpretTripleSolutionStatus(string tripleSolutionStatus)
        {
            switch (tripleSolutionStatus)
            {
                case "Optimal":
                    return RouteOptimizationStatus.OptimizedForBothGDVandEV;
                case "Infeasible":
                    return RouteOptimizationStatus.InfeasibleForBothGDVandEV;
                case "GDVOnly":
                    return RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV;
                default:
                    throw new Exception("InterpretTripleSolutionStatus invoked with the wrong input!");
            }
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
