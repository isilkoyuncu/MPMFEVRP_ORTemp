using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.ProblemModels;
using BestRandom;
using MPMFEVRP.Forms;

namespace MPMFEVRP.Implementations.Solutions
{
    public class BackwardLoadingSolution : DefaultSolution
    {
        public BackwardLoadingSolution()
        {
        }

        public BackwardLoadingSolution(ISolution solution, string extensionJobId, IProblemModel problemData)
        {
            this.ids.AddRange(solution.IDs);
            this.ids.Insert(0, extensionJobId);
            this.problemData = problemData;
            ComputeVariables();
        }

        public BackwardLoadingSolution(IProblemModel problemData)
        {
            this.random = new Random(DateTime.Now.Ticks.GetHashCode());
            this.problemData = problemData;
            ComputeVariables();
        }

        public BackwardLoadingSolution(IProblemModel problemData, Random rnd)
        {
            this.random = rnd;
            this.problemData = problemData;
            ComputeVariables();
        }

        public override ISolution GenerateRandom()
        {
            ISolution someSolution = new BackwardLoadingSolution(problemData);
            someSolution.IDs.AddRange(problemData.IDs.ToList().OrderBy(x => random.Next()));
            ComputeVariables();
            return someSolution;
        }

        public override List<ISolution> GetAllChildren()
        {
            List<ISolution> children = new List<ISolution>();

            // Get the job IDs that doesnot exist in solution's sequence.
            List<string> remainingIds = problemData.IDs.Where(x => !ids.Contains(x)).ToList();

            foreach (var id in remainingIds)
            {
                children.Add(new BackwardLoadingSolution(this, id, problemData));
            }
            return children;
        }

        public override string GetName()
        {
            return "Backward Loading Solution";
        }

    }
}
