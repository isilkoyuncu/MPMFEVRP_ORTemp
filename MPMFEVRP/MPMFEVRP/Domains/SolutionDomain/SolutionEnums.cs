using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public enum RouteOptimizationStatus { NotYetOptimized, InfeasibleForBothGDVandEV, OptimizedForGDVButInfeasibleForEV, OptimizedForBothGDVandEV };

    public enum VehicleSpecificRouteOptimizationStatus { NotYetOptimized, Infeasible, Optimized };

    public enum PartialSolutionComparison { IncumbentDominates, IncumbentPreferable, ChallengerPreferable, ChallengerDominates };

    public enum CustomerSetExtensionStatus {Success, Failure};//Extended: success

}
