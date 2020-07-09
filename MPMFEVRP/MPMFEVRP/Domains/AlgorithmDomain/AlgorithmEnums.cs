namespace MPMFEVRP.Domains.AlgorithmDomain
{

    public enum Exploiting_GDVs_Flowchart { a_NoExploiting, b_OnlyIdenticalRoutes, c_PathInsertedRoutes, d_PathInsertedAndSwappedRoutes, e_DataCollection };

    public enum Recovery_Options { AssignmentProbByCPLEX, AnalyticallySolve}

    public enum Selection_Criteria { CompleteUniform, UniformAmongTheBestPercentage, WightedProbSelecAmongTheBestPercentage, WeightedNormalizedProbSelection, UsingShadowPrices, NearestNeighbor };

    public enum Stopping_Criteria {IterationNumber, TimeLimit};

    public enum TSPSolverType { GDVExploiter,PlainAFVSolver,OldiesADF};

    public enum XCPlexSolutionStatus { NotYetSolved = -2, Infeasible, NoFeasibleSolutionFound, Feasible, Optimal };
    //enum values:
    //Unsolved until Solve() is tried, the other four are the same as AlgorithmSolutionStatus

    public enum XGurobiSolutionStatus { NotYetSolved = 1, Optimal, Infeasible, Inf_or_Unbd, Unbounded, Cutoff, Iteration_Limit, Node_Limit, Time_Limit, Solution_Limit, Interrupted, Numeric, Suboptimal, Inprogress };
    //enum values:
    //Unsolved until Solve() is tried

    public enum AlgorithmSolutionStatus { NotYetSolved = -2, Infeasible, NoFeasibleSolutionFound, Feasible, Optimal };
    //enum values:
    //Infeasible (-1): An exhaustive search proved that no feasible solution exists
    //NoFeasibleSolutionFound (0): The search didn't exhaust the solution space; no feasible solution was found however whether a feasible solution exists is unknown
    //Feasible (1): At least one feasible solution was found, however whether an optimal solution has been found is unknown because the search didn't exhaust the solution space
    //Optimal (2): An exhaustive search proved that the optimal solution has been obtained

    public enum XCPlex_Formulation { NodeDuplicating, ArcDuplicating, NodeDuplicatingwoU, ArcDuplicatingwoU, EVRPwRefuelingPaths };

    public enum XCPlexRelaxation { None, LinearProgramming };
    //enum values:
    // None: Full IP with integer variables
    // LinearProgramming: full formulation, only the variables are relaxed to be continuous

    public enum XGurobiRelaxation { None, LinearProgramming };
    //enum values:
    // None: Full IP with integer variables
    // LinearProgramming: full formulation, only the variables are relaxed to be continuous

    public enum CPlex_MIPEmphasisSwitch
    {
        _0,
        _1,
        _2,
        _3,
        _4
    };
    //public enum CPlex_MIPEmphasisSwitch
    //{
    //    [Description("Balanced")]
    //    _0,
    //    [Description("Feasibility")]
    //    _1,
    //    [Description("Optimality")]
    //    _2,
    //    [Description("Best bound")]
    //    _3,
    //    [Description("Hidden feasible")]
    //    _4
    //};

    public enum CPlex_MIPDynamicSearchSwitch
    {
        _0,
        _1,
        _2
    };

    //public enum CPlex_MIPDynamicSearchSwitch
    //{
    //    [Description("Auto: let CPlex choose")]
    //    _0,
    //    [Description("Traditional: Apply traditional B&C, disable dynamic search")]
    //    _1,
    //    [Description("Dynamic: Apply dynamic search")]
    //    _2
    //};
    public enum Gurobi_MIPFocusSwitch
    {
        _0,
        _1,
        _2,
        _3
    };

    //public enum Gurobi_MIPFocusSwitch
    //{
    //    [Description("Balanced")]
    //    _0,
    //    [Description("Feasibility")]
    //    _1,
    //    [Description("Optimality")]
    //    _2,
    //    [Description("Best bound")]
    //    _3,
    //};

}
