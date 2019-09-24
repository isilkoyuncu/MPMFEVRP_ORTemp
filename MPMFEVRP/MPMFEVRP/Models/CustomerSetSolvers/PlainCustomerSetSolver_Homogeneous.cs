using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.CustomerSetSolvers.Interfaces_and_Bases;

namespace MPMFEVRP.Models.CustomerSetSolvers
{
    /// <summary>
    /// This class solves each customer set with only AFV solver, does not exploit GDV optimal routes.
    /// </summary>
    public class PlainCustomerSetSolver_Homogeneous : ICustomerSetSolver
    {
        public RouteOptimizationOutcome Solve(CustomerSet customerSet, List<VehicleCategories> vehicleCategories, bool PreserveCustomerVisitSequence)
        {
            throw new NotImplementedException();
        }
    }
}
