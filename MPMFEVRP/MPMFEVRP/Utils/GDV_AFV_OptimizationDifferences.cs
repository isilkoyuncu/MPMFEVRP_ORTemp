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
    public class GDV_AFV_OptimizationDifferences
    {
        int nCustomers;
        RouteOptimizationStatus ros;

        double GDV_comp_time;
        public double GDV_Comp_Time => GDV_comp_time;

        double AFV_comp_time;
        public double AFV_Comp_Time => AFV_comp_time;

        double VMTdifference;
        /// <summary>
        /// AFV - GDV miles traveled
        /// </summary>
        public double VMTDifference => VMTdifference;

        int nESVisits;
        public int NESVisits => nESVisits;

        int nDifferentPositions;
        public int NDifferentPositions => nDifferentPositions;

        int firstDifferentPosition;
        public int FirstDifferentPosition => firstDifferentPosition;

        int lastDifferentPosition;
        public int LastDifferentPosition => lastDifferentPosition;

        public int RangeOfDifferentPositions => lastDifferentPosition - firstDifferentPosition + 1;

        int nSameNonIntermediatePositions;
        public int NSameNonIntermediatePositions => nSameNonIntermediatePositions;

        public GDV_AFV_OptimizationDifferences(int nCustomers, RouteOptimizationOutcome roo)
        {
            if (roo == null)
                throw new ArgumentNullException();

            this.nCustomers = nCustomers;
            ros = roo.GetRouteOptimizationStatus();

            switch (roo.GetRouteOptimizationStatus())
            {
                case RouteOptimizationStatus.NotYetOptimized:
                case RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV:
                    throw new ArgumentOutOfRangeException("GDV_AFV_OptimizationDifferences can't be run for a customer that has not yet been optimized!");
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV:
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                    GDV_comp_time = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).ComputationTime;
                    AFV_comp_time = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.EV).ComputationTime;
                    break;
                case RouteOptimizationStatus.OptimizedForBothGDVandEV:
                    VehicleSpecificRouteOptimizationOutcome ROO_GDV = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV);
                    VehicleSpecificRouteOptimizationOutcome ROO_AFV = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.EV);
                    GDV_comp_time = ROO_GDV.ComputationTime;
                    AFV_comp_time = ROO_AFV.ComputationTime;
                    VMTdifference = ROO_AFV.VSOptimizedRoute.GetVehicleMilesTraveled() - ROO_GDV.VSOptimizedRoute.GetVehicleMilesTraveled();

                    VehicleSpecificRoute Route_AFV = ROO_AFV.VSOptimizedRoute;
                    nESVisits = Route_AFV.ListOfVisitedNonDepotSiteIDs.Count - Route_AFV.NumberOfCustomersVisited;

                    VehicleSpecificRoute Route_GDV = ROO_GDV.VSOptimizedRoute;
                    List<string> SymElim_ListOfCustomers_AFV = Route_AFV.ListOfVisitedCustomerSiteIDs;
                    if (SymElim_ListOfCustomers_AFV.First().CompareTo(SymElim_ListOfCustomers_AFV.Last()) > -1)
                        SymElim_ListOfCustomers_AFV.Reverse();
                    List<string> SymElim_ListOfCustomers_GDV = Route_GDV.ListOfVisitedCustomerSiteIDs;
                    if (SymElim_ListOfCustomers_GDV.First().CompareTo(SymElim_ListOfCustomers_GDV.Last()) > -1)
                        SymElim_ListOfCustomers_GDV.Reverse();
                    if (SymElim_ListOfCustomers_AFV.Count != SymElim_ListOfCustomers_GDV.Count)
                        throw new DifferentRoutesVisitDifferentSetsOfCustomersException(); //Exception("GDV and AFV optimal routes visit different sets of customers!");
                    nDifferentPositions = 0;
                    for (int p = 0; p < SymElim_ListOfCustomers_AFV.Count; p++)
                    {
                        if (SymElim_ListOfCustomers_AFV[p] != SymElim_ListOfCustomers_GDV[p])
                        {
                            if (nDifferentPositions == 0)
                                firstDifferentPosition = p;
                            lastDifferentPosition = p;
                            nDifferentPositions++;
                        }
                    }
                    nSameNonIntermediatePositions = SymElim_ListOfCustomers_AFV.Count - RangeOfDifferentPositions;
                    break;
                default:
                    throw new Exception("GDV_AFV_OptimizationDifferences doesn't account for all cases of RouteOptimizationStatus!");
            }

        }

        public static string GetHeaderRow()
        {
            return "# Customers\tRoute Optimization Status\tGDV_Comp_Time\tAFV_Comp_Time\tVMT Difference\t# ES Visits\t# Different Customer Positions\tRange of Different Positions\t# Same Non-Intermediate Positions";
        }

        public string GetDataRow()
        {
            return
                nCustomers.ToString() + "\t" +
                ros.ToString() + "\t" +
                GDV_comp_time.ToString() + "\t" +
                AFV_comp_time.ToString() + "\t" +
                VMTDifference.ToString() + "\t" +
                nESVisits.ToString() + "\t" +
                nDifferentPositions.ToString() + "\t" +
                RangeOfDifferentPositions.ToString() + "\t" +
                nSameNonIntermediatePositions.ToString();
        }

    }

    public class DifferentRoutesVisitDifferentSetsOfCustomersException : Exception
    {

    }
}
