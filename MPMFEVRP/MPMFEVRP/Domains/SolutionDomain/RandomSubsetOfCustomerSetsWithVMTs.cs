using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    /// <summary>
    /// This class holds a (pseudo-) random selection of customer sets with VMTs.
    /// </summary>
    public class RandomSubsetOfCustomerSetsWithVMTs
    {
        List<CustomerSetWithVMTs> randomlySelectedCustomerSets;
        public List<CustomerSetWithVMTs> RandomlySelectedCustomerSets { get => randomlySelectedCustomerSets; }

        Dictionary<string, int> numberOfAppearancesByCustomers;

        public int GetMinimumAppearanceOfACustomerInRandomlySelectedSets()
        {
            return numberOfAppearancesByCustomers.Values.Min();
        }

        /// <summary>
        /// Constructs a pseudo-random subset of the provided set of customer sets 
        /// </summary>
        /// <param name="allCustomerIDs">All customer IDs, to get things prepared.</param>
        /// <param name="providedCustomerSets">This is the complete set of customer sets we are to choose from.</param>
        /// <param name="customerSetSizeTreshold">Up to and including this treshold, a customer set is certainly selected. Beyond this number of customers, a customer set is individually and randomly added (or not) to the selection.</param>
        /// <param name="random"></param>
        /// <param name="selectionProbability"></param>
        public RandomSubsetOfCustomerSetsWithVMTs(List<string> allCustomerIDs, List<CustomerSetWithVMTs> providedCustomerSets, int customerSetSizeTreshold, Random random, double selectionProbability)
        {
            numberOfAppearancesByCustomers = new Dictionary<string, int>();
            foreach (string cID in allCustomerIDs)
                numberOfAppearancesByCustomers.Add(cID, 0);
            //What we just did above here assures that all customer ids are already in the dictionary, hence we don't have to check their existence when it comes to updating the counts below.

            randomlySelectedCustomerSets = new List<CustomerSetWithVMTs>();
            bool addToList;
            foreach (CustomerSetWithVMTs cswVMT in providedCustomerSets)
            {
                addToList = false;
                if (cswVMT.CustomerSet.NumberOfCustomers <= customerSetSizeTreshold)
                    addToList = true;
                else if (random.NextDouble() < selectionProbability)
                    addToList = true;
                if(addToList)
                {
                    randomlySelectedCustomerSets.Add(cswVMT);
                    foreach (string c in cswVMT.CustomerSet.Customers)
                        numberOfAppearancesByCustomers[c]++;
                }
            }
        }
    }
}
