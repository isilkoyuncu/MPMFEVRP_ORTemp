﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class PartitionedCustomerSetList
    {
        int deepestLevel;
        public int DeepestLevel { get { return deepestLevel; } }

        List<CustomerSetList> CSLList;

        public PartitionedCustomerSetList()
        {
            deepestLevel = -1;
            CSLList = new List<CustomerSetList>();
        }

        public void ConsiderForAddition(CustomerSet CS)
        {
            int level = CS.NumberOfCustomers;
            if(level > deepestLevel)//this is the first ever customer set seen at this level, the list at the level has not been needed so far, this is the time to create it
            {
                for (int l = deepestLevel + 1; l <= level; l++)
                    CSLList.Add(new CustomerSetList());
                deepestLevel = level;
            }

            if (!CSLList[level].Includes(CS))
                CSLList[level].Add(CS);
        }
        
        public CustomerSetList Pop(int level, int numberToPop)
        {
            return CSLList[level].Pop(numberToPop);
        }

        public int HighestNonemptyLevel()
        {
            for (int level = 0; level <= deepestLevel; level++)
                if (CSLList[level].Count > 0)
                    return level;
            return -1;
        }
        public int DeepestNonemptyLevel()
        {
            for (int level = deepestLevel; level >= 0; level--)
                if (CSLList[level].Count > 0)
                    return level;
            return -1;
        }
        public int TotalCount()
        {
            int outcome = 0;
            foreach (CustomerSetList csl in CSLList)
                outcome += csl.Count;
            return outcome;
        }

    }
}
