using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Models.CustomerSetSolvers.Interfaces_and_Bases
{
    /// <summary>
    /// This is the interface of all "TSP" solvers and alike. There are also some that check for existence of AFV-optimal routes based on already known GDV-optimal routes, without the need to change the sequence of customers.
    /// </summary>
    interface ICustomerSetSolverForASingleVehicleCategory
    {
        VehicleCategories VehicleCategory { get; }//This is to never change!

        /// <summary>
        /// This is what used to be 
        /// </summary>
        /// <param name="customerSet">The customer set for which a new solution is desired.</param>
        /// <param name="PreserveCustomerVisitSequence">Whether this solver is constrained to use the customer visit sequence obtained when previously optimized for the GDV.</param>
        /// <returns></returns>
        VehicleSpecificRouteOptimizationOutcome Solve(CustomerSet customerSet, bool PreserveCustomerVisitSequence);
    }
}
