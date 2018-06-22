using System;
using System.Collections.Generic;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Models.ColumnGeneration
{
    public class ColumnGenerationAlgorithm
    {
        List<double> dualVariables;
        public ColumnGenerationAlgorithm()
        {
            Initialize();
        }
        void Initialize()
        {
            dualVariables = new List<double>();
        }
        public void Run(IProblem theProblem)
        {
            LabelingAlgMaxProfitNoIS LAMP = new LabelingAlgMaxProfitNoIS(theProblem, dualVariables);
            LAMP.Run();
        }
    }
}
