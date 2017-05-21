using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Models;
using MPMFEVRP.Implementations.Solutions;
using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Utils;


namespace MPMFEVRP.Implementations.Algorithms
{
    public class RandomizedGreedy : AlgorithmBase
    {
        ProblemModelBase problemData;

        int poolSize = 20;


        public RandomizedGreedy()
        {
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_POOL_SIZE, "Random Pool Size", "20"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.SELECTION_CRITERIA, "Random Site Selection Criterion", new List<object>() { Selection_Criteria.CompleteUniform, Selection_Criteria.UniformAmongTheBestPercentage, Selection_Criteria.WeightedNormalizedProbSelection }, Selection_Criteria.WeightedNormalizedProbSelection, ParameterType.ComboBox));
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
            poolSize = AlgorithmParameters.GetParameter(ParameterID.RANDOM_POOL_SIZE).GetIntValue();
            int randomSeed = AlgorithmParameters.GetParameter(ParameterID.RANDOM_SEED).GetIntValue();
            Random random = new Random(randomSeed);
        }

        public override void SpecializedRun()
        {
            bestSolutionFound = BestRandom<ISolution>.Find(poolSize, bestSolutionFound, bestSolutionFound.CompareTwoSolutions);
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
