namespace MPMFEVRP.Models
{
    public enum ObjectiveFunctionTypes { Minimize, Maximize };

    public enum ObjectiveFunctions { MinimizeVMT, MinimizeVariableCost, MinimizeTotalCost, MaximizeProfit };

    public enum ParameterID//When adding a new optional Cplex parameter, make sure to add ity to the public static list in the XCplexParameters class and make a note next to it so we can double-check!
    {
        ALG_RUNTIME_SECONDS,
        ALG_RANDOM_POOL_SIZE,
        ALG_RANDOM_SEED,
        ALG_SOLUTION_TYPES,
        ALG_ERROR_TOLERANCE,
        ALG_MIP_EMPHASIS,//This is an optional Cplex parameter
        ALG_MIP_SEARCH,//This is an optional Cplex parameter
        ALG_CUTS_FACTOR,//This is an optional Cplex parameter
        ALG_THREADS,//This is an optional Cplex parameter
        ALG_RELAXATION,
        ALG_XCPLEX_FORMULATION,
        ALG_SELECTION_CRITERIA,
        ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT,
        ALG_TOP_SELECTION_PERCENTAGE,
        ALG_RECOVERY_NEEDED,
        ALG_SET_COVER,
        ALG_RECOVERY_OPTIONS,
        ALG_PROB_SELECTION_POWER,
        ALG_TIGHTER_AUX_BOUNDS,

        PRB_NUM_EV,
        PRB_NUM_GDV,
        PRB_USE_EXACTLY_NUM_EV_AVAILABLE,
        PRB_USE_EXACTLY_NUM_GDV_AVAILABLE,
        PRB_RECHARGING_ASSUMPTION,
        PRB_LAMBDA

        //TODO put solution statistics, things you need select from the form in order to show them to user.
    }
}
