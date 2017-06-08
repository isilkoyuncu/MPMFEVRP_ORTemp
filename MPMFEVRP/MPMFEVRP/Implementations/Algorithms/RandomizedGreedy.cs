using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class RandomizedGreedy : AlgorithmBase
    {
        ProblemModelBase problemData;

        int poolSize = 20;
        Random random;

        Selection_Criteria selectedCriterion;
        double closestPercentSelect;
        bool recoveryOption;
        bool setCoverOption;
        
        CustomerSet CS;
        List<CustomerSet> CS_List;

        public RandomizedGreedy()
        {
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_POOL_SIZE, "Random Pool Size", "20"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection }, Selection_Criteria.WeightedNormalizedProbSelection, ParameterType.ComboBox));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.PERCENTAGE_OF_CUSTOMERS_2SELECT, "% Customers 2 Select", new List<object>() { 10, 25, 50, 100 }, 25, ParameterType.ComboBox)); //TODO find a way to get data from problem, i.e. number of customers or make this percentage
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RECOVERY_OPTION, "Recovery", new List<object>() { true, false }, true, ParameterType.CheckBox));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.SET_COVER, "Set Cover", new List<object>() { true, false }, false, ParameterType.CheckBox));
        }

        public override string GetName()
        {
            return "Randomized Greedy MY";
        }

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            problemData = model;
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;

            poolSize = AlgorithmParameters.GetParameter(ParameterID.RANDOM_POOL_SIZE).GetIntValue();
            int randomSeed = AlgorithmParameters.GetParameter(ParameterID.RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            selectedCriterion =(Selection_Criteria) AlgorithmParameters.GetParameter(ParameterID.SELECTION_CRITERIA).Value;
            closestPercentSelect = AlgorithmParameters.GetParameter(ParameterID.PERCENTAGE_OF_CUSTOMERS_2SELECT).GetIntValue();
            recoveryOption = AlgorithmParameters.GetParameter(ParameterID.RECOVERY_OPTION).GetBoolValue();
            setCoverOption = AlgorithmParameters.GetParameter(ParameterID.SET_COVER).GetBoolValue();

            CS = new CustomerSet();
            CS_List = new List<CustomerSet>();
        }

        public override void SpecializedRun()
        {
            //TODO I'm not sure how is this going to work with a complex solution structure
            bestSolutionFound = BestRandom<ISolution>.Find(poolSize, bestSolutionFound, bestSolutionFound.CompareTwoSolutions);

            // TODO This is my understanding of randomized greedy
            CS.Extend(problemData.SRD.SiteArray[(int)(random.NextDouble())].ID, problemData); //TODO should I record IDs?
            for (int i = 0; i < poolSize; i++)
            {
                switch ((Selection_Criteria)AlgorithmParameters.GetParameter(ParameterID.SELECTION_CRITERIA).Value)
                {
                    case Selection_Criteria.CompleteUniform:
                        break;
                    case Selection_Criteria.UniformAmongTheBestPercentage:
                        break;
                    case Selection_Criteria.WeightedNormalizedProbSelection:
                        break;
                }

            }
        }
        public override void SpecializedConclude()
        {
            //throw new NotImplementedException();
        }

        public override void SpecializedReset()
        {
            //throw new NotImplementedException();
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }
    }
}
