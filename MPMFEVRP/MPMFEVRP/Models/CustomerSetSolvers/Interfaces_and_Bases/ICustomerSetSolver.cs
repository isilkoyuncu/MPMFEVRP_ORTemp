using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Models.CustomerSetSolvers.Interfaces_and_Bases
{
    public interface ICustomerSetSolver
    {

        /// <summary>
        /// This is what used to be 
        /// </summary>
        /// <param name="customerSet">The customer set for which a new solution is desired.</param>
        /// <param name="PreserveCustomerVisitSequence">Whether this solver is constrained to use the customer visit sequence obtained for the GDV when optimizing for the AFV.</param>
        /// <returns></returns>
        RouteOptimizationOutcome Solve(CustomerSet customerSet, bool PreserveCustomerVisitSequence, bool feasibleAFVSolnIsEnough=false, bool performSwap=false);
    }
}
