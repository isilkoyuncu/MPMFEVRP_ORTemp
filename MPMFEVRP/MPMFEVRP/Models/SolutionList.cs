using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Models
{
    public enum PopStrategy { LowestLowerBound, Random, First }

    public class SolutionList : List<ISolution>
    {
        public int Cut(double upperbound)//TODO: Adapt this to the Max-Profit objective
        {
            return this.RemoveAll(item => item.LowerBound >= upperbound);
        }

        public ISolution Pop(PopStrategy strategy = PopStrategy.First)
        {
            ISolution outcome = null;
            if (Count >= 0)
            {
                var resultIndex = 0; // default is the first one
                switch (strategy)
                {
                    case PopStrategy.Random:
                        resultIndex = new Random(DateTime.Now.Ticks.GetHashCode()).Next(Count);
                        break;
                    case PopStrategy.LowestLowerBound://TODO: Adapt this to the Max-Profit objective
                        var minLB = Double.MaxValue;
                        var maxIDCount = int.MinValue;
                        for (int i = 0; i < Count; i++)
                        {
                            if (this[i].LowerBound < minLB)
                            {
                                minLB = this[i].LowerBound;
                                maxIDCount = this[i].IDs.Count;
                                resultIndex = i;
                            }
                            else if (this[i].LowerBound == minLB)
                            {
                                if (this[i].IDs.Count > maxIDCount)
                                {
                                    maxIDCount = this[i].IDs.Count;
                                    resultIndex = i;
                                }
                            }
                        }
                        break;
                }
                outcome = this[resultIndex];
                RemoveAt(resultIndex);
            }
            return outcome;
        }
    }
}
