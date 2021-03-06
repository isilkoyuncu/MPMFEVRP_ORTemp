﻿//using MPMFEVRP.Domains.AlgorithmDomain;
//using MPMFEVRP.Domains.SolutionDomain;
//using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.Solutions;
//using MPMFEVRP.Models;
//using MPMFEVRP.Models.XCPlex;
//using MPMFEVRP.SupplementaryInterfaces.Listeners;
//using System;
//using System.Collections.Generic;
//using MPMFEVRP.Domains.ProblemDomain;
//using System.Linq;
//using MPMFEVRP.Utils;

//namespace MPMFEVRP.Implementations.Algorithms
//{
//    public class CGA_RandomizedSetExplorerForProfitMaximization : AlgorithmBase
//    {
//        //Algorithm parameters
//        double epsilon = 0.01;
//        int totalColumnsNeededPerIteration = 0;
//        int columnsNeededPerCustomer = 2;
//        bool preserveCustomerVisitSequence = false;
//        int randomSeed;
//        private Random random;
//        Selection_Criteria selectedCriterion;
//        TSPSolverType tspSolverType;
//        double runTimeLimitInSeconds = 0.0;
//        List<string> allCustomerIDs;

//        XCPlexBase setPartitionSolver = null;
//        XCPlex_SetCovering_wCustomerSets relaxedSetPartitionSolver;
//        Dictionary<string, double> shadowPrices;
//        XCPlexParameters XcplexParam;

//        CustomerSetList columns2setCover;
//        CustomerSetList exploredCustomerSetMasterList;
//        CustomerSetList singleCustomerSetList;

//        CustomerSetBasedSolution solution = new CustomerSetBasedSolution();
//        List<CustomerSetBasedSolution> allSolutions;
//        CustomerSetBasedSolution incumbentSolution = new CustomerSetBasedSolution();
//        List<double> incumbentTime;
//        List<int> iterationNo;
//        string[] writtenOutput;
//        string[] writtenStatistics;
//        double BKS;

//        //Local statistics
//        double totalRunTimeSec = 0.0;
//        DateTime globalStartTime;
//        DateTime globalFinishTime;
//        DateTime localStartTime;
//        DateTime localFinishTime;


//        bool terminate;

//        public CGA_RandomizedSetExplorerForProfitMaximization()
//        {
//            AddSpecializedParameters();
//        }
//        public override void AddSpecializedParameters()
//        {
//            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE, "Preserve Customer Visit Sequence", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
//            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
//            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_NUM_COLUMNS_ADDED_PER_ITER, "Columns Added Per Iter", "50"));
//            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection, Selection_Criteria.UsingShadowPrices }, Selection_Criteria.UniformAmongTheBestPercentage, UserInputObjectType.ComboBox));
//            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE, "TSP Type", new List<object>() { TSPSolverType.GDVExploiter, TSPSolverType.PlainAFVSolver, TSPSolverType.OldiesADF }, TSPSolverType.GDVExploiter, UserInputObjectType.ComboBox));
//            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.PROB_BKS, "BKS", "0"));
//        }
//        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
//        {
//            //Problem param
//            this.theProblemModel = theProblemModel;
//            allCustomerIDs = theProblemModel.SRD.GetCustomerIDs();

//            //Algorithm param
//            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
//            preserveCustomerVisitSequence = AlgorithmParameters.GetParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE).GetBoolValue();
//            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
//            random = new Random(randomSeed);
//            totalColumnsNeededPerIteration = AlgorithmParameters.GetParameter(ParameterID.ALG_NUM_COLUMNS_ADDED_PER_ITER).GetIntValue();
//            selectedCriterion = (Selection_Criteria)AlgorithmParameters.GetParameter(ParameterID.ALG_SELECTION_CRITERIA).Value;
//            tspSolverType = (TSPSolverType)AlgorithmParameters.GetParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE).Value;
//            BKS = Double.Parse(GetBKS()); // AlgorithmParameters.GetParameter(ParameterID.PROB_BKS).GetDoubleValue();
//            XcplexParam = new XCPlexParameters();

