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

namespace MPMFEVRP.Implementations.Algorithms
{
    public class ColumnGenerationHeuristic : AlgorithmBase
    {
        //Problem parameters
        int numberOfEVs;
        int numberOfGDVs;
        int totalNumVeh;
        int numCustomers;

        //Algorithm parameters
        int poolSize = 0;
        int randomSeed;
        Random random;
        Selection_Criteria selectedCriterion;
        double closestPercentSelect;
        double power;
        double runTimeLimitInSeconds = 0.0;
        double searchTimeLimit = 0.0;
        int infeasibleCountLimit= 0;

        bool couldNotExtend = false;

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam;

        CustomerSetList exploredCustomerSetMasterList = new CustomerSetList();
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
        double totalRunTimeMiliSec = 0.0;
        DateTime globalStartTime;
        DateTime globalFinishTime;
        double[][] iterationTime;

        public ColumnGenerationHeuristic()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_POOL_SIZE, "Random Pool Size", "300"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection }, Selection_Criteria.UniformAmongTheBestPercentage, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT, "% Customers 2 Select", new List<object>() {5, 10, 15, 20, 25, 30, 50 }, 20, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PROB_SELECTION_POWER, "Power", "2.0"));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SEARCH_TILIM, "Infeasible Search Tilim", "30"));
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;
            numberOfEVs = theProblemModel.ProblemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            numberOfGDVs = theProblemModel.ProblemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            totalNumVeh = numberOfEVs + numberOfGDVs;
            numCustomers = theProblemModel.SRD.NumCustomers;

            //Algorithm param
            poolSize = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_POOL_SIZE).GetIntValue();
            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            selectedCriterion = (Selection_Criteria)AlgorithmParameters.GetParameter(ParameterID.ALG_SELECTION_CRITERIA).Value;
            closestPercentSelect = AlgorithmParameters.GetParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT).GetIntValue();
            power = AlgorithmParameters.GetParameter(ParameterID.ALG_PROB_SELECTION_POWER).GetDoubleValue();
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            searchTimeLimit = AlgorithmParameters.GetParameter(ParameterID.ALG_SEARCH_TILIM).GetIntValue();
            XcplexParam = new XCPlexParameters();

            exploredCustomerSetMasterList = new CustomerSetList();
            vsrooList_GDV = new List<VehicleSpecificRouteOptimizationOutcome>();
            vsrooList_EV = new List<VehicleSpecificRouteOptimizationOutcome>();

            //Solution stat
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            allSolutions = new List<CustomerSetBasedSolution>();

            iterationTime = new double[3][];
            for (int i=0; i<3; i++)
                iterationTime[i] = new double[poolSize];
            totalTimeSpentExtendingCustSet = 0.0;
            //totalCompTimeBeforeSetCover = 0.0;
            //totalReassignmentTime_GDV2EV = 0.0;
            infeasibleCountLimit = 25;
        }

        public override void SpecializedRun()
        {

            CustomerSet currentCS1 = new CustomerSet(new List<string>() { "C08", "C10" });
            currentCS1.NewOptimize(theProblemModel);


            globalStartTime = DateTime.Now;
            DateTime localStartTime;
            DateTime localFinishTime;
            for (int i = 1; i <2; i++)
            {
                selectedCriterion = (Selection_Criteria)i;
                int k = 0;
                double objValue = (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Minimize ? double.MaxValue : double.MinValue);
                double objImprovement = 0.0;
                do //this is kind of the number of random starting points, in order not to stuck in a local optima
                {
                    localStartTime = DateTime.Now;
                    random = new Random(randomSeed + i+ k);
                    //Start with an empty customer set and customers2visit = all customers
                    List<string> visitableCustomers = theProblemModel.GetAllCustomerIDs(); //finish a solution when there is no customer in visitableCustomers set                    
                    do
                    {
                        CustomerSet currentCS = new CustomerSet(); //Start a new "customer set" <- route
                        List<string> visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomers, currentCS);
                        //finish a route when there is no customer left in the visitableCustomersForCS
                        extensionTime = 0.0;
                        previousNotEVFeasible = false;
                        countInfeasible = 0;
                        do
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
                                tempExtendedCS.NewOptimize(theProblemModel);
                                extendFinishTime = DateTime.Now;                               
                            }
                            UpdateFeasibilityStatus4EachVehicleCategory(tempExtendedCS);
                            currentCS = UpdateCurrentState(currentCS, customer2add, tempExtendedCS, isCSretrievedFromArchive, visitableCustomersForCS);
                        } while (visitableCustomersForCS.Count > 0);
                        visitableCustomers = visitableCustomers.Except(currentCS.Customers).ToList();
                    } while (visitableCustomers.Count > 0);
                    localFinishTime = DateTime.Now;
                    iterationTime[i][k] = (localFinishTime - localStartTime).TotalSeconds;
                    if (runTimeLimitInSeconds < iterationTime[i].Sum())
                        break;
                    solution = SetCover();
                    if (solution.Status == AlgorithmSolutionStatus.Optimal && solution.UpperBound < objValue)
                    {
                        objImprovement = objValue - solution.UpperBound;
                        objValue = solution.UpperBound;
                    }
                    k++;
                } while (k < poolSize || objImprovement > 0.001);
            }
            solution = SetCover();
            RecoverWithSwap();
            solution = SetCover();

            globalFinishTime = DateTime.Now;
            totalRunTimeMiliSec = (globalFinishTime - globalStartTime).TotalSeconds;
        }

        void RecoverWithSwap()
        {
            for (int k=0; k<poolSize; k++)
            {
                random = new Random(k);
                CustomerSet CS1 = new CustomerSet(exploredCustomerSetMasterList[random.Next(exploredCustomerSetMasterList.Count)]);
                CustomerSet CS2 = new CustomerSet(exploredCustomerSetMasterList[random.Next(exploredCustomerSetMasterList.Count)]);
                CustomerSet.NewRandomSwap(CS1, CS2, random, exploredCustomerSetMasterList, theProblemModel);
            }
        }

        public override string GetName()
        {
            return "Randomized Column Generation Heuristic";

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
            stats.RunTimeMilliSeconds = (long)totalRunTimeMiliSec;
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
        CustomerSet UpdateCurrentState(CustomerSet currentCS, string customer2add, CustomerSet tempExtendedCS, bool isCSretrievedFromArchive, List<string> visitableCustomersForCS, List<VehicleSpecificRouteOptimizationOutcome> vsrooList_GDV, List<VehicleSpecificRouteOptimizationOutcome> vsrooList_EV)
        {
            visitableCustomersForCS.Remove(customer2add);
            if (feas4EV)
            {
                currentCS = new CustomerSet(tempExtendedCS);
            }
            else if (feas4GDV)
            {
                // only GDV feasible, in this case run a mini algorithm that will give you some feasible routes for only gdv!!
                // do not update current CS or anything related to EV
            }
            else
                couldNotExtend = true;
            if (!isCSretrievedFromArchive)
            {
                vsrooList_EV.Add(tempExtendedCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV));
                vsrooList_GDV.Add(tempExtendedCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV));
                exploredCustomerSetMasterList.Add(tempExtendedCS);
            }
            visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomersForCS, currentCS);

            return currentCS;
        }
        CustomerSet UpdateCurrentState(CustomerSet currentCS, string customer2add, CustomerSet tempExtendedCS, bool isCSretrievedFromArchive, List<string> visitableCustomersForCS)
        {

            visitableCustomersForCS.Remove(customer2add);
            if (feas4EV)
            {
                currentCS = new CustomerSet(tempExtendedCS);
            }
            if (!isCSretrievedFromArchive)
            {
                exploredCustomerSetMasterList.Add(tempExtendedCS);
                if (previousNotEVFeasible == true && !feas4EV)
                {
                    extensionTime = extensionTime + totalTimeSpentExtendingCustSet + (extendFinishTime - extendStartTime).TotalSeconds;
                    if (extensionTime > searchTimeLimit)
                        visitableCustomersForCS = new List<string>();
                }
            }
            else
            {
                if (previousNotEVFeasible == true && !feas4EV)
                {
                    countInfeasible++;
                    if (countInfeasible > infeasibleCountLimit)
                        visitableCustomersForCS = new List<string>();
                }
            }
            visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomersForCS, currentCS);
            return currentCS;
        }
        CustomerSetBasedSolution SetCover()
        {
            CustomerSetBasedSolution outcome = new CustomerSetBasedSolution();
            //solve the linear optimization problem to recover from bad cs creation
            CPlexExtender = new XCPlex_SetCovering_wCustomerSets(theProblemModel, XcplexParam, exploredCustomerSetMasterList);
            CPlexExtender.Solve_and_PostProcess();
            outcome = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
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
