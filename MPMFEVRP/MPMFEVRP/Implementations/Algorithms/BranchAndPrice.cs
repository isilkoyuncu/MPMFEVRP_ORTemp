using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Models.ColumnGeneration;
using MPMFEVRP.SupplementaryInterfaces.Listeners;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class BranchAndPrice : AlgorithmBase
    {
        IProblem theProblem;
        public BranchAndPrice() { }
        public BranchAndPrice(IProblem theProblem)
        {
            this.theProblem = theProblem;
        }
        public override void AddSpecializedParameters()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Branch And Price";
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

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            throw new NotImplementedException();
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedRun()
        {
            ColumnGenerationAlgorithm CGA = new ColumnGenerationAlgorithm();
            CGA.Run(theProblem);
        }
    }
}
