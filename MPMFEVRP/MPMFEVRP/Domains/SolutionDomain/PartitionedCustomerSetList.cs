using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class PartitionedCustomerSetList
    {
        int maxNumberOfCustomers = -1;
        List<CustomerSetList> CSList;

        public PartitionedCustomerSetList()
        {
            CSList = new List<CustomerSetList>();
        }

        public void ConsiderForAddition(CustomerSet CS)
        {
            int level = CS.NumberOfCustomers;
            if(level > maxNumberOfCustomers)//this is the first ever customer set seen at this level, the list at the level has not been needed so far, this is the time to create it
            {
                for (int l = maxNumberOfCustomers + 1; l <= level; l++)
                    CSList.Add(new CustomerSetList());
                maxNumberOfCustomers = level;
            }

            if (!CSList[level].Includes(CS))
                CSList[level].Add(CS);
        }
    }
}
