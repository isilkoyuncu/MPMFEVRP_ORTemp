using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.AlgorithmDomain
{
    public class AlgorithmStatistics
    {

        double lowerBound, upperBound;
        public double LowerBound { get { return lowerBound; } set { lowerBound = value; } }
        public double UpperBound { get { return upperBound; } set { upperBound = value; } }

        long runTimeMilliSeconds;
        public long RunTimeMilliSeconds { get { return runTimeMilliSeconds; } set { runTimeMilliSeconds = value; } }

        Dictionary<String, String> otherStats;

        public AlgorithmStatistics()
        {
            otherStats = new Dictionary<string, string>();
        }

        public Dictionary<String, String> getOtherStats()
        {
            return otherStats;
        }

        public void addNewStat(String key, String value)
        {
            otherStats.Add(key, value);
        }

    }
}
