﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Utils;
using System.Windows.Forms;


namespace MPMFEVRP.Implementations.Algorithms
{
    public class RandomizedGreedy : AlgorithmBase
    {
        int poolSize = 20;
        Random random;
        double power;

        Selection_Criteria sc;
        double closestPercentSelect;
        bool isRecoveryNeeded;
        Recovery_Options selectedRecoveryOpt;
        bool setCoverOption;
        
        XCPlex_Assignment_RecoveryForRandGreedy CPlexExtender = null;
        XCPlexParameters XcplexParam = new XCPlexParameters(); //TODO do we need to add additional parameters for the assigment problem?

        public RandomizedGreedy()
        {
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_POOL_SIZE, "Random Pool Size", "20"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection }, Selection_Criteria.WeightedNormalizedProbSelection, ParameterType.ComboBox));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.PERCENTAGE_OF_CUSTOMERS_2SELECT, "% Customers 2 Select", new List<object>() { 10, 25, 50, 100 }, 25, ParameterType.ComboBox));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.PROB_SELECTION_POWER, "Power", "2.0"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RECOVERY_NEEDED, "Recovery", new List<object>() { true, false }, true, ParameterType.CheckBox));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RECOVERY_OPTIONS, "Recovery Options", new List<object>() { Recovery_Options.AssignmentProbByCPLEX, Recovery_Options.AnalyticallySolve }, Recovery_Options.AssignmentProbByCPLEX, ParameterType.ComboBox));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.SET_COVER, "Set Cover", new List<object>() { true, false }, false, ParameterType.CheckBox));
        }

        public override string GetName()
        {
            return "Randomized Greedy";
        }

        public override void SpecializedInitialize(ProblemModelBase problemModel)
        {
            model = problemModel;
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;

            poolSize = AlgorithmParameters.GetParameter(ParameterID.RANDOM_POOL_SIZE).GetIntValue();
            int randomSeed = AlgorithmParameters.GetParameter(ParameterID.RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            sc =(Selection_Criteria) AlgorithmParameters.GetParameter(ParameterID.SELECTION_CRITERIA).Value;
            closestPercentSelect = AlgorithmParameters.GetParameter(ParameterID.PERCENTAGE_OF_CUSTOMERS_2SELECT).GetIntValue();
            power = AlgorithmParameters.GetParameter(ParameterID.PROB_SELECTION_POWER).GetDoubleValue();
            isRecoveryNeeded = AlgorithmParameters.GetParameter(ParameterID.RECOVERY_NEEDED).GetBoolValue();
            selectedRecoveryOpt = (Recovery_Options)AlgorithmParameters.GetParameter(ParameterID.RECOVERY_OPTIONS).Value;
            setCoverOption = AlgorithmParameters.GetParameter(ParameterID.SET_COVER).GetBoolValue();
        }

        public override void SpecializedRun()
        {
            List<Solutions.CustomerSetBasedSolution> allSolutions = new List<Solutions.CustomerSetBasedSolution>();
            int bestSolnIndex = -1;
            int numberOfEVs = model.VRD.NumVehicles[0]; //[0]=EV, [1]=GDV 
            //This is MY understanding of the randomized greedy:
            for (int trial = 0; trial < poolSize; trial++)
            {
                List<CustomerSet> currentCS_List = new List<CustomerSet>();
                Solutions.CustomerSetBasedSolution trialSolution = new Solutions.CustomerSetBasedSolution();//Bunun ne olacagini bilmiyorum, belki de butun CS'leri urettikten sonra onlarin tamamini iceren bir solution olarak bir seferde uretmeliyiz
                //solution blank olarak uretildi ve icinde hicbir customerSet yok
                List<string> visitableCustomers = model.GetAllCustomerIDs();//We assume here that each customer is addable to a currently blank set
                bool csSuccessfullyAdded = false;
                do
                {
                    CustomerSet currentCS = new CustomerSet();
                    bool csSuccessfullyUpdated = false;
                    do
                    {
                        string customerToAdd = SelectACustomer(visitableCustomers, currentCS);
                        CustomerSet extendedCS = new CustomerSet(currentCS);
                        extendedCS.Extend(customerToAdd, model); //Extend function also optimizes the extended customer set
                        csSuccessfullyUpdated = ExtendedCSIsFeasibleForDesiredVehicleCategory(extendedCS, (trialSolution.NumCS_assigned2EV < numberOfEVs));
                        //For EV or GDV (whichever needed), we decided to keep or discard the extendedCS
                        if (csSuccessfullyUpdated)
                        {
                            visitableCustomers.Remove(customerToAdd);
                            currentCS = extendedCS;
                        }
                    } while (csSuccessfullyUpdated);

                    //add the customerSet to the solution
                    trialSolution.AddCustomerSet2EVList(currentCS);
                } while (csSuccessfullyAdded);
                if (isRecoveryNeeded)
                {
                    switch (selectedRecoveryOpt)
                    {
                        case Recovery_Options.AssignmentProbByCPLEX:
                            {
                                //solve the linear optimization problem to recover from bad cs creation
                                CPlexExtender = new XCPlex_Assignment_RecoveryForRandGreedy(model, XcplexParam,trialSolution);
                                break;
                            }
                        case Recovery_Options.AnalyticallySolve:
                            {
                                //TODO code the analytical recovery algorithm
                                break;
                            }
                    }
                    
                    trialSolution = (Solutions.CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(Solutions.CustomerSetBasedSolution));
                    //This gives you the new trialSolution
                }
                //else: do nothing (don't have an else!)ShortestDistanceOfCandidateToCurrentCustomerSet

                allSolutions.Add(trialSolution);
            }//for(int trial = 0; trial<poolSize; trial++)
            if (setCoverOption)
            {
                //TODO solve the set cover formulation, construct a solution from it, and return that!
            }
            else
            {
                double objValue = Double.MinValue;
                bestSolnIndex = -1;
                int counter = -1;
                foreach (Solutions.CustomerSetBasedSolution CSBS in allSolutions)
                {
                    counter++;
                    if (CSBS.ObjectiveFunctionValue > objValue)
                    {
                        objValue = CSBS.ObjectiveFunctionValue;
                        bestSolnIndex = counter;
                    }
                }
            }
            //Return the best of allSolutions
            bestSolutionFound = allSolutions[bestSolnIndex];

            //The following is best of random and I'm not sure how is this going to work with a complex solution structure
            //bestSolutionFound = BestRandom<ISolution>.Find(poolSize, bestSolutionFound, bestSolutionFound.CompareTwoSolutions);
        }
        /// <summary>
        /// This method tries to extend the customer set only once 
        /// </summary>
        /// <param name="VisitableCustomers"></param>
        /// <param name="sc"></param>
        /// <param name="problemModelBase"></param>
        /// <returns></returns>
        CustomerSet ExtendCurrentCustomerSet(CustomerSet currentCS, List<string> VisitableCustomers)
        {
            if (VisitableCustomers.Count == 0)
                return null;

            //Preliminary: make a copy of the current customer set
            CustomerSet outcome = new CustomerSet(currentCS);

            //We first must choose which of the visitable customers to add to the set
            string customerToAdd = VisitableCustomers[0];
            switch (sc)
            {
                case Selection_Criteria.CompleteUniform:
                    //Here goes the specialized selection code
                    break;
                case Selection_Criteria.UniformAmongTheBestPercentage:
                    //Here goes the specialized selection code
                    break;
                case Selection_Criteria.WeightedNormalizedProbSelection:
                    //Here goes the specialized selection code
                    break;
                default:
                    throw new Exception("The selection criterion sent to CustomerSet.Extend was not defined before!");
            }
            //In each case within this block we must identify a customer to add to the list

            //Now that we have a customer to add to the list, we'll request to make the extension
            outcome.Extend(customerToAdd, model);

            //Now return
            return outcome;
        }
        
        /// <summary>
        /// This method selects the next customer to be added in the currect customer set
        /// </summary>
        /// <param name="VisitableCustomers"></param>
        /// <param name="currentCustomerID"></param>
        /// <returns></returns>
        string SelectACustomer(List<string> VisitableCustomers, CustomerSet currentCS)
        {
            string customerToAdd = VisitableCustomers[0];
            switch (sc)
            {
                case Selection_Criteria.CompleteUniform:
                    return (VisitableCustomers[random.Next(VisitableCustomers.Count)]);
                case Selection_Criteria.UniformAmongTheBestPercentage:
                    //Here goes the specialized selection code
                    List<string> theBestTopXPercent = new List<string>();
                    theBestTopXPercent=PopulateTheBestTopXPercentCustomersList(currentCS, VisitableCustomers, closestPercentSelect);
                    return (theBestTopXPercent[random.Next(theBestTopXPercent.Count)]);
                case Selection_Criteria.WeightedNormalizedProbSelection:
                    //Here goes the specialized selection code
                    //We assume probabilities are proportional inverse distances
                    double[] prob = new double[VisitableCustomers.Count];
                    double probSum = 0.0;
                    for(int c=0; c< VisitableCustomers.Count; c++)
                    {
                        prob[c] = 1.0/Math.Max(0.00001,ShortestDistanceOfCandidateToCurrentCustomerSet(currentCS,VisitableCustomers[c]));//Math.max used to eliminate the div by 0 error
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
            for(int i = 0; i < nCustomers; i++)
            {
                shortestDistancesToCS[i] = ShortestDistanceOfCandidateToCurrentCustomerSet(CS, customers[i]);
            }
            Array.Sort(shortestDistancesToCS, customers);

            List<string> outcome = new List<string>();
            int numberToReturn = (int)Math.Ceiling(closestPercentSelect * nCustomers/100.0);
            for (int i= 0; i < numberToReturn; i++)
            {
                outcome.Add(customers[i]);
            }

            return outcome;
        }
        double ShortestDistanceOfCandidateToCurrentCustomerSet(CustomerSet CS, string candidate)
        {
            if (CS.NumberOfCustomers == 0)
                return model.SRD.GetDistance(candidate, model.SRD.GetSingleDepotID());
            else
            {
                double outcome = double.MaxValue;
                foreach (string customer in CS.Customers)
                {
                    double distance = model.SRD.GetDistance(candidate, customer);
                    if (outcome > distance)
                    {
                        outcome = distance;
                    }
                }
                return outcome;
            }
        }
        bool ExtendedCSIsFeasibleForDesiredVehicleCategory(CustomerSet CS, bool consideringForEV)
        {
            switch (CS.RouteOptimizerOutcome.Status)
            {
                case RouteOptimizationStatus.OptimizedForBothGDVandEV:
                    return true;
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV:
                    return false;
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                    return (!consideringForEV);
                case RouteOptimizationStatus.NotYetOptimized:
                    throw new Exception("RandomizedGreedy Extend function also optimizes the extended customer set however we get not yet optimized here");
                default:
                    throw new Exception("ExtendedCSIsFeasibleForDesiredVehicleCategory ran into an undefined case of RouteOptimizationStatus!");
            }
        }
        
        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }
    }
}