//            shadowPrices = new Dictionary<string, double>();
//            columns2setCover = new CustomerSetList();
//            exploredCustomerSetMasterList = new CustomerSetList();
//            singleCustomerSetList = new CustomerSetList();

//            //Solution stat
//            status = AlgorithmSolutionStatus.NotYetSolved;
//            stats.UpperBound = double.MaxValue;
//            allSolutions = new List<CustomerSetBasedSolution>();
//            incumbentTime = new List<double>();
//            iterationNo = new List<int>();
//        }
//        public override void SpecializedRun()
//        {
//            int count = 0;//START ALGORITHM
//            //int k=0;
//            globalStartTime = DateTime.Now;
//            SetInitialColumns(); //INITIALIZE COLUMN GENERATION BY GENERATING SINGLE CUSTOMER ROUTES
//            RunIntegerSetCoverAndUpdateSolutionList(count);
//            terminate = false;
//            while (!terminate) //KEEP ADDING COLUMNS WHILE TERMINATION CRITERIA IS NOT SATISFIED
//            {
//                if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds) { terminate = true; break; }
//                localStartTime = DateTime.Now;
//                UpdateShadowPrices(exploredCustomerSetMasterList); //SOLVE THE RELAXED AND RESTRICTED MASTER PROBLEM; UPDATE SHADOW PRICES                
//                ExploreAndAddNewColumns(); //ADD PREDETERMINED NUMBER OF COLUMNS
//                localFinishTime = DateTime.Now;
//                count++;
//                //k++;
//                //if (k > 5) { RunIntegerSetCoverAndUpdateSolutionList(count); k = 0; } //CHECK IF IT IS TIME TO UPDATE THE INCUMBENT SOLUTION
//                RunIntegerSetCoverAndUpdateSolutionList(count);
//            }
//            RunIntegerSetCoverAndUpdateSolutionList(count++);   //UPDATE THE INCUMBENT ONE LAST TIME
//            globalFinishTime = DateTime.Now;
//            totalRunTimeSec = (globalFinishTime - globalStartTime).TotalSeconds; //FINISH ALGORITHM
//        }
//        public override void SpecializedConclude()
//        {
//            writtenOutput = WriteIncumbentSolutions();
//            string fileName = theProblemModel.InputFileName;
//            fileName = fileName.Replace(".txt", "output.txt");
//            System.IO.File.WriteAllLines(@fileName, writtenOutput);

//            writtenStatistics = WriteSolutionStatistics();
//            string fileName2 = theProblemModel.InputFileName;
//            fileName2 = fileName2.Replace(".txt", "statistics.txt");
//            System.IO.File.WriteAllLines(fileName2, writtenStatistics);

//            //Given that the instance is solved, we need to update status and statistics from it
//            status = (AlgorithmSolutionStatus)((int)setPartitionSolver.SolutionStatus);
//            stats.RunTimeMilliSeconds = (long)totalRunTimeSec;
//            stats.LowerBound = setPartitionSolver.LowerBound_XCPlex;
//            stats.UpperBound = setPartitionSolver.UpperBound_XCPlex;
//            GetOutputSummary();
//            //Create solution based on status: Not yet solved, infeasible, no feasible soln found, feasible, optimal
//            switch (status)
//            {
//                case AlgorithmSolutionStatus.NotYetSolved:
//                    {
//                        //Actual Run Time:N/A, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
//                        bestSolutionFound = new CustomerSetBasedSolution();
//                        break;
//                    }
//                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
//                    {
//                        //Actual Run Time:Report, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
//                        bestSolutionFound = new CustomerSetBasedSolution();
//                        break;
//                    }
//                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
//                    {
//                        //Actual Run Time=Limit:Report, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
//                        bestSolutionFound = new CustomerSetBasedSolution();
//                        break;
//                    }
//                case AlgorithmSolutionStatus.Feasible:
//                    {
//                        //Actual Run Time=Limit:Report, Complete Solution-LB:Report, Best Solution-UB:Report, Best Solution Found:Report
//                        bestSolutionFound = (CustomerSetBasedSolution)setPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
//                        break;
//                    }
//                case AlgorithmSolutionStatus.Optimal:
//                    {
//                        //Actual Run Time:Report<Limit, Complete Solution-LB = Best Solution-UB:Report, Best Solution Found:Report
//                        bestSolutionFound = (CustomerSetBasedSolution)setPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
//                        break;
//                    }
//                default:
//                    break;
//            }
//            bestSolutionFound.Status = status;
//            bestSolutionFound.UpperBound = setPartitionSolver.UpperBound_XCPlex;
//            bestSolutionFound.LowerBound = setPartitionSolver.LowerBound_XCPlex;
//            //_OptimizationComparisonStatistics.WriteToFile(StringOperations.AppendToFilename(theProblemModel.InputFileName, "_OptimizationComparisonStatistics"));
//        }
//        public override void SpecializedReset()
//        {
//            setPartitionSolver.ClearModel();
//            setPartitionSolver.Dispose();
//            setPartitionSolver.End();
//            GC.Collect();
//        }

