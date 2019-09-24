using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Implementations.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Readers;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Algorithms.Tests
{
    [TestClass()]
    public class PathBasedApproachTests
    {
        EMH_ProblemModel theProblemModel;
        List<List<string>> actNonDominatedRefuelingPathIDLists_DC4_VA14;
        List<List<string>> calcNonDominatedRefuelingPathIDLists_DC4_VA14;
        List<List<string>> tempNonDominatedRefuelingPathIDLists_DC4_VA14;

        PathBasedApproach pba;

        [TestMethod()]
        public void AddSpecializedParametersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SpecializedInitializeTest()
        {
            pba.SpecializedInitialize(theProblemModel);
            if (pba.RPL.Count - 1 != actNonDominatedRefuelingPathIDLists_DC4_VA14.Count)
                Assert.Fail();
            else
            {
                List<string> calcNDRPIDs;
                calcNonDominatedRefuelingPathIDLists_DC4_VA14 = new List<List<string>>();
                tempNonDominatedRefuelingPathIDLists_DC4_VA14 = new List<List<string>>();
                for (int j = 1; j < pba.RPL.Count; j++)
                {
                    RefuelingPath rp = pba.RPL[j];
                    calcNDRPIDs = new List<string>();
                    for (int i = 0; i < rp.RefuelingStops.Count; i++)
                        calcNDRPIDs.Add(rp.RefuelingStops[i].ID.ToString());
                    calcNonDominatedRefuelingPathIDLists_DC4_VA14.Add(calcNDRPIDs);
                    tempNonDominatedRefuelingPathIDLists_DC4_VA14.Add(calcNDRPIDs);
                }
                foreach (List<string> rp_act in actNonDominatedRefuelingPathIDLists_DC4_VA14)
                {
                    foreach (List<string> rp_calc in calcNonDominatedRefuelingPathIDLists_DC4_VA14)
                    {
                        IEnumerable<string> difference = rp_act.Except(rp_calc);
                        if (!difference.Any())
                        {
                            tempNonDominatedRefuelingPathIDLists_DC4_VA14.Remove(rp_calc);
                        }
                    }
                }
                if (tempNonDominatedRefuelingPathIDLists_DC4_VA14.Any())
                    Assert.Fail();
            }
        }

        [TestMethod()]
        public void SpecializedRunTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SpecializedConcludeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SpecializedResetTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOutputSummaryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNameTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void setListenerTest()
        {
            Assert.Fail();
        }
        [TestInitialize()]
        public void TestInit()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("dc4-va14.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            EMH_Problem theProblem = new EMH_Problem(pdp);
            //The problem has been created
            theProblemModel = new EMH_ProblemModel(theProblem, null);
            actNonDominatedRefuelingPathIDLists_DC4_VA14 = new List<List<string>> { new List<string> { "S3" }, new List<string> { "BD1" }, new List<string> { "BD3" }, new List<string> { "BD5" }, new List<string> { "BD9" }, new List<string> { "BD11" }, new List<string> { "BD1", "BD5" }, new List<string> { "BD1", "BD9" }, new List<string> { "BD1", "BD11" }, new List<string> { "BD5", "BD9" }, new List<string> { "BD5", "BD11" } };

            pba = new PathBasedApproach();
        }

        [TestMethod()]
        public void CalculateNDRP_DynamicallyTest()
        {
            Assert.Fail();
        }
    }
}