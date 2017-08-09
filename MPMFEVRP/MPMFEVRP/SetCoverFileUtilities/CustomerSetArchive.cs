using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using System.IO;

namespace MPMFEVRP.SetCoverFileUtilities
{
    public class CustomerSetArchive
    {
        public static void WriteToFile(PartitionedCustomerSetList pcsl, string filename)
        {
            StreamWriter sw = new StreamWriter(filename, false);//append not allowed
            foreach(CustomerSet cs in pcsl.ToCustomerSetList()) { }
        }
        string CustomerSetToString(CustomerSet cs)
        {
            if (cs.RouteOptimizationOutcome == null)
                return SeparateBySpace(cs.Customers);
            string outcome = "";
            return outcome;
        }
        string SeparateBySpace(List<string> theList)
        {
            if ((theList == null) || (theList.Count == 0))
                return "";
            string outcome = theList[0];
            for (int i = 1; i < theList.Count; i++)
                outcome += " " + theList[i];
            return outcome;
        }
    }

}
