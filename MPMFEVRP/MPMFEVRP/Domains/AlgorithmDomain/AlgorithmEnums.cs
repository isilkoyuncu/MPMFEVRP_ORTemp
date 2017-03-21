using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.AlgorithmDomain
{
    public enum XCPlexSolutionStatus { NotYetSolved = -2, Infeasible, NoFeasibleSolutionFound, Feasible, Optimal };
    //enum values:
    //Unsolved until Solve() is tried, the other four are the same as AlgorithmSolutionStatus

    public enum XCPlexRelaxation { None, LinearProgramming };
    //enum values:
    // None: Full IP with integer variables
    // LinearProgramming: full formulation, only the variables are relaxed to be continuous
}
