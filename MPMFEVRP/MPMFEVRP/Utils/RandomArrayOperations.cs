using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Utils
{
    class RandomArrayOperations
    {
        public static int Select(double randomKey, double[] probDist)
        {
            //We can double-check the probDist is actually a probability distribution with the array sum being 1
            for (int i = 0; i < probDist.Length - 1; i++)
            {
                if (randomKey <= probDist[i])
                {
                    return i;
                }
                else
                {
                    randomKey -= probDist[i];
                }
            }
            return probDist.Length - 1;
        }
    }
}
