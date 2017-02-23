using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Utils
{
    public class DeepCopier
    {
        List<int> toReturnInt;
        public DeepCopier()
        {
            
        }
        public List<int> Copy(List<int> copyFrom)
        {
            toReturnInt = new List<int>();
            for (int i = 0; i < copyFrom.Count; i++)
                toReturnInt.Add(copyFrom[i]);
            return toReturnInt;
        }
    }
}
