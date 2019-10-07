using System;
using System.Collections.Generic;
using System.Linq;
using MPMFEVRP.Models;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class SiteRelatedData
    {
        int numCustomers;    //Number of Customers, not all of which must be served
        int numES;  // Number of ES may or may not include a replica of the depot
        int numNodes;   //This is numCustomers+nES+1 = siteArray.Length

        Site[] siteArray;
        double[,] distance;   //[numNodes,numNodes]  
        double[,] timeConsumption;  //[numNodes,numNodes]
        double[,,] energyConsumption;    //[numNodes,numNodes,numVehicleTypes]

        //ISSUE (#6): The following are all public however as for giving these fields to outside, the public Get...() methods below must be used as the only way.
        public int NumCustomers { get { return numCustomers; } } 
        public int NumES { get { return numES; } }  
        public int NumNodes { get { return numNodes;  } }   
        public double[,] Distance { get { return distance; } }
        public double[,] TimeConsumption { get { return timeConsumption; } }
        public double[,,] EnergyConsumption { get { return energyConsumption; } }

        SiteWithAuxiliaryVariables[] allOriginalSWAVs; public SiteWithAuxiliaryVariables[] AllOriginalSWAVs { get { return allOriginalSWAVs; } }

        public SiteRelatedData() { }
        public SiteRelatedData(int numCustomers, int numES, int numNodes, Site[] siteArray, double[,] distance, double[,] timeConsumption, double[,,] energyConsumption)
        {
            this.numCustomers = numCustomers;
            this.numES = numES;
            this.numNodes = numNodes;
            this.siteArray = siteArray;
            this.distance = distance;
            this.timeConsumption = timeConsumption;
            this.energyConsumption = energyConsumption;
            allOriginalSWAVs = new SiteWithAuxiliaryVariables[siteArray.Length];
            for (int i = 0; i < siteArray.Length; i++)
                allOriginalSWAVs[i] = new SiteWithAuxiliaryVariables(siteArray[i]);
        }
        public SiteRelatedData(SiteRelatedData twinSRD)
        {
            numCustomers = twinSRD.NumCustomers;
            numES = twinSRD.NumES;
            numNodes = twinSRD.NumNodes;
            siteArray = twinSRD.GetAllSitesArray();
            distance = twinSRD.Distance;
            timeConsumption = twinSRD.TimeConsumption;
            energyConsumption = twinSRD.EnergyConsumption;
            allOriginalSWAVs = twinSRD.GetAllSWAVsArray();
        }
        public Site[] GetAllSitesArray()
        {
            return siteArray;
        }
        public SiteWithAuxiliaryVariables[] GetAllSWAVsArray()
        {
            return allOriginalSWAVs;
        }
        public List<Site> GetAllSitesList()
        {
            return siteArray.ToList();
        }
        public List<SiteWithAuxiliaryVariables> GetAllSWAVsList()
        {
            return allOriginalSWAVs.ToList();
        }
        public List<SiteWithAuxiliaryVariables> GetAllNonESSWAVsList()
        {
            List<SiteWithAuxiliaryVariables> outcome = new List<SiteWithAuxiliaryVariables>();
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                if (swav.SiteType != SiteTypes.ExternalStation)
                    outcome.Add(swav);
            return outcome;
        }

        public List<Site> GetSitesList(SiteTypes siteType)
        {
            List<Site> outcome = new List<Site>();
            foreach (Site s in siteArray)
                if (s.SiteType == siteType)
                    outcome.Add(s);
            return outcome;
        }
        public List<SiteWithAuxiliaryVariables> GetSWAVsList(SiteTypes siteType)
        {
            List<SiteWithAuxiliaryVariables> outcome = new List<SiteWithAuxiliaryVariables>();
            foreach (SiteWithAuxiliaryVariables s in allOriginalSWAVs)
                if (s.SiteType == siteType)
                    outcome.Add(s);
            return outcome;
        }
        public List<string> GetCustomerIDs()
        {
            List<string> outcome = new List<string>();
            foreach (Site s in siteArray)
                if (s.SiteType == SiteTypes.Customer)
                    outcome.Add(s.ID);
            return outcome;
        }
        public List<string> GetESIDs()
        {
            List<string> outcome = new List<string>();
            foreach (Site s in siteArray)
                if (s.SiteType == SiteTypes.ExternalStation)
                    outcome.Add(s.ID);
            return outcome;
        }
        public List<string> GetAllIDs()
        {
            List<string> outcome = new List<string>();
            foreach (Site s in siteArray)
                outcome.Add(s.ID);
            return outcome;
        }
        public double GetTotalCustomerServiceTime()
        {
            double outcome = 0;
            foreach (Site s in siteArray)
                if (s.SiteType == SiteTypes.Customer)
                    outcome += s.ServiceDuration;
            return outcome;
        }
        public double GetTotalCustomerServiceTime(List<string> customers)
        {
            double outcome = 0;
            foreach (string cID in customers)
                    outcome += GetSiteByID(cID).ServiceDuration;
            return outcome;
        }
        public string GetSingleDepotID()
        {
            foreach (Site s in siteArray)
                if (s.SiteType == SiteTypes.Depot)
                {
                    return s.ID;
                }
            throw new Exception("No depot found!");
        }
        public Site GetSingleDepotSite()
        {
            foreach (Site s in siteArray)
                if (s.SiteType == SiteTypes.Depot)
                {
                    return s;
                }
            throw new Exception("No depot found!");
        }
        public SiteWithAuxiliaryVariables GetSingleDepotSWAV()
        {
            foreach (SiteWithAuxiliaryVariables s in allOriginalSWAVs)
                if (s.SiteType == SiteTypes.Depot)
                {
                    return s;
                }
            throw new Exception("No depot found!");
        }
        public Site GetSiteByID(string siteID)
        {
            foreach (Site s in siteArray)
                if (s.ID == siteID)
                {
                    return s;
                }
            throw new Exception("No site found!");
        }
        public SiteWithAuxiliaryVariables GetSWAVByID(string svawID)
        {
            foreach (SiteWithAuxiliaryVariables s in allOriginalSWAVs)
                if (s.ID == svawID)
                {
                    return s;
                }
            throw new Exception("No site found!");
        }

        public List<string> GetClosenessOrder(string nodeID, Site[] onlyES = null)
        {
            Dictionary<string, double> preOrder = new Dictionary<string, double>();
            List<string> toReturn = new List<string>();
            if (onlyES == null)
                foreach (Site s in siteArray)
                {
                    preOrder.Add(s.ID, GetDistance(nodeID, s.ID));
                }

            else
                foreach (Site es in onlyES)
                {
                    preOrder.Add(es.ID, GetDistance(nodeID, es.ID));
                }
            var sortedDict = from entry in preOrder orderby entry.Value ascending select entry.Key;
            toReturn = sortedDict.ToList();
            return toReturn;
        }
        public string GetSiteIdRequiresMinimumEnergyFromSite(string nodeID)
        {
            double minimumEnergy = double.MaxValue;
            string nextNodeID = "";
            foreach (Site s in siteArray)
                if (s.ID != nodeID)
                    if (minimumEnergy > GetEVEnergyConsumption(nodeID, s.ID))
                    {
                        minimumEnergy = GetEVEnergyConsumption(nodeID, s.ID);
                        nextNodeID = s.ID;
                    }
            return nextNodeID;
        }
        public string GetSiteIdRequiresMinimumEnergyToSite(string nodeID)
        {
            double minimumEnergy = double.MaxValue;
            string previousNodeID = "";
            foreach (Site s in siteArray)
                if (s.ID != nodeID)
                    if (minimumEnergy > GetEVEnergyConsumption(s.ID, nodeID))
                    {
                        minimumEnergy = GetEVEnergyConsumption(s.ID, nodeID);
                        previousNodeID = s.ID;
                    }
            return previousNodeID;
        }
        public double GetDistance(string currentNodeID, string nextNodeID)
        {
            int currentIndex = 0, nextIndex = 0;
            double distance = 0.0;

            for (int i = 0; i < siteArray.Length; i++)
            {
                if (siteArray[i].ID == currentNodeID)
                    currentIndex = i;
                if (siteArray[i].ID == nextNodeID)
                    nextIndex = i;
            }

            distance = Distance[currentIndex, nextIndex];

            return distance;
        }
        public double[,] GetES2ESDistanceMatrix()
        {
            double[,] outcome = new double[NumES, NumES];
            int from = -1, to = -1;

            for (int i = 0; i < siteArray.Length; i++)
            {
                if (siteArray[i].SiteType == SiteTypes.ExternalStation)
                {
                    from++;
                    for (int j = 0; j < siteArray.Length; j++)
                    {
                        if (siteArray[j].SiteType == SiteTypes.ExternalStation)
                            outcome[from, ++to] = Distance[i, j];
                    }
                    to = -1;
                }
            }

            return outcome;
        }
        public double[,] GetCust2CustDistanceMatrix()
        {
            double[,] outcome = new double[NumCustomers, NumCustomers];
            int from = -1, to = -1;

            for (int i = 0; i < siteArray.Length; i++)
            {
                if (siteArray[i].SiteType == SiteTypes.Customer)
                {
                    from++;
                    for (int j = 0; j < siteArray.Length; j++)
                    {
                        if (siteArray[j].SiteType == SiteTypes.Customer)
                            outcome[from, ++to] = Distance[i, j];
                    }
                    to = -1;
                }
            }

            return outcome;
        }

        public double GetTravelTime(string currentNodeID, string nextNodeID)
        {
            int currentIndex = 0, nextIndex = 0;
            double travelTime = 0.0;

            for (int i = 0; i < siteArray.Length; i++)
            {
                if (siteArray[i].ID == currentNodeID)
                    currentIndex = i;
                if (siteArray[i].ID == nextNodeID)
                    nextIndex = i;
            }

            travelTime = TimeConsumption[currentIndex, nextIndex];

            return travelTime;
        }
        public double GetEVEnergyConsumption(string currentNodeID, string nextNodeID)
        {
            int currentIndex = 0, nextIndex = 0;
            double eVenergyConsumption = 0.0;

            for (int i = 0; i < siteArray.Length; i++)
            {
                if (siteArray[i].ID == currentNodeID)
                    currentIndex = i;
                if (siteArray[i].ID == nextNodeID)
                    nextIndex = i;
            }

            eVenergyConsumption = EnergyConsumption[currentIndex, nextIndex,0];

            return eVenergyConsumption;
        }

        public RefuelingPathList PopulateRefuelingPathsBetween(RefuelingPathGenerator rpg, SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination)
        {
            return rpg.GenerateNonDominatedBetweenODPairIK(origin, destination, this);
        }
        public RefuelingPathList PopulateAllNonDominatedRPs(RefuelingPathGenerator rpg)
        {
            List<SiteWithAuxiliaryVariables> nonESnodes = GetAllNonESSWAVsList();
            RefuelingPathList allNonDominatedArcs = new RefuelingPathList();
            RefuelingPathList nonDominatedRPs = new RefuelingPathList();
            foreach (SiteWithAuxiliaryVariables from in nonESnodes)
            {
                foreach (SiteWithAuxiliaryVariables to in nonESnodes)
                    nonDominatedRPs = rpg.GenerateNonDominatedBetweenODPairIK(from, to, this);
                allNonDominatedArcs.AddRange(nonDominatedRPs);
            }
            return allNonDominatedArcs;
        }
        public string GetSiteID(int siteIndex)//TODO: Its only mission is to convert siteIndex to siteID, which will never be used after SiteArray is no longer given out. Update: checked on 11/10/17, this method was still needed and could not be deleted.
        {
                    return siteArray[siteIndex].ID;
            throw new Exception("SiteRelatedData.GetSiteID can't find the site with the given index!");
        }
    }
}
