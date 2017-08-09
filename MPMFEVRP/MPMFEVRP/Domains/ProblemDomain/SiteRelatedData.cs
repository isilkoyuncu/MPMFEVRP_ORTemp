using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int NumCustomers { get { return numCustomers; } set { numCustomers = value; } } 
        public int NumES { get { return numES; } set { numES = value; } }  
        public int NumNodes { get { return numNodes;  } set { numNodes = value; } }   
        public Site[] SiteArray { get { return siteArray; } set { siteArray = value; } }
        public double[,] Distance { get { return distance; } set { distance = value; } }
        public double[,] TimeConsumption { get { return timeConsumption; } set { timeConsumption = value; } }
        public double[,,] EnergyConsumption{ get { return energyConsumption; } set { energyConsumption = value; } }

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

        public double GetDistance(string currentNode, string nextNode)
        {
            int currentIndex = 0, nextIndex = 0;
            double distance = 0.0;

            for (int i = 0; i < siteArray.Length; i++)
            {
                if (siteArray[i].ID == currentNode)
                    currentIndex = i;
                if (siteArray[i].ID == nextNode)
                    nextIndex = i;
            }

            distance = Distance[currentIndex, nextIndex];

            return distance;
        }

        public int GetSiteIndex(string siteID)
        {
            for (int i = 0; i < siteArray.Length; i++)
                if (SiteArray[i].ID == siteID)
                    return i;
            throw new Exception("SiteRelatedData.GetSiteIndex can't find the site with the given ID!");
        }
    }
}
