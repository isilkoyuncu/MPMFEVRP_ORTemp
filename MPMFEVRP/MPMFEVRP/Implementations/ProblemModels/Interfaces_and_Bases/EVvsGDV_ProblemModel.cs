using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using System;
using System.Collections.Generic;
using System.Linq;
using MPMFEVRP.Utils;
using MPMFEVRP.Models.CustomerSetSolvers;

namespace MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases
{
    public abstract class EVvsGDV_ProblemModel : ProblemModelBase
    {
        protected string problemName;
        protected XCPlexVRPBase EV_TSPSolver;
        public Dictionary<string, int> EV_TSP_NumberOfCustomerSetsByStatus { get { if (EV_TSPSolver == null) return theGDVExploiter.AFV_Solver.NumberOfTimesSolveFoundStatus; else return EV_TSPSolver.NumberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> EV_TSP_TimeSpentAccount { get { if (EV_TSPSolver == null) return theGDVExploiter.AFV_Solver.TotalTimeInSolveOnStatus; else return EV_TSPSolver.TotalTimeInSolveOnStatus; } }
        //TheOther is created only for two EV_TSP vs. 1 GDV_TSP model comparison 
        protected XCPlexVRPBase TheOtherEV_TSPSolver;
        public Dictionary<string, int> TheOtherEV_TSP_NumberOfCustomerSetsByStatus { get { if (EV_TSPSolver == null) return theGDVExploiter.AFV_Solver.NumberOfTimesSolveFoundStatus; else return EV_TSPSolver.NumberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> TheOtherEV_TSP_TimeSpentAccount { get { if (EV_TSPSolver == null) return theGDVExploiter.AFV_Solver.TotalTimeInSolveOnStatus; else return EV_TSPSolver.TotalTimeInSolveOnStatus; } }
        protected XCPlexVRPBase GDV_TSPSolver;
        public Dictionary<string, int> GDV_TSP_NumberOfCustomerSetsByStatus { get { if (GDV_TSPSolver == null) return theGDVExploiter.GDV_Solver.NumberOfTimesSolveFoundStatus; else return GDV_TSPSolver.NumberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> GDV_TSP_TimeSpentAccount { get { if (GDV_TSPSolver == null) return theGDVExploiter.GDV_Solver.TotalTimeInSolveOnStatus; else return GDV_TSPSolver.TotalTimeInSolveOnStatus; } }

        protected XCPlexVRPBase GDV_OrienteeringSolver;
        protected XCPlexVRPBase EV_NDF_OrienteeringSolver;
        protected XCPlexVRPBase EV_ADF_OrienteeringSolver;

        XCPlex_Model_AFV_SingleCustomerSet newTSPsolverEV;
        XCPlex_Model_GDV_SingleCustomerSet newTSPsolverGDV;
        public RefuelingPathList NonDominatedRefuelingPaths = new RefuelingPathList();

        CustomerSetSolver_Homogeneous_ExploitingVirtualGDVs theGDVExploiter;
        PlainCustomerSetSolver_Homogeneous thePlainAFVSolver;



        public RefuelingPathGenerator rpg;

        public bool GDVOptimalRouteFeasibleForEV = false;
        public List<string> RouteConstructionMethodForEV = new List<string>(); // Here for statistical purposes
        public EVvsGDV_ProblemModel() { }
        public EVvsGDV_ProblemModel(EVvsGDV_Problem problem, Type TSPModelType)
        {
            pdp = new ProblemDataPackage(problem.PDP);
            problemCharacteristics = problem.ProblemCharacteristics;
            inputFileName = problem.PDP.InputFileName;
            problemName = problem.GetName();

            objectiveFunctionType = problem.ObjectiveFunctionType;
            objectiveFunction = problem.ObjectiveFunction;
            objectiveFunctionCoefficientsPackage = problem.ObjectiveFunctionCoefficientsPackage;
            coverConstraintType = problem.CoverConstraintType;
            SetNumVehicles();
            rechargingDuration_status = (RechargingDurationAndAllowableDepartureStatusFromES)problemCharacteristics.GetParameter(ParameterID.PRB_RECHARGING_ASSUMPTION).Value;
            //lambda = problemCharacteristics.GetParameter(ParameterID.PRB_LAMBDA).GetIntValue();
            CalculateBoundsForAllOriginalSWAVs();

            if (problemCharacteristics.GetParameter(ParameterID.PRB_CREATETSPSOLVERS).GetBoolValue())
                CreateTSPSolvers(typeof(XCPlex_Model_GDV_SingleCustomerSet)); //TODO: 10/1/19 this code make sure we create tsp solvers with adfwu, so no matter what we choose on the form it doesn't affect anything.
            if(problemCharacteristics.GetParameter(ParameterID.PRB_CREATEEXPLOITINGTSPSOLVER).GetBoolValue() || problemCharacteristics.GetParameter(ParameterID.PRB_CREATEPLAINTSPSOLVER).GetBoolValue())
                CreateNewTspSolvers();

            PopulateCompatibleSolutionTypes();
            CreateCustomerSetArchive();
            rpg = new RefuelingPathGenerator(this);
            PopulateNonDominatedRefuelingPaths();
        }
        void CreateNewTspSolvers()
        {
            //newTSPsolverEV = new XCPlex_Model_AFV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true), coverConstraintType);
            //newTSPsolverGDV = new XCPlex_Model_GDV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true, tighterAuxBounds: true), coverConstraintType);
            if(problemCharacteristics.GetParameter(ParameterID.PRB_CREATEEXPLOITINGTSPSOLVER).GetBoolValue())
                theGDVExploiter = new CustomerSetSolver_Homogeneous_ExploitingVirtualGDVs(this);
            if(problemCharacteristics.GetParameter(ParameterID.PRB_CREATEPLAINTSPSOLVER).GetBoolValue())
                thePlainAFVSolver = new PlainCustomerSetSolver_Homogeneous(this);
        }
        public string GetInstanceName(string inputFileName)
        {
            return inputFileName.Split('.')[0];
        }
        void CreateTSPSolvers(Type TSPModelType)
        {
            if (TSPModelType == typeof(XCPlex_Model_GDV_SingleCustomerSet))
            {
                EV_TSPSolver = new XCPlex_Model_AFV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
                GDV_TSPSolver = new XCPlex_Model_GDV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
            }
            else if (TSPModelType == typeof(XCPlex_ArcDuplicatingFormulation_woU))
            {
                EV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
                GDV_TSPSolver = new XCPlex_Model_GDV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
            }
            //else if (TSPModelType == typeof(XCPlex_NodeDuplicatingFormulation_woU))
            //{
            //    EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true));
            //    GDV_TSPSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true, tighterAuxBounds: true));
            //}
            else
            {
                throw new NotImplementedException("I do not trust any other model other than ADF and NDF without U.(IK)");
                //if (TSPModelType == typeof(XCPlex_ArcDuplicatingFormulation))
                //{
                //    EV_TSPSolver = new XCPlex_ArcDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true));
                //    GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true, tighterAuxBounds: true));
                //}
                //else if (TSPModelType == typeof(XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special))
                //{
                //    EV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true));
                //    GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU_GDV_TSP_special(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true, tighterAuxBounds: true));
                //}
                //else if (TSPModelType == typeof(XCPlex_NodeDuplicatingFormulation))
                //{
                //    EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, tighterAuxBounds: true));
                //    GDV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true, tighterAuxBounds: true));
                //}
                //else
                //{
                //    System.Windows.Forms.MessageBox.Show("The formulation you asked for doesn't exist as far as EVvsGDV_ProblemModel.CreateTSPSolvers method is concerned.");
                //    throw new Exception("The formulation you asked for doesn't exist as far as EVvsGDV_ProblemModel.CreateTSPSolvers method is concerned.");
                //}
            }
        }
        double GetWorstCaseOFV()
        {
            if (objectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                return double.MinValue;
            else //(objectiveFunctionType == ObjectiveFunctionTypes.Minimize)
                return double.MaxValue;
        }

        public override RouteOptimizationOutcome RouteOptimize(CustomerSet CS)
        {
            //TODO unit test: the following should combine all EV and GDVs with Concat, Concat works as long as the lists are not null. Since we create new instances here, even though the GetVehiclesOfCategory method returns nothing, they shouldn't be null.
            List<Vehicle> EVvehicles = new List<Vehicle>(VRD.GetVehiclesOfCategory(VehicleCategories.EV));
            List<Vehicle> GDVvehicles = new List<Vehicle>(VRD.GetVehiclesOfCategory(VehicleCategories.GDV));
            return RouteOptimize(CS, EVvehicles.Concat(GDVvehicles).ToList());
        }
        public override RouteOptimizationOutcome RouteOptimize(CustomerSet CS, List<Vehicle> vehicles)
        {
            //This method is designed to work with exactly one GDV and exactly one EV
            int GDVPositionInList = 0;
            int EVPositionInList = 1;
            if (vehicles.Count != 2)
                throw new Exception("EVvsGDV_ProblemModel.RouteOptimize must be invoked with a list of 2 vehicles!");
            if (vehicles[0].Category == vehicles[1].Category)
                throw new Exception("EVvsGDV_ProblemModel.RouteOptimize must be invoked with exactly one GDV and one EV!");
            if (vehicles[0].Category == VehicleCategories.EV)
            {
                GDVPositionInList = 1;
                EVPositionInList = 0;
            }

            List<VehicleSpecificRouteOptimizationOutcome> theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            VehicleSpecificRouteOptimizationOutcome vsroo_GDV = RouteOptimize(CS, vehicles[GDVPositionInList]);
            if (vsroo_GDV.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
                return new RouteOptimizationOutcome(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
            theList.Add(vsroo_GDV);
            VehicleSpecificRouteOptimizationOutcome vsroo_EV = RouteOptimize(CS, vehicles[EVPositionInList], GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
            theList.Add(vsroo_EV);
            return new RouteOptimizationOutcome(theList);
        }
        public override VehicleSpecificRouteOptimizationOutcome RouteOptimize(CustomerSet CS, Vehicle vehicle, VehicleSpecificRoute GDVOptimalRoute = null)
        {
            //This method has nothing to do with a list of previously generated customer sets, management of that is completely some other class's responsibility

            if ((vehicle.Category == VehicleCategories.GDV) || (GDVOptimalRoute == null))
                return RouteOptimize(CS, vehicle);
            //else: we are looking at an EV problem with a provided GDV-optimal route
            //First, we must check for AFV-feasibility of the provided GDV-optimal route
            //Make an AFV-optimized route out of the given GDV-optimized route:

            VehicleSpecificRoute fittedRoute = FitGDVOptimalRouteToEV(GDVOptimalRoute, vehicle);
            if (fittedRoute.Feasible)//if the fitted route is feasible:
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: fittedRoute);
            if (ProveAFVInfeasibilityOfCustomerSet(GDVOptimalRoute: GDVOptimalRoute) == AFVInfOfCustomerSet.AFVInfeasibilityOfCSProved)
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Infeasible);


            //If none of the previous conditions worked, we must solve an EV-TSP
            return RouteOptimize(CS, vehicle);
        }
        public RouteOptimizationOutcome NewRouteOptimize(CustomerSet CS)
        {
            //TODO unit test: the following should combine all EV and GDVs with Concat, Concat works as long as the lists are not null. Since we create new instances here, even though the GetVehiclesOfCategory method returns nothing, they shouldn't be null.
            List<Vehicle> EVvehicles = new List<Vehicle>(VRD.GetVehiclesOfCategory(VehicleCategories.EV));
            List<Vehicle> GDVvehicles = new List<Vehicle>(VRD.GetVehiclesOfCategory(VehicleCategories.GDV));
            return NewRouteOptimize(CS, EVvehicles.Concat(GDVvehicles).ToList());
        }
        public RouteOptimizationOutcome NewRouteOptimize(CustomerSet CS, List<Vehicle> vehicles)
        {
            //This method is designed to work with exactly one GDV and exactly one EV
            int GDVPositionInList = 0;
            int EVPositionInList = 1;
            if (vehicles.Count != 2)
                throw new Exception("EVvsGDV_ProblemModel.RouteOptimize must be invoked with a list of 2 vehicles!");
            if (vehicles[0].Category == vehicles[1].Category)
                throw new Exception("EVvsGDV_ProblemModel.RouteOptimize must be invoked with exactly one GDV and one EV!");
            if (vehicles[0].Category == VehicleCategories.EV)
            {
                GDVPositionInList = 1;
                EVPositionInList = 0;
            }

            List<VehicleSpecificRouteOptimizationOutcome> theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            VehicleSpecificRouteOptimizationOutcome vsroo_GDV = NewRouteOptimize(CS, vehicles[GDVPositionInList]);
            if (vsroo_GDV.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
                return new RouteOptimizationOutcome(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
            theList.Add(vsroo_GDV);
            VehicleSpecificRouteOptimizationOutcome vsroo_EV = NewEVRouteOptimize(CS, vehicles[EVPositionInList], GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
            theList.Add(vsroo_EV);
            return new RouteOptimizationOutcome(theList);
        }
        public VehicleSpecificRouteOptimizationOutcome NewEVRouteOptimize(CustomerSet CS, Vehicle theEV, VehicleSpecificRoute GDVOptimalRoute)
        {
            //We are looking at an EV problem with a provided GDV-optimal route
            //First, we must check for AFV-feasibility of the provided GDV-optimal route
            //Make an AFV-optimized route out of the given GDV-optimized route:

            VehicleSpecificRoute EVfittedRoute = NewFitGDVOptimalRouteToEV(GDVOptimalRoute, theEV);

            if ((EVfittedRoute.Feasible == CheckAFVFeasibilityOfGDVOptimalRoute(GDVOptimalRoute)) && EVfittedRoute.Feasible)//if the fitted route is feasible:
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: EVfittedRoute);
            if (ProveAFVInfeasibilityOfCustomerSet(GDVOptimalRoute: GDVOptimalRoute) == AFVInfOfCustomerSet.AFVInfeasibilityOfCSProved)
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Infeasible);
            //If none of the previous conditions worked, we must solve an EV-TSP
            return NewRouteOptimize(CS, theEV);
        }
        public VehicleSpecificRouteOptimizationOutcome NewRouteOptimize(CustomerSet CS, Vehicle vehicle)
        {
            VehicleSpecificRouteOptimizationOutcome vsroo;
            XCPlexVRPBase solver;
            if (vehicle.Category == VehicleCategories.EV)
            {
                solver = newTSPsolverEV;
            }
            else
            {
                solver = newTSPsolverGDV;
            }
            solver.RefineDecisionVariables(CS);
            //solver.ExportModel("model.lp");
            solver.Solve_and_PostProcess();
            if (solver.SolutionStatus == XCPlexSolutionStatus.Infeasible)
            {
                vsroo = new VehicleSpecificRouteOptimizationOutcome(vehicle.Category, solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
            }
            else if (solver.SolutionStatus == XCPlexSolutionStatus.Optimal)
            {
                vsroo = new VehicleSpecificRouteOptimizationOutcome(vehicle.Category, solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: solver.GetVehicleSpecificRoutes().First());
            }
            else
                throw new Exception("The TSPsolverEV.SolutionStatus is neither infeasible nor optimal for vehicle category: " + vehicle.Category.ToString());
            return vsroo;

        }
        VehicleSpecificRouteOptimizationOutcome RouteOptimize(CustomerSet CS, Vehicle vehicle)
        {
            XCPlexVRPBase solver = (vehicle.Category == VehicleCategories.GDV ? GDV_TSPSolver : EV_TSPSolver);
            solver.RefineDecisionVariables(CS);
            solver.Solve_and_PostProcess();
            VehicleSpecificRouteOptimizationOutcome vsroo;
            if (solver.SolutionStatus == XCPlexSolutionStatus.Infeasible)
                vsroo = new VehicleSpecificRouteOptimizationOutcome(vehicle.Category, solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Infeasible);
            else//optimal
                vsroo = new VehicleSpecificRouteOptimizationOutcome(vehicle.Category, solver.CPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: solver.GetVehicleSpecificRoutes().First()); //TODO unit test if GetVehicleSpecificRoutes returns only 1 VSR when TSP is chosen.
                                                                                                                                                                                                                      //            solver.ClearModel();
            return vsroo;
        }
        VehicleSpecificRoute FitGDVOptimalRouteToEV(VehicleSpecificRoute GDVOptimalRoute, Vehicle vehicle)
        {
            if (GDVOptimalRoute == null)//The GDVOptimalRoute was not provided
                throw new Exception("The GDVOptimalRoute was not provided to FitGDVOptimalRouteToEV!");
            else if (vehicle.Category != VehicleCategories.EV)
                throw new Exception("FitGDVOptimalRouteToEV invoked for a non-EV!");
            else
                return new VehicleSpecificRoute(this, vehicle, GDVOptimalRoute.ListOfVisitedNonDepotSiteIDs);
        }
        VehicleSpecificRoute NewFitGDVOptimalRouteToEV(VehicleSpecificRoute GDVOptimalRoute, Vehicle vehicle)//TODO: The name of this method is worse than horrible! Must fix it.
        {
            if (GDVOptimalRoute == null)//The GDVOptimalRoute was not provided
                throw new ArgumentException("The GDVOptimalRoute was not provided to FitGDVOptimalRouteToEV!");
            if (!GDVOptimalRoute.Feasible)
                throw new ArgumentException("EVvsGDV_ProblemModel.NewFitGDVOptimalRouteToEV invoked with an infeasible GDV route!");
            if(GDVOptimalRoute.NumberOfCustomersVisited == 0)
                throw new ArgumentException("EVvsGDV_ProblemModel.NewFitGDVOptimalRouteToEV invoked with a GDV route that serves no customers!");
            if (vehicle.Category != VehicleCategories.EV)
                throw new ArgumentException("FitGDVOptimalRouteToEV invoked for a non-EV!");

            return new VehicleSpecificRoute(this, vehicle, GDVOptimalRoute.ListOfVisitedNonDepotSiteIDs);
        }
        public AFVInfOfCustomerSet ProveAFVInfeasibilityOfCustomerSet(VehicleSpecificRoute GDVOptimalRoute = null)
        {
            Vehicle theAFV = VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
            double batteryCap = theAFV.BatteryCapacity;
            double energyConsumpRate = theAFV.ConsumptionRate;
            double refuelingRate = Double.MinValue;

            foreach (Site es in SRD.GetSitesList(SiteTypes.ExternalStation))
                if (refuelingRate < es.RechargingRate)
                    refuelingRate = es.RechargingRate;
            refuelingRate = Math.Min(refuelingRate, theAFV.MaxChargingRate);

            //This method returns enum: AFVInfOfCustomerSet outcomes are as follows: AFVInfeasibilityOfCSProved, AFVFeasibilityOfCSProved, and AFVInfeasibilityOfCSUnkown
            switch (rechargingDuration_status)
            {
                case RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full:
                    {
                        if (!SRD.GetSitesList(SiteTypes.Customer).Any(z => z.RechargingRate > 0.0)) //No internal station case
                        {
                            VehicleSpecificRoute EVfittedRoute = NewFitGDVOptimalRouteToEV(GDVOptimalRoute, theAFV);
                            double endOfRouteNetEnergy = (EVfittedRoute.GetVehicleMilesTraveled() * energyConsumpRate)- batteryCap;
                            if (endOfRouteNetEnergy < 0.0)
                                throw new Exception("ProveAFVInfeasibilityOfCustomerSet method is called for a feasble EVfittedRoute.");
                            else
                            {
                                double extraTimeNeeded = (Math.Ceiling(endOfRouteNetEnergy / batteryCap)) * (batteryCap / refuelingRate);
                                if (extraTimeNeeded > CRD.TMax - GDVOptimalRoute.GetTotalTime())
                                    return AFVInfOfCustomerSet.AFVInfeasibilityOfCSProved;
                            }
                        }

                        return AFVInfOfCustomerSet.AFVInfeasibilityOfCSUnkown;
                    }
                case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full:
                    {
                        return AFVInfOfCustomerSet.AFVInfeasibilityOfCSUnkown;
                    }
                case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial:
                    {
                        return AFVInfOfCustomerSet.AFVInfeasibilityOfCSUnkown;
                    }
                default:
                    throw new Exception("RechargingDuration_status is not specified!");
            }
        }

        public bool CheckAFVFeasibilityOfGDVOptimalRoute(VehicleSpecificRoute GDVOptimalRoute)
        {
            //Return true if and only if the GDV-Optimal Route is AFV-Feasible (in which case it's known to be AFV-Optimal as well
            if (GDVOptimalRoute == null)//The GDVOptimalRoute that was not provided cannot be feasible
                throw new Exception("The GDV optimal is not provided to CheckAFVFeasibilityOfGDVOptimalRoute method.");

            List<string> sitesVisitedOnTheRoute = GDVOptimalRoute.ListOfVisitedSiteIncludingDepotIDs;

            double SOC = VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
            for (int i = 0; i < sitesVisitedOnTheRoute.Count - 1; i++)
            {
                SOC = SOC - SRD.GetEVEnergyConsumption(sitesVisitedOnTheRoute[i], sitesVisitedOnTheRoute[i + 1]) + (SRD.GetSiteByID(sitesVisitedOnTheRoute[i + 1]).ServiceDuration * SRD.GetSiteByID(sitesVisitedOnTheRoute[i + 1]).RechargingRate);
                SOC = Math.Min(SOC, VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity);
                if (SOC < 0.0)
                    return false;
            }
            return true;
        }
        protected void CalculateBoundsForAllOriginalSWAVs()
        {
            CalculateEpsilonBounds();
            CalculateDeltaBounds();
            CalculateTBounds();
        }
        void CalculateEpsilonBounds()
        {
            double epsilonMax = double.MinValue;
            Vehicle theEV = VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
            foreach (SiteWithAuxiliaryVariables swav in SRD.AllOriginalSWAVs)
            {
                switch (swav.SiteType)
                {
                    case SiteTypes.Customer:
                        epsilonMax = Calculators.MaxSOCGainAtSite(swav, theEV, maxStayDuration: swav.ServiceDuration);
                        break;
                    case SiteTypes.ExternalStation:
                        epsilonMax = Calculators.MaxSOCGainAtESSite(SRD, CRD, swav, theEV);
                        break;
                    default:
                        epsilonMax = 0.0;
                        break;
                }
                swav.UpdateEpsilonBounds(epsilonMax);
            }
        }
        void CalculateDeltaBounds()
        {
            CalculateDeltaMinsViaLabelSetting();
            CalculateDeltaMaxsViaLabelSetting();
        }
        void CalculateDeltaMinsViaLabelSetting()
        {
            List<SiteWithAuxiliaryVariables> tempSWAVs = new List<SiteWithAuxiliaryVariables>(SRD.AllOriginalSWAVs);
            List<SiteWithAuxiliaryVariables> permSWAVs = new List<SiteWithAuxiliaryVariables>();

            foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                if (swav.SiteType == SiteTypes.Depot)
                    swav.UpdateDeltaMin(0.0);
                else
                    swav.UpdateDeltaMin(Math.Max(0, SRD.GetEVEnergyConsumption(swav.ID, SRD.GetSingleDepotID()) - swav.EpsilonMax));
            
            while (tempSWAVs.Count != 0)
            {
                SiteWithAuxiliaryVariables swavToPerm = tempSWAVs.First();
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    if (swav.DeltaMin < swavToPerm.DeltaMin)
                    {
                        swavToPerm = swav;
                    }
                tempSWAVs.Remove(swavToPerm);
                permSWAVs.Add(swavToPerm);
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    swav.UpdateDeltaMin(Math.Min(swav.DeltaMin, Math.Max(0, swavToPerm.DeltaMin + SRD.GetEVEnergyConsumption(swav.ID, swavToPerm.ID) - swav.EpsilonMax)));
            }
            if ((SRD.AllOriginalSWAVs.Length) != (permSWAVs.Count))
                throw new System.Exception("XCPlexVRPBase.SetDeltaMinViaLabelSetting could not produce proper delta bounds hence allOriginalSWAVs.Count!=permSWAVs.Count");
        }
        void CalculateDeltaMaxsViaLabelSetting()
        {
            List<SiteWithAuxiliaryVariables> tempSWAVs = new List<SiteWithAuxiliaryVariables>(SRD.AllOriginalSWAVs);
            List<SiteWithAuxiliaryVariables> permSWAVs = new List<SiteWithAuxiliaryVariables>();

            foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                if (swav.SiteType == SiteTypes.Depot)
                {
                    swav.UpdateDeltaMax(0.0);
                    swav.UpdateDeltaPrimeMax(VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity);
                }
                else
                {
                    swav.UpdateDeltaMax(VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity - SRD.GetEVEnergyConsumption(SRD.GetSingleDepotID(), swav.ID));
                    swav.UpdateDeltaPrimeMax(Math.Min(VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity, (swav.DeltaMax + swav.EpsilonMax)));
                }
            while (tempSWAVs.Count != 0)
            {
                SiteWithAuxiliaryVariables swavToPerm = tempSWAVs.First();
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    if (swav.DeltaPrimeMax > swavToPerm.DeltaPrimeMax)
                    {
                        swavToPerm = swav;
                    }
                tempSWAVs.Remove(swavToPerm);
                permSWAVs.Add(swavToPerm);
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                {
                    swav.UpdateDeltaMax(Math.Max(swav.DeltaMax, swavToPerm.DeltaPrimeMax - SRD.GetEVEnergyConsumption(swav.ID, swavToPerm.ID)));
                    swav.UpdateDeltaPrimeMax(Math.Min(VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity, (swav.DeltaMax + swav.EpsilonMax)));
                }
            }
            if (SRD.AllOriginalSWAVs.Length != permSWAVs.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMaxViaLabelSetting could not produce proper delta bounds hence allOriginalSWAVs.Count!=permSWAVs.Count");

            //Revisiting the depot and its duplicates
            foreach (SiteWithAuxiliaryVariables swav in SRD.AllOriginalSWAVs)
                if ((swav.X == SRD.GetSingleDepotSite().X) && (swav.Y == SRD.GetSingleDepotSite().Y))
                    if (swav.SiteType != SiteTypes.Customer)
                        swav.UpdateDeltaMax(VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity - GetMinEnergyConsumptionFromNonDepotToDepot());
        }
        void CalculateTBounds()
        {
            double tLS = double.MinValue;
            double tES = double.MaxValue;
            Site theDepot = SRD.GetSingleDepotSite();
            foreach (SiteWithAuxiliaryVariables swav in SRD.AllOriginalSWAVs)
            {
                if (swav.X == theDepot.X && swav.Y == theDepot.Y)
                {
                    if (swav.SiteType != SiteTypes.Depot)
                    {
                        tLS = CRD.TMax - GetMinTravelTimeFromDepotDuplicateToDepotThroughANode(swav);
                        tES = GetMinTravelTimeFromDepotToDepotDuplicateThroughANode(swav);
                    }
                    else
                    {
                        tLS = CRD.TMax;
                        tES = 0.0;
                    }
                }
                else
                {
                    tLS = CRD.TMax - SRD.GetTravelTime(swav.ID, theDepot.ID);
                    tES = SRD.GetTravelTime(theDepot.ID, swav.ID);
                }
                switch (swav.SiteType)
                {
                    case SiteTypes.Customer:
                        tLS -= swav.ServiceDuration;
                        break;
                    case SiteTypes.ExternalStation:
                        if (RechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                            tLS -= (VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity / Math.Min(VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate, swav.RechargingRate));
                        break;
                    default:
                        break;
                }
                swav.UpdateTBounds(tLS, tES);
            }
        }
        double GetMinTravelTimeFromDepotDuplicateToDepotThroughANode(SiteWithAuxiliaryVariables depotDuplicate)
        {
            Site theDepot = SRD.GetSingleDepotSite();
            double minTravelTime = double.MaxValue;
            foreach (SiteWithAuxiliaryVariables otherSwav in SRD.AllOriginalSWAVs)
                if (otherSwav.X != theDepot.X || otherSwav.Y != theDepot.Y)
                    minTravelTime = Math.Min(minTravelTime, SRD.GetTravelTime(depotDuplicate.ID, otherSwav.ID) + SRD.GetTravelTime(otherSwav.ID, theDepot.ID));

            return minTravelTime;
        }
        double GetMinTravelTimeFromDepotToDepotDuplicateThroughANode(SiteWithAuxiliaryVariables depotDuplicate)
        {
            Site theDepot = SRD.GetSingleDepotSite();
            double minTravelTime = double.MaxValue;
            foreach (SiteWithAuxiliaryVariables otherSwav in SRD.AllOriginalSWAVs)
                if (otherSwav.X != theDepot.X || otherSwav.Y != theDepot.Y)
                    minTravelTime = Math.Min(minTravelTime, SRD.GetTravelTime(theDepot.ID, otherSwav.ID) + SRD.GetTravelTime(otherSwav.ID, depotDuplicate.ID));

            return minTravelTime;
        }
        double GetMinEnergyConsumptionFromNonDepotToDepot()
        {
            Site theDepot = SRD.GetSingleDepotSite();
            double eMinToDepot = double.MaxValue;

            foreach (SiteWithAuxiliaryVariables swav in SRD.AllOriginalSWAVs)
                if ((swav.X != SRD.GetSingleDepotSite().X) || (swav.Y != SRD.GetSingleDepotSite().Y))
                    eMinToDepot = Math.Min(eMinToDepot, SRD.GetEVEnergyConsumption(swav.ID, SRD.GetSingleDepotID()));

            return eMinToDepot;
        }

        public void SetNumVehicles()
        {
            NumVehicles = new int[pdp.VRD.NumVehicleCategories];
            NumVehicles[0] = problemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            NumVehicles[1] = problemCharacteristics.GetParameter(ParameterID.PRB_NUM_GDV).GetIntValue();
        }
        public int GetNumVehicles(VehicleCategories vc)
        {
            if (vc == VehicleCategories.EV)
                return NumVehicles[0];
            else
                return NumVehicles[1];
        }
        void PopulateCompatibleSolutionTypes()
        {
            compatibleSolutions = new List<Type>()
            {
                typeof(CustomerSetBasedSolution),
                typeof(RouteBasedSolution)
            };
        }
        void CreateCustomerSetArchive()
        {
            archiveAllCustomerSets = true;
            customerSetArchive = new CustomerSetList();
        }
        //It can be useful if you want to check customer set archive ever
        public void ExportCustomerSetArchive2txt()
        {
            //System.IO.StreamWriter sw;
            //String fileName = InputFileName;
            //fileName = fileName.Replace(".txt", "");
            //string outputFileName = fileName + "_CustomerSetArchiveWcplex" + (!GDVOptimalRouteFeasibleForEV).ToString() + ".txt";
            //sw = new System.IO.StreamWriter(outputFileName);
            //sw.WriteLine("EV Routes" + "\t" + "EV Routes Profit" + "\t" + "OFV[0]" + "\t" + "GDV Routes" + "\t" + "GDV Routes Profit" + "\t" + "OFV[1]" + "\t" + "EV Feasible" + "\t" + "GDV Feasible" + "\t" + "EV Calc Type");
            //for (int i = 0; i < CustomerSetArchive.Count; i++)
            //{
            //    if (CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0] == null && CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1] == null)
            //    {
            //        sw.Write("null" + "\t" + "null" + "\t" + "null" + "\t" + "null" + "\t" + "null" + "\t" + "null");
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[0] + "-" + "null");
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[1] + "-" + "null");
            //        sw.Write("\t" + RouteConstructionMethodForEV[i]);

            //    }
            //    else if (CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited == null && CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1] != null)
            //    {
            //        sw.Write("null" + "\t" + "null" + "\t" + "null" + "\t");
            //        for (int j = 0; j < CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1; j++)
            //            sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[j] + "-");
            //        sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1]);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].TotalProfit);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OFV[1]);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[0] + "-" + "null");
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[1] + "-" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].Feasible.Last());
            //        sw.Write("\t" + "null");
            //    }
            //    else //if(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited != null)
            //    {
            //        for (int j = 0; j < CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited.Count - 1; j++)
            //            sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited[j] + "-");
            //        sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited[CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].SitesVisited.Count - 1]);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].TotalProfit);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OFV[0] + "\t");
            //        for (int j = 0; j < CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1; j++)
            //            sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[j] + "-");
            //        sw.Write(CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited[CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].SitesVisited.Count - 1] + "-");
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].TotalProfit);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.OFV[1]);
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[0] + "-" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[0].Feasible.Last());
            //        sw.Write("\t" + CustomerSetArchive[i].RouteOptimizerOutcome.Feasible[1] + "-" + CustomerSetArchive[i].RouteOptimizerOutcome.OptimizedRoute[1].Feasible.Last());
            //        sw.Write("\t" + RouteConstructionMethodForEV[i]);
            //    }
            //    sw.WriteLine();
            //}
            //sw.Close();
        }

        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            return CalculateObjectiveFunctionValue(solution.OFIDP);
        }
        public double CalculateObjectiveFunctionValue(ObjectiveFunctionInputDataPackage OFIDP)
        {
            //This method still uses everything from the problemModel, because this method resides in 
            double outcome = 0.0;
            List<VehicleCategories> vcList = new List<VehicleCategories>() { VehicleCategories.EV, VehicleCategories.GDV };
            switch (objectiveFunction)
            {
                case ObjectiveFunctions.MaximizeProfit:
                    foreach (VehicleCategories vc in vcList)
                    {
                        outcome += OFIDP.GetPrizeCollected(vc);
                        outcome -= VRD.GetTheVehicleOfCategory(vc).FixedCost * OFIDP.GetNumberOfVehiclesUsed(vc);
                        outcome -= VRD.GetTheVehicleOfCategory(vc).VariableCostPerMile * OFIDP.GetVMT(vc);


                        //TODO objectiveFunctionCoefficientsPackage is not set properly, I do not want to multiply these by 0.0 and loose all the data

                        //outcome += objectiveFunctionCoefficientsPackage.GetEVTotalPrizeCoefficient(vc) * OFIDP.GetPrizeCollected(vc);
                        //outcome -= objectiveFunctionCoefficientsPackage.GetFixedCostPerVehicle(vc) * OFIDP.GetNumberOfVehiclesUsed(vc);
                        //outcome -= objectiveFunctionCoefficientsPackage.GetCostPerMileOfTravel(vc) * OFIDP.GetVMT(vc);
                    }
                    break;
                case ObjectiveFunctions.MinimizeVMT:
                    outcome += OFIDP.GetTotalVMT();
                    break;
                case ObjectiveFunctions.MinimizeVariableCost:
                    foreach (VehicleCategories vc in vcList)
                    {
                        outcome += objectiveFunctionCoefficientsPackage.GetCostPerMileOfTravel(vc) * OFIDP.GetVMT(vc);
                    }
                    break;
                case ObjectiveFunctions.MinimizeTotalCost:
                    foreach (VehicleCategories vc in vcList)
                    {
                        outcome += objectiveFunctionCoefficientsPackage.GetFixedCostPerVehicle(vc) * OFIDP.GetNumberOfVehiclesUsed(vc);
                        outcome += objectiveFunctionCoefficientsPackage.GetCostPerMileOfTravel(vc) * OFIDP.GetVMT(vc);
                    }
                    break;
                default:
                    throw new NotImplementedException("EVvsGDV_ProblemModel.CalculateObjectiveFunctionValue");
            }
            return outcome;
        }
        // These following 3 methods exports calculated information as a text file
        public void ExportDistancesAsTxt()
        {
            System.IO.StreamWriter sw;
            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_distances.txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("Instance Name", inputFileName);
            for (int i = 0; i < pdp.SRD.NumNodes; i++)
            {
                sw.WriteLine();
                for (int j = 0; j < pdp.SRD.NumNodes; j++)
                {
                    sw.Write(pdp.SRD.Distance[i, j] + " ");
                }
            }
            sw.Close();
        }
        public void ExportTravelDurationAsTxt()
        {
            System.IO.StreamWriter sw;
            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_travelDurations.txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("Instance Name", inputFileName);
            for (int i = 0; i < pdp.SRD.NumNodes; i++)
            {
                sw.WriteLine();
                for (int j = 0; j < pdp.SRD.NumNodes; j++)
                {
                    sw.Write(pdp.SRD.TimeConsumption[i, j] + " ");
                }
            }
            sw.Close();
        }
        public void ExportEnergyConsumpionAsTxt()
        {
            System.IO.StreamWriter sw;
            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_energies.txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("Instance Name", inputFileName);
            for (int i = 0; i < pdp.SRD.NumNodes; i++)
            {
                sw.WriteLine();
                for (int j = 0; j < pdp.SRD.NumNodes; j++)
                {
                    sw.Write(pdp.SRD.EnergyConsumption[i, j, 0] + " ");
                }
            }
            sw.Close();
        }

        /// <summary>
        /// The output is a string array
        /// [0] is the "status," which is "Optimal," "Infeasible" or "GDVOnly," meaning it is Optimal for GDV but Infeasible for EV.
        /// [1-3] are the computation times with GDV, EV_NDF and EV_ADF, respectively
        /// [4-5] are the GDV_VMT and EV_VMT, respectively
        /// </summary>
        /// <param name="customerSet"></param>
        /// <returns></returns>
        public string[] TripleSolve(CustomerSet customerSet)
        {
            if (TheOtherEV_TSPSolver == null)
                InitializeForTripleSolve();

            GDV_TSPSolver.RefineDecisionVariables(customerSet);
            GDV_TSPSolver.Solve_and_PostProcess();

            EV_TSPSolver.RefineDecisionVariables(customerSet);
            EV_TSPSolver.Solve_and_PostProcess();

            TheOtherEV_TSPSolver.RefineDecisionVariables(customerSet);
            TheOtherEV_TSPSolver.Solve_and_PostProcess();

            string csStatus = GDV_TSPSolver.GetStatus().ToString();
            string EVNDFStatus = EV_TSPSolver.GetStatus().ToString();
            string EVADFStatus = TheOtherEV_TSPSolver.GetStatus().ToString();
            if (EVNDFStatus != EVADFStatus)
                throw new Exception("NDF and ADF found different statuses for the same customer set!");
            else
                if (EVNDFStatus != csStatus)
            {
                if ((csStatus == "Optimal") && (EVNDFStatus == "Infeasible"))
                    csStatus = "GDVOnly";
                else
                    throw new Exception("Some unrecognized status combination obtained!");
            }
            return new string[]
            {
                csStatus,
                GDV_TSPSolver.CPUtime.ToString(), EV_TSPSolver.CPUtime.ToString(), TheOtherEV_TSPSolver.CPUtime.ToString(),
                (csStatus=="Infeasible")?"-1.0":GDV_TSPSolver.ObjValue.ToString(), (EVNDFStatus=="Infeasible")?"-1.0":EV_TSPSolver.ObjValue.ToString()
            };
        }
        void InitializeForTripleSolve()
        {
            GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true), customerCoverageConstraint: CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
            //EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true), customerCoverageConstraint: CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
            EV_TSPSolver = new XCPlex_Model_AFV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true), customerCoverageConstraint: CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
            TheOtherEV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true), customerCoverageConstraint: CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce);
        }
        public string[] TripleOrienteeringSolve(Dictionary<string, double> customerCoverageConstraintShadowPrices, bool useRuntimeLimit = false, double runTimeLimit = double.MaxValue, bool compareToGDV = false, bool compareToEV_NDF = false)
        {
            if (GDV_OrienteeringSolver == null)
                InitializeForTripleOrienteeringSolve(useRuntimeLimit, runTimeLimit);

            if (compareToGDV)
            {
                GDV_OrienteeringSolver.RefineObjectiveFunctionCoefficients(customerCoverageConstraintShadowPrices);
                GDV_OrienteeringSolver.Solve_and_PostProcess();
            }

            if (compareToEV_NDF)
            {
                EV_NDF_OrienteeringSolver.RefineObjectiveFunctionCoefficients(customerCoverageConstraintShadowPrices);
                EV_NDF_OrienteeringSolver.Solve_and_PostProcess();
            }

            EV_ADF_OrienteeringSolver.RefineObjectiveFunctionCoefficients(customerCoverageConstraintShadowPrices);
            EV_ADF_OrienteeringSolver.Solve_and_PostProcess();

            //Time for output now
            if ((compareToEV_NDF) && (!useRuntimeLimit))
            {
                if (Math.Abs(EV_NDF_OrienteeringSolver.ObjValue - EV_ADF_OrienteeringSolver.ObjValue) > 0.00001)
                    throw new Exception("EV_NDF and EV_ADF Orienteering Solvers found different solutions!");
            }

            List<string> outcome = new List<string>();
            if (compareToGDV)
            {
                outcome.Add(GDV_OrienteeringSolver.GetStatus().ToString());
                outcome.Add(GDV_OrienteeringSolver.CPUtime.ToString("0.000"));
                outcome.Add(GDV_OrienteeringSolver.ObjValue.ToString("0.000"));
            }
            if (compareToEV_NDF)
            {
                outcome.Add(EV_NDF_OrienteeringSolver.GetStatus().ToString());
                outcome.Add(EV_NDF_OrienteeringSolver.CPUtime.ToString("0.000"));
                outcome.Add(EV_NDF_OrienteeringSolver.ObjValue.ToString("0.000"));
            }
            outcome.Add(EV_ADF_OrienteeringSolver.GetStatus().ToString());
            outcome.Add(EV_ADF_OrienteeringSolver.CPUtime.ToString("0.000"));
            outcome.Add(EV_ADF_OrienteeringSolver.ObjValue.ToString("0.000"));

            return outcome.ToArray();
        }
        void InitializeForTripleOrienteeringSolve(bool limitComputationTime, double runtimeLimit_Seconds)
        {
            GDV_OrienteeringSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true, limitComputationTime: limitComputationTime, runtimeLimit_Seconds: runtimeLimit_Seconds, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
            //EV_NDF_OrienteeringSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, limitComputationTime: limitComputationTime, runtimeLimit_Seconds: runtimeLimit_Seconds, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
            EV_NDF_OrienteeringSolver = new XCPlex_Model_AFV_SingleCustomerSet(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, limitComputationTime: limitComputationTime, runtimeLimit_Seconds: runtimeLimit_Seconds, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
            EV_ADF_OrienteeringSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true, limitComputationTime: limitComputationTime, runtimeLimit_Seconds: runtimeLimit_Seconds, tighterAuxBounds: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
        }

        void PopulateNonDominatedRefuelingPaths()
        {
            foreach (SiteWithAuxiliaryVariables swav1 in SRD.GetAllNonESSWAVsList())
                foreach (SiteWithAuxiliaryVariables swav2 in SRD.GetAllNonESSWAVsList())
                    if (swav1.ID != swav2.ID)
                    {
                        NonDominatedRefuelingPaths.AddRange(rpg.GenerateNonDominatedBetweenODPairIK(swav1, swav2, SRD));
                    }
        }

        public RouteOptimizationOutcome RouteOptimizeByExploitingGDVs(CustomerSet CS, Exploiting_GDVs_Flowchart flowchart, bool preserveCustomerVisitSequence, bool feasibleAFVSolnIsEnough, bool performSwap)
        {
            return theGDVExploiter.Solve(CS, flowchart, preserveCustomerVisitSequence, feasibleAFVSolnIsEnough, performSwap);
        }

        public OptimizationStatistics RetrieveExploitingGDVoptStat ()
        {
            return theGDVExploiter.optimizationStatstics;
        }
        public RouteOptimizationOutcome RouteOptimizeByPlainAFVSolver(CustomerSet CS, Exploiting_GDVs_Flowchart flowchart)
        {
            return thePlainAFVSolver.Solve(CS, flowchart);
        }

        public OptimizationStatistics RetrievePlainOptStat()
        {
            return thePlainAFVSolver.optimizationStatstics;
        }
    }
}
