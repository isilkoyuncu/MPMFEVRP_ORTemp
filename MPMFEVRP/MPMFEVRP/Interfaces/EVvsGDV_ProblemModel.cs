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



        public override RouteOptimizationOutcome OptimizeRoute(CustomerSet CS, List<Vehicle> vehicles)
        {
            //This method is designed to work with exactly one GDV and exactly one EV
            int GDVPositionInList = 0;
            int EVPositionInList = 1;
            if (vehicles.Count != 2)
                throw new Exception("EVvsGDV_ProblemModel.OptimizeRoute must be invoked with a list of 2 vehicles!");
            if (vehicles[0].Category== vehicles[1].Category)
                throw new Exception("EVvsGDV_ProblemModel.OptimizeRoute must be invoked with exactly one GDV and one EV!");
            if (vehicles[0].Category == VehicleCategories.EV)
            {
                GDVPositionInList = 1;
                EVPositionInList = 0;
            }

            List<VehicleSpecificRouteOptimizationOutcome> theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            VehicleSpecificRouteOptimizationOutcome vsroo_GDV = OptimizeRoute(CS, vehicles[GDVPositionInList]);
            if (vsroo_GDV.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
                return new RouteOptimizationOutcome(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
            theList.Add(vsroo_GDV);
            VehicleSpecificRouteOptimizationOutcome vsroo_EV = OptimizeRoute(CS, vehicles[EVPositionInList], GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
            theList.Add(vsroo_EV);
            return new RouteOptimizationOutcome(theList);
        }
        public override VehicleSpecificRouteOptimizationOutcome OptimizeRoute(CustomerSet CS, Vehicle vehicle, AssignedRoute GDVOptimalRoute=null)
        {
            //This method has nothing to do with a list of previously generated customer sets, management of that is completely some other class's responsibility

            if ((vehicle.Category == VehicleCategories.GDV) || (GDVOptimalRoute == null))
                return OptimizeRoute(CS, vehicle);
            //else: we are looking at an EV problem with a provided GDV-optimal route
            //First, we must check for AFV-feasibility of the provided GDV-optimal route
            //Make an AFV-optimized route out of the given GDV-optimized route:
            AssignedRoute fittedRoute = FitGDVOptimalRouteToAFV(GDVOptimalRoute, vehicle);
            if (fittedRoute.Feasible.Last())//if the fitted route is feasible:
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, VehicleSpecificRouteOptimizationStatus.Optimized, objectiveFunctionValue: fittedRoute.TotalProfit, optimizedRoute: fittedRoute);
            if (ProveAFVInfeasibilityOfCustomerSet(CS, GDVOptimalRoute: GDVOptimalRoute))
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, VehicleSpecificRouteOptimizationStatus.Infeasible);
            //If none of the previous conditions worked, we must solve an EV-TSP
            return OptimizeRoute(CS, vehicle);
        }
        bool CheckAFVFeasibilityOfGDVOptimalRoute(VehicleSpecificRouteOptimizationOutcome GDVOptimalRoute)
        {
            //Return true if and only if the GDV-Optimal Route is AFV-Feasible (in which case it's known to be AFV-Optimal as well

            if (GDVOptimalRoute == null)//The GDVOptimalRoute that was not provided cannot be feasible
                return false;

            return false;//TODO Do the actual checking here!
        }
        AssignedRoute FitGDVOptimalRouteToAFV(AssignedRoute GDVOptimalRoute, Vehicle vehicle)
        {
            AssignedRoute outcome = new AssignedRoute(this, 0);
            bool GDVOptimalRouteFeasibleForEV = true;
            for (int siteIndex = 1; ((siteIndex < GDVOptimalRoute.SitesVisited.Count) && (GDVOptimalRouteFeasibleForEV)); siteIndex++)
            {
                outcome.Extend(GDVOptimalRoute.SitesVisited[siteIndex]);
                GDVOptimalRouteFeasibleForEV = outcome.Feasible.Last();
            }
            return outcome;
        }
        bool ProveAFVInfeasibilityOfCustomerSet(CustomerSet CS, AssignedRoute GDVOptimalRoute = null)
        {
            //Return true if and only if the GDV-Optimal route must be AFV-infeasible based on the data
            //This method may be a little confusing because it returns true when infeasible
            //For now we ignore this method //TODO: Develop the analytically provable conditions of AFV-infeasibility of a given GDV-feasible route

            return false;
        }
        VehicleSpecificRouteOptimizationOutcome OptimizeRoute(CustomerSet CS, Vehicle vehicle)
        {
            XCPlex_NodeDuplicatingFormulation solver = (vehicle.Category == VehicleCategories.GDV ? GDV_TSPSolver : EV_TSPSolver); //TODO this needs to be fixed: if GDV infeasible, then we shouldn't try to optimize it for EV here
            solver.RefineDecisionVariables(CS);
            solver.Solve_and_PostProcess();
            if (solver.SolutionStatus == XCPlexSolutionStatus.Infeasible)
                return new VehicleSpecificRouteOptimizationOutcome(vehicle.Category, VehicleSpecificRouteOptimizationStatus.Infeasible);
            else//optimal
                return new VehicleSpecificRouteOptimizationOutcome(vehicle.Category, VehicleSpecificRouteOptimizationStatus.Optimized, objectiveFunctionValue: solver.GetObjValue(), optimizedRoute: ExtractTheSingleRouteFromSolution((RouteBasedSolution)solver.GetCompleteSolution(typeof(RouteBasedSolution))));
        }

        public VehicleSpecificRoute GetVSRFromFlowVariables(Vehicle vehicle, List<Tuple<int, int, int>> allXSetTo1)
        {
            List<string> nondepotSiteIDsInOrder = new List<string>();
            int vehicleCategoryIndex = (vehicle.Category == VehicleCategories.EV ? 0 : 1);
            int lastSiteIndex = -1;
            string lastSiteID = "";

            //first determining the number of routes
            List<Tuple<int, int, int>> tobeRemoved = new List<Tuple<int, int, int>>();
            foreach (Tuple<int, int, int> x in allXSetTo1)
                if ((x.Item1 == 0) && (x.Item3 == vehicleCategoryIndex))
                {
                    lastSiteIndex = x.Item2;
                    lastSiteID = SRD.SiteArray[lastSiteIndex].ID;
                    nondepotSiteIDsInOrder.Add(lastSiteID);
                    tobeRemoved.Add(x);
                }
            if (tobeRemoved.Count != 1)
                throw new Exception("EVvsGDV_ProblemModel.GetVSRFromFlowVariables invoked with allXSetTo1 including multiple departures from the depot!");
            foreach (Tuple<int, int, int> x in tobeRemoved)
            {
                allXSetTo1.Remove(x);
            }
            tobeRemoved.Clear();
            //Next, completeing the routes one-at-a-time
            int lastSiteIndex_2 = -1;
            bool extensionDetected = false;
            while ((lastSiteIndex != 0) && (allXSetTo1.Count > 0))
            {
                lastSiteIndex_2 = lastSiteIndex;
                extensionDetected = false;
                foreach (Tuple<int, int, int> x in allXSetTo1)
                {
                    if (x.Item1 == lastSiteIndex_2)
                    {
                        lastSiteIndex = x.Item2;
                        lastSiteID = SRD.SiteArray[lastSiteIndex].ID;
                        nondepotSiteIDsInOrder.Add(lastSiteID);
                        allXSetTo1.Remove(x);
                        extensionDetected = true;
                        break;
                    }
                }
                if (!extensionDetected)
                    throw new Exception("Infeasible complete solution due to an incomplete route!");
            }

            if (allXSetTo1.Count > 0)
                throw new Exception("Infeasible complete solution due to subtours or routes that don't start/end at the depot");

            return new VehicleSpecificRoute(this, vehicle, nondepotSiteIDsInOrder);
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
                    sw.Write("\t" + RouteConstructionMethodForEV[i]);

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
