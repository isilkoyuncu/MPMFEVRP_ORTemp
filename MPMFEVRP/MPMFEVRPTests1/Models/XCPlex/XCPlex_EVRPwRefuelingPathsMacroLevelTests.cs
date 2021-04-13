using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Readers;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;

namespace MPMFEVRPTests1.Models.XCPlex
{
    /// <summary>
    /// The purpose of this test is to solve EMH#10 using XCPlex_EVRPwRefuelingPaths
    /// </summary>
    [TestClass]
    public class XCPlex_EVRPwRefuelingPathsMacroLevelTests
    {
        Outsource2Cplex toCplex;

        [TestMethod]
        public void ADFTest()
        {
            toCplex.AlgorithmParameters.UpdateParameter(MPMFEVRP.Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            toCplex.Run();
            toCplex.Conclude();
            ISolution solution = toCplex.Solution;

            Assert.AreEqual(1181.30807, solution.UpperBound);
        }

        [TestMethod]
        public void RefuelingPathsFormulationTest()
        {
            toCplex.AlgorithmParameters.UpdateParameter(MPMFEVRP.Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.MixedEVRPwRefuelingPaths);
            toCplex.Run();
            toCplex.Conclude();
            ISolution solution = toCplex.Solution;

            Assert.AreEqual(1181.30807, solution.UpperBound);
        }

        [TestInitialize()]
        public void TestInit()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("20c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            EMH_Problem theProblem = new EMH_Problem(pdp);
            //The problem has been created
            EMH_ProblemModel theProblemModel = new EMH_ProblemModel(theProblem, null);
            toCplex = new Outsource2Cplex(); 
            toCplex.Initialize(theProblemModel);
        }

        
    }
}
