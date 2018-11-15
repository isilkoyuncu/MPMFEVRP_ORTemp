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

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam;

        CustomerSetList exploredCustomerSetMasterList = new CustomerSetList();
        List<VehicleSpecificRouteOptimizationOutcome> vsrooList_GDV = new List<VehicleSpecificRouteOptimizationOutcome>();
        List<VehicleSpecificRouteOptimizationOutcome> vsrooList_EV = new List<VehicleSpecificRouteOptimizationOutcome>();

        CustomerSetBasedSolution solution = new CustomerSetBasedSolution();
        List<CustomerSetBasedSolution> allSolutions;

        bool feas4EV = true;
        bool feas4GDV = true;

        //Local statistics
        double totalTimeSpentExtendingCustSet;
        double totalCompTimeBeforeSetCover;
        double totalReassignmentTime_GDV2EV;
        double totalRunTimeMiliSec = 0.0;
        DateTime globalStartTime;

        public ColumnGenerationHeuristic()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_POOL_SIZE, "Random Pool Size", "30"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection }, Selection_Criteria.WeightedNormalizedProbSelection, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT, "% Customers 2 Select", new List<object>() { 10, 30, 50 }, 30, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PROB_SELECTION_POWER, "Power", "2.0"));
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
            XcplexParam = new XCPlexParameters();

            //Solution stat
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            allSolutions = new List<CustomerSetBasedSolution>();

            totalTimeSpentExtendingCustSet = 0.0;
            totalCompTimeBeforeSetCover = 0.0;
            totalReassignmentTime_GDV2EV = 0.0;
        }

        public override void SpecializedRun()
        {
            globalStartTime = DateTime.Now;
            DateTime localStartTime;
            DateTime localFinishTime;

            for (int k = 0; k < poolSize; k++) //this is kind of the number of random starting points, in order not to stuck in a local optima
            {
                random = new Random(randomSeed + k);
                //Start with an empty customer set and customers2visit = all customers
                List<string> visitableCustomers = theProblemModel.GetAllCustomerIDs(); //finish a solution when there is no customer in visitableCustomers set
                do
                {
                    CustomerSet currentCS = new CustomerSet(); //Start a new "customer set" <- route
                    List<string> visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomers, currentCS);
                    //finish a route when there is no customer left in the visitableCustomersForCS
                    do
                    {
                        string customer2add = SelectACustomer(visitableCustomersForCS, currentCS); //Extend the CS with one customer from visitableCustomersForCS; here k plays a role for randomness
                        CustomerSet tempExtendedCS = new CustomerSet(currentCS);
                        tempExtendedCS.NewExtend(customer2add);
                        bool isCSretrievedFromArchive = false; //Consider turining this check off and see how it performs, especially for small number of customers, this check might be more cumbersome than reoptimizing
                        if (exploredCustomerSetMasterList.Includes(tempExtendedCS))
                        {
                            tempExtendedCS = exploredCustomerSetMasterList.Retrieve_CS(tempExtendedCS); //retrieve the info needed from exploredCustomerSetList
                            isCSretrievedFromArchive = true;
                        }
                        else
                        {
                            tempExtendedCS.NewOptimize(theProblemModel);
                        }
                        UpdateFeasibilityStatus4EachVehicleCategory(tempExtendedCS);
                        currentCS = UpdateCurrentState(currentCS, customer2add, tempExtendedCS, isCSretrievedFromArchive, visitableCustomersForCS, vsrooList_GDV, vsrooList_EV);
                    } while (visitableCustomersForCS.Count > 0);
                    visitableCustomers = visitableCustomers.Except(currentCS.Customers).ToList();
                } while (visitableCustomers.Count > 0);

            }
            solution = SetCover();
            //CustomerSet customerSet;
            //RouteOptimizationOutcome routeOptimizationOutcome;
            //double tempSec = 0.0;
            //localStartTime = DateTime.Now;
            ////customerSet = new CustomerSet(new List<string> { "C11", "C17", "C19", "C26", "C29", "C31", "C34", "C40", "C41" });
            //customerSet = new CustomerSet(new List<string> { "C5", "C9", "C12", "C25", "C42", "C43" });
            //routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet);
            //localFinishTime = DateTime.Now;
            //tempSec = (localFinishTime - localStartTime).TotalSeconds;

        }

        public override string GetName()
        {
            return "Randomized Column Generation Heuristic";

        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }

        public override bool setListener(IListener listener)
        {
            throw new NotImplementedException();
        }

        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
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
                feas4GDV = true;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
            {
                feas4EV = false;
                feas4GDV = false;
            }
            else if (tempExtendedCS.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)
            {
                feas4EV = false;
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
                if (!isCSretrievedFromArchive)
                {
                    vsrooList_EV.Add(tempExtendedCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV));
                    vsrooList_GDV.Add(tempExtendedCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV));
                    exploredCustomerSetMasterList.Add(tempExtendedCS);
                }
                visitableCustomersForCS = GetVisitableCustomersForCS(visitableCustomersForCS, currentCS);
            }
            else if (feas4GDV)
            {
                // only GDV feasible, in this case run a mini algorithm that will give you some feasible routes for only gdv!!
                // do not update current CS or anything related to EV
            }
            else
                visitableCustomersForCS = new List<string>();
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
    }
}
