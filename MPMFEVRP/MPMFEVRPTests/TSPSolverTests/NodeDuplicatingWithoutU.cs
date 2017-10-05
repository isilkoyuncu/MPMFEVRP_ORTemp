using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Implementations.Problems;
using System.Collections.Generic;

namespace MPMFEVRPTests.TSPSolverTests
{
    [TestClass]
    public class NodeDuplicatingWithoutU
    {
        [TestMethod]
        public void InfeasibilityProvenQuickly()
        {
            //Read the file #1
            MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader kyr = new MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader("C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\EMH Comparison Data Sets\\Table1\\20\\20c3sU1_0(0,0+0+0)_4(4+0+0)_E60.txt");
            kyr.Read();
            EMH_Problem theProblem = new EMH_Problem(new MPMFEVRP.Domains.ProblemDomain.ProblemDataPackage(kyr));
            //Create model
            MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel theProblemModel = new MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel(theProblem, typeof(MPMFEVRP.Models.XCPlex.XCPlex_NodeDuplicatingFormulation_woUvariables));
            //Create customer set C6,9,19,20
            MPMFEVRP.Domains.SolutionDomain.CustomerSet theCustomerSet = new MPMFEVRP.Domains.SolutionDomain.CustomerSet(new List<string>() { "C6", "C8", "C19", "C20" });
            //Solve
            DateTime startTime = DateTime.Now;
            theCustomerSet.Optimize(theProblemModel, theProblemModel.VRD.GetVehiclesOfCategory(MPMFEVRP.Domains.ProblemDomain.VehicleCategories.GDV)[0]);
            DateTime endTime = DateTime.Now;
            double compTime = (endTime - startTime).TotalMilliseconds;
            //Solution time must be below 0.1 seconds
            Assert.IsTrue(compTime <= 100);
        }
    }
}
