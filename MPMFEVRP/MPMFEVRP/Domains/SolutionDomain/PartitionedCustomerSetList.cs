using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class PartitionedCustomerSetList
    {
        CustomerSetList.CustomerListPopStrategy popStrategy;

        int deepestLevel;
        public int DeepestLevel { get { return deepestLevel; } }

        List<CustomerSetList> CSLList;

        public PartitionedCustomerSetList()
        {
            deepestLevel = -1;
            CSLList = new List<CustomerSetList>();
        }
        public PartitionedCustomerSetList(CustomerSetList.CustomerListPopStrategy popStrategy)
        {
            this.popStrategy = popStrategy;
            deepestLevel = -1;
            CSLList = new List<CustomerSetList>();
        }

        public void ConsiderForAddition(CustomerSetList CSL)
        {
            while (CSL.Count > 0)
                ConsiderForAddition(CSL.Pop(CustomerSetList.CustomerListPopStrategy.First));//TODO: Verify that this actually depletes the CSL one-CS-at-a-time
        }
        public void ConsiderForAddition(CustomerSet CS)
        {
            int level = CS.NumberOfCustomers;
            if(level > deepestLevel)//this is the first ever customer set seen at this level, the list at the level has not been needed so far, this is the time to create it
            {
                for (int l = deepestLevel + 1; l <= level; l++)
                    CSLList.Add(new CustomerSetList(popStrategy));
                deepestLevel = level;
            }

            if (!CSLList[level].Includes(CS))
                CSLList[level].Add(CS);
        }
        
        public CustomerSetList Pop(int level, int numberToPop)
        {
            return CSLList[level].Pop(numberToPop);
        }

        public int GetHighestNonemptyLevel()
        {
            for (int level = 0; level <= deepestLevel; level++)
                if (CSLList[level].Count > 0)
                    return level;
            return -1;
        }
        public int GetDeepestNonemptyLevel()
        {
            for (int level = deepestLevel; level >= 0; level--)
                if (CSLList[level].Count > 0)
                    return level;
            return -1;
        }
        public int TotalCount
        {
            get
            {
                int outcome = 0;
                foreach (CustomerSetList csl in CSLList)
                    outcome += csl.Count;
                return outcome;
            }
        }

        public bool Contains(CustomerSet cs) { return (CSLList.Count == 0 ? false : (CSLList[cs.NumberOfCustomers] == null ? false : CSLList[cs.NumberOfCustomers].Contains(cs))); }
        public void Add(CustomerSet cs)
        {
            if ((CSLList.Count==0)||(cs.NumberOfCustomers>deepestLevel))
                ConsiderForAddition(cs);
            else
                CSLList[cs.NumberOfCustomers].Add(cs);
        }

        public CustomerSetList ToCustomerSetList()
        {
            CustomerSetList outcome = new CustomerSetList();
            for (int l = 0; l <= deepestLevel; l++)
                foreach (CustomerSet cs in CSLList[l])
                    outcome.Add(cs);
            return outcome;
        }
    }
}
