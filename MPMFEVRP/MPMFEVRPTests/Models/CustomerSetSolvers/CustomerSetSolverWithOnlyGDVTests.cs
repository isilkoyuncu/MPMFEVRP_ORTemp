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

        [TestInitialize()]
        public void Initialize()
        {
            KoyuncuYavuzReader reader = new KoyuncuYavuzReader("10c3sU10_0(0,0+0+0)_4(4+0+0)_E60.txt");
            reader.Read();
            ProblemDataPackage pdp = new ProblemDataPackage(reader);
            theProblem = new EMH_Problem(pdp);
            //The problem has been created
            theProblemModel = new EMH_ProblemModel(theProblem, null);
        }

        [TestMethod()]
        public void SolveForASingleCustomerTest()
        {
            CustomerSet cs = new CustomerSet(theProblemModel.SRD.GetCustomerIDs().First(), theProblemModel.SRD.GetCustomerIDs());

            CustomerSetSolverWithOnlyGDV csswog = new CustomerSetSolverWithOnlyGDV(theProblemModel);
            VehicleSpecificRouteOptimizationOutcome vsroo = csswog.Solve(cs);

            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Optimized, vsroo.Status);
            Assert.AreEqual(1, vsroo.VSOptimizedRoute.NumberOfCustomersVisited);
            Assert.AreEqual(180.5072, Math.Round(vsroo.VSOptimizedRoute.GetVehicleMilesTraveled(),4));
        }

        [TestMethod()]
        public void SolveForThreeCustomersTest()
        {
            CustomerSet cs = new CustomerSet("C4", theProblemModel.SRD.GetCustomerIDs());
            cs.NewExtend("C11");
            cs.NewExtend("C3");

            CustomerSetSolverWithOnlyGDV csswog = new CustomerSetSolverWithOnlyGDV(theProblemModel);
            VehicleSpecificRouteOptimizationOutcome vsroo = csswog.Solve(cs);

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
                    cs.NewExtend(c);
                }
            }

            CustomerSetSolverWithOnlyGDV csswog = new CustomerSetSolverWithOnlyGDV(theProblemModel);
            VehicleSpecificRouteOptimizationOutcome vsroo = csswog.Solve(cs);

            Assert.AreEqual(VehicleSpecificRouteOptimizationStatus.Infeasible, vsroo.Status);
        }
    }
}