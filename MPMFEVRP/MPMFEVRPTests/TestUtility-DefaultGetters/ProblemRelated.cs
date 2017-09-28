using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models.XCPlex;

namespace MPMFEVRPTests.TestUtility_DefaultGetters
{
    class ProblemRelated
    {
        public static EVvsGDV_MaxProfit_VRP GetDefaultProblem()
        {
            //MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader kyr = new MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader("C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\20c3sU2_10(10,3+2+5)_4(1+1+2)_E60.txt");
            MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader kyr = new MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader("C:\\Users\\ikoyuncu\\Desktop\\MPMFEVRP_ORTemp\\MPMFEVRP\\MPMFEVRPTests\\bin\\Debug\\1emh debugging.txt");
            kyr.Read();
            return new EVvsGDV_MaxProfit_VRP(
                new ProblemDataPackage(
                    kyr
                )
            );
        }

        public static EVvsGDV_MaxProfit_VRP_Model GetDefaultProblemModel(EVvsGDV_MaxProfit_VRP problem)
        {
            throw new NotImplementedException();
            //return new EVvsGDV_MaxProfit_VRP_Model(problem);
        }

        public static XCPlexParameters GetXcplexParam()
        {
            return new XCPlexParameters(0.00001, false, double.MaxValue, XCPlexRelaxation.None, false, VehicleCategories.GDV);
        }
    }
}
