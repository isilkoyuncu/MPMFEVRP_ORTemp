using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Implementations.Solutions;


namespace MPMFEVRP.Implementations.Algorithms
{
    public class RandomizedGreedy : AlgorithmBase
    {
        //Local algorithm parameters
        int poolSize = 0;
        Random random;
        Selection_Criteria selectedCriterion;
        double closestPercentSelect;
        double power;
        bool isRecoveryNeeded;
        Recovery_Options selectedRecoveryOpt;
        bool setCoverOption;

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam = new XCPlexParameters(); //TODO do we need to add additional parameters for the assigment problem?

        //Local statistics
        double extensionCompTime;
        double reassignmentCompTime;
        double totalCompTimeBeforeSetCover;
        double totalRunTimeMiliSec = 0.0;
        DateTime globalStartTime;

        double runTimeLimitInSeconds=0.0;
         
        public RandomizedGreedy():base()
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
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RECOVERY_NEEDED, "Recovery", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RECOVERY_OPTIONS, "Recovery Options", new List<object>() { Recovery_Options.AssignmentProbByCPLEX, Recovery_Options.AnalyticallySolve }, Recovery_Options.AssignmentProbByCPLEX, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_SET_COVER, "Set Cover", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
        }

        public override void SpecializedInitialize(ProblemModelBase problemModel)
        {
            model = problemModel;
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;

            poolSize = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_POOL_SIZE).GetIntValue();
            int randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            selectedCriterion =(Selection_Criteria) AlgorithmParameters.GetParameter(ParameterID.ALG_SELECTION_CRITERIA).Value;
            closestPercentSelect = AlgorithmParameters.GetParameter(ParameterID.ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT).GetIntValue();
            power = AlgorithmParameters.GetParameter(ParameterID.ALG_PROB_SELECTION_POWER).GetDoubleValue();
            isRecoveryNeeded = AlgorithmParameters.GetParameter(ParameterID.ALG_RECOVERY_NEEDED).GetBoolValue();
            selectedRecoveryOpt = (Recovery_Options)AlgorithmParameters.GetParameter(ParameterID.ALG_RECOVERY_OPTIONS).Value;
            setCoverOption = AlgorithmParameters.GetParameter(ParameterID.ALG_SET_COVER).GetBoolValue();
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
        }

        public override void SpecializedRun()
        {
            globalStartTime = DateTime.Now;

            extensionCompTime = 0.0;
            reassignmentCompTime = 0.0;
            totalCompTimeBeforeSetCover = 0.0;
            DateTime localStartTime;
            DateTime localFinishTime;

            List<CustomerSetBasedSolution> allSolutions = new List<CustomerSetBasedSolution>();
            int numberOfEVs = model.ProblemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue(); 
            //for (int trial = 0; trial < poolSize; trial++)
            do{
                CustomerSetBasedSolution trialSolution = new CustomerSetBasedSolution();
                List<string> visitableCustomers = model.GetAllCustomerIDs();//We assume here that each customer is addable to a currently blank set
                bool csSuccessfullyAdded = false;
                do
                {
                    CustomerSet currentCS = new CustomerSet();
                    bool csSuccessfullyUpdated = false;
                    do
                    {
                        string customerToAdd = SelectACustomer(visitableCustomers, currentCS);
                        localStartTime = DateTime.Now;
                        CustomerSet extendedCS = new CustomerSet(currentCS);
                        extendedCS.Extend(customerToAdd, model); //Extend function also optimizes the extended customer set
                        extensionCompTime += (DateTime.Now - localStartTime).TotalMilliseconds;

                        csSuccessfullyUpdated = ExtendedCSIsFeasibleForDesiredVehicleCategory(extendedCS, (trialSolution.NumCS_assigned2EV < numberOfEVs));
                        //For EV or GDV (whichever needed), we decided to keep or discard the extendedCS
                        if (csSuccessfullyUpdated)
                        {
                            visitableCustomers.Remove(customerToAdd);
                            currentCS = extendedCS;
                        }
                    } while (csSuccessfullyUpdated && visitableCustomers.Count > 0);

                    csSuccessfullyAdded = AddCustomerSet2Soln(trialSolution, currentCS); //return true if the customerSet is successfully added to the solution; return false if it is skipped  

                } while (visitableCustomers.Count > 0);

                if (isRecoveryNeeded)
                {
                    localStartTime = DateTime.Now;
                    trialSolution = Recover(trialSolution);
                    reassignmentCompTime += (DateTime.Now - localStartTime).TotalMilliseconds;
                }//This gives you the new trialSolution

                allSolutions.Add(trialSolution);
                localFinishTime = DateTime.Now;
            } while ((localFinishTime - globalStartTime).TotalSeconds < runTimeLimitInSeconds);

            totalCompTimeBeforeSetCover = (DateTime.Now - globalStartTime).TotalMilliseconds;

            if (setCoverOption) { RunSetCover(); }
            else { bestSolutionFound = allSolutions[GetBestSolnIndex(allSolutions)]; }

            totalRunTimeMiliSec = (DateTime.Now - globalStartTime).TotalMilliseconds;
        }

        public override void SpecializedConclude()
        {
            //Given that the instance is solved, we need to update status and statistics from it
            //Since Randomized Greedy always provides feasible solutions we need to know if it is a max or min problem so that we can fill UB or LB accordingly
            if (bestSolutionFound != null)
            {
                bestSolutionFound.Status = AlgorithmSolutionStatus.Feasible;

                if (model.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)//If it is a maximization problem, LB is the incumbent solution's objective value
                {
                    bestSolutionFound.LowerBound = bestSolutionFound.ObjectiveFunctionValue;
                    bestSolutionFound.UpperBound = double.MaxValue;
                }
                else //If it is a minimization problem, UB is the incumbent solution's objective value
                {
                    bestSolutionFound.UpperBound = bestSolutionFound.ObjectiveFunctionValue;
                    bestSolutionFound.LowerBound = double.MinValue;
                }
                stats.LowerBound = bestSolutionFound.LowerBound;
                stats.UpperBound = bestSolutionFound.UpperBound;
            }
            stats.RunTimeMilliSeconds = (long)totalRunTimeMiliSec;
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }

        public bool IsSupportingStepwiseSolutionCreation()
        {
            return true;
        }

        public override string GetName()
        {
            return "Randomized Greedy";
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
            switch (selectedCriterion)
            {
                case Selection_Criteria.CompleteUniform:
                    return (VisitableCustomers[random.Next(VisitableCustomers.Count)]);

                case Selection_Criteria.UniformAmongTheBestPercentage:
                    List<string> theBestTopXPercent = new List<string>();
                    theBestTopXPercent=PopulateTheBestTopXPercentCustomersList(currentCS, VisitableCustomers, closestPercentSelect);
                    return (theBestTopXPercent[random.Next(theBestTopXPercent.Count)]);

                case Selection_Criteria.WeightedNormalizedProbSelection:
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
        CustomerSetBasedSolution Recover (CustomerSetBasedSolution oldTrialSolution)
        {
            CustomerSetBasedSolution outcome = new CustomerSetBasedSolution();
            switch (selectedRecoveryOpt)
            {
                case Recovery_Options.AssignmentProbByCPLEX:
                    {
                        CustomerSetList cs_List = new CustomerSetList();
                        for (int i = 0; i < oldTrialSolution.NumCS_assigned2EV; i++)
                        {
                            cs_List.Add(oldTrialSolution.Assigned2EV[i]);
                        }
                        for (int i = 0; i < oldTrialSolution.NumCS_assigned2GDV; i++)
                        {
                            cs_List.Add(oldTrialSolution.Assigned2GDV[i]);
                        }
                        //solve the linear optimization problem to recover from bad cs creation
                        CPlexExtender = new XCPlex_SetCovering_wCustomerSets(model, XcplexParam, cs_List);
                        CPlexExtender.Solve_and_PostProcess();
                        outcome = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
                        break;
                    }
                case Recovery_Options.AnalyticallySolve:
                    {
                        CustomerSet[] cs_Array = new CustomerSet[oldTrialSolution.NumCS_total];
                        double[] dP = new double[oldTrialSolution.NumCS_total];
                        int count = 0;
                        for (int i = 0; i < oldTrialSolution.NumCS_assigned2EV; i++)
                        {
                            cs_Array[count] = oldTrialSolution.Assigned2EV[i];
                            dP[count] = cs_Array[count].RouteOptimizerOutcome.OFV[0] - cs_Array[count].RouteOptimizerOutcome.OFV[1];
                            count++;
                        }
                        for (int i =0; i < oldTrialSolution.NumCS_assigned2GDV; i++)
                        {
                            cs_Array[count] = oldTrialSolution.Assigned2GDV[i];
                            dP[count] = cs_Array[count].RouteOptimizerOutcome.OFV[0] - cs_Array[count].RouteOptimizerOutcome.OFV[1];
                            count++;
                        }
                        Array.Sort(dP, cs_Array);
                        for (int i = cs_Array.Length-1; i >=0; i--)
                        {
                            if (dP[i] > 0 && cs_Array[i].RouteOptimizerOutcome.OFV[0] > 0 && outcome.NumCS_assigned2EV < model.NumVehicles[0])
                            {
                                outcome.AddCustomerSet2EVList(cs_Array[i]);
                            }
                            else if (cs_Array[i].RouteOptimizerOutcome.OFV[1] > 0)
                            {
                                outcome.AddCustomerSet2GDVList(cs_Array[i]);
                            }
                        }
                        break;
                    }
            }
            return outcome;
        }
        bool AddCustomerSet2Soln(CustomerSetBasedSolution solution, CustomerSet CS)
        {
            if ((CS.RouteOptimizerOutcome.OFV[0] <= 0) && (CS.RouteOptimizerOutcome.OFV[1] <= 0))//The route is a negaive profit for either vehicle category!
                return false;

            //If we're here, we know at least one of the profits is a positive! And that's all we need, if this is a sub-optimal decision, we can still recover from it later, no worries.
            if ((solution.NumCS_assigned2EV < model.NumVehicles[0]))
            {
                solution.AddCustomerSet2EVList(CS);
            }
            else
            {
                solution.AddCustomerSet2GDVList(CS);
            }
            return true;
        }
        void RunSetCover()
        {
            CPlexExtender = new XCPlex_SetCovering_wCustomerSets(model, XcplexParam);
            //CPlexExtender.ExportModel(((XCPlex_Formulation)algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value).ToString()+"model.lp");
            CPlexExtender.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
        }
        int GetBestSolnIndex(List<CustomerSetBasedSolution> allSolutions)
        {
            int bestSolnIndex = -1;
            double objValue = Double.MinValue;
            bestSolnIndex = -1;
            int counter = -1;
            foreach (CustomerSetBasedSolution CSBS in allSolutions)
            {
                counter++;
                if (CSBS.ObjectiveFunctionValue > objValue)
                {
                    objValue = CSBS.ObjectiveFunctionValue;
                    bestSolnIndex = counter;
                }
            }
            return bestSolnIndex;
        }
    }
}