//        //THESE ARE UPDATED METHODS
//        void SetInitialColumns()
//        {
//            for (int i = 0; i < theProblemModel.SRD.NumCustomers; i++)
//            {
//                CustomerSet singleCustomerCS = new CustomerSet(theProblemModel.GetAllCustomerIDs()[i], theProblemModel.GetAllCustomerIDs());
//                columns2setCover.Add(OptimizeCS(singleCustomerCS));
//            }
//            exploredCustomerSetMasterList.AddRange(columns2setCover);
//            singleCustomerSetList.AddRange(columns2setCover);
//        }
//        CustomerSet OptimizeCS(CustomerSet cs)
//        {
//            if (tspSolverType == TSPSolverType.GDVExploiter)
//                cs.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
//            else if (tspSolverType == TSPSolverType.PlainAFVSolver)
//                cs.OptimizeByPlainAFVSolver(theProblemModel);
//            else
//                cs.NewOptimize(theProblemModel);
//            return cs;
//        }
//        CustomerSetBasedSolution SetCover(CustomerSetList columnsToSetCover)
//        {
//            setPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, XcplexParam, columnsToSetCover);
//            setPartitionSolver.Solve_and_PostProcess();

//            CustomerSetBasedSolution outcome = (CustomerSetBasedSolution)setPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
//            if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.Feasible)
//                outcome.Status = AlgorithmSolutionStatus.Feasible;
//            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)
//                outcome.Status = AlgorithmSolutionStatus.Infeasible;
//            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.NoFeasibleSolutionFound)
//                outcome.Status = AlgorithmSolutionStatus.NoFeasibleSolutionFound;
//            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.NotYetSolved)
//                outcome.Status = AlgorithmSolutionStatus.NotYetSolved;
//            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.Optimal)
//                outcome.Status = AlgorithmSolutionStatus.Optimal;

