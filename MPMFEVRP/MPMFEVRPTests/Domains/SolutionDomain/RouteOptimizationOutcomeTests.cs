using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPMFEVRP.Domains.SolutionDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRPTests.TestUtility_DefaultGetters;

namespace MPMFEVRP.Domains.SolutionDomain.Tests
{
    [TestClass()]
    public class RouteOptimizationOutcomeTests
    {
        [TestMethod()]
        public void GetVehicleSpecificRouteOptimizationOutcomeTest()
        {
            //Implementations.Problems.EVvsGDV_MaxProfit_VRP theProblem = ProblemRelated.GetDefaultProblem();
            //Implementations.ProblemModels.EVvsGDV_MaxProfit_VRP_Model theProblemModel = ProblemRelated.GetDefaultProblemModel(theProblem);

            //CustomerSet theCustomerSet = SolutionRelated.GetDefaultCustomerSet(theProblemModel);

            VehicleSpecificRouteOptimizationOutcome GDVOutcome = new VehicleSpecificRouteOptimizationOutcome(ProblemDomain.VehicleCategories.GDV, VehicleSpecificRouteOptimizationStatus.Optimized, objectiveFunctionValue: 12.0);
            VehicleSpecificRouteOptimizationOutcome EVOutcome = new VehicleSpecificRouteOptimizationOutcome(ProblemDomain.VehicleCategories.EV, VehicleSpecificRouteOptimizationStatus.Optimized, objectiveFunctionValue: 36.0);
            RouteOptimizationOutcome overallOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { GDVOutcome, EVOutcome });

            Assert.IsNotNull(overallOutcome.GetVehicleSpecificRouteOptimizationOutcome(ProblemDomain.VehicleCategories.GDV));
            Assert.AreEqual(12.0, overallOutcome.GetVehicleSpecificRouteOptimizationOutcome(ProblemDomain.VehicleCategories.GDV).ObjectiveFunctionValue);
            Assert.IsNotNull(overallOutcome.GetVehicleSpecificRouteOptimizationOutcome(ProblemDomain.VehicleCategories.EV));
            Assert.AreEqual(36.0, overallOutcome.GetVehicleSpecificRouteOptimizationOutcome(ProblemDomain.VehicleCategories.EV).ObjectiveFunctionValue);
        }
    }
}