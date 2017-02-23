using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class SiteRelatedData
    {
        public int numCustomers;    //Number of Customers, not all of which must be served
        public int numES;  // Number of ES may or may not include a replica of the depot
        public int numNodes;   //This is numCustomers+nES+1 = siteArray.Length
        public Site[] siteArray;

        public SiteRelatedData() { }
        public SiteRelatedData(int numCustomers, int numES, int numNodes, Site[] siteArray) { }
    }
}
