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
            this.siteArray = siteArray; // TODO ask if I can do it??!!!!
            this.distance = distance;
            this.timeConsumption = timeConsumption;
            this.energyConsumption = energyConsumption;
        }

        public SiteRelatedData(SiteRelatedData SRD)
        {
            numCustomers = SRD.NumCustomers;
            numES = SRD.NumES;
            numNodes = SRD.NumNodes;
            siteArray = SRD.SiteArray; // TODO small or upper case?? CONFUSED
            distance = SRD.Distance;
            timeConsumption = SRD.TimeConsumption;
            energyConsumption = SRD.EnergyConsumption;
        }
    }
}
