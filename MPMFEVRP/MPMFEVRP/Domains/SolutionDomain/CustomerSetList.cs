using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class CustomerSetList : List<CustomerSet>
    {

        public CustomerSetList() { }

        public bool Includes(CustomerSet candidate)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return true;
            return false;
        }
    }
}
