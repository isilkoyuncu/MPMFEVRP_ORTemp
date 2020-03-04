using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using System.IO;

namespace MPMFEVRP.Utils
{
    public class GDVvsAFV_OptimizationComparisonStatistics
    {
        Dictionary<RouteOptimizationStatus, Tuple<int,double,double>> rosCounts;
        public Dictionary<RouteOptimizationStatus, Tuple<int, double, double>> RouteOptimizationStatusFrequencyDistribution => rosCounts;

        List<GDV_AFV_OptimizationDifferences> optDifferences;
        public List<GDV_AFV_OptimizationDifferences> OptimizationDifferences => optDifferences;

        public GDVvsAFV_OptimizationComparisonStatistics()
        {
            rosCounts = new Dictionary<RouteOptimizationStatus, Tuple<int, double, double>>();
            foreach (RouteOptimizationStatus ros in (RouteOptimizationStatus[])Enum.GetValues(typeof(RouteOptimizationStatus)))
                rosCounts.Add(ros, new Tuple<int, double, double>(0,0.0,0.0));

            optDifferences = new List<GDV_AFV_OptimizationDifferences>();
        }

        public void RecordObservation(int nCustomers, RouteOptimizationOutcome roo, List<string> customers)
        {
            switch(roo.Status)
            {
                case RouteOptimizationStatus.NotYetOptimized:
                    rosCounts[roo.Status] = new Tuple<int, double, double>(rosCounts[roo.Status].Item1 + 1, 0.0, 0.0);
                    break;
                case RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV:
                    rosCounts[roo.Status] = new Tuple<int, double, double>(rosCounts[roo.Status].Item1 + 1, rosCounts[roo.Status].Item2 + roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).ComputationTime, 0.0);
                    break;
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV:
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                case RouteOptimizationStatus.OptimizedForBothGDVandEV:
                    rosCounts[roo.Status] = new Tuple<int, double, double>(rosCounts[roo.Status].Item1 + 1, rosCounts[roo.Status].Item2 + roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).ComputationTime, rosCounts[roo.Status].Item3 + roo.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.EV).ComputationTime);
                    optDifferences.Add(new GDV_AFV_OptimizationDifferences(nCustomers, roo, customers));
                    break;
                default:
                    throw new Exception("GDVvsAFV_OptimizationComparisonStatistics.RecordObservation doesn't account for all values of RouteOptimizationStatus!");
            }
        }

        public void WriteToFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            sw.AutoFlush = true;

            sw.WriteLine("Route Optimization Status\tFrequency\tGDV Comp Time (total)\tAFV Comp Time (total)");
            foreach (RouteOptimizationStatus ros in rosCounts.Keys)
                sw.WriteLine(ros.ToString() + "\t" + rosCounts[ros].Item1.ToString() + "\t" + rosCounts[ros].Item2.ToString() + "\t" + rosCounts[ros].Item3.ToString());
            sw.WriteLine();
            sw.WriteLine(GDV_AFV_OptimizationDifferences.GetHeaderRow());
            foreach (GDV_AFV_OptimizationDifferences diff in optDifferences)
                sw.WriteLine(diff.GetDataRow());

            sw.Close();
        }
    }
}
