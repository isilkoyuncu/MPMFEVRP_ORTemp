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
    public class CustomerSetSolverWithOnlyGDVTests
    {
        EMH_Problem theProblem;
        EMH_ProblemModel theProblemModel;
        CustomerSetSolverWithOnlyGDV theSolver;

        [TestInitialize()]
        public void Initialize()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("10c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            theProblem = new EMH_Problem(pdp);
            //The problem has been created
            theProblemModel = new EMH_ProblemModel(theProblem, null);

            theSolver = new CustomerSetSolverWithOnlyGDV(theProblemModel);
        }

        [TestMethod()]
        public void SolveForASingleCustomerTest()
        {
            CustomerSet cs = new CustomerSet(theProblemModel.SRD.GetCustomerIDs().First(), theProblemModel.SRD.GetCustomerIDs());

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs);
            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Optimized, vsroo.Status);
            Assert.AreEqual(1, vsroo.VSOptimizedRoute.NumberOfCustomersVisited);
            Assert.AreEqual(180.5072, Math.Round(vsroo.VSOptimizedRoute.GetVehicleMilesTraveled(),4));
        }
        [TestMethod()]
        public void SolveForFourCustomersTest()
        {
            CustomerSet cs = new CustomerSet("C6", theProblemModel.SRD.GetCustomerIDs());
            cs.Extend("C8");
            cs.Extend("C14");
            cs.Extend("C19");

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs);

            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Optimized, vsroo.Status);
            Assert.AreEqual(4, vsroo.VSOptimizedRoute.NumberOfCustomersVisited);
            Assert.AreEqual(333.07, Math.Round(vsroo.VSOptimizedRoute.GetVehicleMilesTraveled(), 2));
            //Assert.AreEqual(335.19, Math.Round(theSolver.GetBestObjValue(), 2));
        }

        [TestMethod()]
        public void SolveForThreeCustomersTest()
        {
            CustomerSet cs = new CustomerSet("C4", theProblemModel.SRD.GetCustomerIDs());
            cs.Extend("C11");
            cs.Extend("C3");

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs);
            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Optimized, vsroo.Status);
            Assert.AreEqual(3, vsroo.VSOptimizedRoute.NumberOfCustomersVisited);
            Assert.AreEqual(225.09, Math.Round(vsroo.VSOptimizedRoute.GetVehicleMilesTraveled(), 2));
        }

        [TestMethod()]
        public void SolveForTenCustomersTest()
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            CustomerSet cs = new CustomerSet(allCustomers.First(), theProblemModel.SRD.GetCustomerIDs());
            foreach(var c in allCustomers)
            {
                if(c!= allCustomers.First())
                {
                    cs.Extend(c);
                }
            }

            VehicleSpecificRouteOptimizationOutcome vsroo = theSolver.Solve(cs);
            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Infeasible, vsroo.Status);
        }
    }
}