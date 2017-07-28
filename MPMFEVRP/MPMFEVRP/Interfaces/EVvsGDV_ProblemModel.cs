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
    public abstract class EVvsGDV_ProblemModel: ProblemModelBase
    {
        RouteOptimizerOutput roo;

        protected XCPlex_NodeDuplicatingFormulation EV_TSPSolver;
        protected XCPlex_NodeDuplicatingFormulation GDV_TSPSolver;

        double retrieve_CompTime = 0.0;
        double GDV_TSP_CompTime = 0.0;
        double EV_TSP_CompTime = 0.0;
        
        public bool GDVOptimalRouteFeasibleForEV = false;
        public List<string> RouteConstructionMethodForEV = new List<string>(); // Here for statistical purposes

        /// <summary>
        /// EVvsGDV_MaxProfit_VRP_Model.OptimizeForSingleVehicle can only be called from CustomerSet, which requests to be optimized and passes itself hereinto
        /// </summary>
        /// <param name="CS"></param>
        /// <returns></returns>
        public override RouteOptimizerOutput OptimizeForSingleVehicle(CustomerSet CS)
        {

            if (CS.NumberOfCustomers == 2)
                if(CS.Customers.Contains("C1"))
                    if (CS.Customers.Contains("C6"))
                    {
                        Console.WriteLine("This is the customer set that sets the OFV equal to total profit whereas the GDV route is not even EV-feasible!!!");
                    }

            if (archiveAllCustomerSets)
            {
                DateTime startTime = DateTime.Now;
                RouteOptimizerOutput ROO = customerSetArchive.Retrieve(CS);
                retrieve_CompTime += (DateTime.Now - startTime).TotalMilliseconds;
                if (ROO != null)
                {
                    roo = new RouteOptimizerOutput(true, ROO);
                    return roo;
                }
            }
            // TODO I think we need the following but for some reason it didn't work as intended.
            //CS.RouteOptimizerOutcome = new RouteOptimizerOutput();
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
                RouteConstructionMethodForEV.Add("CPLEX proved both infeasible");
                roo = new RouteOptimizerOutput(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
                return roo;
            }
            else//GDV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Infeasible
            {
                if (GDV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Optimal)
                {
                    System.Windows.Forms.MessageBox.Show("GDV_TSPSolver status is other than Infeasible or Optimal!");
                }
                //If we're here, we know GDV route has been successfully optimized
                assignedRoutes[1] = ExtractTheSingleRouteFromSolution((RouteBasedSolution)GDV_TSPSolver.GetCompleteSolution(typeof(RouteBasedSolution)));
                ofv[1] = GDV_TSPSolver.GetObjValue();
                //If we're here we know the optimal GDV solution, now it is time to optimize the EV route

                GDVOptimalRouteFeasibleForEV = true; //First check for EV feasibility of the route constructed for GDV, if it is feasible then it must be optimal for EV

                assignedRoutes[0] = new AssignedRoute(this, 0);
                for (int siteIndex = 1; ((siteIndex < assignedRoutes[1].SitesVisited.Count) && (GDVOptimalRouteFeasibleForEV)); siteIndex++)
                {
                    assignedRoutes[0].Extend(assignedRoutes[1].SitesVisited[siteIndex]);
                    GDVOptimalRouteFeasibleForEV = assignedRoutes[0].Feasible.Last();
                }
                if (GDVOptimalRouteFeasibleForEV)
                {
                    //seems like this is all taken care of by cloning the GDV optimal route for EV in a step-by-step manner
                    ofv[0] = assignedRoutes[0].TotalProfit;
                    RouteConstructionMethodForEV.Add("Checked Feasibility of GDV Route");
                    roo = new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForBothGDVandEV, ofv: ofv, optimizedRoute: assignedRoutes);
                    return roo;
                }
                else
                {
                    assignedRoutes[0] = new AssignedRoute();
                    ofv[0] = double.MinValue; //TODO: Revert this when working with a minimization problem
                    DateTime startTime_ev = DateTime.Now;
                    EV_TSPSolver.RefineDecisionVariables(CS);
                    //EV_TSPSolver.ExportModel("EV_TSP_model.lp"); //Turn this on if you want to export the lp model of the problem
                    EV_TSPSolver.Solve_and_PostProcess();
                    EV_TSP_CompTime += (DateTime.Now - startTime_ev).TotalMilliseconds;

                    if (EV_TSPSolver.SolutionStatus == XCPlexSolutionStatus.Infeasible)//if EV infeasible, return only GDV 
                    {
                        RouteConstructionMethodForEV.Add("CPLEX proved EV infeasibility");
                        roo = new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV, ofv: ofv, optimizedRoute: assignedRoutes);
                        return roo;
                    }
                    else//EV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Infeasible
                    {
                        if (EV_TSPSolver.SolutionStatus != XCPlexSolutionStatus.Optimal)
                        {
                            System.Windows.Forms.MessageBox.Show("EV_TSPSolver status is other than Infeasible or Optimal!");
                        }
                        //If we're here, we know GDV route has been successfully optimized
                        assignedRoutes[0] = ExtractTheSingleRouteFromSolution((RouteBasedSolution)EV_TSPSolver.GetCompleteSolution(typeof(RouteBasedSolution)));
                        ofv[0] = EV_TSPSolver.GetObjValue();

                        RouteConstructionMethodForEV.Add("Optimized with CPLEX");
                        roo = new RouteOptimizerOutput(RouteOptimizationStatus.OptimizedForBothGDVandEV, ofv: ofv, optimizedRoute: assignedRoutes);
                        return roo;
                    }
                }
            }
        }
        //It can be useful if you want to check customer set archive ever
        public void ExportCustomerSetArchive2txt()
        {
            System.IO.StreamWriter sw;
            String fileName = InputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_CustomerSetArchiveWcplex" + (!GDVOptimalRouteFeasibleForEV).ToString() + ".txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("EV Routes" + "\t" + "EV Routes Profit" + "\t" + "OFV[0]" + "\t" + "GDV Routes" + "\t" + "GDV Routes Profit" + "\t" + "OFV[1]" + "\t" + "EV Feasible" + "\t" + "GDV Feasible" + "\t" + "EV Calc Type");
            for (int i = 0; i < CustomerSetArchive.Count; i++)
            {
                if (CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0] == null && CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1] == null)
                {
                    sw.Write("null" + "\t" + "null" + "\t" + "null" + "\t" + "null" + "\t" + "null" + "\t" + "null");
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[0] + "-" + "null");
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[1] + "-" + "null");
                    sw.Write("\t" + "");//RouteConstructionMethodForEV[i]);

                }
                else if (CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited == null && CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1] != null)
                {
                    sw.Write("null" + "\t" + "null" + "\t" + "null" + "\t");
                    for (int j = 0; j < CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1; j++)
                        sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[j] + "-");
                    sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1]);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].TotalProfit);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OFV[1]);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[0] + "-" + "null");
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[1] + "-" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].Feasible.Last());
                    sw.Write("\t" + "null");
                }
                else //if(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited != null)
                {
                    for (int j = 0; j < CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited.Count - 1; j++)
                        sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited[j] + "-");
                    sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited[CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited.Count - 1]);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].TotalProfit);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OFV[0] + "\t");
                    for (int j = 0; j < CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1; j++)
                        sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[j] + "-");
                    sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1] + "-");
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].TotalProfit);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OFV[1]);
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[0] + "-" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].Feasible.Last());
                    sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[1] + "-" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].Feasible.Last());
                    sw.Write("\t" + RouteConstructionMethodForEV[i]);
                }
                sw.WriteLine();
            }
            sw.Close();
        }
    }
}
