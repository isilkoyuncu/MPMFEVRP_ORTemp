namespace MPMFEVRP.Domains.SolutionDomain
{
    public enum RouteOptimizationStatus { NotYetOptimized, InfeasibleForBothGDVandEV, OptimizedForGDVButNotYetOptimizedForEV, OptimizedForGDVButInfeasibleForEV, OptimizedForBothGDVandEV };

    public enum VehicleSpecificRouteOptimizationStatus { NotYetOptimized, Infeasible, Optimized };

    public enum PartialSolutionComparison { IncumbentDominates, IncumbentPreferable, ChallengerPreferable, ChallengerDominates };

    public enum CustomerSetExtensionStatus {Success, Failure};//Extended: success
}
