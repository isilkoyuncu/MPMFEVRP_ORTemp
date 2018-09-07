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


namespace MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases
{
    public abstract class EVvsGDV_ProblemModel: ProblemModelBase
    {
        protected string problemName;

        protected XCPlexVRPBase EV_TSPSolver;
        public Dictionary<string, int> EV_TSP_NumberOfCustomerSetsByStatus { get { return EV_TSPSolver.NumberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> EV_TSP_TimeSpentAccount { get { return EV_TSPSolver.TotalTimeInSolveOnStatus; } }
        //TheOther is created only for two EV_TSP vs. 1 GDV_TSP model comparison 
        protected XCPlexVRPBase TheOtherEV_TSPSolver;
        public Dictionary<string, int> TheOtherEV_TSP_NumberOfCustomerSetsByStatus { get { return EV_TSPSolver.NumberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> TheOtherEV_TSP_TimeSpentAccount { get { return EV_TSPSolver.TotalTimeInSolveOnStatus; } }
        protected XCPlexVRPBase GDV_TSPSolver;
        public Dictionary<string, int> GDV_TSP_NumberOfCustomerSetsByStatus { get { return GDV_TSPSolver.NumberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> GDV_TSP_TimeSpentAccount { get { return GDV_TSPSolver.TotalTimeInSolveOnStatus; } }

        protected XCPlexVRPBase GDV_OrienteeringSolver;
        protected XCPlexVRPBase EV_NDF_OrienteeringSolver;
        protected XCPlexVRPBase EV_ADF_OrienteeringSolver;


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
            lambda = problemCharacteristics.GetParameter(ParameterID.PRB_LAMBDA).GetIntValue();

            CreateTSPSolvers(TSPModelType);
            
            PopulateCompatibleSolutionTypes();
            CreateCustomerSetArchive();
        }
        public string GetInstanceName(string inputFileName)
        {
            return inputFileName.Split('.')[0];
        }
        void CreateTSPSolvers(Type TSPModelType)
        {
           if(TSPModelType == typeof(XCPlex_ArcDuplicatingFormulation))
            {
                EV_TSPSolver = new XCPlex_ArcDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
                GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
            }
           else if(TSPModelType == typeof(XCPlex_ArcDuplicatingFormulation_woU))
            {
                EV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
                GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
            }
           else if (TSPModelType == typeof(XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special))
            {
                EV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
                GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU_GDV_TSP_special(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
            }
            else if (TSPModelType == typeof(XCPlex_NodeDuplicatingFormulation))
            {
                EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
                GDV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
            }
            else if (TSPModelType == typeof(XCPlex_NodeDuplicatingFormulation_woU))
            {
                EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
                GDV_TSPSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("The formulation you asked for doesn't exist as far as EVvsGDV_ProblemModel.CreateTSPSolvers method is concerned.");
                throw new Exception("The formulation you asked for doesn't exist as far as EVvsGDV_ProblemModel.CreateTSPSolvers method is concerned.");
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
            if (vehicles[0].Category== vehicles[1].Category)
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
            if (ProveAFVInfeasibilityOfCustomerSet(CS, GDVOptimalRoute: GDVOptimalRoute))
                return new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Infeasible);
            //If none of the previous conditions worked, we must solve an EV-TSP
            return RouteOptimize(CS, vehicle);
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
        bool ProveAFVInfeasibilityOfCustomerSet(CustomerSet CS, VehicleSpecificRoute GDVOptimalRoute = null)
        {
            //Return true if and only if the GDV-Optimal route must be AFV-infeasible based on the data
            //This method may be a little confusing because it returns true when infeasible
            //For now we ignore this method //TODO: Develop the analytically provable conditions of AFV-infeasibility of a given GDV-feasible route

            return false;
        }
        
        /* We never use the following two methods?? */
        bool CheckAFVFeasibilityOfGDVOptimalRoute(VehicleSpecificRouteOptimizationOutcome GDVOptimalRoute) //TODO either delete or update: we never use this method, actually we need the fitted route to check the feasibility
        {
            //Return true if and only if the GDV-Optimal Route is AFV-Feasible (in which case it's known to be AFV-Optimal as well

            if (GDVOptimalRoute == null)//The GDVOptimalRoute that was not provided cannot be feasible
                return false;

            return false;//TODO Do the actual checking here!
        }
        public VehicleSpecificRoute GetVSRFromFlowVariables(Vehicle vehicle, List<Tuple<int, int, int>> allXSetTo1) //ISSUE (#7): Is this the solver's responsibility or the problem model's? We already have the code in the solver
        {
            List<string> nondepotSiteIDsInOrder = new List<string>();
            int vehicleCategoryIndex = (vehicle.Category == VehicleCategories.EV ? 0 : 1);
            int lastSiteIndex = -1;
            string lastSiteID = "";

            //first determining the number of routes
            List<Tuple<int, int, int>> tobeRemoved = new List<Tuple<int, int, int>>();
            foreach (Tuple<int, int, int> x in allXSetTo1)
                if ((x.Item1 == 0) && (x.Item3 == vehicleCategoryIndex))
                {
                    lastSiteIndex = x.Item2;
                    lastSiteID = SRD.GetSiteID(lastSiteIndex);
                    nondepotSiteIDsInOrder.Add(lastSiteID);
                    tobeRemoved.Add(x);
                }
            if (tobeRemoved.Count != 1)
                throw new Exception("EVvsGDV_ProblemModel.GetVSRFromFlowVariables invoked with allXSetTo1 including multiple departures from the depot!");
            foreach (Tuple<int, int, int> x in tobeRemoved)
            {
                allXSetTo1.Remove(x);
            }
            tobeRemoved.Clear();
            //Next, completeing the routes one-at-a-time
            int lastSiteIndex_2 = -1;
            bool extensionDetected = false;
            while ((lastSiteIndex != 0) && (allXSetTo1.Count > 0))
            {
                lastSiteIndex_2 = lastSiteIndex;
                extensionDetected = false;
                foreach (Tuple<int, int, int> x in allXSetTo1)
                {
                    if (x.Item1 == lastSiteIndex_2)
                    {
                        lastSiteIndex = x.Item2;
                        lastSiteID = SRD.GetSiteID(lastSiteIndex);
                        if(lastSiteID!=SRD.GetSingleDepotID())
                            nondepotSiteIDsInOrder.Add(lastSiteID);
                        allXSetTo1.Remove(x);
                        extensionDetected = true;
                        break;
                    }
                }
                if (!extensionDetected)
                    throw new Exception("Infeasible complete solution due to an incomplete route!");
            }

            if (allXSetTo1.Count > 0)
                throw new Exception("Infeasible complete solution due to subtours or routes that don't start/end at the depot");

            return new VehicleSpecificRoute(this, vehicle, nondepotSiteIDsInOrder);
        }

        public void SetNumVehicles()
        {
            NumVehicles = new int[pdp.VRD.NumVehicleCategories];
            NumVehicles[0] = problemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            NumVehicles[1] = problemCharacteristics.GetParameter(ParameterID.PRB_NUM_GDV).GetIntValue();
        }
        public int GetNumVehicles(VehicleCategories vc)
        {
            if(vc==VehicleCategories.EV)
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
                        outcome += objectiveFunctionCoefficientsPackage.GetEVTotalPrizeCoefficient(vc) * OFIDP.GetPrizeCollected(vc);
                        outcome -= objectiveFunctionCoefficientsPackage.GetFixedCostPerVehicle(vc) * OFIDP.GetNumberOfVehiclesUsed(vc);
                        outcome -= objectiveFunctionCoefficientsPackage.GetCostPerMileOfTravel(vc) * OFIDP.GetVMT(vc);
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
            GDV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));
            EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
            TheOtherEV_TSPSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
        }

        public string[] TripleOrienteeringSolve(Dictionary<string, double> customerCoverageConstraintShadowPrices, bool compareToGDV = false, bool compareToEV_NDF = false)
        {
            if (GDV_OrienteeringSolver == null)
                InitializeForTripleOrienteeringSolve();

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
            if (compareToEV_NDF)
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
        void InitializeForTripleOrienteeringSolve()
        {
            GDV_OrienteeringSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
            EV_NDF_OrienteeringSolver = new XCPlex_NodeDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
            EV_ADF_OrienteeringSolver = new XCPlex_ArcDuplicatingFormulation_woU(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true), CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce);
        }
    }
}