//            outcome.UpperBound = setPartitionSolver.UpperBound_XCPlex;
//            outcome.LowerBound = setPartitionSolver.LowerBound_XCPlex;
//            return outcome;
//        }
//        void UpdateShadowPrices(CustomerSetList columnsToSetCover)
//        {
//            relaxedSetPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, new XCPlexParameters(relaxation: XCPlexRelaxation.LinearProgramming), columnsToSetCover);
//            relaxedSetPartitionSolver.Solve_and_PostProcess();
//            shadowPrices = relaxedSetPartitionSolver.GetCustomerCoverageConstraintShadowPrices();
//        }
//        void ExploreAndAddNewColumns()
//        {
//            CustomerSetList newColumns = new CustomerSetList();
//            List<string> sortedPotentialCustomerIDs = GetSortedPotentialCustomerIDs(); //SORT PROMISING CUSTOMERS IN DESCENDING SHADOW PRICES            
//            foreach (string csID in sortedPotentialCustomerIDs) //SELECT A PROMISING CUSTOMER
//            {
//                CustomerSet cs = new CustomerSet(csID, allCustomerIDs);
//                newColumns.AddRange(GetColumnsToBeAdded(cs));
//                if (newColumns.Count >= totalColumnsNeededPerIteration)
//                    break;
//            }
//            columns2setCover.AddRange(newColumns);
//        }
//        List<string> GetSortedPotentialCustomerIDs()
//        {
//            var temp = (from entry in shadowPrices orderby entry.Value descending select entry);
//            return temp.Where(x => x.Value >= 0).Select(x => x.Key).ToList();
//        }
//        CustomerSetList GetColumnsToBeAdded(string customerID) // columns to be added
//        {
//            double SPofCustomer = shadowPrices[customerID];
//            CustomerSetList tempColumns = new CustomerSetList();
//            List<double> tempReducedCosts = new List<double>();
//            List<int> numCustomersInCS = new List<int>();
//            List<double> keys = new List<double>();
//            //SELECT A PROMISING COLUMN TO ADD THAT CUSTOMER
//            foreach (CustomerSet cs in columns2setCover)
//            {
//                if (cs.Contains(customerID) || cs.ImpossibleOtherCustomers.Contains(customerID))
//                    continue;
//                else
//                {
//                    double estimatedReducedCost = cs.GetReducedCost()+100 - cs.GetLongestArc() - GetShadowPricesInCS(cs) - SPofCustomer + cs.GetShortestTwoArcsToCSfromCustomerID(customerID, theProblemModel);
//                    if (estimatedReducedCost > 0)
//                    {
//                        CustomerSet tempCS = new CustomerSet(cs);
//                        tempCS.NewExtend(customerID);
//                        tempColumns.Add(tempCS);
//                        tempReducedCosts.Add(estimatedReducedCost);
//                        numCustomersInCS.Add(tempCS.NumberOfCustomers);
//                    }
//                }
//            }
//            CustomerSet[] tempColsArray = tempColumns.ToArray();

//            if (tempReducedCosts.Count >= 2 && numCustomersInCS.Count >= 2)
//            {
//                double max_X = tempReducedCosts.Max();
//                double min_X = tempReducedCosts.Min();

//                double max_Y = numCustomersInCS.Max();
//                double min_Y = numCustomersInCS.Min();

//                for (int i = 0; i < tempReducedCosts.Count; i++)
//                {
//                    double coeff1 = 1 - (min_X - tempReducedCosts[i]) / (min_X - max_X);
//                    double coeff2 = (numCustomersInCS[i] - min_Y) / Math.Max(1.0, max_Y - min_Y);

//                    keys.Add(0.5 * coeff1 + 0.5 * coeff2);
//                }
//                double[] keysArray = keys.ToArray();
//                numCustomersInCS = tempColsArray.Select(x => x.NumberOfCustomers).ToList();//.ToArray();//.OrderByDescending(c => c).ToArray();

//                //Array.Sort(tempReducedCosts.ToArray(), tempColsArray);
//                //Array.Sort(numCustomersInCS, tempColsArray);
//                Array.Sort(keysArray, tempColsArray);
//                Array.Reverse(tempColsArray);
//            }
//            CustomerSetList columnsToAdd = new CustomerSetList();

