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

        public int NumCustomers { get { return numCustomers; } set {; } } 
        public int NumES { get { return numES; } set {; } }  
        public int NumNodes { get { return numNodes;  } set {; } }   
        public Site[] SiteArray { get { return siteArray; } set {; } }

        public SiteRelatedData() { }
        public SiteRelatedData(int numCustomers, int numES, int numNodes, Site[] siteArray)
        {

        }
    }
}
