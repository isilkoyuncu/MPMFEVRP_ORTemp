using System;
using System.Collections.Generic;
using System.Linq;

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

        //TODO: The following are all public however as for giving these fields to outside, the public Get...() methods below must be used as the only way.
        public int NumCustomers { get { return numCustomers; } } 
        public int NumES { get { return numES; } }  
        public int NumNodes { get { return numNodes;  } }   
        //public Site[] SiteArray { get { return siteArray; } }
        public double[,] Distance { get { return distance; } }
        public double[,] TimeConsumption { get { return timeConsumption; } }
        public double[,,] EnergyConsumption { get { return energyConsumption; } }

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
        }
        public SiteRelatedData(SiteRelatedData twinSRD)
        {
            numCustomers = twinSRD.NumCustomers;
            numES = twinSRD.NumES;
            numNodes = twinSRD.NumNodes;
            siteArray = twinSRD.SiteArray;
            distance = twinSRD.Distance;
            timeConsumption = twinSRD.TimeConsumption;
            energyConsumption = twinSRD.EnergyConsumption;
        }
        
        public List<string> GetCustomerIDs()
        {
            List<string> outcome = new List<string>();
            foreach (Site s in siteArray)
                if (s.SiteType == SiteTypes.Customer)
                    outcome.Add(s.ID);
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
        public Site GetSiteByID(string siteID)
        {
            foreach (Site s in siteArray)
                if (s.ID == siteID)
                {
                    return s;
                }
            throw new Exception("No depot found!");
        }
        public List<string> GetClosenessOrder(string nodeID)
        {
            Dictionary<double, string> preOrder = new Dictionary<double, string>();
            List<string> toReturn = new List<string>();
            foreach (Site s in siteArray)
            {
                preOrder.Add(GetDistance(nodeID, s.ID), s.ID);
                toReturn.Add(s.ID);
            }
            var sortedDict = from entry in preOrder orderby entry.Key ascending select entry;
            return toReturn;
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


        public int GetSiteIndex(string siteID)//TODO: Delete this method. Its only mission is to convert siteID to siteIndex, which will never be used after SiteArray is no longer given out.
        {
            for (int i = 0; i < siteArray.Length; i++)
                if (siteArray[i].ID == siteID)
                    return i;
            throw new Exception("SiteRelatedData.GetSiteIndex can't find the site with the given ID!");
        }
        public string GetSiteID(int siteIndex)//TODO: Delete this method. Its only mission is to convert siteIndex to siteID, which will never be used after SiteArray is no longer given out.
        {
                    return siteArray[siteIndex].ID;
            throw new Exception("SiteRelatedData.GetSiteID can't find the site with the given index!");
        }
    }
}
