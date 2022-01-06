namespace MPMFEVRP.Models
{
    public enum ObjectiveFunctionTypes { Minimize, Maximize };

    public enum OldObjectiveFunctions { MinimizeVMT, MinimizeVariableCost, MinimizeTotalCost, MaximizeProfit, MinimizeFuelCost };

    public enum ParameterID//When adding a new optional Cplex parameter, make sure to add it to the public static list in the XCplexParameters class and make a note next to it so we can double-check!
    {
        ALG_RUNTIME_SECONDS,
        ALG_RANDOM_POOL_SIZE,
        ALG_STOPPING_CRITERIA,
        ALG_OBTAIN_COLUMNS_UNTIL_OPT,
        ALG_PRESERVE_CUST_SEQUENCE,
        ALG_FEASIBLE_SUBSOLN_IS_ENOUGH,
        ALG_PERFORM_SWAP,
        ALG_ADD_INITIAL_COLUMNS,
        ALG_SHAKE,
        ALG_FLOWCHART,
        ALG_RANDOM_SEED,
        ALG_RESUME,
        ALG_NUM_COLUMNS_ADDED_PER_ITER,
        ALG_SOLUTION_TYPES,
        ALG_ERROR_TOLERANCE,
        ALG_MIP_EMPHASIS,//This is an optional Cplex parameter
        ALG_MIP_SEARCH,//This is an optional Cplex parameter
        ALG_CUTS_FACTOR,//This is an optional Cplex parameter
        ALG_THREADS,//This is an optional Cplex parameter
        ALG_LOG_OUTPUT_TYPE,
        ALG_EXPORT_LP_MODEL,
        ALG_RELAXATION,
        ALG_XCPLEX_FORMULATION,
        ALG_XCPLEX_OUTPUT_LEVEL,
        ALG_SELECTION_CRITERIA,
        ALG_PERCENTAGE_OF_CUSTOMERS_2SELECT,
        ALG_TOP_SELECTION_PERCENTAGE,
        ALG_RECOVERY_NEEDED,
        ALG_SET_COVER,
        ALG_RECOVERY_OPTIONS,
        ALG_PROB_SELECTION_POWER,
        ALG_SEARCH_TILIM,
        ALG_TIGHTER_AUX_BOUNDS,
        ALG_NUM_CUSTOMER_SETS_TO_REPORT_PER_CATEGORY,
        ALG_MIN_NUM_CUSTOMERS_IN_A_SET,
        ALG_MAX_NUM_CUSTOMERS_IN_A_SET,
        ALG_BEAM_WIDTH,
        ALG_NUM_RANDOM_SUBSETS_OF_CUSTOMER_SETS,
        ALG_PROB_SELECTING_A_CUSTOMER_SET,
        ALG_PROB_PRICING_USE_RUNTIME_LIMIT,
        ALG_PRICING_RUNTIME_LIMIT,
        ALG_COMPARE_TO_GDV_PRICINGPROBLEM,
        ALG_COMPARE_TO_EV_NDF_PRICINGPROBLEM,
        ALG_TSP_OPTIMIZATION_MODEL_TYPE,
        ALG_NUM_ROUTES_EACH_ITER,

        PRB_NUM_EV,
        PRB_NUM_GDV,
        PRB_USE_EXACTLY_NUM_EV_AVAILABLE,
        PRB_USE_EXACTLY_NUM_GDV_AVAILABLE,
        PRB_RECHARGING_ASSUMPTION,
        PRB_LAMBDA,
        PRB_CREATETSPSOLVERS,
        PRB_CREATEEXPLOITINGTSPSOLVER,
        PRB_CREATEPLAINTSPSOLVER,
        PROB_BKS
        //TODO put solution statistics, things you need select from the form in order to show them to user.
    }
}
