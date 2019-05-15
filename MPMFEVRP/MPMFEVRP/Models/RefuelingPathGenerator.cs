using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using Facet.Combinatorics;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Models
{
    public class RefuelingPathGenerator
    {
        int maxPossibleNumberOfRefuelingStops = 4;

        Dictionary<int, List<List<SiteWithAuxiliaryVariables>>> allEnumeratedRefuelingStops;
        AllPairsShortestPaths apss;
        public RefuelingPathGenerator()
        {
            allEnumeratedRefuelingStops = new Dictionary<int, List<List<SiteWithAuxiliaryVariables>>>();
        }
        public RefuelingPathGenerator(AllPairsShortestPaths apss)
        {
            this.apss = apss;
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
            RefuelingPathList outcome = new RefuelingPathList();
            List<SiteWithAuxiliaryVariables> refuelingStops = new List<SiteWithAuxiliaryVariables>();
            RefuelingPath refuelingPath;


            //Add direct arc
            if (SRD.GetEVEnergyConsumption(origin.ID, destination.ID) < origin.DeltaPrimeMax)
            {
                refuelingPath = new RefuelingPath(origin, destination, refuelingStops, SRD);
                int addResult = outcome.AddIfNondominated(refuelingPath);
            }
            //Add one-, two- or more-stops refueling paths
            foreach (SiteWithAuxiliaryVariables ES in externalStations)
            {
                refuelingPath = new RefuelingPath(origin, destination, new List<SiteWithAuxiliaryVariables> { ES }, SRD);
                if (refuelingPath.Feasible)
                {
                    int addResult = outcome.AddIfNondominated(refuelingPath);
                }
            }


            return outcome;
        }

        public RefuelingPathList GenerateSingleESNonDominatedBetweenODPair(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> externalStations, SiteRelatedData SRD)
        {
            RefuelingPathList outcome = new RefuelingPathList();
            RefuelingPath refuelingPath;
            foreach (SiteWithAuxiliaryVariables ES in externalStations)
            {
                refuelingPath = new RefuelingPath(origin, destination, new List<SiteWithAuxiliaryVariables> {ES}, SRD);
                if (refuelingPath.Feasible)
                {
                    int addResult = outcome.AddIfNondominated(refuelingPath);
                }
            }
            return outcome;
        }
        public RefuelingPathList GenerateSingleESNonDominatedBetweenODPairIK(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> externalStations, SiteRelatedData SRD)
        {
            RefuelingPathList outcome = new RefuelingPathList();
            List<SiteWithAuxiliaryVariables> singleESvisits = new List<SiteWithAuxiliaryVariables>();
            SiteWithAuxiliaryVariables minFromOrigin, minOverall, minToDestination;
            RefuelingPath refuelingPath;
            //Hypothesis: There will be at most three options here: 1-Closest to the origin, 2-Closest to the destination, 3-Overall minimizer
                       
            foreach (SiteWithAuxiliaryVariables ES in externalStations)
            {
                
            }
            
            return outcome;

        }

        /// <summary>
        /// This method ENUMERATES all refueling stops, however it is very slow!! Do not use it unless you want to test something with a small number of ESs such as 4-6.
        /// </summary>
        /// <param name="externalStations"></param>
        /// <returns></returns>
        public List<List<SiteWithAuxiliaryVariables>> EnumerateAllRefuelingPaths(List<SiteWithAuxiliaryVariables> externalStations)
        {
            List<List<SiteWithAuxiliaryVariables>> output = new List<List<SiteWithAuxiliaryVariables>>();           
            for (int i = 1; i <= externalStations.Count; i++)
            {
                Facet.Combinatorics.Variations<SiteWithAuxiliaryVariables> var = new Facet.Combinatorics.Variations<SiteWithAuxiliaryVariables>(externalStations, i);
                for (int k = 0; k < var.Count(); k++)
                    output.Add(var.ElementAt(k).ToList());
            }
            return output;
        }
    }
}
