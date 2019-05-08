using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using Facet.Combinatorics;

namespace MPMFEVRP.Models
{
    public class RefuelingPathGenerator
    {
        int maxPossibleNumberOfRefuelingStops = 4;

        Dictionary<int, List<List<SiteWithAuxiliaryVariables>>> allEnumeratedRefuelingStops;

        public RefuelingPathGenerator()
        {
            allEnumeratedRefuelingStops = new Dictionary<int, List<List<SiteWithAuxiliaryVariables>>>();
        }

        public RefuelingPathList GenerateNonDominatedBetweenODPair(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> externalStations, SiteRelatedData SRD, int minNumberOfRefuelingStops = 0, int maxNumberOfRefuelingStops = int.MaxValue)
        {
            //verification: 
            int nUB = Math.Min(maxNumberOfRefuelingStops, maxPossibleNumberOfRefuelingStops);
            if (minNumberOfRefuelingStops > nUB)
                throw new Exception("minNumberOfRefuelingStops cannot exceed max(Possible)NumberOfRefuelingStops!");

            //implementation
            RefuelingPathList outcome = new RefuelingPathList();

            for(int nRefuelingStops = minNumberOfRefuelingStops; nRefuelingStops<= nUB; nRefuelingStops++)
            {
                if (!allEnumeratedRefuelingStops.Keys.Contains(nRefuelingStops))
                    allEnumeratedRefuelingStops.Add(nRefuelingStops, EnumerateRefuelingStops(externalStations, nRefuelingStops));
                List<List<SiteWithAuxiliaryVariables>> enumeratedRefuelingStops = allEnumeratedRefuelingStops[nRefuelingStops];

                foreach (List<SiteWithAuxiliaryVariables> refuelingStops in enumeratedRefuelingStops)
                {
                    if(refuelingStops.Count>0)//non-direct arc{
                    {
                        if (((origin.X == refuelingStops.First().X) && (origin.Y == refuelingStops.First().Y))
                            ||
                            ((destination.X == refuelingStops.Last().X) && (destination.Y == refuelingStops.Last().Y)))
                            continue;
                    }

                    RefuelingPath refuelingPath = new RefuelingPath(origin, destination, refuelingStops, SRD);
                    if (refuelingPath.Feasible)
                    {
                        int addResult = outcome.AddIfNondominated(refuelingPath);
                    }
                }
            }


            return outcome;
        }

        List<List<SiteWithAuxiliaryVariables>> EnumerateRefuelingStops(List<SiteWithAuxiliaryVariables> externalStations, int numberOfStops)
        {
            //verification:
            if (numberOfStops > maxPossibleNumberOfRefuelingStops)
                throw new Exception("Enumeration of longer (than " + maxPossibleNumberOfRefuelingStops.ToString() + " refueling stops) paths is not allowed!");

            List<List<SiteWithAuxiliaryVariables>> output = new List<List<SiteWithAuxiliaryVariables>>();

            if (numberOfStops == 0)
            {
                output.Add(new List<SiteWithAuxiliaryVariables>());
            }
            else
            {
                List<List<SiteWithAuxiliaryVariables>> input = EnumerateRefuelingStops(externalStations, numberOfStops - 1);
                foreach (List<SiteWithAuxiliaryVariables> inputPermutation in input)
                    foreach (SiteWithAuxiliaryVariables s in externalStations)
                        if (!inputPermutation.Contains(s))
                        {
                            for (int position = 0; position < numberOfStops; position++)
                            {
                                List<SiteWithAuxiliaryVariables> outputPermutation = new List<SiteWithAuxiliaryVariables>();
                                for (int i = 0; i < position; i++)
                                    outputPermutation.Add(inputPermutation[i]);
                                outputPermutation.Add(s);
                                for (int i = position; i < inputPermutation.Count; i++)
                                    outputPermutation.Add(inputPermutation[i]);
                                output.Add(outputPermutation);
                            }
                        }
            }

            return output;
        }

        /// <summary>
        /// This is the new (May'19) method to generate all non-dominated refueling paths using Andelmin&Bartolini's approach.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="externalStations"></param>
        /// <param name="SRD"></param>
        /// <returns></returns>
        public RefuelingPathList GenerateNonDominatedBetweenODPair(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> externalStations, SiteRelatedData SRD)
        {
            return null;
        }

        public List<List<SiteWithAuxiliaryVariables>> EnumerateAllRefuelingPaths(List<SiteWithAuxiliaryVariables> externalStations)
        {

            List<List<int>> output = new List<List<int>>();
            List<int> list = new List<int>();
            for (int i = 0; i <= externalStations.Count; i++)
            {
                list.Add(i);
            }
            List<List<SiteWithAuxiliaryVariables>> output2 = new List<List<SiteWithAuxiliaryVariables>>();           
            for (int i = 1; i <= externalStations.Count; i++)
            {
                DateTime sdate = DateTime.Now;
                //Facet.Combinatorics.Variations<SiteWithAuxiliaryVariables> var = new Facet.Combinatorics.Variations<SiteWithAuxiliaryVariables>(externalStations, i);
                Facet.Combinatorics.Variations<int> var = new Facet.Combinatorics.Variations<int>(list, i);
                for (int k = 0; k < var.Count(); k++)
                    output.Add(var.ElementAt(k).ToList());
                DateTime fdate = DateTime.Now;
                TimeSpan diff = fdate - sdate;
                double miliseconds = diff.TotalMilliseconds;
                int a = 3;
            }

            return output2;
        }
        public RefuelingPathList GenerateNondominatedRefuelingPathsBetweenODPair(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> externalStations, SiteRelatedData SRD)
        {
            if(SRD.GetEVEnergyConsumption(origin.ID,destination.ID)<60)
            {

            }
            RefuelingPathList outcome = new RefuelingPathList();

            List<List<SiteWithAuxiliaryVariables>> refuelPath = new List<List<SiteWithAuxiliaryVariables>>();




            return outcome;
        }
    }
}
