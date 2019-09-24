using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Problems.Readers;

namespace MPMFEVRP.Domains.ProblemDomain.Tests
{
    [TestClass()]
    public class SiteRelatedDataTests
    {
        private EMH_ProblemModel theProblemModel;
        List<string> actualClosenessFromTheDepot;
        double[,] actualEs2esDistanceMatrix;
        [TestMethod()]
        public void GetClosenessOrderTest()
        {
            actualClosenessFromTheDepot = new List<string>() { "D", "D2", "C18", "C17", "C3", "C13", "C12", "C14", "C5", "C16", "C4", "C2", "BD12", "C8", "C20", "C15", "C11", "BD13", "C6", "C1", "C19", "C9", "C10", "C7", "BD20" };
            List<string> closenessFromTheDepot = new List<string>();
            closenessFromTheDepot = theProblemModel.SRD.GetClosenessOrder(theProblemModel.SRD.GetSingleDepotID());
            for (int i = 0; i < actualClosenessFromTheDepot.Count; i++)
            {
                Assert.AreEqual(actualClosenessFromTheDepot[i], closenessFromTheDepot[i]);
            }

            actualClosenessFromTheDepot = new List<string>() { "D2", "BD12", "BD13", "BD20" };
            closenessFromTheDepot = new List<string>();
            closenessFromTheDepot = theProblemModel.SRD.GetClosenessOrder("D", theProblemModel.SRD.GetSitesList(SiteTypes.ExternalStation).ToArray());
            for (int i = 0; i < actualClosenessFromTheDepot.Count; i++)
            {
                Assert.AreEqual(actualClosenessFromTheDepot[i], closenessFromTheDepot[i]);
            }
        }

        [TestMethod()]
        public void GetES2ESDistanceMatrixTest()
        {
            actualEs2esDistanceMatrix = new double[,] { {0.0,136.98214,89.62465,97.63018},
                                                        {136.98214,0.0,199.00686,192.15173},
                                                        {89.62465,199.00686,0.0,169.55728},
                                                        {97.63018,192.15173,169.55728,0.0}};
            double[,] es2esDist = theProblemModel.SRD.GetES2ESDistanceMatrix();
            for (int i = 0; i < actualEs2esDistanceMatrix.GetLength(0); i++)
            {
                for(int j=0; j< actualEs2esDistanceMatrix.GetLength(1); j++)
                Assert.AreEqual(actualEs2esDistanceMatrix[i,j], es2esDist[i,j]);
            }
        }

        [TestInitialize()]
        public void TestInit()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("20c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            EMH_Problem theProblem = new EMH_Problem(pdp);
            //The problem has been created
            theProblemModel = new EMH_ProblemModel(theProblem, null);
        }

        
    }
}