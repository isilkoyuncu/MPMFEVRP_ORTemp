//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MPMFEVRP.Domains.SolutionDomain;
//using MPMFEVRPTests.TestUtility_DefaultGetters;
//using System.Collections.Generic;
//using System.Linq;


//namespace MPMFEVRP.Models.XCPlex.Tests
//{
//    [TestClass()]
//    public class XCPlex_NodeDuplicatingFormulation_woUvariablesTests
//    {        
//        [TestMethod()]
//        public void GetVehicleSpecificRoutesTest()
//        {
//            Implementations.Problems.EVvsGDV_MaxProfit_VRP theProblem = ProblemRelated.GetDefaultProblem();
//            Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel = ProblemRelated.GetDefaultProblemModel(theProblem);

//            theProblemModel.NumVehicles[0] = 3;
//            theProblemModel.NumVehicles[1] = 3;

//            IEnumerable<IEnumerable<CustomerSet>> allPossibleCSs;
//            List<CustomerSet> cslist = new List<CustomerSet>();
//            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
//            foreach (string customerID in allCustomers)
//            {
//                cslist.Add(new CustomerSet(customerID, allCustomers));
//            }
//            allPossibleCSs = SubSetsOf(cslist);


//            Assert.Fail();
//        }
//        public static IEnumerable<IEnumerable<T>> SubSetsOf<T>(IEnumerable<T> source)
//        {
//            if (!source.Any())
//                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);

//            var element = source.Take(1);

//            var haveNots = SubSetsOf(source.Skip(1));
//            var haves = haveNots.Select(set => element.Concat(set));

//            return haves.Concat(haveNots);
//        }
//    }
//}