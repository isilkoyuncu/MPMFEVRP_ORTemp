using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.FormSections
{
    public class Experiment_RelatedData
    {
        int seed;
        public int Seed { get { return seed; } }
        
        public Experiment_RelatedData(int seed)
        {
            this.seed = seed;
        }

    }
}
