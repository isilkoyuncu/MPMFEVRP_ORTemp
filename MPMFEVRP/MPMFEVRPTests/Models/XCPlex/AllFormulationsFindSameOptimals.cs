//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MPMFEVRP.Implementations.Problems;
//using MPMFEVRP.Implementations.Algorithms;
//using MPMFEVRP.Implementations.Solutions;

//namespace MPMFEVRPTests.Models.XCPlex
//{
//    [TestClass]
//    public class AllFormulationsFindSameOptimals
//    {
//        [TestMethod]
//        public void FormulationsWithoutUFindSameOptimals()
//        {
//            string[] files = new string[] { "C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\Instances for unit and integration tests\\10c3sU1_0(0,0+0+0)_4(4+0+0)_E60.txt", "C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\Instances for unit and integration tests\\10c3sU5_0(0,0+0+0)_4(4+0+0)_E60.txt", "C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\Instances for unit and integration tests\\10c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt" };
//            foreach (string file in files)
//            {
//                //Read the file
//                MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader kyr = new MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader(file);
//                kyr.Read();
//                EMH_Problem theProblem = new EMH_Problem(new MPMFEVRP.Domains.ProblemDomain.ProblemDataPackage(kyr));
//                //Create models
//                MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel theProblemModelUsingNodewoUFormulation = new MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel(theProblem, typeof(MPMFEVRP.Models.XCPlex.XCPlex_NodeDuplicatingFormulation_woU));
//                MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel theProblemModelUsingArcwoUFormulation = new MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel(theProblem, typeof(MPMFEVRP.Models.XCPlex.XCPlex_ArcDuplicatingFormulation_woU));
//                //Create algorithm & run
//                Outsource2Cplex theAlgorithm_node = new Outsource2Cplex();
//                theAlgorithm_node.Initialize(theProblemModelUsingNodewoUFormulation);
//                theAlgorithm_node.Run();
//                theAlgorithm_node.Conclude();
//                Outsource2Cplex theAlgorithm_arc = new Outsource2Cplex();
//                theAlgorithm_arc.Initialize(theProblemModelUsingNodewoUFormulation);
//                theAlgorithm_arc.Run();
//                theAlgorithm_arc.Conclude();
//                //the test
//                Assert.IsTrue(Math.Abs(theAlgorithm_node.Stats.UpperBound - theAlgorithm_arc.Stats.UpperBound) <= 0.001);
//            }
//        }
//    }
//}
