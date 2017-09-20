using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Models.XCPlex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRPTests.TestUtility_DefaultGetters;
using MPMFEVRP.Domains.SolutionDomain;


namespace MPMFEVRP.Models.XCPlex.Tests
{
    [TestClass()]
    public class XCPlex_NodeDuplicatingFormulation_woUvariablesTests
    {        
        [TestMethod()]
        public void GetVehicleSpecificRoutesTest()
        {
            Implementations.Problems.EVvsGDV_MaxProfit_VRP theProblem = ProblemRelated.GetDefaultProblem();
            Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel = ProblemRelated.GetDefaultProblemModel(theProblem);

            theProblemModel.NumVehicles[0] = 3;
            theProblemModel.NumVehicles[1] = 3;

            IEnumerable<IEnumerable<CustomerSet>> allPossibleCSs;
            List<CustomerSet> cslist = new List<CustomerSet>();
            foreach (string customerID in theProblemModel.SRD.GetCustomerIDs())
                cslist.Add(new CustomerSet(customerID));
            allPossibleCSs = SubSetsOf(cslist);


            Assert.Fail();
        }
        public static IEnumerable<IEnumerable<T>> SubSetsOf<T>(IEnumerable<T> source)
        {
            if (!source.Any())
                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);

            var element = source.Take(1);

            var haveNots = SubSetsOf(source.Skip(1));
            var haves = haveNots.Select(set => element.Concat(set));

            return haves.Concat(haveNots);
        }
    }
}