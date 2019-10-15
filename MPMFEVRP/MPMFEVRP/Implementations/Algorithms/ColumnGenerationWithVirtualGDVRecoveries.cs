using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;
using MPMFEVRP.Domains.ProblemDomain;
using System.Linq;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class ColumnGenerationWithVirtualGDVRecoveries : AlgorithmBase
    {
        //Algorithm parameters
        int poolSize = 0;
        bool preserveCustomerVisitSequence = false;
        int randomSeed;
        Random random;
        Selection_Criteria selectedCriterion;
        double closestPercentSelect;
        double power;
        TSPSolverType tspSolverType;
        double runTimeLimitInSeconds = 0.0;

        XCPlexBase setPartitionSolver = null;
        XCPlex_SetCovering_wCustomerSets relaxedSetPartitionSolver;
        Dictionary<string, double> shadowPrices;
        XCPlexParameters XcplexParam;

        CustomerSetList exploredCustomerSetMasterList;
        CustomerSetList exploredSingleCustomerSetList;

        CustomerSetBasedSolution solution = new CustomerSetBasedSolution();
        List<CustomerSetBasedSolution> allSolutions;
        List<double> incumbentTime;
        List<int> iterationNo;
        string[] writtenOutput;
        string[] writtenStatistics;
        double BKS;

        bool feas4EV = true;

        //Local statistics
        double totalRunTimeSec = 0.0;
        DateTime globalStartTime;
        DateTime globalFinishTime;
        DateTime localStartTime;
        DateTime localFinishTime;

        bool terminate;

        public ColumnGenerationWithVirtualGDVRecoveries()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_POOL_SIZE, "Random Pool Size", "600"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE, "Preserve Customer Visit Sequence", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection, Selection_Criteria.UsingShadowPrices }, Selection_Criteria.WeightedNormalizedProbSelection, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT, "% Customers 2 Select", new List<object>() { 5, 10, 15, 20, 25, 30, 50 }, 20, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PROB_SELECTION_POWER, "Power", "2.0"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE, "TSP Type", new List<object>() { TSPSolverType.GDVExploiter, TSPSolverType.PlainAFVSolver, TSPSolverType.OldiesADF }, TSPSolverType.GDVExploiter, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.PROB_BKS, "BKS", "0"));
        }
        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;

            //Algorithm param
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            poolSize = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_POOL_SIZE).GetIntValue();
            preserveCustomerVisitSequence = AlgorithmParameters.GetParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE).GetBoolValue();
            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            selectedCriterion = (Selection_Criteria)AlgorithmParameters.GetParameter(ParameterID.ALG_SELECTION_CRITERIA).Value;
            closestPercentSelect = AlgorithmParameters.GetParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT).GetIntValue();
            power = AlgorithmParameters.GetParameter(ParameterID.ALG_PROB_SELECTION_POWER).GetDoubleValue();
            tspSolverType = (TSPSolverType)AlgorithmParameters.GetParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE).Value;
            BKS = Double.Parse(GetBKS());//AlgorithmParameters.GetParameter(ParameterID.PROB_BKS).GetDoubleValue();
            XcplexParam = new XCPlexParameters();

            exploredCustomerSetMasterList = new CustomerSetList();
            exploredSingleCustomerSetList = new CustomerSetList();

            //Solution stat
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            allSolutions = new List<CustomerSetBasedSolution>();
            incumbentTime = new List<double>();
            iterationNo = new List<int>();
        }
        public override void SpecializedRun()
        {
            globalStartTime = DateTime.Now;
            for (int i = 0; i < theProblemModel.SRD.NumCustomers; i++)
            {
                CustomerSet singleCustomerCS = new CustomerSet(theProblemModel.GetAllCustomerIDs()[i], theProblemModel.GetAllCustomerIDs());
                if (tspSolverType == TSPSolverType.GDVExploiter)
                    singleCustomerCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                else if (tspSolverType == TSPSolverType.PlainAFVSolver)
                    singleCustomerCS.OptimizeByPlainAFVSolver(theProblemModel);
                else
                    singleCustomerCS.NewOptimize(theProblemModel);
                exploredSingleCustomerSetList.Add(singleCustomerCS);
            }
            exploredCustomerSetMasterList.AddRange(exploredSingleCustomerSetList);
            solution = SetCover();            
            int count = 0;
            double obj = double.MaxValue;
            terminate = false;
            while (count < poolSize && !terminate)
            {
                localStartTime = DateTime.Now;
                List<string> visitableCustomers = theProblemModel.GetAllCustomerIDs(); //finish a solution when there is no customer in visitableCustomers set
                CustomerSet currentCS = new CustomerSet(exploredSingleCustomerSetList[random.Next(exploredSingleCustomerSetList.Count)],theProblemModel,true);//new CustomerSet(exploredSingleCustomerSetList[count % exploredSingleCustomerSetList.Count], theProblemModel, true);//Start a new "customer set" <- route
                while (visitableCustomers.Count > 0 && !terminate)
                {
                    List<string> possibleCustomersForCS = GetPossibleCustomersForCS(visitableCustomers, currentCS); //finish a route when there is no customer left in the visitableCustomersForCS
                    while (possibleCustomersForCS.Count > 0)
                    {
                        CustomerSet tempExtendedCS;
                        if (currentCS.Customers.Count == 0)
                        {
                            tempExtendedCS = new CustomerSet();
                        }
                        else
                        {
                            tempExtendedCS = new CustomerSet(currentCS, theProblemModel, true);
                        }
                        string customer2add = SelectACustomer(possibleCustomersForCS, currentCS); //Extend the CS with one customer from visitableCustomersForCS; here k plays a role for randomness
                        tempExtendedCS.NewExtend(customer2add);
                        if (exploredCustomerSetMasterList.Includes(tempExtendedCS))
                        {
                            tempExtendedCS = exploredCustomerSetMasterList.Retrieve_CS(tempExtendedCS); //retrieve the info needed from exploredCustomerSetList
                        }
                        else
                        {
                            if (tspSolverType == TSPSolverType.GDVExploiter)
                                tempExtendedCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                            else if (tspSolverType == TSPSolverType.PlainAFVSolver)
                                tempExtendedCS.OptimizeByPlainAFVSolver(theProblemModel);
                            else
                                tempExtendedCS.NewOptimize(theProblemModel);
                            exploredCustomerSetMasterList.Add(tempExtendedCS);
                        }
                        UpdateFeasibilityStatus4EachVehicleCategory(tempExtendedCS);

                        if (feas4EV)
                        {
                            currentCS = new CustomerSet(tempExtendedCS, theProblemModel, true);
                            possibleCustomersForCS = GetPossibleCustomersForCS(possibleCustomersForCS, currentCS);
                        }
                        else
                        {
                            if (tempExtendedCS.PossibleOtherCustomers.Count == 0)
                                possibleCustomersForCS = new List<string>();
                            else
                                possibleCustomersForCS.Remove(customer2add);
                        }
                        if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds)
                        {
                            terminate = true;
                            break;
                        }
                    }
                    visitableCustomers = visitableCustomers.Except(currentCS.Customers).ToList();
                    currentCS = new CustomerSet();//Start a new "customer set" <- route
                }
                solution = SetCover();
                localFinishTime = DateTime.Now;
                if (solution.Status == AlgorithmSolutionStatus.Optimal)
                {
                    if (obj > solution.UpperBound)
                    {
                        obj = solution.UpperBound;
                        allSolutions.Add(solution);
                        iterationNo.Add(count);
                        incumbentTime.Add((localFinishTime - globalStartTime).TotalSeconds);
                        if (obj - BKS <= 0.01)
                            terminate = true;
                    }
                }
                count++;
            }
            globalFinishTime = DateTime.Now;
            totalRunTimeSec = (globalFinishTime - globalStartTime).TotalSeconds;
        }
        public override void SpecializedConclude()
        {
            writtenOutput = WriteIncumbentSolutions();
            string fileName = theProblemModel.InputFileName;
            fileName = fileName.Replace(".txt", "output.txt");
            System.IO.File.WriteAllLines(@fileName, writtenOutput);

            writtenStatistics = WriteSolutionStatistics();
            string fileName2 = theProblemModel.InputFileName;
            fileName2 = fileName2.Replace(".txt", "statistics.txt");
            System.IO.File.WriteAllLines(fileName2, writtenStatistics);

            //Given that the instance is solved, we need to update status and statistics from it
            status = (AlgorithmSolutionStatus)((int)setPartitionSolver.SolutionStatus);
            stats.RunTimeMilliSeconds = (long)totalRunTimeSec;
            stats.LowerBound = setPartitionSolver.LowerBound_XCPlex;
            stats.UpperBound = setPartitionSolver.UpperBound_XCPlex;
            GetOutputSummary();
            //Create solution based on status: Not yet solved, infeasible, no feasible soln found, feasible, optimal
            switch (status)
            {
                case AlgorithmSolutionStatus.NotYetSolved:
                    {
                        //Actual Run Time:N/A, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
                        bestSolutionFound = new CustomerSetBasedSolution();
                        break;
                    }
                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
                    {
                        //Actual Run Time:Report, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
                        bestSolutionFound = new CustomerSetBasedSolution();
                        break;
                    }
                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
                    {
                        //Actual Run Time=Limit:Report, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
                        bestSolutionFound = new CustomerSetBasedSolution();
                        break;
                    }
                case AlgorithmSolutionStatus.Feasible:
                    {
                        //Actual Run Time=Limit:Report, Complete Solution-LB:Report, Best Solution-UB:Report, Best Solution Found:Report
                        bestSolutionFound = (CustomerSetBasedSolution)setPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
                        break;
                    }
                case AlgorithmSolutionStatus.Optimal:
                    {
                        //Actual Run Time:Report<Limit, Complete Solution-LB = Best Solution-UB:Report, Best Solution Found:Report
                        bestSolutionFound = (CustomerSetBasedSolution)setPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
                        break;
                    }
                default:
                    break;
            }
            bestSolutionFound.Status = status;
            bestSolutionFound.UpperBound = setPartitionSolver.UpperBound_XCPlex;
            bestSolutionFound.LowerBound = setPartitionSolver.LowerBound_XCPlex;
            //_OptimizationComparisonStatistics.WriteToFile(StringOperations.AppendToFilename(theProblemModel.InputFileName, "_OptimizationComparisonStatistics"));
        }
        string GetBKS()
        {
            double bestKnownSoln;
            if (theProblemModel.InputFileName.Contains("AB101"))
                bestKnownSoln = 2566.62;
            else if (theProblemModel.InputFileName.Contains("AB102"))
                bestKnownSoln = 2876.26;
            else if (theProblemModel.InputFileName.Contains("AB103"))
                bestKnownSoln = 2804.07;
            else if (theProblemModel.InputFileName.Contains("AB104"))
                bestKnownSoln = 2634.17;
            else if (theProblemModel.InputFileName.Contains("AB105"))
                bestKnownSoln = 3939.96;
            else if (theProblemModel.InputFileName.Contains("AB106"))
                bestKnownSoln = 3915.15;
            else if (theProblemModel.InputFileName.Contains("AB107"))
                bestKnownSoln = 3732.97;
            else if (theProblemModel.InputFileName.Contains("AB108"))
                bestKnownSoln = 3672.4;
            else if (theProblemModel.InputFileName.Contains("AB109"))
                bestKnownSoln = 3722.17;
            else
                throw new Exception("AB101-109 must be solved first.");
            return bestKnownSoln.ToString();
        }
        public override void SpecializedReset()
        {
            setPartitionSolver.ClearModel();
            setPartitionSolver.Dispose();
            setPartitionSolver.End();
            GC.Collect();
        }
        string SelectACustomer(List<string> possibleCustomersForCS, CustomerSet currentCS)
        {
            switch (selectedCriterion)
            {
                case Selection_Criteria.CompleteUniform:
                    return (possibleCustomersForCS[random.Next(possibleCustomersForCS.Count)]);

                case Selection_Criteria.UniformAmongTheBestPercentage:
                    List<string> theBestTopXPercent = new List<string>();
                    theBestTopXPercent = PopulateTheBestTopXPercentCustomersList(currentCS, possibleCustomersForCS, closestPercentSelect);
                    return (theBestTopXPercent[random.Next(theBestTopXPercent.Count)]);

                case Selection_Criteria.WeightedNormalizedProbSelection:
                    //We assume probabilities are proportional inverse distances
                    double[] prob = new double[possibleCustomersForCS.Count];
                    double probSum = 0.0;
                    for (int c = 0; c < possibleCustomersForCS.Count; c++)
                    {
                        prob[c] = 1.0 / Math.Max(0.00001, ShortestDistanceOfCandidateToCurrentCustomerSet(currentCS, possibleCustomersForCS[c]));//Math.max used to eliminate the div by 0 error
                        prob[c] = Math.Pow(prob[c], power);
                        probSum += prob[c];
                    }
                    for (int c = 0; c < possibleCustomersForCS.Count; c++)
                        prob[c] /= probSum;
                    return possibleCustomersForCS[Utils.RandomArrayOperations.Select(random.NextDouble(), prob)];

                case Selection_Criteria.UsingShadowPrices:
                    if (currentCS.Customers.Count == 0)
                    {
                        return (possibleCustomersForCS[random.Next(possibleCustomersForCS.Count)]);
                    }
                    else
                    {
                        List<string> mostNegativeReducedCostCustomers = CalculateAndSortNegativeReducedCosts(currentCS, possibleCustomersForCS);
                        if (mostNegativeReducedCostCustomers.Count == 0)
                            return (possibleCustomersForCS[random.Next(possibleCustomersForCS.Count)]);
                        else
                            return (mostNegativeReducedCostCustomers[random.Next(mostNegativeReducedCostCustomers.Count)]);
                    }
                default:
                    throw new Exception("The selection criterion sent to CustomerSet.Extend was not defined before!");
            }

        }
        List<string> PopulateTheBestTopXPercentCustomersList(CustomerSet CS, List<string> possibleCustomersForCS, double closestPercentSelect)
        {
            string[] customers = possibleCustomersForCS.ToArray();
            int nCustomers = customers.Length;
            double[] shortestDistancesToCS = new double[nCustomers];
            for (int i = 0; i < nCustomers; i++)
            {
                shortestDistancesToCS[i] = ShortestDistanceOfCandidateToCurrentCustomerSet(CS, customers[i]);
            }
            Array.Sort(shortestDistancesToCS, customers);

            List<string> outcome = new List<string>();
            int numberToReturn = (int)Math.Ceiling(closestPercentSelect * nCustomers / 100.0);
            for (int i = 0; i < numberToReturn; i++)
            {
                outcome.Add(customers[i]);
            }

            return outcome;
        }

        double ShortestDistanceOfCandidateToCurrentCustomerSet(CustomerSet CS, string candidate)
        {
            if (CS.NumberOfCustomers == 0)
                return theProblemModel.SRD.GetDistance(candidate, theProblemModel.SRD.GetSingleDepotID());
            else
            {
                double outcome = double.MaxValue;
                foreach (string customer in CS.Customers)
                {
                    double distance = theProblemModel.SRD.GetDistance(candidate, customer);
                    if (outcome > distance)
                    {
                        outcome = distance;
                    }
                }
                return outcome;
            }
        }
        List<string> GetPossibleCustomersForCS(List<string> VisitableCustomers, CustomerSet currentCS)
        {
            List<string> visitableCustomersForCS = VisitableCustomers;
            visitableCustomersForCS = visitableCustomersForCS.Except(currentCS.ImpossibleOtherCustomers).ToList();
            visitableCustomersForCS = visitableCustomersForCS.Except(currentCS.Customers).ToList();
            return visitableCustomersForCS;
        }
        void UpdateFeasibilityStatus4EachVehicleCategory(CustomerSet tempExtendedCS)
        {
            if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV)
            {
                feas4EV = true;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
            {
                feas4EV = false;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)
            {
                feas4EV = false;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.NotYetOptimized)
                throw new Exception("Column Generation with Refueling Path Recoveries - New Extend function also optimizes the extended customer set however we get not yet optimized here");
            else
                throw new Exception("ExtendedCSIsFeasibleForDesiredVehicleCategory ran into an undefined case of RouteOptimizationStatus!");

        }
        List<string> CalculateAndSortNegativeReducedCosts(CustomerSet cs, List<string> possibleCustomersForCS)
        {
            List<string> remainingCustomers_list = possibleCustomersForCS;
            string[] remainingCustomers_array = remainingCustomers_list.ToArray();
            double[] bestEstimates = new double[remainingCustomers_array.Length];

            string candidate;
            for (int i = 0; i < remainingCustomers_array.Length; i++)
            {
                candidate = remainingCustomers_array[i];
                bestEstimates[i] = ShortestDistanceOfCandidateToCurrentCustomerSet(cs, candidate) - shadowPrices[candidate];
            }
            Array.Sort(bestEstimates, remainingCustomers_array);
            List<string> output = new List<string>();

            for (int i = 0; i < bestEstimates.Length; i++)
                if (bestEstimates[i] < 0.0)
                    output.Add(remainingCustomers_array[i]);
            return output;
        }
        CustomerSetBasedSolution SetCover()
        {
            if (selectedCriterion == Selection_Criteria.UsingShadowPrices)
            {
                relaxedSetPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, new XCPlexParameters(relaxation: XCPlexRelaxation.LinearProgramming), exploredCustomerSetMasterList, noGDVUnlimitedEV: true);
                relaxedSetPartitionSolver.Solve_and_PostProcess();
                shadowPrices = relaxedSetPartitionSolver.GetCustomerCoverageConstraintShadowPrices();
            }
            setPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, XcplexParam, exploredCustomerSetMasterList);
            setPartitionSolver.Solve_and_PostProcess();
            CustomerSetBasedSolution outcome = (CustomerSetBasedSolution)setPartitionSolver.GetCompleteSolution(typeof(CustomerSetBasedSolution));
            if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.Feasible)
                outcome.Status = AlgorithmSolutionStatus.Feasible;
            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)
                outcome.Status = AlgorithmSolutionStatus.Infeasible;
            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.NoFeasibleSolutionFound)
                outcome.Status = AlgorithmSolutionStatus.NoFeasibleSolutionFound;
            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.NotYetSolved)
                outcome.Status = AlgorithmSolutionStatus.NotYetSolved;
            else if (setPartitionSolver.SolutionStatus == XCPlexSolutionStatus.Optimal)
                outcome.Status = AlgorithmSolutionStatus.Optimal;
            outcome.UpperBound = setPartitionSolver.UpperBound_XCPlex;
            outcome.LowerBound = setPartitionSolver.LowerBound_XCPlex;
            return outcome;
        }
        public override string GetName()
        {
            return "Randomized Column Generation with Refueling Path Insert";

        }
        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>{
                 //Algorithm Name has to be the first entry for output file name purposes
                "Algorithm Name: " + GetName()+ "-" +algorithmParameters.GetParameter(ParameterID.ALG_RANDOM_POOL_SIZE).Value.ToString(),
                //Run time limit has to be the second entry for output file name purposes
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
                
                //Optional
                "Parameter: ",
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
            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
        }
        public override bool setListener(IListener listener)
        {
            throw new NotImplementedException();
        }
        string[] WriteIncumbentSolutions()
        {
            List<string> output = new List<string>();
            output.Add("ID#\tObjValue\tNumAFVUsed\tNumGDVUsed\tSolnTime\tIterationNo");
            for (int i = 0; i < allSolutions.Count; i++)
            {
                CustomerSetBasedSolution solution = allSolutions[i];
                output.Add(i.ToString() + "\t" + solution.UpperBound.ToString() + "\t" + solution.NumCS_assigned2EV.ToString() + "\t" + solution.NumCS_assigned2GDV.ToString() + "\t" + incumbentTime[i].ToString() + "\t" + iterationNo[i].ToString());
            }
            return output.ToArray();
        }
        string[] WriteSolutionStatistics()
        {
            List<string> output = new List<string>();
            output.Add("ID#\tNumCustomers\tStatus\tObjFncDifference\tAFVSolnTime\tGDVSolnTime\tNumESVisits\tGDVExploited\tAFVSolved\tAFVInfeasibileProved");
            int IDindex = 0;

            foreach (CustomerSet cs in exploredCustomerSetMasterList)
            {
                double objDiff = 9999999;
                double afvSolnTime = 0;
                double gdvSolnTime = 0;
                int numESVisits = 0;
                int numCustomers = cs.Customers.Count;
                bool gdvExploited = false;
                bool afvSolved = true;
                bool afvInfeasibleProved = false;

                if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV)
                {
                    objDiff = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetVehicleMilesTraveled() - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled();
                    numESVisits = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs.Count - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.NumberOfCustomersVisited;
                    afvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
                    gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                    if (afvSolnTime == 0)
                    {
                        gdvExploited = true;
                        afvSolved = false;
                    }
                }
                else if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                {
                    if (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV) != null)
                    {
                        gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                    }
                    afvInfeasibleProved = true;
                    gdvExploited = true;
                    afvSolved = false;
                }
                else if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)
                {
                    afvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
                    gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                    if (afvSolnTime == 0)
                    {
                        gdvExploited = true;
                        afvSolved = false;
                        afvInfeasibleProved = true;
                    }
                }
                output.Add(IDindex.ToString() + "\t" + numCustomers.ToString() + "\t" + cs.RouteOptimizationOutcome.Status.ToString() + "\t" + objDiff.ToString() + "\t" + afvSolnTime.ToString() + "\t" + gdvSolnTime.ToString() + "\t" + numESVisits.ToString() + "\t" + gdvExploited.ToString() + "\t" + afvSolved.ToString() + "\t" + afvInfeasibleProved.ToString());
                IDindex++;
            }
            return output.ToArray();
        }
    }
}
