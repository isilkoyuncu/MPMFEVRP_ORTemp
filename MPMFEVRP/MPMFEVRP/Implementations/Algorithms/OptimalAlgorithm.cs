using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Utils;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class OptimalAlgorithm : AlgorithmBase
    {
        string[] auxIdArray;

        public override string GetName()
        {
            return "Optimal Algorithm";
        }

        public override void SpecializedInitialize(IProblemModel model)
        {
            auxIdArray = model.IDs.ToArray();
        }

        int CompareTwoJobs(string id1, string id2)
        {
            int index1 = 0, index2 = 0;
            for (int i = 0; i < model.IDs.Length; i++)
            {
                if (id1 == model.IDs[i])
                    index1 = i;
                else if (id2 == model.IDs[i])
                    index2 = i;
            }
            if (model.DueDates[index1] < model.DueDates[index2])
                return -1;
            if (model.DueDates[index1] > model.DueDates[index2])
                return 1;
            return 0;
        }

        public override void SpecializedRun()
        {
            Array.Sort(auxIdArray, CompareTwoJobs);
        }

        public override void SpecializedConclude()
        {
            bestSolutionFound = new DefaultSolution();
            bestSolutionFound.IDs.AddRange(auxIdArray);
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }
    }
}
