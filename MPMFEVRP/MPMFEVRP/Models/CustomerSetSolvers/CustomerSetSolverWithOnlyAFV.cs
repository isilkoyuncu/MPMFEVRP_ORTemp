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
    public class CustomerSetSolverWithOnlyAFV : ICustomerSetSolverForASingleVehicleCategory
    {
        public VehicleCategories VehicleCategory => VehicleCategories.EV;
        

        public VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool PreserveCustomerVisitSequence)
        {
            throw new NotImplementedException();
        }
    }
}
