using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.CustomerSetSolvers.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Domains.AlgorithmDomain;
using System.Diagnostics;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Models.CustomerSetSolvers.CustomerSetSolverWithConstraintRelaxation
{
    public class CustomerSetSolverWithRelaxatingEnergyConstraint
    {
        readonly EVvsGDV_ProblemModel theProblemModel;
        readonly Vehicle theAFV;
        readonly Vehicle theGDV;
        public readonly ETSPSolver EV_Solver;
        public readonly TSPSolver CV_Solver;
        Stopwatch stopwatch = new Stopwatch();
        double tilim = 60.0;
        public OptimizationStatistics optimizationStatstics;

        public CustomerSetSolverWithRelaxatingEnergyConstraint(EVvsGDV_ProblemModel theProblemModel)
        {
            this.theProblemModel = theProblemModel;
            theAFV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
            theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);

            EV_Solver = new ETSPSolver(theProblemModel);
            CV_Solver = new TSPSolver(theProblemModel);
        }
    }
}
