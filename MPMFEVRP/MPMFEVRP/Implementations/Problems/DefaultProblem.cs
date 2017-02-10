using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Implementations.Problems //TODO DELETE THIS!!!!!
{
    public class DefaultProblem : ProblemBase
    {
        public DefaultProblem()
        {
            objectiveFunctionType = Models.ObjectiveFunctionTypes.Minimize;
        }
        public override string GetName()
        {
            return "Single Machine Scheduling Problem to Minimize Maximum Lateness";
        }
    }
}
