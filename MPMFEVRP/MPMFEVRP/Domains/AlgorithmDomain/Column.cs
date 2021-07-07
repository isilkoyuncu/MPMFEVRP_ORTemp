using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Domains.AlgorithmDomain
{
    public class Column
    {
        public CustomerSet optimizedCS;
        string id; public string ID { get { return id; } }        
        int iterationNo=-1; public int IterationNo { get { return iterationNo; } }
        int countTried2Use=0; public int CountTried2Use { get { return countTried2Use; } set { countTried2Use = value; } }
        int countUsed=0; public int CountUsed { get { return countUsed; } set { countUsed = value; } }
        bool partOftheRelaxedSolution=false; public bool PartOftheRelaxedSolution { get { return partOftheRelaxedSolution; } set { partOftheRelaxedSolution = value; } }
        bool partOftheIntegerSolution=false; public bool PartOftheIntegerSolution { get { return partOftheIntegerSolution; } set { partOftheIntegerSolution = value; } }
        public Column(CustomerSet cs)
        {
            optimizedCS = new CustomerSet(cs,copyROO:true);
        }
    }
}
