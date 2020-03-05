using System;
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
        int stopAfter;

        double vmt3RefuelingPathInsert;
        double vmt4SwapAndInsert;
        double vmt5AFVSoln;

        double GDV_comp_time;        public double GDV_Comp_Time => GDV_comp_time;
        double GDV_vmt;        public double GDV_Vmt => GDV_vmt;
        string GDV_route;        public string GDV_Route => GDV_route;
        double AFV_comp_time;        public double AFV_Comp_Time => AFV_comp_time;
        double AFV_vmt;        public double AFV_Vmt => AFV_vmt;
        string AFV_route;        public string AFV_Route => AFV_route;
        double VMTdifference;        public double VMTDifference => VMTdifference;
        int nESVisits;        public int NESVisits => nESVisits;
                
        public OptimizationStatistics(int nCustomers, RouteOptimizationOutcome roo, List<string> customers, double t0GDVSoln, double t1CheckAFVfeas, double t2CheckAFVinfeas, double t3RefuelingPathInsert, double t4SwapAndInsert, double t5AFVSoln, int stopAfter, 
            double vmt3RefuelingPathInsert,
            double vmt4SwapAndInsert,
            double vmt5AFVSoln)
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

            this.stopAfter = stopAfter;

            this.vmt3RefuelingPathInsert = vmt3RefuelingPathInsert;
            this.vmt4SwapAndInsert = vmt4SwapAndInsert;
            this.vmt5AFVSoln = vmt5AFVSoln;

            switch (roo.GetRouteOptimizationStatus())
            {
                case RouteOptimizationStatus.NotYetOptimized:
                case RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV:
                    throw new ArgumentOutOfRangeException("GDV_AFV_OptimizationDifferences can't be run for a customer that has not yet been optimized!");
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV:
                    GDV_comp_time = t0GDVSoln;
                    AFV_comp_time = t0GDVSoln+t1CheckAFVfeas+ t2CheckAFVinfeas+ t3RefuelingPathInsert+ t4SwapAndInsert+ t5AFVSoln;
                    break;
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                    GDV_comp_time = t0GDVSoln;
                    VehicleSpecificRoute Route_GDV1 = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).VSOptimizedRoute;
                    for (int c = 0; c < Route_GDV1.ListOfVisitedNonDepotSiteIDs.Count - 1; c++)
                        GDV_route = GDV_route + Route_GDV1.ListOfVisitedNonDepotSiteIDs[c] + "-";
                    GDV_route = GDV_route + Route_GDV1.ListOfVisitedNonDepotSiteIDs.Last();
                    GDV_vmt = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled();
                    AFV_comp_time = t0GDVSoln + t1CheckAFVfeas + t2CheckAFVinfeas + t3RefuelingPathInsert + t4SwapAndInsert + t5AFVSoln;
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
                    nESVisits = Route_AFV.ListOfVisitedNonDepotSiteIDs.Count - Route_AFV.NumberOfCustomersVisited;
                    for (int c = 0; c < Route_AFV.ListOfVisitedNonDepotSiteIDs.Count - 1; c++)
                        AFV_route = AFV_route + Route_AFV.ListOfVisitedNonDepotSiteIDs[c] + "-";
                    AFV_route = AFV_route + Route_AFV.ListOfVisitedNonDepotSiteIDs.Last();

                    VehicleSpecificRoute Route_GDV = ROO_GDV.VSOptimizedRoute;
                    for (int c = 0; c < Route_GDV.ListOfVisitedNonDepotSiteIDs.Count - 1; c++)
                        GDV_route = GDV_route + Route_GDV.ListOfVisitedNonDepotSiteIDs[c] + "-";
                    GDV_route = GDV_route + Route_GDV.ListOfVisitedNonDepotSiteIDs.Last();                   
                    break;
                default:
                    throw new Exception("GDV_AFV_OptimizationDifferences doesn't account for all cases of RouteOptimizationStatus!");
            }

        }

        public static string GetHeaderRow()
        {
            return "Customers\t# Customers\tRoute Optimization Status\tAFV_Route\tGDV_Route\tAFV_Comp_Time\tGDV_Comp_Time\tT0GDVSoln\tT1CheckAFVfeas\tT2CheckAFVinfeas\tT3RefuelingPathInsert\tT4SwapAndInsert\tT5AFVSoln\tAFV_VMT\tGDV_VMT\tVMT Difference\t# ES Visits\tStopAfterStepNo\tVMT3RefuelingPathInsert\tVMT4SwapAndInsert\tVMT5AFVSoln";
        }

        public string GetDataRow()
        {
            return
                customers + "\t" +
                nCustomers.ToString() + "\t" +
                ros.ToString() + "\t" +
                AFV_Route + "\t" +
                GDV_Route + "\t" +
                AFV_comp_time.ToString() + "\t" +
                GDV_comp_time.ToString() + "\t" +
                t0GDVSoln.ToString() + "\t" +
                t1CheckAFVfeas.ToString() + "\t" +
                t2CheckAFVinfeas.ToString() + "\t" +
                t3RefuelingPathInsert.ToString() + "\t" +
                t4SwapAndInsert.ToString() + "\t" +
                t5AFVSoln.ToString() + "\t" +
                AFV_Vmt.ToString() + "\t" +
                GDV_Vmt.ToString() + "\t" +
                VMTDifference.ToString() + "\t" +
                nESVisits.ToString() + "\t" +
                stopAfter.ToString() + "\t" +
                vmt3RefuelingPathInsert.ToString() + "\t" +
                vmt4SwapAndInsert.ToString() + "\t" +
                vmt5AFVSoln.ToString();
        }

        

    }
}
