using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Problems.Readers;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Models.Tests
{
    [TestClass()]
    public class RefuelingPathGeneratorTests
    {
        EMH_Problem theProblem;
        EMH_ProblemModel theProblemModel;
        RefuelingPathGenerator rpg;
        RefuelingPathList[,] rpl;
        int numNonESNodes;
        SiteWithAuxiliaryVariables[] preprocessedSites;

        [TestInitialize()]
        public void Initialize()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("5c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            theProblem = new EMH_Problem(pdp);
            //The problem has been created
            theProblemModel = new EMH_ProblemModel(theProblem, null);
            numNonESNodes = theProblemModel.SRD.NumCustomers + 1;
            rpg = new RefuelingPathGenerator(theProblemModel);
            rpl = new RefuelingPathList[numNonESNodes, numNonESNodes];
            preprocessedSites = theProblemModel.SRD.GetAllNonESSWAVsList().ToArray();
        }


        [TestMethod()]
        public void GenerateNonDominatedBetweenODPairIKTest()
        {
            for (int i = 0; i < numNonESNodes; i++)
            {
                SiteWithAuxiliaryVariables from = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    SiteWithAuxiliaryVariables to = preprocessedSites[j];
                    rpl[i, j] = rpg.GenerateNonDominatedBetweenODPairIK(from, to, theProblemModel.SRD);
                }
            }

            for (int i = 0; i < numNonESNodes; i++)
                Assert.AreEqual(rpl[i, i].Count, 0);


        }

        [TestMethod()]
        public void GenerateNonDominatedBetweenC6C19Test()
        {
            RefuelingPathList rplC6C19 = new RefuelingPathList();
            for (int i = 0; i < numNonESNodes; i++)
            {
                SiteWithAuxiliaryVariables from = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    SiteWithAuxiliaryVariables to = preprocessedSites[j];
                    if (from.ID == "C6" && to.ID == "C19")
                    {
                        rplC6C19 = rpg.GenerateNonDominatedBetweenODPairIK(from, to, theProblemModel.SRD);
                    }
                }
            }
            Assert.AreEqual(rplC6C19.Count, 2);
            Assert.AreEqual(rplC6C19[0].RefuelingStops.Count, 0);
            Assert.AreEqual(rplC6C19[1].RefuelingStops.Count, 1);
            Assert.AreEqual(rplC6C19[1].RefuelingStops.First().ID, "BD13");
        }
    }
}