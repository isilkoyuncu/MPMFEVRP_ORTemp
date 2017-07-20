﻿using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Models;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.Solutions;

namespace MPMFEVRP.Interfaces
{
    public abstract class ProblemModelBase : IProblemModel
    {
        protected string inputFileName; // This is not for reading but just for record keeping and reporting 
        public string InputFileName { get { return inputFileName; } set { inputFileName=value; } }

        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        protected CustomerCoverageConstraint_EachCustomerMustBeCovered coverConstraintType;
        public CustomerCoverageConstraint_EachCustomerMustBeCovered CoverConstraintType { get { return coverConstraintType; }set { coverConstraintType = value; } }

        protected ProblemDataPackage pdp;
        public SiteRelatedData SRD { get { return pdp.SRD; } }
        public VehicleRelatedData VRD { get { return pdp.VRD; } }
        public ContextRelatedData CRD { get { return pdp.CRD; } }

        protected XCPlex_NodeDuplicatingFormulation EV_TSPSolver;
        protected XCPlex_NodeDuplicatingFormulation GDV_TSPSolver;

        protected bool archiveAllCustomerSets; public bool ArchiveAllCustomerSets { get { return archiveAllCustomerSets; } }
        protected CustomerSetList customerSetArchive; public CustomerSetList CustomerSetArchive { get { return customerSetArchive; } }

        double retrieve_CompTime = 0.0;
        double GDV_TSP_CompTime = 0.0;
        double EV_TSP_CompTime = 0.0;

