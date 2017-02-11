using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ProblemConstants
    {
        public static readonly List<string> PROBLEMS = new List<string>() { "KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17" };

        public static readonly double ERROR_TOLERANCE = 0.00001;
    }

    public class AlgorithmConstants
    {
        public static readonly List<string> ALGORITHMS = new List<string>() { "CPlex", "Route-First Cluster-Second", "Greedy", "Worst Case" };
    }
}
