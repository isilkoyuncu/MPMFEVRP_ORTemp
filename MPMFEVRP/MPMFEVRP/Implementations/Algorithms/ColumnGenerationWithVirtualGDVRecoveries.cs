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
        //GDVvsAFV_OptimizationComparisonStatistics _OptimizationComparisonStatistics;

        //Algorithm parameters
        int poolSize = 0;
        bool preserveCustomerVisitSequence = false;
        int randomSeed;
        Random random;
        Selection_Criteria selectedCriterion;
        double closestPercentSelect;
        double power;
        double runTimeLimitInSeconds = 0.0;
        double searchTimeLimit = 0.0;

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam;

        CustomerSetList exploredCustomerSetMasterList;
        CustomerSetList exploredSingleCustomerSetList;

        List<VehicleSpecificRouteOptimizationOutcome> vsrooList_GDV = new List<VehicleSpecificRouteOptimizationOutcome>();
        List<VehicleSpecificRouteOptimizationOutcome> vsrooList_EV = new List<VehicleSpecificRouteOptimizationOutcome>();

        CustomerSetBasedSolution solution = new CustomerSetBasedSolution();
        List<CustomerSetBasedSolution> allSolutions;

        bool previousNotEVFeasible = false;
        int countInfeasible = 0;
        double extensionTime = 0.0;
        DateTime extendStartTime;
        DateTime extendFinishTime;

        bool feas4EV = true;
        bool feas4GDV = true;

        //Local statistics
        double totalTimeSpentExtendingCustSet;
        //double totalCompTimeBeforeSetCover;
        //double totalReassignmentTime_GDV2EV;
        double totalRunTimeSec = 0.0;
        DateTime globalStartTime;
        DateTime globalFinishTime;
        List<double>[] iterationTime;

        public ColumnGenerationWithVirtualGDVRecoveries()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_POOL_SIZE, "Random Pool Size", "300"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE, "Preserve Customer Visit Sequence", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection }, Selection_Criteria.UniformAmongTheBestPercentage, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT, "% Customers 2 Select", new List<object>() { 5, 10, 15, 20, 25, 30, 50 }, 20, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PROB_SELECTION_POWER, "Power", "2.0"));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SEARCH_TILIM, "Infeasible Search Tilim", "30"));
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;
            
            //Algorithm param
            poolSize = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_POOL_SIZE).GetIntValue();
            preserveCustomerVisitSequence = algorithmParameters.GetParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE).GetBoolValue();
            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            selectedCriterion = (Selection_Criteria)AlgorithmParameters.GetParameter(ParameterID.ALG_SELECTION_CRITERIA).Value;
            closestPercentSelect = AlgorithmParameters.GetParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT).GetIntValue();
            power = AlgorithmParameters.GetParameter(ParameterID.ALG_PROB_SELECTION_POWER).GetDoubleValue();
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            searchTimeLimit = AlgorithmParameters.GetParameter(ParameterID.ALG_SEARCH_TILIM).GetIntValue();
            XcplexParam = new XCPlexParameters();

            exploredCustomerSetMasterList = new CustomerSetList();
            exploredSingleCustomerSetList = new CustomerSetList();

            vsrooList_GDV = new List<VehicleSpecificRouteOptimizationOutcome>();
            vsrooList_EV = new List<VehicleSpecificRouteOptimizationOutcome>();

            //Solution stat
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            allSolutions = new List<CustomerSetBasedSolution>();

            iterationTime = new List<double>[3];
            for (int i = 0; i < 3; i++)
                iterationTime[i] = new List<double>();
            totalTimeSpentExtendingCustSet = 0.0;
            //totalCompTimeBeforeSetCover = 0.0;
            //totalReassignmentTime_GDV2EV = 0.0;            
        }

        public override void SpecializedRun()
        {
            globalStartTime = DateTime.Now;
            for (int i = 0; i < theProblemModel.SRD.NumCustomers; i++)
            {
                CustomerSet currentCS = new CustomerSet(theProblemModel.GetAllCustomerIDs()[i], theProblemModel.GetAllCustomerIDs());
                currentCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                exploredSingleCustomerSetList.Add(currentCS);
            }
            exploredCustomerSetMasterList.AddRange(exploredSingleCustomerSetList);
            int k = 0;
            int index = 0;
            do //this is kind of the number of random starting points, in order not to stuck in a local optima
            {
                random = new Random(randomSeed + k);
                //Start with an empty customer set and customers2visit = all customers
                List<string> visitableCustomers = theProblemModel.GetAllCustomerIDs(); //finish a solution when there is no customer in visitableCustomers set  
                do //(visitableCustomers.Count > 0)
                {
                    CustomerSet currentCS = new CustomerSet(exploredSingleCustomerSetList[index]);//Start a new "customer set" <- route
                    List<string> visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomers, currentCS); //finish a route when there is no customer left in the visitableCustomersForCS
                    extensionTime = 0.0;
                    previousNotEVFeasible = false;
                    countInfeasible = 0;
                    do //(visitableCustomersForCS.Count > 0)
                    {
                        CustomerSet tempExtendedCS = new CustomerSet(currentCS);
                        string customer2add = SelectACustomer(visitableCustomersForCS, currentCS); //Extend the CS with one customer from visitableCustomersForCS; here k plays a role for randomness
                        tempExtendedCS.NewExtend(customer2add);
                        bool isCSretrievedFromArchive = false; //Consider turining this check off and see how it performs, especially for small number of customers, this check might be more cumbersome than reoptimizing
                        if (exploredCustomerSetMasterList.Includes(tempExtendedCS))
                        {
                            tempExtendedCS = exploredCustomerSetMasterList.Retrieve_CS(tempExtendedCS); //retrieve the info needed from exploredCustomerSetList
                            isCSretrievedFromArchive = true;
                        }
                        else
                        {
                            extendStartTime = DateTime.Now;
                            tempExtendedCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                            extendFinishTime = DateTime.Now;
                        }
                        UpdateFeasibilityStatus4EachVehicleCategory(tempExtendedCS);
                        if (feas4GDV)
                            visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomers, tempExtendedCS);
                        currentCS = UpdateCurrentState(currentCS, customer2add, tempExtendedCS, isCSretrievedFromArchive, visitableCustomersForCS);
                    } while (visitableCustomersForCS.Count > 0);
                    visitableCustomers = visitableCustomers.Except(currentCS.Customers).ToList();
                } while (visitableCustomers.Count > 0);
                solution = SetCover();
                k++;
                index++;
            } while (index < theProblemModel.SRD.NumCustomers);
            solution = SetCover();

            globalFinishTime = DateTime.Now;
            totalRunTimeSec = (globalFinishTime - globalStartTime).TotalSeconds;
        }

        void RecoverWithSwap()
        {
            for (int k = 0; k < poolSize; k++)
            {
                random = new Random(k);
                int i = random.Next(exploredCustomerSetMasterList.Count);
                CustomerSet CS1 = new CustomerSet(exploredCustomerSetMasterList[i]);
                int j = random.Next(exploredCustomerSetMasterList.Count);
                CustomerSet CS2 = new CustomerSet(exploredCustomerSetMasterList[j]);
                CustomerSet.NewRandomSwap(CS1, CS2, random, exploredCustomerSetMasterList, theProblemModel);
            }
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

        public override void SpecializedConclude()
        {
            //Given that the instance is solved, we need to update status and statistics from it
            status = (AlgorithmSolutionStatus)((int)CPlexExtender.SolutionStatus);
            stats.RunTimeMilliSeconds = (long)totalRunTimeSec;
            stats.LowerBound = CPlexExtender.LowerBound_XCPlex;
            stats.UpperBound = CPlexExtender.UpperBound_XCPlex;
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
                        bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
                        break;
                    }
                case AlgorithmSolutionStatus.Optimal:
                    {
                        //Actual Run Time:Report<Limit, Complete Solution-LB = Best Solution-UB:Report, Best Solution Found:Report
                        bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
                        break;
                    }
                default:
                    break;
            }
            bestSolutionFound.Status = status;
            bestSolutionFound.UpperBound = CPlexExtender.UpperBound_XCPlex;
            bestSolutionFound.LowerBound = CPlexExtender.LowerBound_XCPlex;
            //_OptimizationComparisonStatistics.WriteToFile(StringOperations.AppendToFilename(theProblemModel.InputFileName, "_OptimizationComparisonStatistics"));
        }

        public override void SpecializedReset()
        {
            CPlexExtender.ClearModel();
            CPlexExtender.Dispose();
            CPlexExtender.End();
            GC.Collect();
        }

        string SelectACustomer(List<string> VisitableCustomers, CustomerSet currentCS)
        {
            string customerToAdd = VisitableCustomers[0];
            switch (selectedCriterion)
            {
                case Selection_Criteria.CompleteUniform:
                    return (VisitableCustomers[random.Next(VisitableCustomers.Count)]);

                case Selection_Criteria.UniformAmongTheBestPercentage:
                    List<string> theBestTopXPercent = new List<string>();
                    theBestTopXPercent = PopulateTheBestTopXPercentCustomersList(currentCS, VisitableCustomers, closestPercentSelect);
                    return (theBestTopXPercent[random.Next(theBestTopXPercent.Count)]);

                case Selection_Criteria.WeightedNormalizedProbSelection:
                    //We assume probabilities are proportional inverse distances
                    double[] prob = new double[VisitableCustomers.Count];
                    double probSum = 0.0;
                    for (int c = 0; c < VisitableCustomers.Count; c++)
                    {
                        prob[c] = 1.0 / Math.Max(0.00001, ShortestDistanceOfCandidateToCurrentCustomerSet(currentCS, VisitableCustomers[c]));//Math.max used to eliminate the div by 0 error
                        prob[c] = Math.Pow(prob[c], power);
                        probSum += prob[c];
                    }
                    for (int c = 0; c < VisitableCustomers.Count; c++)
                        prob[c] /= probSum;
                    return VisitableCustomers[Utils.RandomArrayOperations.Select(random.NextDouble(), prob)];

                default:
                    throw new Exception("The selection criterion sent to CustomerSet.Extend was not defined before!");
            }

        }
        List<string> PopulateTheBestTopXPercentCustomersList(CustomerSet CS, List<string> VisitableCustomers, double closestPercentSelect)
        {
            string[] customers = VisitableCustomers.ToArray();
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
        List<string> GetVisitableCustomersForCS(List<string> VisitableCustomers, CustomerSet currentCS)
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
                previousNotEVFeasible = false;
                feas4GDV = true;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
            {
                feas4EV = false;
                previousNotEVFeasible = true;
                feas4GDV = false;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)
            {
                feas4EV = false;
                previousNotEVFeasible = true;
                feas4GDV = true;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.NotYetOptimized)
                throw new Exception("RandomizedGreedy Extend function also optimizes the extended customer set however we get not yet optimized here");
            else
                throw new Exception("ExtendedCSIsFeasibleForDesiredVehicleCategory ran into an undefined case of RouteOptimizationStatus!");

        }
        CustomerSet UpdateCurrentState(CustomerSet currentCS, string customer2add, CustomerSet tempExtendedCS, bool isCSretrievedFromArchive, List<string> visitableCustomersForCS)
        {
            //visitableCustomersForCS = GetVisitableCustomersForCS(VisitableCustomers,currentCS);
            if (!isCSretrievedFromArchive)
            {
                exploredCustomerSetMasterList.Add(tempExtendedCS);
            }
            if (feas4EV)
            {
                currentCS = new CustomerSet(tempExtendedCS);
                visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomersForCS, currentCS);
            }
            if (currentCS.ImpossibleOtherCustomers.Count == theProblemModel.SRD.NumCustomers)
                visitableCustomersForCS = new List<string>();
            else
                visitableCustomersForCS.Remove(customer2add);

            return currentCS;
        }
        CustomerSetBasedSolution SetCover()
        {
            CustomerSetBasedSolution outcome = new CustomerSetBasedSolution();
            //solve the linear optimization problem to recover from bad cs creation
            CPlexExtender = new XCPlex_SetCovering_wCustomerSets(theProblemModel, XcplexParam, exploredCustomerSetMasterList);
            CPlexExtender.Solve_and_PostProcess();
            outcome = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
            if (CPlexExtender.SolutionStatus == XCPlexSolutionStatus.Feasible)
                outcome.Status = AlgorithmSolutionStatus.Feasible;
            else if (CPlexExtender.SolutionStatus == XCPlexSolutionStatus.Infeasible)
                outcome.Status = AlgorithmSolutionStatus.Infeasible;
            else if (CPlexExtender.SolutionStatus == XCPlexSolutionStatus.NoFeasibleSolutionFound)
                outcome.Status = AlgorithmSolutionStatus.NoFeasibleSolutionFound;
            else if (CPlexExtender.SolutionStatus == XCPlexSolutionStatus.NotYetSolved)
                outcome.Status = AlgorithmSolutionStatus.NotYetSolved;
            else if (CPlexExtender.SolutionStatus == XCPlexSolutionStatus.Optimal)
                outcome.Status = AlgorithmSolutionStatus.Optimal;
            outcome.UpperBound = CPlexExtender.UpperBound_XCPlex;
            outcome.LowerBound = CPlexExtender.LowerBound_XCPlex;
            return outcome;
        }
        void DecompressArcDuplicatingFormulationVariables(double[] allVariableValues)
        {
            double[][] X_value;
            double[][][] Y_value;
            double[] T_value;
            double[] delta_value;
            double[] epsilon_value;

            int numCustomers = theProblemModel.SRD.NumCustomers;
            int numES = theProblemModel.SRD.NumES;
            int counter = 0;
            X_value = new double[numCustomers + 1][];
            for (int i = 0; i <= numCustomers; i++)
            {
                X_value[i] = new double[numCustomers + 1];
                for (int j = 0; j <= numCustomers; j++)
                {
                    X_value[i][j] = allVariableValues[counter++];
                }
            }

            Y_value = new double[numCustomers + 1][][];
            for (int i = 0; i <= numCustomers; i++)
            {
                Y_value[i] = new double[numES][];
                for (int r = 0; r < numES; r++)
                {
                    Y_value[i][r] = new double[numCustomers + 1];
                    for (int j = 0; j <= numCustomers; j++)
                        Y_value[i][r][j] = allVariableValues[counter++];
                }
            }

            T_value = new double[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
                T_value[j] = allVariableValues[counter++];

            delta_value = new double[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
                delta_value[j] = allVariableValues[counter++];

            epsilon_value = new double[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
                epsilon_value[j] = allVariableValues[counter++];
        }
    }
}