//            //EXPLORE THE CUSTOMER + CUSTOMER SET PAIR USING THE SELECTED METHOD
//            foreach (CustomerSet potentialColumn in tempColsArray)
//            {
//                OptimizeCS(potentialColumn);
//                exploredCustomerSetMasterList.Add(potentialColumn);
//                if (potentialColumn.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV)
//                    if (potentialColumn.OFIDP.GetVMT(VehicleCategories.EV) - GetShadowPricesInCS(potentialColumn) < 0)
//                        columnsToAdd.Add(potentialColumn);
//                if (columnsToAdd.Count >= columnsNeededPerCustomer)
//                    break;
//            }
//            return columnsToAdd;
//        }
//        CustomerSetList GetColumnsToBeAdded(CustomerSet startingCS)
//        {
//            CustomerSet currentCS = new CustomerSet(startingCS);
//            CustomerSetList columnsToAdd = new CustomerSetList();
//            List<string> visitableCustomers = startingCS.PossibleOtherCustomers;
//            while (visitableCustomers.Count > 0)
//            {
//                CustomerSet tempCS = new CustomerSet(currentCS);
//                string customerID = SelectACustomer(visitableCustomers, tempCS);
//                tempCS.NewExtend(customerID);
//                OptimizeCS(tempCS);
//                exploredCustomerSetMasterList.Add(tempCS);
//                if (tempCS.RouteOptimizationOutcome.Status != RouteOptimizationStatus.InfeasibleForBothGDVandEV && tempCS.RouteOptimizationOutcome.Status != RouteOptimizationStatus.NotYetOptimized)
//                {
//                    if (tempCS.GetVehicleSpecificRouteOptimizationStatus(VehicleCategories.GDV) == VehicleSpecificRouteOptimizationStatus.Optimized)
//                    {
//                        columnsToAdd.Add(tempCS);
//                        currentCS = tempCS;
//                        visitableCustomers = currentCS.PossibleOtherCustomers;
//                    }
//                }
//                else
//                    visitableCustomers.Remove(customerID);
//            }
//            return columnsToAdd;
//        }
//        double GetShadowPricesInCS(CustomerSet cs)
//        {
//            double outcome = 0;
//            foreach (string id in cs.Customers)
//                outcome += shadowPrices[id];
//            return outcome;
//        }
//        void RunIntegerSetCoverAndUpdateSolutionList(int count)
//        {
//            solution = SetCover(exploredCustomerSetMasterList);
//            if (solution.Status == AlgorithmSolutionStatus.Optimal)
//            {
//                if (count == 0 || incumbentSolution.UpperBound < solution.UpperBound)
//                {
//                    allSolutions.Add(solution);
//                    iterationNo.Add(count);
//                    incumbentTime.Add((localFinishTime - globalStartTime).TotalSeconds);
//                    incumbentSolution = solution;
//                    if (BKS - solution.UpperBound <= epsilon)
//                        terminate = true;
//                }
//            }
//        }
//        string SelectACustomer(List<string> VisitableCustomers, CustomerSet currentCS)
//        {
//            string customerToAdd = VisitableCustomers[0];
//            switch (selectedCriterion)
//            {
//                case Selection_Criteria.CompleteUniform:
//                    return (VisitableCustomers[random.Next(VisitableCustomers.Count)]);

//                case Selection_Criteria.UniformAmongTheBestPercentage:
//                    List<string> theBestTopXPercent = new List<string>();
//                    theBestTopXPercent = PopulateTheBestTopXPercentCustomersList(currentCS, VisitableCustomers, 0.50);
//                    return (theBestTopXPercent[random.Next(theBestTopXPercent.Count)]);

//                case Selection_Criteria.WeightedNormalizedProbSelection:
//                    //We assume probabilities are proportional inverse distances
//                    double[] prob = new double[VisitableCustomers.Count];
//                    double probSum = 0.0;
//                    for (int c = 0; c < VisitableCustomers.Count; c++)
//                    {
//                        prob[c] = 1.0 / Math.Max(0.00001, ShortestDistanceOfCandidateToCurrentCustomerSet(currentCS, VisitableCustomers[c]));//Math.max used to eliminate the div by 0 error
//                        prob[c] = Math.Pow(prob[c], 2.0);
//                        probSum += prob[c];
//                    }
//                    for (int c = 0; c < VisitableCustomers.Count; c++)
//                        prob[c] /= probSum;
//                    return VisitableCustomers[Utils.RandomArrayOperations.Select(random.NextDouble(), prob)];

