using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Models
{
    public enum ObjectiveFunctionTypes { Minimize, Maximize };

    public enum ParameterID//When adding a new optional Cplex parameter, make sure to add ity to the public static list in the XCplexParameters class and make a note next to it so we can double-check!
    {
        RUNTIME_SECONDS,
        RANDOM_POOL_SIZE,
        RANDOM_SEED,
        SOLUTION_TYPES,
        ERROR_TOLERANCE,
        MIP_EMPHASIS,//This is an optional Cplex parameter
        MIP_SEARCH,//This is an optional Cplex parameter
        CUTS_FACTOR,//This is an optional Cplex parameter
        THREADS,//This is an optional Cplex parameter
        RELAXATION,
        XCPLEX_FORMULATION,
        SELECTION_CRITERIA,
        PERCENTAGE_OF_CUSTOMERS_2SELECT,
        TOP_SELECTION_PERCENTAGE,
        RECOVERY_NEEDED,
        SET_COVER,
        RECOVERY_OPTIONS,
        PROB_SELECTION_POWER
    }
}
