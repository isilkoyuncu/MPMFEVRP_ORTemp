using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Models.CustomerSetSolvers;
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

namespace MPMFEVRP.Models.CustomerSetSolvers.Tests
{
    [TestClass()]
    public class CustomerSetSolverWithOnlyAFVTests
    {
        EMH_Problem theProblem;
        EMH_ProblemModel theProblemModel;
        CustomerSetSolverWithOnlyAFV theSolver;

        [TestInitialize()]
        public void Initialize()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("10c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            theProblem = new EMH_Problem(pdp);
            //The problem has been created
            theProblemModel = new EMH_ProblemModel(theProblem, null);

            theSolver = new CustomerSetSolverWithOnlyAFV(theProblemModel);
        }


        [TestMethod()]
        public void SolveForASingleCustomerTest()
        {
            CustomerSet cs = new CustomerSet(theProblemModel.SRD.GetCustomerIDs().First(), theProblemModel.SRD.GetCustomerIDs());

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs,false);

            Assert.AreEqual(180.5072, Math.Round(theSolver.GetBestObjValue(),4));

            //Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Optimized, vsroo.Status);
            //Assert.AreEqual(1, vsroo.VSOptimizedRoute.NumberOfCustomersVisited);
            //Assert.AreEqual(180.5072, Math.Round(vsroo.VSOptimizedRoute.GetVehicleMilesTraveled(), 4));
        }

        [TestMethod()]
        public void SolveForFourCustomersTest()
        {
            CustomerSet cs = new CustomerSet("C6", theProblemModel.SRD.GetCustomerIDs());
            cs.NewExtend("C8");
            cs.NewExtend("C14");
            cs.NewExtend("C19");

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs,false);

            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Optimized, vsroo.Status);
            Assert.AreEqual(335.19, Math.Round(theSolver.GetBestObjValue(), 2));

            for (int i = 0; i < theSolver.DeltaValues.Length; i++)
            {
                if (theSolver.DeltaValues[i] > theSolver.preprocessedSites[i].DeltaMax)
                    Assert.Fail();
                else if (theSolver.DeltaValues[i] < theSolver.preprocessedSites[i].DeltaMin)
                    Assert.Fail();
                else if (theSolver.TValues[i] > theSolver.preprocessedSites[i].TLS)
                    Assert.Fail();
                else if (theSolver.TValues[i] < theSolver.preprocessedSites[i].TES)
                    Assert.Fail();
            }
            Assert.AreEqual(4, vsroo.VSOptimizedRoute.NumberOfCustomersVisited);
            Assert.AreEqual(7, vsroo.VSOptimizedRoute.NumberOfSitesVisited);//4 customers + 1 ES + 2 depots
            Assert.AreEqual(335.19, Math.Round(vsroo.VSOptimizedRoute.GetVehicleMilesTraveled(), 2));
        }

        [TestMethod()]
        public void SolveForTenCustomersTest()
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            CustomerSet cs = new CustomerSet(allCustomers.First(), theProblemModel.SRD.GetCustomerIDs());
            foreach (var c in allCustomers)
            {
                if (c != allCustomers.First())
                {
                    cs.NewExtend(c);
                }
            }

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs,false);
            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Infeasible, vsroo.Status);
        }
    }
}