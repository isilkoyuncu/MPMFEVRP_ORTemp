using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.ProblemModels;

namespace MPMFEVRPTests.TestUtility_DefaultGetters
{
    class ProblemRelated
    {
        public static EVvsGDV_MaxProfit_VRP GetDefaultProblem()
        {
            return new EVvsGDV_MaxProfit_VRP(
                new MPMFEVRP.Domains.ProblemDomain.ProblemDataPackage(
                    new MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader("C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\20c3sU2_10(10,3+2+5)_4(1+1+2)_E60.txt")
                )
            );
        }

        public static EVvsGDV_MaxProfit_VRP_Model GetDefaultProblemModel(MPMFEVRP.Implementations.Problems.EVvsGDV_MaxProfit_VRP problem)
        {
            return new EVvsGDV_MaxProfit_VRP_Model(problem);
        }
    }
}
