using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.SetCoverFileUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRPTests.TestUtility_DefaultGetters;

namespace MPMFEVRP.SetCoverFileUtilities.Tests
{
    [TestClass()]
    public class CustomerSetArchiveTests
    {
        [TestMethod()]
        public void SaveToFileTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RecreateFromFileTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SaveToAndRecreateFromFileTest()
        {
            Implementations.Problems.EVvsGDV_MaxProfit_VRP theProblem = ProblemRelated.GetDefaultProblem();
            Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel = ProblemRelated.GetDefaultProblemModel(theProblem);

            PartitionedCustomerSetList originalPartitionedCustomerSetList = SolutionRelated.GetDefaultPartitionedCustomerSetList(theProblemModel);

            string filename = "ArchiveFile4Test.txt";

            CustomerSetArchive.SaveToFile(originalPartitionedCustomerSetList, filename, theProblemModel);

            PartitionedCustomerSetList recreatedPartitionedCustomerSetList = CustomerSetArchive.RecreateFromFile(filename, theProblemModel);

            Assert.AreEqual(originalPartitionedCustomerSetList.TotalCount, recreatedPartitionedCustomerSetList.TotalCount);

            CustomerSetList flatOriginal = originalPartitionedCustomerSetList.ToCustomerSetList();
            CustomerSetList flatRecreated = recreatedPartitionedCustomerSetList.ToCustomerSetList();
            for (int i = 0; i < flatOriginal.Count; i++)
            {
                Assert.IsTrue(flatRecreated[i].IsIdentical(flatOriginal[i]));
            }
        }
    }
}