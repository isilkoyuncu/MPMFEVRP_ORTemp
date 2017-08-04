using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRPTests.TestUtility_DefaultGetters
{
    class SolutionRelated
    {
        public static CustomerSet GetDefaultCustomerSet(MPMFEVRP.Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel)
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            CustomerSet outcome = new CustomerSet(allCustomers[5], theProblemModel);
            outcome.Extend(allCustomers[2], theProblemModel);
            return outcome;
        }
    }
}