        /// <summary>
        /// EVvsGDV_MaxProfit_VRP_Model.OptimizeForSingleVehicle can only be called from CustomerSet, which requests to be optimized and passes itself hereinto
        /// </summary>
        /// <param name="CS"></param>
        /// <returns></returns>
        public RouteOptimizerOutput OptimizeForSingleVehicle(CustomerSet CS)
        {
            // TODO you can delete this after the debugging
            RouteOptimizerOutput outcome = new RouteOptimizerOutput(RouteOptimizationStatus.NotYetOptimized);
            if (archiveAllCustomerSets)
            {
                DateTime startTime = DateTime.Now;
                RouteOptimizerOutput ROO = customerSetArchive.Retrieve(CS);
                retrieve_CompTime += (DateTime.Now - startTime).TotalMilliseconds;
                if (ROO != null)
                {
                    outcome = new RouteOptimizerOutput(true, ROO);
                    return outcome;
                }
            }
            customerSetArchive.Add(CS);

            double worstCaseOFV = double.MinValue; //TODO: Revert this when working with a minimization problem

            AssignedRoute[] assignedRoutes = new AssignedRoute[2];
            double[] ofv = new double[] { worstCaseOFV, worstCaseOFV };

            //GDV First: if it is infeasible, no need to check EV

            DateTime startTime_gdv = DateTime.Now;
            GDV_TSPSolver.RefineDecisionVariables(CS);
            GDV_TSPSolver.Solve_and_PostProcess();
            GDV_TSP_CompTime += (DateTime.Now - startTime_gdv).TotalMilliseconds;

            if (GDV_TSPSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)//if GDV infeasible, no need to check EV, stop
            {
                outcome = new RouteOptimizerOutput(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
                return outcome;
            }
            else//GDV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Infeasible
            {
                if (GDV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Optimal)
                {
                    //TODO Figure out clearly and get rid of both or at least one
                    System.Windows.Forms.MessageBox.Show("GDV_TSPSolver status is other than Infeasible or Optimal!");
                    throw new Exception("GDV_TSPSolver status is other than Infeasible or Optimal!");
                }
                //If we're here, we know GDV route has been successfully optimized
                assignedRoutes[1] = ExtractTheSingleRouteFromSolution((RouteBasedSolution)GDV_TSPSolver.GetCompleteSolution(typeof(RouteBasedSolution)));
                ofv[1] = GDV_TSPSolver.GetBestObjValue();
                //If we're here we know the optimal GDV solution, now it is time to optimize the EV route

                // TODO first check for EV feasibility of the route constructed for GDV, if it is feasible then it must be optimal for EV
                bool GDVOptimalRouteFeasibleForEV = true;
                //                assignedRoutes[0] = new AssignedRoute(this, 0);
                AssignedRoute newAR = new AssignedRoute(this, 0);
                for (int siteIndex = 1; ((siteIndex < assignedRoutes[1].SitesVisited.Count) && (GDVOptimalRouteFeasibleForEV)); siteIndex++)
                {
                    //                    assignedRoutes[0].Extend(assignedRoutes[1].SitesVisited[siteIndex]);
                    //                    GDVOptimalRouteFeasibleForEV = assignedRoutes[0].Feasible.Last();
                    newAR.Extend(assignedRoutes[1].SitesVisited[siteIndex]);
                    GDVOptimalRouteFeasibleForEV = newAR.Feasible.Last();
                }
                double newOFV = newAR.TotalProfit;

                //if (GDVOptimalRouteFeasibleForEV)
                //{
                //    //seems like this is all taken care of by cloning the DGV optimal route for EV in a step-by-step manner
                //    ofv[0] = assignedRoutes[0].TotalProfit;
                //    return new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForBothGDVandEV, ofv: ofv, optimizedRoute: assignedRoutes);
                //}
                //else
                //{

                if (CS.NumberOfCustomers == 2)
                    if (CS.Customers[0] == "C15")
                        if (CS.Customers[1] == "C4")
                            System.Windows.Forms.MessageBox.Show("This is the suspicious instance");

                DateTime startTime_ev = DateTime.Now;
                EV_TSPSolver.RefineDecisionVariables(CS);
                //EV_TSPSolver.ExportModel("EV_TSP_model.lp");
                EV_TSPSolver.Solve_and_PostProcess();
                EV_TSP_CompTime += (DateTime.Now - startTime_ev).TotalMilliseconds;

                if (EV_TSPSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)//if EV infeasible, return only GDV 
                {
                    outcome = new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV, ofv: ofv, optimizedRoute: assignedRoutes);
                    return outcome;
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
                    assignedRoutes[0] = ExtractTheSingleRouteFromSolution((RouteBasedSolution)EV_TSPSolver.GetCompleteSolution(typeof(RouteBasedSolution)));
                    ofv[0] = EV_TSPSolver.GetBestObjValue();

                    if (GDVOptimalRouteFeasibleForEV)
                    {
                        //We'll compare the two EV  routes
                        if ((!newAR.IsSame(assignedRoutes[0])) || (Math.Abs(newOFV - ofv[0]) > 0.00001))
                            System.Windows.Forms.MessageBox.Show("The GDV-optimal route that happens to be EV-feasible somehow is not the same as the EV-optimal obtained independently!");
                    }

                    outcome = new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForBothGDVandEV, ofv: ofv, optimizedRoute: assignedRoutes);
                    return outcome;
                }
                //}
            }
        }

        public List<string> GetAllCustomerIDs()
        {
            List<string> outcome = new List<string>();
            foreach (Site s in SRD.SiteArray)
                if (s.SiteType == SiteTypes.Customer)
                    outcome.Add(s.ID);
            return outcome;
        }

        AssignedRoute ExtractTheSingleRouteFromSolution(RouteBasedSolution ncs)
        {
            if (ncs.Routes.Count != 1)
            {
                //This is a problem!
                System.Windows.Forms.MessageBox.Show("Single vehicle optimization resulted in none or multiple AssignedRoute in a Solution!");
                throw new Exception("Single vehicle optimization resulted in none or multiple AssignedRoute in a Solution!");
            }
            return ncs.Routes[0];
        }


        public abstract string GetName();
        public abstract string GetDescription();
        public abstract string GetNameOfProblemOfModel();

        protected List<Type> compatibleSolutions;
        public List<Type> GetCompatibleSolutions() { return compatibleSolutions; }

        public abstract ISolution GetRandomSolution(int seed, Type SolutionType);
        public abstract bool CheckFeasibilityOfSolution(ISolution solution);
        public abstract double CalculateObjectiveFunctionValue(ISolution solution);
        public abstract bool CompareTwoSolutions(ISolution solution1, ISolution solution2);

        protected bool IsSolutionTypeCompatible(Type solutionType)
        {
            return compatibleSolutions.Contains(solutionType);
        }
    }
}
