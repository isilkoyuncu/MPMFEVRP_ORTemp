﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Utils
{
    /// <summary>
    /// Designed for the case of both optimal.
    /// </summary>
    public class OptimizationStatistics
    {
        int nCustomers;
        RouteOptimizationStatus ros;
        string customers;
        double t0GDVSoln;
        double t1CheckAFVfeas;
        double t2CheckAFVinfeas;
        double t3RefuelingPathInsert;
        double t4SwapAndInsert;
        double t5AFVSoln;
        int idealStopAfter;

        double vmt3RefuelingPathInsert;
        double vmt4SwapAndInsert;
        double vmt5AFVSoln;

        double GDV_comp_time;        public double GDV_Comp_Time => GDV_comp_time;
        double GDV_vmt = double.MaxValue;        public double GDV_Vmt => GDV_vmt;
        string GDV_route;        public string GDV_Route => GDV_route;
        double AFV_comp_time;        public double AFV_Comp_Time => AFV_comp_time;
        double AFV_vmt = double.MaxValue;        public double AFV_Vmt => AFV_vmt;
        string AFV_route;        public string AFV_Route => AFV_route;
        double VMTdifference = double.MaxValue;        public double VMTDifference => VMTdifference;
        int nESVisits;        public int NESVisits => nESVisits;

        string outcomeStatus;
        double epsilon = 0.0001;

        string bestRoute;

        double vmtImprovementPercent34;
        double vmtImprovementPercent45;
        double vmtImprovementPercent35;

        string afv_nonES_route = "";
        string routeDifference = "";

        int nDifferentPositions; //0, 1, 2
        public int NDifferentPositions => nDifferentPositions;

        public OptimizationStatistics(int nCustomers, RouteOptimizationOutcome roo, List<string> customers, double t0GDVSoln, double t1CheckAFVfeas, double t2CheckAFVinfeas, double t3RefuelingPathInsert, double t4SwapAndInsert, double t5AFVSoln, int idealStopAfter,
            double vmt3RefuelingPathInsert,
            double vmt4SwapAndInsert,
            double vmt5AFVSoln,
            List<string> bestRoute)
        {
            if (roo == null)
                throw new ArgumentNullException();

            this.customers = String.Join("-", customers);

            this.nCustomers = nCustomers;
            ros = roo.GetRouteOptimizationStatus();

            this.t0GDVSoln = t0GDVSoln;
            this.t1CheckAFVfeas = t1CheckAFVfeas;
            this.t2CheckAFVinfeas = t2CheckAFVinfeas;
            this.t3RefuelingPathInsert = t3RefuelingPathInsert;
            this.t4SwapAndInsert = t4SwapAndInsert;
            this.t5AFVSoln = t5AFVSoln;

            this.idealStopAfter = idealStopAfter;
            outcomeStatus = "";

            this.vmt3RefuelingPathInsert = vmt3RefuelingPathInsert;
            this.vmt4SwapAndInsert = vmt4SwapAndInsert;
            this.vmt5AFVSoln = vmt5AFVSoln;

            this.bestRoute = String.Join("-", bestRoute);

            switch (roo.GetRouteOptimizationStatus())
            {
                case RouteOptimizationStatus.NotYetOptimized:
                case RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV:
                    throw new ArgumentOutOfRangeException("GDV_AFV_OptimizationDifferences can't be run for a customer that has not yet been optimized!");
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV:
                    outcomeStatus = "InfDetectedAfterGDVSoln";//0
                    GDV_comp_time = this.t0GDVSoln;
                    AFV_comp_time = this.t0GDVSoln;
                    break;
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                    if (idealStopAfter == 2)
                        outcomeStatus = "InfDetectedByCSInfProven";//2
                    else if (idealStopAfter == 5)
                        outcomeStatus = "InfDetectedAfterAFVSoln";//5
                    GDV_comp_time = this.t0GDVSoln;
                    GDV_route = String.Join("-", roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs);
                    GDV_vmt = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled();
                    AFV_comp_time = this.t0GDVSoln + this.t1CheckAFVfeas + this.t2CheckAFVinfeas + this.t3RefuelingPathInsert + this.t4SwapAndInsert + this.t5AFVSoln;
                    break;
                case RouteOptimizationStatus.OptimizedForBothGDVandEV:
                    VehicleSpecificRouteOptimizationOutcome ROO_GDV = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV);
                    VehicleSpecificRouteOptimizationOutcome ROO_AFV = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.EV);
                    GDV_comp_time = t0GDVSoln;
                    GDV_vmt = ROO_GDV.VSOptimizedRoute.GetVehicleMilesTraveled();
                    AFV_comp_time = t0GDVSoln + t1CheckAFVfeas + t2CheckAFVinfeas + t3RefuelingPathInsert + t4SwapAndInsert + t5AFVSoln;
                    AFV_vmt = ROO_AFV.VSOptimizedRoute.GetVehicleMilesTraveled();
                    VMTdifference = ROO_AFV.VSOptimizedRoute.GetVehicleMilesTraveled() - ROO_GDV.VSOptimizedRoute.GetVehicleMilesTraveled();
                    VehicleSpecificRoute Route_AFV = ROO_AFV.VSOptimizedRoute;
                    AFV_route = String.Join("-", Route_AFV.ListOfVisitedNonDepotSiteIDs);
                    afv_nonES_route = String.Join("-", Route_AFV.ListOfVisitedCustomerSiteIDs);
                    GDV_route = String.Join("-", ROO_GDV.VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs);
                    nESVisits = Route_AFV.ListOfVisitedNonDepotSiteIDs.Count - Route_AFV.NumberOfCustomersVisited;
                    if (idealStopAfter == 1)
                        outcomeStatus = "OptimalityProvenAfterGDVSoln";//1
                    else if (idealStopAfter == 3)
                        outcomeStatus = "FeasibilityProvenAfterPathInsert";//3
                    else if (idealStopAfter == 4)
                        if (vmt3RefuelingPathInsert != 0.0)
                            outcomeStatus = "FeasibilityProvenAfterPathInsert";//3
                        else
                            outcomeStatus = "FeasibilityProvenAfterSwapAndPathInsert";//4
                    else if (idealStopAfter == 5)
                        if (vmt3RefuelingPathInsert != 0.0)
                            outcomeStatus = "FeasibilityProvenAfterPathInsert";//3
                        else if (vmt4SwapAndInsert != 0.0)
                            outcomeStatus = "FeasibilityProvenAfterSwapAndPathInsert";//4
                        else
                            outcomeStatus = "OptimalityProvenAfterAFVSoln";//5

                    if (vmt3RefuelingPathInsert != 0.0 && vmt4SwapAndInsert != 0.0)
                        vmtImprovementPercent34 = (vmt3RefuelingPathInsert - vmt4SwapAndInsert) / vmt4SwapAndInsert;
                    else if (vmt4SwapAndInsert != 0.0)
                        vmtImprovementPercent34 = 1.0;

                    if (vmt3RefuelingPathInsert != 0.0 && vmt5AFVSoln != 0.0)
                        vmtImprovementPercent35 = (vmt3RefuelingPathInsert - vmt5AFVSoln) / vmt5AFVSoln;
                    else if (vmt5AFVSoln != 0.0)
                        vmtImprovementPercent35 = 1.0;

                    if (vmt4SwapAndInsert != 0.0 && vmt5AFVSoln != 0.0)
                        vmtImprovementPercent45 = (vmt4SwapAndInsert - vmt5AFVSoln) / vmt5AFVSoln;
                    else if (vmt5AFVSoln != 0.0)
                        vmtImprovementPercent45 = 1.0;

                    List<string> SymElim_ListOfCustomers_AFV = Route_AFV.ListOfVisitedCustomerSiteIDs;
                    if (SymElim_ListOfCustomers_AFV.First().CompareTo(SymElim_ListOfCustomers_AFV.Last()) > -1)
                        SymElim_ListOfCustomers_AFV.Reverse();
                    List<string> SymElim_ListOfCustomers_GDV = ROO_GDV.VSOptimizedRoute.ListOfVisitedCustomerSiteIDs;
                    if (SymElim_ListOfCustomers_GDV.First().CompareTo(SymElim_ListOfCustomers_GDV.Last()) > -1)
                        SymElim_ListOfCustomers_GDV.Reverse();
                    if (SymElim_ListOfCustomers_AFV.Count != SymElim_ListOfCustomers_GDV.Count)
                        throw new DifferentRoutesVisitDifferentSetsOfCustomersException(); //Exception("GDV and AFV optimal routes visit different sets of customers!");
                    nDifferentPositions = 0;
                    if (SymElim_ListOfCustomers_GDV.Count == 1)
                    {
                        if (nESVisits == 0)
                            routeDifference = "identical";
                        else
                            routeDifference = "sameSequence";
                    }
                    else
                    {
                        for (int p = 0; p < SymElim_ListOfCustomers_GDV.Count - 1; p++)
                            if (SymElim_ListOfCustomers_GDV[p] == SymElim_ListOfCustomers_AFV[p + 1] && SymElim_ListOfCustomers_GDV[p + 1] == SymElim_ListOfCustomers_AFV[p])
                            {
                                nDifferentPositions++;
                                if (nDifferentPositions == 1)
                                    routeDifference = "adjacentSwapped";
                                else
                                {
                                    routeDifference = "differentRoutes";
                                    break;
                                }
                            }
                            else if (SymElim_ListOfCustomers_GDV[p] == SymElim_ListOfCustomers_AFV[p] && nDifferentPositions == 0)
                            {
                                if (nESVisits == 0)
                                    routeDifference = "identical";
                                else
                                    routeDifference = "sameSequence";
                            }
                            else
                            {
                                if (nDifferentPositions == 0)
                                    nDifferentPositions = nDifferentPositions + 2;
                                else
                                    nDifferentPositions++;
                                routeDifference = "differentRoutes";
                                break;
                            }
                    }


                    break;
                default:
                    throw new Exception("GDV_AFV_OptimizationDifferences doesn't account for all cases of RouteOptimizationStatus!");
            }

        }

        public static string GetHeaderRow()
        {
            return "Customers\t# Customers\tRoute Optimization Status\tOutcome Status\tAFV_Route\tAFV_nonES_Route\tGDV_Route\tAFV_Comp_Time\tGDV_Comp_Time\tT0GDVSoln\tT1CheckAFVfeas\tT2CheckAFVinfeas\tT3RefuelingPathInsert\tT4SwapAndInsert\tT5AFVSoln\tAFV_VMT\tGDV_VMT\tVMT Difference\t# ES Visits\tStopAfterStepNo\tVMT3RefuelingPathInsert\tVMT4SwapAndInsert\tVMT5AFVSoln\tGap3-4\tGap3-5\tGap4-5\tBestRoute\tNum Diff Positions\tRoute Difference";
        }

        public string GetDataRow()
        {
            return
                customers + "\t" +
                nCustomers.ToString() + "\t" +
                ros.ToString() + "\t" +
                outcomeStatus + "\t" +
                AFV_Route + "\t" +
                afv_nonES_route + "\t" +
                GDV_Route + "\t" +
                AFV_comp_time.ToString() + "\t" +
                GDV_comp_time.ToString() + "\t" +
                t0GDVSoln + "\t" +
                t1CheckAFVfeas + "\t" +
                t2CheckAFVinfeas + "\t" +
                t3RefuelingPathInsert + "\t" +
                t4SwapAndInsert + "\t" +
                t5AFVSoln + "\t" +
                AFV_Vmt.ToString() + "\t" +
                GDV_Vmt.ToString() + "\t" +
                VMTDifference.ToString() + "\t" +
                nESVisits.ToString() + "\t" +
                idealStopAfter.ToString() + "\t" +
                vmt3RefuelingPathInsert.ToString() + "\t" +
                vmt4SwapAndInsert.ToString() + "\t" +
                vmt5AFVSoln.ToString() + "\t" +
                vmtImprovementPercent34.ToString() + "\t" +
                vmtImprovementPercent35.ToString() + "\t" +
                vmtImprovementPercent45.ToString() + "\t" +
                bestRoute + "\t" +
                nDifferentPositions.ToString() + "\t" +
                routeDifference;
        }
    }
}
