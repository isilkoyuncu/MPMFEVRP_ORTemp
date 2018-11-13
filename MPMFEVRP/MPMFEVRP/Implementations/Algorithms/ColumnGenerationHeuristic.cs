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

        List<CustomerSetBasedSolution> allSolutions;

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

            CustomerSetList optimizedCustomerSetList = new CustomerSetList();
            CustomerSetBasedSolution trialSoln;

            CustomerSet customerSet;
            VehicleSpecificRouteOptimizationOutcome routeOptimizationOutcome;
            //for (int k = 0; k < poolSize; k++)
            //{
                customerSet = new CustomerSet(new List<string> { "C6", "C8", "C13", "C14", "C16", "C18" });
                routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.EV));
                //routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.GDV));

                customerSet = new CustomerSet(new List<string> { "C1", "C7", "C10", "C12", "C15" });
                routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.EV));
                //routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.GDV));

                customerSet = new CustomerSet(new List<string> { "C2", "C5", "C9", "C17", "C20" });
                routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.EV));
                //routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.GDV));

                customerSet = new CustomerSet(new List<string> { "C3", "C4", "C11", "C19" });
                routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.EV));
            //routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.GDV));

            //}
            double tempSec = 0.0;
            localStartTime = DateTime.Now;
            List<string> csIDs = theProblemModel.SRD.GetCustomerIDs();
            for (int k=1; k<numCustomers-1; k++)
            {
                customerSet = new CustomerSet(new List<string> { csIDs[k], csIDs[k+1] });
                routeOptimizationOutcome = theProblemModel.NewRouteOptimize(customerSet, theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.EV));
            }
            localFinishTime = DateTime.Now;
            tempSec = (localFinishTime - localStartTime).TotalMilliseconds;

            for (int k=0; k<poolSize; k++)
            {
                random = new Random(randomSeed+k);
                trialSoln = new CustomerSetBasedSolution(theProblemModel);
                bool csSuccessfullyAdded = false;
                List<string> visitableCustomers = theProblemModel.GetAllCustomerIDs();
                do
                {

                } while (!csSuccessfullyAdded || visitableCustomers.Count > 0);
            }
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

       
    }
}