//                default:
//                    throw new Exception("The selection criterion sent to CustomerSet.Extend was not defined before!");
//            }
//        }
//        List<string> PopulateTheBestTopXPercentCustomersList(CustomerSet CS, List<string> VisitableCustomers, double closestPercentSelect)
//        {
//            string[] customers = VisitableCustomers.ToArray();
//            int nCustomers = customers.Length;
//            double[] shortestDistancesToCS = new double[nCustomers];
//            for (int i = 0; i < nCustomers; i++)
//            {
//                shortestDistancesToCS[i] = ShortestDistanceOfCandidateToCurrentCustomerSet(CS, customers[i]);
//            }
//            Array.Sort(shortestDistancesToCS, customers);

//            List<string> outcome = new List<string>();
//            int numberToReturn = (int)Math.Ceiling(closestPercentSelect * nCustomers / 100.0);
//            for (int i = 0; i < numberToReturn; i++)
//            {
//                outcome.Add(customers[i]);
//            }

//            return outcome;
//        }
//        double ShortestDistanceOfCandidateToCurrentCustomerSet(CustomerSet CS, string candidate)
//        {
//            if (CS.NumberOfCustomers == 0)
//                return theProblemModel.SRD.GetDistance(candidate, theProblemModel.SRD.GetSingleDepotID());
//            else
//            {
//                double outcome = double.MaxValue;
//                foreach (string customer in CS.Customers)
//                {
//                    double distance = theProblemModel.SRD.GetDistance(candidate, customer);
//                    if (outcome > distance)
//                    {
//                        outcome = distance;
//                    }
//                }
//                return outcome;
//            }
//        }
//        //DO NOT TOUCH THESE FOR NOW!!!
//        string GetBKS()
//        {
//            double bestKnownSoln;
//            if (theProblemModel.InputFileName.Contains("AB101"))
//                bestKnownSoln = 2566.62;
//            else if (theProblemModel.InputFileName.Contains("AB102"))
//                bestKnownSoln = 2876.26;
//            else if (theProblemModel.InputFileName.Contains("AB103"))
//                bestKnownSoln = 2804.07;
//            else if (theProblemModel.InputFileName.Contains("AB104"))
//                bestKnownSoln = 2634.17;
//            else if (theProblemModel.InputFileName.Contains("AB105"))
//                bestKnownSoln = 3939.96;
//            else if (theProblemModel.InputFileName.Contains("AB106"))
//                bestKnownSoln = 3915.15;
//            else if (theProblemModel.InputFileName.Contains("AB107"))
//                bestKnownSoln = 3732.97;
//            else if (theProblemModel.InputFileName.Contains("AB108"))
//                bestKnownSoln = 3672.4;
//            else if (theProblemModel.InputFileName.Contains("AB109"))
//                bestKnownSoln = 3722.17;
//            else if (theProblemModel.InputFileName.Contains("AB110"))
//                bestKnownSoln = 3612.95;
//            else if (theProblemModel.InputFileName.Contains("AB111"))
//                bestKnownSoln = 3996.96;
//            else if (theProblemModel.InputFileName.Contains("AB112"))
//                bestKnownSoln = 5487.87;
//            else if (theProblemModel.InputFileName.Contains("AB113"))
//                bestKnownSoln = 4804.62;
//            else if (theProblemModel.InputFileName.Contains("AB114"))
//                bestKnownSoln = 5324.17;
//            else if (theProblemModel.InputFileName.Contains("AB115"))
//                bestKnownSoln = 5035.35;
//            else if (theProblemModel.InputFileName.Contains("AB116"))
//                bestKnownSoln = 4511.64;
//            else if (theProblemModel.InputFileName.Contains("AB117"))
//                bestKnownSoln = 5370.28;
//            else if (theProblemModel.InputFileName.Contains("AB118"))
//                bestKnownSoln = 5756.88;
//            else if (theProblemModel.InputFileName.Contains("AB119"))
//                bestKnownSoln = 5599.96;
//            else if (theProblemModel.InputFileName.Contains("AB120"))
//                bestKnownSoln = 5679.81;
//            else if (theProblemModel.InputFileName.Contains("AB201"))
//                bestKnownSoln = 1836.25;
//            else if (theProblemModel.InputFileName.Contains("AB202"))
//                bestKnownSoln = 1966.82;
//            else if (theProblemModel.InputFileName.Contains("AB203"))
//                bestKnownSoln = 1921.59;
//            else if (theProblemModel.InputFileName.Contains("AB204"))
//                bestKnownSoln = 2001.7;
//            else if (theProblemModel.InputFileName.Contains("AB205"))
//                bestKnownSoln = 2793.01;
//            else if (theProblemModel.InputFileName.Contains("AB206"))
//                bestKnownSoln = 2891.48;
//            else if (theProblemModel.InputFileName.Contains("AB207"))
//                bestKnownSoln = 2717.34;
//            else if (theProblemModel.InputFileName.Contains("AB208"))
//                bestKnownSoln = 2552.18;
//            else if (theProblemModel.InputFileName.Contains("AB209"))
//                bestKnownSoln = 2517.69;
//            else if (theProblemModel.InputFileName.Contains("AB210"))
//                bestKnownSoln = 2479.97;
//            else if (theProblemModel.InputFileName.Contains("AB211"))
//                bestKnownSoln = 2977.73;
//            else if (theProblemModel.InputFileName.Contains("AB212"))
//                bestKnownSoln = 3341.43;
//            else if (theProblemModel.InputFileName.Contains("AB213"))
//                bestKnownSoln = 3133.24;
//            else if (theProblemModel.InputFileName.Contains("AB214"))
//                bestKnownSoln = 3384.28;
//            else if (theProblemModel.InputFileName.Contains("AB215"))
//                bestKnownSoln = 3480.52;
//            else if (theProblemModel.InputFileName.Contains("AB216"))
//                bestKnownSoln = 3221.78;
//            else if (theProblemModel.InputFileName.Contains("AB217"))
//                bestKnownSoln = 3714.94;
//            else if (theProblemModel.InputFileName.Contains("AB218"))
//                bestKnownSoln = 3658.17;
//            else if (theProblemModel.InputFileName.Contains("AB219"))
//                bestKnownSoln = 3790.71;
//            else if (theProblemModel.InputFileName.Contains("AB220"))
//                bestKnownSoln = 3737.88;
//            else if (theProblemModel.InputFileName.Contains("20c3sU1_"))
//                bestKnownSoln = 2000-(1797.49*0.5);
//            else if (theProblemModel.InputFileName.Contains("20c3sU2"))
//                bestKnownSoln = 1574.78;
//            else if (theProblemModel.InputFileName.Contains("20c3sU3"))
//                bestKnownSoln = 1704.48;
//            else if (theProblemModel.InputFileName.Contains("20c3sU4"))
//                bestKnownSoln = 1482.00;
//            else if (theProblemModel.InputFileName.Contains("20c3sU5"))
//                bestKnownSoln = 1689.37;
//            else if (theProblemModel.InputFileName.Contains("20c3sU6"))
//                bestKnownSoln = 1618.65;
//            else if (theProblemModel.InputFileName.Contains("20c3sU7"))
//                bestKnownSoln = 1713.66;
//            else if (theProblemModel.InputFileName.Contains("20c3sU8"))
//                bestKnownSoln = 1706.50;
//            else if (theProblemModel.InputFileName.Contains("20c3sU9"))
//                bestKnownSoln = 1708.82;
//            else if (theProblemModel.InputFileName.Contains("20c3sU10"))
//                bestKnownSoln = 1181.31;
//            else
//                throw new Exception("AB101-109 must be solved first.");
//            return bestKnownSoln.ToString();
//        }
//        public override string GetName()
//        {
//            return "CGA RandomizedSetExplorer For ProfitMaximization";

