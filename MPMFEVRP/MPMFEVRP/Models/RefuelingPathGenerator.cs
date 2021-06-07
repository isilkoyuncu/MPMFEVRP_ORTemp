using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using Facet.Combinatorics;
using MPMFEVRP.Utils;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;

namespace MPMFEVRP.Models
{
    public class RefuelingPathGenerator
    {
        int maxPossibleNumberOfRefuelingStops = 4;
        double epsilon = 0.01;

        Dictionary<int, List<List<SiteWithAuxiliaryVariables>>> allEnumeratedRefuelingStops;
        AllPairsShortestPaths apss;
        List<List<SiteWithAuxiliaryVariables>> refuelingStopsList;

        public RefuelingPathGenerator()
        {
            allEnumeratedRefuelingStops = new Dictionary<int, List<List<SiteWithAuxiliaryVariables>>>();
        }
        public RefuelingPathGenerator(AllPairsShortestPaths apss)
        {
            this.apss = apss;
        }
        public RefuelingPathGenerator(EVvsGDV_ProblemModel theProblemModel, List<SiteWithAuxiliaryVariables> externalStations=null)//(SiteRelatedData SRD, VehicleRelatedData VRD, List<SiteWithAuxiliaryVariables> externalStations)
        {
            if (externalStations != null) //Fill this part so that if refueling path generator is initiated with an ES set other than the one in the problem model, constructor will work accordingly
                throw new NotImplementedException();
            apss = new AllPairsShortestPaths();
            double[,] distances = apss.ModifyDistanceMatrix(theProblemModel.SRD.GetES2ESDistanceMatrix(), theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).ConsumptionRate);
            apss.InitializeAndSolveAPSS(distances, theProblemModel.SRD.GetESIDs().ToArray());
            GenerateRefuelingStops(theProblemModel.SRD.GetSWAVsList(SiteTypes.ExternalStation));
        }
        public RefuelingPathList GenerateNonDominatedBetweenODPair(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, SiteRelatedData SRD, List<SiteWithAuxiliaryVariables> externalStations,VehicleRelatedData VRD, int minNumberOfRefuelingStops = 0, int maxNumberOfRefuelingStops = int.MaxValue)
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

                    RefuelingPath refuelingPath = new RefuelingPath(origin, destination, refuelingStops, SRD, VRD);
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
        void GenerateRefuelingStops(List<SiteWithAuxiliaryVariables> externalStations)
        {
            refuelingStopsList = new List<List<SiteWithAuxiliaryVariables>>();
            List<SiteWithAuxiliaryVariables> refuelingStops = new List<SiteWithAuxiliaryVariables>();
            //Add one-stop
            for (int i = 0; i < externalStations.Count; i++)
                refuelingStopsList.Add(new List<SiteWithAuxiliaryVariables> { externalStations[i] });
            //Add two-or-more stops
            for (int i = 0; i < externalStations.Count; i++)
                for (int j = 0; j < externalStations.Count; j++)
                    if (i != j)
                    {
                        refuelingStops = new List<SiteWithAuxiliaryVariables>();
                        for (int k = 0; k < apss.ShortestPaths[i, j].Count; k++)
                        {
                            foreach (SiteWithAuxiliaryVariables es in externalStations)
                                if (es.ID == apss.ShortestPaths[i, j][k])
                                    refuelingStops.Add(es);
                        }
                        refuelingStopsList.Add(refuelingStops);
                    }
        }
        /// <summary>
        /// This is the new (May'19) method to generate all non-dominated refueling paths using Andelmin&Bartolini's approach.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="externalStations"></param>
        /// <param name="SRD"></param>
        /// <returns></returns>
        public RefuelingPathList GenerateNonDominatedBetweenODPairIK(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, SiteRelatedData SRD, VehicleRelatedData VRD)
        {
            RefuelingPathList outcome = new RefuelingPathList();
            RefuelingPath refuelingPath;
            if (origin.ID != destination.ID) //no refueling path from itself to itself
            {
                //Add direct arc
                if (origin.DeltaPrimeMax - SRD.GetEVEnergyConsumption(origin.ID, destination.ID) + epsilon >= destination.DeltaMin)
                {
                    refuelingPath = new RefuelingPath(origin, destination, new List<SiteWithAuxiliaryVariables>() { }, SRD, VRD);
                    int addResult = outcome.AddIfNondominated(refuelingPath);
                }
                //Add one-or-more-stop refueling paths
                foreach (List<SiteWithAuxiliaryVariables> rs in refuelingStopsList)
                {
                    if ((origin.SiteType == SiteTypes.Depot && rs[0].X == SRD.GetSingleDepotSite().X && rs[0].Y == SRD.GetSingleDepotSite().Y) || (destination.SiteType == SiteTypes.Depot && rs[rs.Count - 1].X == SRD.GetSingleDepotSite().X && rs[rs.Count - 1].Y == SRD.GetSingleDepotSite().Y))
                    {
                        //Do not add the ES at the depot if first or last 
                    }
                    else
                    {
                        refuelingPath = new RefuelingPath(origin, destination, rs, SRD, VRD);
                        if (refuelingPath.Feasible)
                        {
                            int addResult = outcome.AddIfNondominated(refuelingPath);
                        }
                    }
                }
            }
            return outcome;
        }

        public RefuelingPathList GenerateSingleESNonDominatedBetweenODPair(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> externalStations, SiteRelatedData SRD, VehicleRelatedData VRD)
        {
            RefuelingPathList outcome = new RefuelingPathList();
            RefuelingPath refuelingPath;
            foreach (SiteWithAuxiliaryVariables ES in externalStations)
            {
                refuelingPath = new RefuelingPath(origin, destination, new List<SiteWithAuxiliaryVariables> {ES}, SRD, VRD);
                if (refuelingPath.Feasible)
                {
                    int addResult = outcome.AddIfNondominated(refuelingPath);
                }
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
