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

        public int RangeOfDifferentPositions => lastDifferentPosition - firstDifferentPosition;

        int nSameNonIntermediatePositions;
        public int NSameNonIntermediatePositions => nSameNonIntermediatePositions;

        public GDV_AFV_OptimizationDifferences(RouteOptimizationOutcome roo)
        {
            if(roo==null)
                throw new ArgumentNullException();
            if (roo.GetRouteOptimizationStatus() != RouteOptimizationStatus.OptimizedForBothGDVandEV)
                throw new ArgumentOutOfRangeException("The customer set must be optimized for both GDV and AFV");

            VehicleSpecificRouteOptimizationOutcome ROO_GDV = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV);
            VehicleSpecificRouteOptimizationOutcome ROO_AFV = roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.EV);
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
                throw new Exception("GDV and AFV optimal routes visit different sets of customers!");
            nDifferentPositions = 0;
            for(int p = 0; p< SymElim_ListOfCustomers_AFV.Count; p++)
            {
                if(SymElim_ListOfCustomers_AFV[p]!= SymElim_ListOfCustomers_GDV[p])
                {
                    if (nDifferentPositions == 0)
                        firstDifferentPosition = p;
                    lastDifferentPosition = p;
                    nDifferentPositions++;
                }
            }
            nSameNonIntermediatePositions = SymElim_ListOfCustomers_AFV.Count - RangeOfDifferentPositions - 1;
        }

        public static string GetHeaderRow()
        {
            return "VMT Difference\t# ES Visits\t# Different Customer Positions\tRange of Different Positions\t# Same Non-Intermediate Positions";
        }

        public string GetDataRow()
        {
            return
                VMTDifference.ToString() + "\t" +
                nESVisits.ToString() + "\t" +
                nDifferentPositions.ToString() + "\t" +
                RangeOfDifferentPositions.ToString() + "\t" +
                nSameNonIntermediatePositions.ToString();
        }
    }
}
