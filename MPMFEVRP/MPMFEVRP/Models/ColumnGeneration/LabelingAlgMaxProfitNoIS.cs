using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Models.ColumnGeneration
{
    public class LabelingAlgMaxProfitNoIS
    {
        IProblem theProblem;


        public LabelingAlgMaxProfitNoIS(IProblem theProblem, List<double> dualVariables)
        {
            this.theProblem = theProblem;

        }
        void Initialize()
        {

        }
        public void Run()
        {

        }
    }
}