//        }
//        public override string[] GetOutputSummary()
//        {
//            List<string> list = new List<string>{
//                 //Algorithm Name has to be the first entry for output file name purposes
//                "Algorithm Name: " + GetName(),
//                //Run time limit has to be the second entry for output file name purposes
//                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
                
//                //Optional
//                "Parameter: ",
//                //algorithmParameters.GetAllParameters();
//                //var asString = string.Join(";", algorithmParameters.GetAllParameters());
//                //list.Add(asString);
                
//                //Necessary statistics
//                "CPU Run Time(sec): " + stats.RunTimeMilliSeconds.ToString(),
//                "Solution Status: " + status.ToString()
//            };
//            switch (status)
//            {
//                case AlgorithmSolutionStatus.NotYetSolved:
//                    {
//                        break;
//                    }
//                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
//                    {
//                        break;
//                    }
//                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
//                    {
//                        break;
//                    }
//                default:
//                    {
//                        list.Add("UB(Best Int): " + stats.UpperBound.ToString());
//                        list.Add("LB(Relaxed): " + stats.LowerBound.ToString());
//                        break;
//                    }
//            }
//            string[] toReturn = new string[list.Count];
//            toReturn = list.ToArray();
//            return toReturn;
//        }
//        public override bool setListener(IListener listener)
//        {
//            throw new NotImplementedException();
//        }
//        string[] WriteIncumbentSolutions()
//        {
//            List<string> output = new List<string>();
//            output.Add("ID#\tObjValue\tNumAFVUsed\tNumGDVUsed\tSolnTime\tIterationNo");
//            for (int i = 0; i < allSolutions.Count; i++)
//            {
//                CustomerSetBasedSolution solution = allSolutions[i];
//                output.Add(i.ToString() + "\t" + solution.UpperBound.ToString() + "\t" + solution.NumCS_assigned2EV.ToString() + "\t" + solution.NumCS_assigned2GDV.ToString() + "\t" + incumbentTime[i].ToString() + "\t" + iterationNo[i].ToString());
//            }
//            return output.ToArray();
//        }
//        string[] WriteSolutionStatistics()
//        {
//            List<string> output = new List<string>();
//            output.Add("ID#\tNumCustomers\tStatus\tObjFncDifference\tAFVSolnTime\tGDVSolnTime\tNumESVisits\tGDVExploited\tAFVSolved\tAFVInfeasibileProved");
//            int IDindex = 0;

