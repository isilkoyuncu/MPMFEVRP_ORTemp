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
    public class CGHwithShadowPrices : AlgorithmBase
    {
        //Algorithm parameters
        bool preserveCustomerVisitSequence = false;
        int randomSeed;
        Random random;
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

        public CGHwithShadowPrices()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE, "Preserve Customer Visit Sequence", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE, "TSP Type", new List<object>() { TSPSolverType.GDVExploiter, TSPSolverType.PlainAFVSolver, TSPSolverType.OldiesADF }, TSPSolverType.GDVExploiter, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.PROB_BKS, "BKS", ""));
        }
        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;

            //Algorithm param
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            preserveCustomerVisitSequence = AlgorithmParameters.GetParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE).GetBoolValue();
            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            tspSolverType = (TSPSolverType)AlgorithmParameters.GetParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE).Value;
            BKS = Double.Parse(GetBKS()); //AlgorithmParameters.GetParameter(ParameterID.PROB_BKS).GetDoubleValue();
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
            double obj = double.MaxValue;
            terminate = false;
            int currentTreeLevel = 0;
            CustomerSetList[] unexploredCustomerSetListsAtEachLevel = new CustomerSetList[theProblemModel.SRD.NumCustomers];
            CustomerSetList currentLevel = new CustomerSetList(exploredSingleCustomerSetList, false);
            unexploredCustomerSetListsAtEachLevel[0] = currentLevel;
            CustomerSet currentCS = new CustomerSet();
            while (!terminate)
            {
                localStartTime = DateTime.Now;
                currentLevel = new CustomerSetList();
                if (unexploredCustomerSetListsAtEachLevel[currentTreeLevel].Count == 0)
                {
                    currentTreeLevel = 0;
                }
                for (int k = 0; k < 5; k++)
                {
                    if (unexploredCustomerSetListsAtEachLevel[currentTreeLevel].Count > 0)
                    {
                        CustomerSet cs = unexploredCustomerSetListsAtEachLevel[currentTreeLevel][random.Next(unexploredCustomerSetListsAtEachLevel[currentTreeLevel].Count)];
                        Dictionary<string, double> negativeReducedCostCustomers = CalculateAndSortNegativeReducedCosts(cs, cs.PossibleOtherCustomers);
                        if (negativeReducedCostCustomers.Count > 0)
                        {
                            string customer2add = negativeReducedCostCustomers.ElementAt(random.Next(negativeReducedCostCustomers.Count)).Key;//random.Next(negativeReducedCostCustomers.Count)).Key;
                            CustomerSet tempExtendedCS = new CustomerSet(cs, theProblemModel, true);
                            tempExtendedCS.NewExtend(customer2add);
                            if (exploredCustomerSetMasterList.Includes(tempExtendedCS))
                                tempExtendedCS = exploredCustomerSetMasterList.Retrieve_CS(tempExtendedCS); //retrieve the info needed from exploredCustomerSetList                       
                            else
                            {
                                tempExtendedCS.OptimizeByExploitingGDVs(theProblemModel, true);
                                exploredCustomerSetMasterList.Add(tempExtendedCS);
                            }
                            UpdateFeasibilityStatus4EachVehicleCategory(tempExtendedCS);
                            if (feas4EV)
                            {
                                if (tempExtendedCS.PossibleOtherCustomers.Count != 0)
                                {
                                    cs = new CustomerSet(tempExtendedCS, theProblemModel, true);
                                    currentLevel.Add(cs);
                                }
                            }
                        }

                    }
                    else
                        break;
                }
                currentTreeLevel++;
                if (unexploredCustomerSetListsAtEachLevel[currentTreeLevel] == null)
                {
                    unexploredCustomerSetListsAtEachLevel[currentTreeLevel] = new CustomerSetList();
                }
                if (currentLevel.Count > 0)
                {                    
                    unexploredCustomerSetListsAtEachLevel[currentTreeLevel].AddRange(currentLevel);
                    solution = SetCover();
                }
                localFinishTime = DateTime.Now;
                if (runTimeLimitInSeconds < (localFinishTime - globalStartTime).TotalSeconds)
                    terminate = true;

                if (solution.Status == AlgorithmSolutionStatus.Optimal)
                {
                    if (obj > solution.UpperBound)
                    {
                        obj = solution.UpperBound;
                        allSolutions.Add(solution);
                        incumbentTime.Add((localFinishTime - globalStartTime).TotalSeconds);
                        if (obj < 3000)
                            terminate = false;
                        if (obj - BKS <= 0.01)
                            terminate = true;
                    }
                }
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
            else if (theProblemModel.InputFileName.Contains("AB110"))
                bestKnownSoln = 3612.95;
            else if (theProblemModel.InputFileName.Contains("AB111"))
                bestKnownSoln = 3996.96;
            else if (theProblemModel.InputFileName.Contains("AB112"))
                bestKnownSoln = 5487.87;
            else if (theProblemModel.InputFileName.Contains("AB113"))
                bestKnownSoln = 4804.62;
            else if (theProblemModel.InputFileName.Contains("AB114"))
                bestKnownSoln = 5324.17;
            else if (theProblemModel.InputFileName.Contains("AB115"))
                bestKnownSoln = 5035.35;
            else if (theProblemModel.InputFileName.Contains("AB116"))
                bestKnownSoln = 4511.64;
            else if (theProblemModel.InputFileName.Contains("AB117"))
                bestKnownSoln = 5370.28;
            else if (theProblemModel.InputFileName.Contains("AB118"))
                bestKnownSoln = 5756.88;
            else if (theProblemModel.InputFileName.Contains("AB119"))
                bestKnownSoln = 5599.96;
            else if (theProblemModel.InputFileName.Contains("AB120"))
                bestKnownSoln = 5679.81;
            else if (theProblemModel.InputFileName.Contains("U1"))
                bestKnownSoln = 1797.4946;
            else if (theProblemModel.InputFileName.Contains("U2"))
                bestKnownSoln = 1574.77981;
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
        Dictionary<string,double> CalculateAndSortNegativeReducedCosts(CustomerSet cs, List<string> possibleCustomersForCS)
        {
            List<string> remainingCustomers_list = possibleCustomersForCS;
            string[] remainingCustomers_array = remainingCustomers_list.ToArray();
            double[] bestEstimates = new double[remainingCustomers_array.Length];

            string candidate;
            for (int i = 0; i < remainingCustomers_array.Length; i++)
            {
                candidate = remainingCustomers_array[i];
                bestEstimates[i] = cs.MinAdditionalDistanceForPossibleOtherCustomer[candidate] - shadowPrices[candidate];
            }
            Array.Sort(bestEstimates, remainingCustomers_array);
            Dictionary<string,double> output = new Dictionary<string,double>();

            for (int i = 0; i < bestEstimates.Length; i++)
                if (bestEstimates[i] < 0.0)
                    output.Add(remainingCustomers_array[i], bestEstimates[i]);
            return output;
        }
        CustomerSetBasedSolution SetCover()
        {

            relaxedSetPartitionSolver = new XCPlex_SetCovering_wCustomerSets(theProblemModel, new XCPlexParameters(relaxation: XCPlexRelaxation.LinearProgramming), exploredCustomerSetMasterList, noGDVUnlimitedEV: true);
            relaxedSetPartitionSolver.Solve_and_PostProcess();
            shadowPrices = relaxedSetPartitionSolver.GetCustomerCoverageConstraintShadowPrices();

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
            return "CGH with Shadow Prices";

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
                output.Add(i.ToString() + "\t" + solution.UpperBound.ToString() + "\t" + solution.NumCS_assigned2EV.ToString() + "\t" + solution.NumCS_assigned2GDV.ToString() + "\t" + incumbentTime[i].ToString());
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
