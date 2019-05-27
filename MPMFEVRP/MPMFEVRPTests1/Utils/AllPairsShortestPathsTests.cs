using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Readers;

namespace MPMFEVRP.Utils.Tests
{
    [TestClass()]
    public class AllPairsShortestPathsTests
    {
        AllPairsShortestPaths apss;
        double[,] distances, shortestDistances;
        List<string>[,] shortestPaths;
        string[] IDs;

        [TestMethod()]
        public void ModifiedFloydWarshallTest()
        {
            apss.ModifiedFloydWarshall();
            for (int i = 0; i < distances.GetLength(0); i++)
                for (int j = 0; j < distances.GetLength(0); j++)
                {
                    Assert.AreEqual(shortestDistances[i, j], apss.ShortestDistance[i, j]);
                    Assert.AreEqual(shortestPaths[i, j].Count, apss.ShortestPaths[i, j].Count);
                    for (int k = 0; k < shortestPaths[i, j].Count; k++)
                        Assert.AreEqual(shortestPaths[i, j][k], apss.ShortestPaths[i, j][k]);
                }
            List<string>[] actualFrom2 = new List<string>[] { new List<string> { "2", "3", "0" }, new List<string> { "2", "3", "0", "1" }, new List<string> { "2", "2" }, new List<string> { "2", "3" } };
            List<string>[] methodFrom2 = apss.GetShortestPathsFrom("2");
            
            for (int i = 0; i < distances.GetLength(0); i++)
            {
                Assert.AreEqual(methodFrom2[i].Count, actualFrom2[i].Count);
                for(int index=0;index<methodFrom2[i].Count;index++)
                    Assert.AreEqual(methodFrom2[i][index], actualFrom2[i][index]);
            }
        }

        [TestInitialize()]
        public void TestInit()
        {
            //KoyuncuYavuzReader reader = new KoyuncuYavuzReader("Large VA Input_111c_28s_0(0,0+0+0)_28(28+0+0)_E60.txt");
            //reader.Read();
            //ProblemDataPackage pdp = new ProblemDataPackage(reader);
            //EMH_Problem theProblem = new EMH_Problem(pdp);
            ////The problem has been created
            //EMH_ProblemModel theProblemModel = new EMH_ProblemModel(theProblem, null);
            distances = new double[,] { {0.0, 3.0, Double.MaxValue, 7.0 },
                                        {8.0, 0.0, 2.0, Double.MaxValue },
                                        {5.0, Double.MaxValue, 0.0, 1.0 },
                                        {2.0, Double.MaxValue, Double.MaxValue, 0.0 }};
            shortestDistances = new double[,] { {0.0, 3.0, 5.0 , 6.0 },
                                                {5.0, 0.0, 2.0 , 3.0 },
                                                {3.0, 6.0, 0.0 , 1.0 },
                                                {2.0, 5.0, 7.0 , 0.0 }};

            shortestPaths = new List<string>[,] { { new List<string> {"0","0" } , new List<string> { "0","1" }, new List<string>{ "0","1","2" } , new List<string> { "0","1","2","3" } },
                                               { new List<string> { "1", "2", "3", "0" } , new List<string> { "1", "1" }, new List<string> { "1", "2" } , new List<string> { "1", "2", "3" } },
                                               { new List<string> { "2", "3", "0" } , new List<string> { "2", "3", "0", "1"  }, new List<string> { "2", "2" } , new List<string> { "2", "3" } },
                                               { new List<string> { "3", "0" } , new List<string> { "3", "0", "1"}, new List<string> { "3", "0", "1", "2" } , new List<string> { "3", "3" } }};
            IDs = new string[] { "0", "1", "2", "3" };

            apss = new AllPairsShortestPaths(distances, IDs);
        }
    }
}