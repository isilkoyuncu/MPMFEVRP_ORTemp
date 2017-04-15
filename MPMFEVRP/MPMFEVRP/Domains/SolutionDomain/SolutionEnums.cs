using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public enum RouteOptimizationStatus { NotYetOptimized, WontOptimize_Duplicate, InfeasibleForBothGDVandEV, OptimizedForGDVButInfeasibleForEV, OptimizedForBothGDVandEV };

    public enum PartialSolutionComparison { IncumbentDominates, IncumbentPreferable, ChallengerPreferable, ChallengerDominates };

}
