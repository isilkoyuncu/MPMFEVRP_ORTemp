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
        Dictionary<RouteOptimizationStatus, int> rosCounts;
        public Dictionary<RouteOptimizationStatus, int> RouteOptimizationStatusFrequencyDistribution => rosCounts;

        List<GDV_AFV_OptimizationDifferences> optDifferences;
        public List<GDV_AFV_OptimizationDifferences> OptimizationDifferences => optDifferences;

        int nDexsCaught;

        public GDVvsAFV_OptimizationComparisonStatistics()
        {
            rosCounts = new Dictionary<RouteOptimizationStatus, int>();
            foreach (RouteOptimizationStatus ros in (RouteOptimizationStatus[])Enum.GetValues(typeof(RouteOptimizationStatus)))
                rosCounts.Add(ros, 0);

            optDifferences = new List<GDV_AFV_OptimizationDifferences>();

            nDexsCaught = 0;
        }

        public void RecordObservation(RouteOptimizationOutcome roo)
        {
            rosCounts[roo.Status]++;
            if (roo.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV)
            {
                try
                {
                    optDifferences.Add(new GDV_AFV_OptimizationDifferences(roo));
                }
                catch (DifferentRoutesVisitDifferentSetsOfCustomersException dex)
                {
                    //throw dex;
                    nDexsCaught++;
                }
            }
        }

        public void WriteToFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            sw.AutoFlush = true;

            sw.WriteLine("Route Optimization Status\tFrequency");
            foreach (RouteOptimizationStatus ros in rosCounts.Keys)
                sw.WriteLine(ros.ToString() + "\t" + rosCounts[ros].ToString());
            sw.WriteLine();
            sw.WriteLine(GDV_AFV_OptimizationDifferences.GetHeaderRow());
            foreach (GDV_AFV_OptimizationDifferences diff in optDifferences)
                sw.WriteLine(diff.GetDataRow());

            sw.Close();
        }
    }
}