//            foreach (CustomerSet cs in exploredCustomerSetMasterList)
//            {
//                double objDiff = 9999999;
//                double afvSolnTime = 0;
//                double gdvSolnTime = 0;
//                int numESVisits = 0;
//                int numCustomers = cs.Customers.Count;
//                bool gdvExploited = false;
//                bool afvSolved = true;
//                bool afvInfeasibleProved = false;

//                if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV)
//                {
//                    objDiff = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetVehicleMilesTraveled() - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled();
//                    numESVisits = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs.Count - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.NumberOfCustomersVisited;
//                    afvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
//                    gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
//                    if (afvSolnTime == 0)
//                    {
//                        gdvExploited = true;
//                        afvSolved = false;
//                    }
//                }
//                else if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
//                {
//                    if (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV) != null)
//                    {
//                        gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
//                    }
//                    afvInfeasibleProved = true;
//                    gdvExploited = true;
//                    afvSolved = false;
//                }
//                else if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)
//                {
//                    afvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
//                    gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
//                    if (afvSolnTime == 0)
//                    {
//                        gdvExploited = true;
//                        afvSolved = false;
//                        afvInfeasibleProved = true;
//                    }
//                }
//                output.Add(IDindex.ToString() + "\t" + numCustomers.ToString() + "\t" + cs.RouteOptimizationOutcome.Status.ToString() + "\t" + objDiff.ToString() + "\t" + afvSolnTime.ToString() + "\t" + gdvSolnTime.ToString() + "\t" + numESVisits.ToString() + "\t" + gdvExploited.ToString() + "\t" + afvSolved.ToString() + "\t" + afvInfeasibleProved.ToString());
//                IDindex++;
//            }
//            return output.ToArray();
//        }
//    }
//}
