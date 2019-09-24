using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Models
{
    public class RefuelingPathList : List<RefuelingPath>
    {
        public int AddIfNondominated(RefuelingPath challenger)
        {
            List<RefuelingPath> incumbentsDominatedByChallenger = new List<RefuelingPath>();
            foreach (RefuelingPath incumbent in this)
            {
                RefuelingPathDominance dominance = incumbent.CheckDominance(challenger);
                if ((dominance == RefuelingPathDominance.IncumbentDominates) || (dominance == RefuelingPathDominance.BothAreTheSame))
                {
                    if (incumbentsDominatedByChallenger.Count > 0)
                        throw new Exception("Challenger both dominates some incumbents and is dominated by some others!");
                    return 0;
                }
                else if (dominance == RefuelingPathDominance.ChallengerDominates)
                {
                    incumbentsDominatedByChallenger.Add(incumbent);
                }
            }
            //If we're here, the incumbent is not dominated
            int outcome = 1 - incumbentsDominatedByChallenger.Count;
            if (incumbentsDominatedByChallenger.Count > 0)
            {
                foreach (RefuelingPath rp in incumbentsDominatedByChallenger)
                {
                    Remove(rp);
                }
                incumbentsDominatedByChallenger.Clear();
            }

            Add(challenger);
            return outcome;
        }
        public int CountByNumberOfRefuelingStops(int numberOfRefuelingStops)
        {
            return this.Where(x => x.RefuelingStops.Count.Equals(numberOfRefuelingStops)).Count();
        }
        public Dictionary<int,int> CountByNumberOfRefuelingStops()
        {
            Dictionary<int, int> outcome = new Dictionary<int, int>();
            int n = -1;
            while (outcome.Values.Sum() < Count)
            {
                n++;
                outcome.Add(n, CountByNumberOfRefuelingStops(n));
            }
            return outcome;
        }
        public RefuelingPathList RetrieveNonDominatedRefuelingPaths(string originID, string destinationID)
        {
            RefuelingPathList outcome = new RefuelingPathList();
            foreach(RefuelingPath rp in this)
            {
                if(rp.Origin.ID==originID && rp.Destination.ID==destinationID)
                {
                    outcome.Add(rp);
                }
            }
            return outcome;
        }
    }
}
