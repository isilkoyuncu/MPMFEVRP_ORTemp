using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRPTests.TSPSolverTests
{
    [TestClass]
    public class EquivalenceOfFormulations
    {
        [TestMethod]
        public void NodeOrArcDuplicateForAGivenCustomerSet()
        {
            //Read the file #1
            MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader kyr = new MPMFEVRP.Implementations.Problems.Readers.KoyuncuYavuzReader("C:\\Users\\myavuz\\Google Drive\\KoyuncuYavuzInstances\\Instances for unit and integration tests\\20c3sU1_0(0,0+0+0)_4(4+0+0)_E60.txt");
            kyr.Read();
            EMH_Problem theProblem = new EMH_Problem(new MPMFEVRP.Domains.ProblemDomain.ProblemDataPackage(kyr));
            Vehicle gdv = theProblem.PDP.VRD.GetVehiclesOfCategory(VehicleCategories.GDV)[0];
            Vehicle ev = theProblem.PDP.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0];
            //Create models
            MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel nd = new MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel(theProblem, typeof(MPMFEVRP.Models.XCPlex.XCPlex_NodeDuplicatingFormulation_woU));
            MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel ad = new MPMFEVRP.Implementations.ProblemModels.EMH_ProblemModel(theProblem, typeof(MPMFEVRP.Models.XCPlex.XCPlex_ArcDuplicatingFormulation_woU));
            //Create customer set C6
            List<string> customers = new List<string>() { "C6" };
            CustomerSet theCustomerSet = new CustomerSet(customers);

            VehicleSpecificRoute vsrManual_nd_gdv = new VehicleSpecificRoute(nd, gdv, theCustomerSet.Customers);
            VehicleSpecificRoute vsrManual_nd_ev = new VehicleSpecificRoute(nd, ev, theCustomerSet.Customers);
            VehicleSpecificRoute vsrManual_ad_gdv = new VehicleSpecificRoute(nd, gdv, theCustomerSet.Customers);
            VehicleSpecificRoute vsrManual_ad_ev = new VehicleSpecificRoute(nd, ev, theCustomerSet.Customers);
            Assert.AreEqual(vsrManual_nd_gdv.Feasible, vsrManual_ad_gdv.Feasible);
            Assert.AreEqual(vsrManual_nd_gdv.GetVehicleMilesTraveled(), vsrManual_ad_gdv.GetVehicleMilesTraveled());
            Assert.AreEqual(vsrManual_nd_ev.Feasible, vsrManual_ad_ev.Feasible);
            Assert.AreEqual(vsrManual_nd_ev.GetVehicleMilesTraveled(), vsrManual_ad_ev.GetVehicleMilesTraveled());

            CustomerSet customerSet_nd = new CustomerSet(customers);
            customerSet_nd.Optimize(nd);
            CustomerSet customerSet_ad = new CustomerSet(customers);
            customerSet_ad.Optimize(ad);
            Assert.AreEqual(customerSet_nd.RouteOptimizationOutcome.Status, customerSet_ad.RouteOptimizationOutcome.Status);
            Assert.IsTrue(Math.Abs(customerSet_nd.RouteOptimizationOutcome.OFIDP.GetVMT(VehicleCategories.GDV) - customerSet_ad.RouteOptimizationOutcome.OFIDP.GetVMT(VehicleCategories.GDV)) <= 0.001);
            Assert.IsTrue(Math.Abs(customerSet_nd.RouteOptimizationOutcome.OFIDP.GetVMT(VehicleCategories.EV) - customerSet_ad.RouteOptimizationOutcome.OFIDP.GetVMT(VehicleCategories.EV)) <= 0.001);
        }
    }
}
