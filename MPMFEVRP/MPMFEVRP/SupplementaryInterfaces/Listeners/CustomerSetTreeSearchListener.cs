using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.SupplementaryInterfaces.Listeners
{
    public interface CustomerSetTreeSearchListener
    {
        void OnChangeOfNumberOfUnexploredCustomerSets(int newNumberUnexplored);

        void OnChangeOfNumberOfUnexploredCustomerSets(int[] newNumberUnexplored);

        void OnChangeOfNumberOfAllCustomerSets(int newNumberAll);

        void OnChangeOfNumberOfAllCustomerSets(int[] newNumberAll);

        void OnChangeOfNumbersOfUnexploredAndExploredCustomerSets(int newNumberUnexplored, int newNumberAll);

        void OnChangeOfNumbersOfUnexploredAndExploredCustomerSets(int[] newNumberUnexplored, int[] newNumberAll);

    }
}
