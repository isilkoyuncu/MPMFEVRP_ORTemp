using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EVvsGDV_MaxProfit_VRP_Model: ProblemModelBase
    {
        string problemName;
        public EVvsGDV_MaxProfit_VRP_Model()
        {
            EVvsGDV_MaxProfit_VRP problem = new EVvsGDV_MaxProfit_VRP();
            problemName = problem.GetName();
        }//empty constructor
        public EVvsGDV_MaxProfit_VRP_Model(EVvsGDV_MaxProfit_VRP problem)
        {
            pdp.InputFileName = problem.PDP.InputFileName;
            pdp.SRD = new SiteRelatedData(problem.PDP.SRD);
            pdp.VRD = new VehicleRelatedData(problem.PDP.VRD);
            pdp.CRD = new ContextRelatedData(problem.PDP.CRD);
            problemName = problem.GetName();

            EV_TSPSolver = new Models.XCPlex.XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
            GDV_TSPSolver = new Models.XCPlex.XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }
        public override string GetName()
        {
            return "EV vs GDV Profit Maximization Problem's Model";
        }
        public override string GetNameOfProblemOfModel()
        {
            return problemName;
        }

        /// <summary>
        /// EVvsGDV_MaxProfit_VRP_Model.OptimizeForSingleVehicle can only be called from CustomerSet, which requests to be optimized and passes itself hereinto
        /// </summary>
        /// <param name="CS"></param>
        /// <returns></returns>
        public RouteOptimizerOutput OptimizeForSingleVehicle(CustomerSet CS)
        {
            double worstCaseOFV = double.MinValue; //Revert this when working with a minimization problem

            AssignedRoute[] assignedRoutes = new AssignedRoute[2];
            double[] ofv = new double[] { worstCaseOFV, worstCaseOFV };

            //GDV First: if it is infeasible, no need to check EV
            GDV_TSPSolver.RefineDecisionVariables(CS);
            GDV_TSPSolver.Solve_and_PostProcess();        
            if (GDV_TSPSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)//if GDV infeasible, no need to check EV, stop
            {
                return new RouteOptimizerOutput(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
            }
            else//GDV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Infeasible
            {
                if(GDV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Optimal)
                {
                    //TODO Figure out clearly and get rid of both or at least one
                    System.Windows.Forms.MessageBox.Show("GDV_TSPSolver status is other than Infeasible or Optimal!");
                    throw new Exception ("GDV_TSPSolver status is other than Infeasible or Optimal!");
                }
                //If we're here, we know GDV route has been successfully optimized
                assignedRoutes[1] = extractTheSingleRouteFromSolution(GDV_TSPSolver.GetCompleteSolution());
                ofv[1] = GDV_TSPSolver.GetBestObjValue();
                //If we're here we know the optimal GDV solution, now it is time to optimize the EV route
                EV_TSPSolver.RefineDecisionVariables(CS);
                EV_TSPSolver.Solve_and_PostProcess();
                if(EV_TSPSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)//if EV infeasible, return only GDV 
                {
                    return new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV, ofv: ofv, optimizedRoute: assignedRoutes);
                }
                else//EV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Infeasible
                {
                    if (EV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Optimal)
                    {
                        //TODO Figure out clearly and get rid of both or at least one
                        System.Windows.Forms.MessageBox.Show("EV_TSPSolver status is other than Infeasible or Optimal!");
                        throw new Exception("GDV_TSPSolver status is other than Infeasible or Optimal!");
                    }
                    //If we're here, we know GDV route has been successfully optimized
                    assignedRoutes[0] = extractTheSingleRouteFromSolution(EV_TSPSolver.GetCompleteSolution());
                    ofv[0] = EV_TSPSolver.GetBestObjValue();
                    return new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForBothGDVandEV, ofv: ofv, optimizedRoute: assignedRoutes);
                }
            }
        }

        AssignedRoute extractTheSingleRouteFromSolution(NewCompleteSolution ncs)
        {
            if (ncs.Routes.Count != 1)
            {
                //This is a problem!
                System.Windows.Forms.MessageBox.Show("Single vehicle optimization resulted in none or multiple AssignedRoute in a Solution!");
                throw new Exception("Single vehicle optimization resulted in none or multiple AssignedRoute in a Solution!");
            }
            return ncs.Routes[0];
        }

        public override ISolution GetRandomSolution(int seed, Type solutionType)
        {
            throw new NotImplementedException();
        }

        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            Type solutionType = solution.GetType();

            if (solutionType == typeof(CustomerSetBasedSolution))
            {
                CustomerSetBasedSolution csbs = (CustomerSetBasedSolution)solution;
                return CheckFeasibilityOfSolution(csbs);
            }
            //else if (solutionType == typeof(RouteBasedSolution))
            //{
            //    RouteBasedSolution rbs = (RouteBasedSolution)solution;
            //    return CheckFeasibilityOfSolution(rbs);
            //}
            else
            {
                System.Windows.Forms.MessageBox.Show("solution plugged into EVvsGDV_MaxProfit_VRP_Model.CheckFeasibilityOfSolution is incompatible!");
                return false;
            }
        }
        bool CheckFeasibilityOfSolution(CustomerSetBasedSolution solution)
        {
            bool outcome = true;
            //TODO check for any infeasibility and return false as soon as one is found!
            return outcome;
        }

        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            throw new NotImplementedException();

        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        void PopulateCompatibleSolutionTypes()
        {
            compatibleSolutions = new List<Type>()
            {
                typeof(RouteBasedSolution),
                typeof(CustomerSetBasedSolution)
            };
        }

        
    }
}
