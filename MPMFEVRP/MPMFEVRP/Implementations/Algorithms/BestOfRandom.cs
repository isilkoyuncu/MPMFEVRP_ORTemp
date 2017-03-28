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


namespace MPMFEVRP.Implementations.Algorithms
{
    public class BestOfRandom : AlgorithmBase
    {
        IProblemModel problemData;

        int poolSize = 20;

        public BestOfRandom()
        {
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_POOL_SIZE, "Random Pool Size", "20"));
            AlgorithmParameters.AddParameter(new Parameter(ParameterID.RANDOM_SEED, "Random Seed", "50"));
        }

        public override string GetName()
        {
            return "Best of N Randoms";
        }

        public override void SpecializedInitialize(IProblemModel model)
        {
            problemData = model;
            poolSize = AlgorithmParameters.GetParameter(ParameterID.RANDOM_POOL_SIZE).GetIntValue();
            int randomSeed = AlgorithmParameters.GetParameter(ParameterID.RANDOM_SEED).GetIntValue();
            Random random = new Random(randomSeed);
            //TODO write new default solution
            //bestSolutionFound = new DefaultSolution(model, random);
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
    }
}
