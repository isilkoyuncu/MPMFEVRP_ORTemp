using ILOG.Concert;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_ETSP_VP : XCPlexVRPBase
    {
        //Problem specific parameters
        int numNonESNodes;
        double batteryCapacity; //kWh
        double refuelingRate;
        double planningHorizonLength; //mins
        int totalTravelTimeConstraintIndex = -1;     

        RefuelingPathGenerator rpg;
        RefuelingPathList[,] allNondominatedRPs;
        RefuelingPathList[,] nondominatedRPsExceptDirectArcs;

        int[,] numNondominatedRPs;

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets
        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

        //Decision Variables
        INumVar[][] X; double[][] X_LB, X_UB;    //X_ij
        INumVar[][][] Y; double[][][] Y_LB, Y_UB;    //Y_ijp

        INumVar[] ArrivalSOE; double[] ArrivalSOE_LB, ArrivalSOE_UB; //Delta_j
        INumVar[] ArrivalTime; double[] ArrivalTime_LB, ArrivalTime_UB; //Tau_j
        INumVar[][] RefueledEnergyOnPath; double[][] RefueledEnergyOnPath_LB, RefueledEnergyOnPath_UB; //Epsilon_ij

        //Extracted decision variables. They are public for only testing purposes. Never ever use these or manipulate these outside of this class.
        //The output of this solver should never be the values of these decision variables but rather the vehicle specific routes.
        public double[] DeltaValues;
        public double[] TauValues;
        public double[,] EpsilonValues;
        public double[,] x_ij_Values;
        public double[,][] y_ijp_Values;

        public XCPlex_ETSP_VP() { }
        public XCPlex_ETSP_VP(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
            //Do not uncomment these. They are here to show which methods are being implemented in the base

            //Initialize()
            //SpecializedInitialize()
            //DefineDecisionVariables()
            //AddTheObjectiveFunction()
            //AddAllConstraints()
            //SetCplexParameters()
            //InitializeOutputVariables()
        }
        /// <summary>
        /// SpecializedInitialize method is responsible from pulling all necessary parameters from theProblemModel, creating the model specific parameters, nondominated refueling arcs, and everything else before the decision variables are defined.
        /// Based on the model parameter selection, rechargingDuration_status, useTighterBounds, numVehicles are set in the Initialize() method.
        /// </summary>
        protected override void SpecializedInitialize()
        {
            numNonESNodes = numCustomers + 1; //because there's only a single depot
            batteryCapacity = BatteryCapacity(VehicleCategories.EV);
            planningHorizonLength = theProblemModel.CRD.TMax;
            refuelingRate = theProblemModel.SRD.GetSiteByID(theProblemModel.SRD.GetESIDs().First()).RechargingRate;
            SetAllOriginalSWAVs(); //from base
            PopulateSubLists(); //from base
            CalculateMinimumAndMaximumArrivalTimeBounds();
            CalculateMinimumAndMaximumArrivalSOEBounds();
            PopulatePreprocessedSWAVs(); //from base. No ES should be copied to the preprocessedSites
            rpg = new RefuelingPathGenerator(theProblemModel);
            PopulateNondominatedRPsBetweenODPairs();
            CalculateMinNumVehicles();
        }

        // Minimum and Maximum Arrival SOE Limits
        void CalculateMinimumAndMaximumArrivalSOEBounds()
        {
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                double deltaMin = batteryCapacity;
                double deltaMax = 0.0;
                double deltaPrimeMax = 0.0;
                if (swav.SiteType == SiteTypes.Customer)
                {
                    if (deltaMin > EnergyConsumption(swav, TheDepot, VehicleCategories.EV))
                        deltaMin = EnergyConsumption(swav, TheDepot, VehicleCategories.EV);
                    if (deltaMax < batteryCapacity - EnergyConsumption(TheDepot, swav, VehicleCategories.EV))
                    {
                        deltaMax = batteryCapacity - EnergyConsumption(TheDepot, swav, VehicleCategories.EV);
                        deltaPrimeMax = deltaMax;
                    }
                    foreach (SiteWithAuxiliaryVariables ES in externalStations)
                    {
                        if (deltaMin > EnergyConsumption(swav, ES, VehicleCategories.EV))
                            deltaMin = EnergyConsumption(swav, ES, VehicleCategories.EV);
                        if (deltaMax < batteryCapacity - EnergyConsumption(ES, swav, VehicleCategories.EV))
                        {
                            deltaMax = batteryCapacity - EnergyConsumption(ES, swav, VehicleCategories.EV);
                            deltaPrimeMax = deltaMax;
                        }
                    }
                }
                else
                {
                    deltaMin = 0.0;
                    deltaMax = batteryCapacity;
                    deltaPrimeMax = batteryCapacity;
                }
                swav.UpdateArrivalSOEBounds(deltaMax, deltaMin, deltaPrimeMax);
            }
        }

        // Minimum and Maximum Arrival Time Limits
        void CalculateMinimumAndMaximumArrivalTimeBounds()
        {
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                double tauMin = TravelTime(TheDepot, swav);
                double tauMax = planningHorizonLength - TravelTime(swav, TheDepot) - ServiceDuration(swav);
                swav.UpdateArrivalTimeBounds(tauMax, tauMin);
            }
        }

        void PopulateNondominatedRPsBetweenODPairs()
        {
            allNondominatedRPs = new RefuelingPathList[numNonESNodes, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    allNondominatedRPs[i, j] = rpg.GenerateNonDominatedBetweenODPairIK(preprocessedSites[i], preprocessedSites[j], theProblemModel.SRD, theProblemModel.VRD);
                }

            nondominatedRPsExceptDirectArcs = new RefuelingPathList[numNonESNodes, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    nondominatedRPsExceptDirectArcs[i, j] = new RefuelingPathList();
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    {
                        if (allNondominatedRPs[i, j][r].RefuelingStops.Count != 0)
                        {
                            nondominatedRPsExceptDirectArcs[i, j].Add(allNondominatedRPs[i, j][r]);
                        }
                    }
                }
        }

        //Populate the general domain of the decision variables
        void PopulateUpperAndLowerBoundsOfDecisionVariables()
        {
            //Make sure you invoke this method when preprocessedSWAVs have been created!
            ArrivalSOE_LB = new double[numNonESNodes];
            ArrivalSOE_UB = new double[numNonESNodes];

            ArrivalTime_LB = new double[numNonESNodes];
            ArrivalTime_UB = new double[numNonESNodes];

            RefueledEnergyOnPath_LB = new double[numNonESNodes][];
            RefueledEnergyOnPath_UB = new double[numNonESNodes][];

            X_LB = new double[numNonESNodes][]; //numNonESNodes, numNonESNodes
            X_UB = new double[numNonESNodes][]; //numNonESNodes, numNonESNodes

            Y_LB = new double[numNonESNodes][][]; //numNonESNodes, numNonESNodes, numNonDominatedRPs
            Y_UB = new double[numNonESNodes][][]; //numNonESNodes, numNonESNodes, numNonDominatedRPs

            RHS_forNodeCoverage = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                ArrivalSOE_LB[i] = preprocessedSites[i].DeltaMin;
                ArrivalSOE_UB[i] = preprocessedSites[i].DeltaMax;

                ArrivalTime_LB[i] = preprocessedSites[i].TauMin;
                ArrivalTime_UB[i] = preprocessedSites[i].TauMax;

                RefueledEnergyOnPath_LB[i] = new double[numNonESNodes];
                RefueledEnergyOnPath_UB[i] = new double[numNonESNodes];

                X_LB[i] = new double[numNonESNodes];
                X_UB[i] = new double[numNonESNodes];

                Y_LB[i] = new double[numNonESNodes][];
                Y_UB[i] = new double[numNonESNodes][];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    X_LB[i][j] = 0.0;
                    X_UB[i][j] = 1.0;

                    int numRPS = nondominatedRPsExceptDirectArcs[i, j].Count;
                    Y_LB[i][j] = new double[numRPS];
                    Y_UB[i][j] = new double[numRPS];
                    double epsilonUB = 0.0;
                    double epsilonLB = double.MaxValue;

                    for (int r = 0; r < numRPS; r++)
                    {
                        Y_LB[i][j][r] = 0.0;
                        Y_UB[i][j][r] = 1.0;

                        if (epsilonUB < nondominatedRPsExceptDirectArcs[i, j][r].MaximumEnergyRefueledOnRoad)
                            epsilonUB = nondominatedRPsExceptDirectArcs[i, j][r].MaximumEnergyRefueledOnRoad;

                        if (epsilonLB > nondominatedRPsExceptDirectArcs[i, j][r].MinimumEnergyRefueledOnRoad)
                            epsilonLB = nondominatedRPsExceptDirectArcs[i, j][r].MinimumEnergyRefueledOnRoad;
                    }
                    if (epsilonLB > epsilonUB)
                        epsilonLB = epsilonUB;
                    RefueledEnergyOnPath_LB[i][j] = epsilonLB;
                    RefueledEnergyOnPath_UB[i][j] = epsilonUB;
                }
                RHS_forNodeCoverage[i] = 1.0;
            }
        }

        protected override void DefineDecisionVariables()
        {
            numNondominatedRPs = new int[numNonESNodes, numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    numNondominatedRPs[i, j] = nondominatedRPsExceptDirectArcs[i, j].Count;

            PopulateUpperAndLowerBoundsOfDecisionVariables();
            allVariables_list = new List<INumVar>();

            //dvs: X_ij
            AddTwoDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, out X);

            //dvs: Y_ijr
            AddThreeDimensionalDecisionVariable("Y", Y_LB, Y_UB, NumVarType.Int, numNonESNodes, numNonESNodes, numNondominatedRPs, out Y);
            //auxiliaries (T_j, Delta_j, Epsilon_ij)
            AddOneDimensionalDecisionVariable("ArrivalTime", ArrivalTime_LB, ArrivalTime_UB, NumVarType.Float, numNonESNodes, out ArrivalTime);
            AddOneDimensionalDecisionVariable("ArrivalSOE", ArrivalSOE_LB, ArrivalSOE_UB, NumVarType.Float, numNonESNodes, out ArrivalSOE);
            AddTwoDimensionalDecisionVariable("RefueledEnergy", RefueledEnergyOnPath_LB, RefueledEnergyOnPath_UB, NumVarType.Float, numNonESNodes, numNonESNodes, out RefueledEnergyOnPath);
            //All variables defined
            //allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            DecisionVariableFixing();
        }
        void DecisionVariableFixing()
        {
            ArrivalTime[0].LB = theProblemModel.CRD.TMax;
            ArrivalTime[0].UB = theProblemModel.CRD.TMax;

            ArrivalSOE[0].LB = 0.0;
            ArrivalSOE[0].UB = 0.0;
        }

        public override string GetDescription_AllVariables_Array()
        {
            throw new NotImplementedException();
        }
        protected override void AddTheObjectiveFunction()
        {
            if (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                AddMaxTypeObjectiveFunction();
            else
                AddMinTypeObjectiveFunction();
        }
        void AddMaxTypeObjectiveFunction()
        {
            throw new NotImplementedException();
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT) //For a mixed fleet vrp this is not a meaningful objective function
            {
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                            objFunction.AddTerm(Distance(preprocessedSites[i], preprocessedSites[j]), X[i][j]);
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                            objFunction.AddTerm(nondominatedRPsExceptDirectArcs[i, j][r].TotalDistance, Y[i][j][r]);
                    }
            }
            else
            {
                double refuelCostAtDepotPerKwh = theProblemModel.CRD.RefuelCostAtDepot;
                double refuelCostAtDepotPerMile = refuelCostAtDepotPerKwh * theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).ConsumptionRate;                
                double refuelCostInNetworkPerKwh = theProblemModel.CRD.RefuelCostInNetwork;
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        objFunction.AddTerm((refuelCostInNetworkPerKwh - refuelCostAtDepotPerKwh), RefueledEnergyOnPath[i][j]);
                        objFunction.AddTerm(refuelCostAtDepotPerMile * Distance(preprocessedSites[i], preprocessedSites[j]), X[i][j]);
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                            objFunction.AddTerm(refuelCostAtDepotPerMile * nondominatedRPsExceptDirectArcs[i, j][r].TotalDistance, Y[i][j][r]);
                    }
            }
            //Now adding the objective function to the model
            objective = AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();

            //Now adding the constraints one (family) at a time
            AddConstraint_NumberOfVisitsPerCustomerNodeIs1();
            AddConstraint_TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0();

            AddConstraint_ArrivingNumberOfEVsAtDepotMustBeOne();

            AddConstraint_ArrivalTimeRegulationFollowingACustomerAndRefuel();
            AddConstraint_ArrivalTimeRegulationFollowingACustomerDirectly();

            AddConstraint_ArrivalTimeRegulationFollowingTheDepotAndRefuel();
            AddConstraint_ArrivalTimeRegulationFollowingTheDepotDirectly();

            AddConstraint_ArrivalEnergyRegulationFollowingACustomerAndRefuel();
            AddConstraint_ArrivalEnergyRegulationFollowingACustomerDirectly();
            AddConstraint_ArrivalEnergyRegulationFollowingTheDepotAndRefuel();
            AddConstraint_ArrivalEnergyRegulationFollowingTheDepotDirectly();

            AddConstraint_MinimumArrivalEnergyAtACustomerBeforeRefuel();
            AddConstraint_MinimumArrivalEnergyAtACustomerBeforeDirectlyLeaving();

            AddConstraint_MinimumArrivalEnergyFollowingTheDepotAndRefuel();
            AddConstraint_MaximumArrivalEnergyAtANodeAfterRefuel();
            AddConstraint_MaximumArrivalEnergyAtANodeWhenArrivedDirectly();


            AddConstraint_TotalTravelTime();

            //All constraints and cuts added
            allConstraints_array = allConstraints_list.ToArray();
        }
        
        void AddConstraint_NumberOfVisitsPerCustomerNodeIs1()
        {
            for (int j = 1; j < numNonESNodes; j++)//Index 0 is the depot
            {
                ILinearNumExpr IncomingXandYToCustomerNodes = CreateCoreOf_Constraint_NumberOfVisitsPerCustomerNodeIs1(j);
                string constraint_name = "Exactly_1_vehicle_must_visit_the_customer_node_" + j.ToString();
                allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_NumberOfVisitsPerCustomerNodeIs1(int j)
        {
            ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
            {
                IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j]);
                for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    IncomingXandYToCustomerNodes.AddTerm(1.0, Y[i][j][r]);
            }
            return IncomingXandYToCustomerNodes;
        }

        void AddConstraint_TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0()
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0 = CreateCoreOf_Constraint_TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0(j);
                string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
                allConstraints_list.Add(AddEq(TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0, 0.0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0(int j)
        {
            ILinearNumExpr TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0 = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
            {
                TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0.AddTerm(1.0, X[i][j]);
                for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0.AddTerm(1.0, Y[i][j][r]);
            }
            for (int k = 0; k < numNonESNodes; k++)
            {
                TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0.AddTerm(-1.0, X[j][k]);
                for (int g = 0; g < nondominatedRPsExceptDirectArcs[j, k].Count; g++)
                    TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0.AddTerm(-1.0, Y[j][k][g]);
            }
            return TotalIncomingEVArcsMinusTotalOutgoingEVArcsIs0;
        }
        
        void AddConstraint_ArrivingNumberOfEVsAtDepotMustBeOne()
        {
            ILinearNumExpr NumberOfEVsIncomingToTheDepotIsLimited = CreateCoreOf_Constraint_ArrivingNumberOfEVsAtDepotMustBeOne();
            string constraint_name = "Number_of_EVs_incoming_to_node_0_must_be_equal_to_1";
            allConstraints_list.Add(AddEq(NumberOfEVsIncomingToTheDepotIsLimited, 1, constraint_name));
        }
        private ILinearNumExpr CreateCoreOf_Constraint_ArrivingNumberOfEVsAtDepotMustBeOne()
        {
            ILinearNumExpr NumberOfEVsIncomingToTheDepotIsLimited = LinearNumExpr();
            for (int i = 1; i < numNonESNodes; i++)
            {
                NumberOfEVsIncomingToTheDepotIsLimited.AddTerm(1.0, X[i][0]);
                for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, 0].Count; r++)
                    NumberOfEVsIncomingToTheDepotIsLimited.AddTerm(1.0, Y[i][0][r]);
            }
            return NumberOfEVsIncomingToTheDepotIsLimited;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingACustomerAndRefuel()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        double M = (preprocessedSites[i].TauMax - preprocessedSites[j].TauMin);
                        ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingACustomerAndRefuel(i, j, r, M);
                        string constraint_name = "Time_Regulation_Following_Customer_" + i.ToString() + "_to_node_" + j.ToString() + "_through_refueling_path_" + r.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, (-1.0 * M), constraint_name));
                    }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingACustomerAndRefuel(int i, int j, int r, double M)
        {
            double totalArcTravelTime = preprocessedSites[i].ServiceDuration + nondominatedRPsExceptDirectArcs[i, j][r].TotalTravelTime; //service time and travel time
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            TimeDifference.AddTerm(-1.0, ArrivalTime[i]);
            TimeDifference.AddTerm(-1.0 * totalArcTravelTime, Y[i][j][r]);
            TimeDifference.AddTerm(-1.0 / refuelingRate, RefueledEnergyOnPath[i][j]);
            TimeDifference.AddTerm(-1.0 * M, Y[i][j][r]);
            return TimeDifference;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingACustomerDirectly()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    double M = (preprocessedSites[i].TauMax - preprocessedSites[j].TauMin);
                    ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingACustomerDirectly(i, j, M);
                    string constraint_name = "Time_Regulation_Following_Customer_" + i.ToString() + "_to_node_" + j.ToString() + "_directly";
                    allConstraints_list.Add(AddGe(TimeDifference, (-1.0 * M), constraint_name));
                }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingACustomerDirectly(int i, int j, double M)
        {
            double totalArcTravelTime = preprocessedSites[i].ServiceDuration + TravelTime(preprocessedSites[i], preprocessedSites[j]); //service time and travel time
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            TimeDifference.AddTerm(-1.0, ArrivalTime[i]);
            TimeDifference.AddTerm(-1.0 * totalArcTravelTime, X[i][j]);
            TimeDifference.AddTerm(-1.0 * M, X[i][j]);
            return TimeDifference;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingTheDepotAndRefuel()
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingTheDepotAndRefuel(j);
                string constraint_name = "Time_Regulation_Following_TheDepot_to_node_" + j.ToString() + "_through_a_refueling_path";
                allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingTheDepotAndRefuel(int j)
        {
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            for (int r = 0; r < nondominatedRPsExceptDirectArcs[0, j].Count; r++)
            {
                double totalArcTravelTime = nondominatedRPsExceptDirectArcs[0, j][r].TotalTravelTime; //travel time only (from the depot)
                TimeDifference.AddTerm(-1.0 * totalArcTravelTime, Y[0][j][r]);
            }
            TimeDifference.AddTerm(-1.0 / refuelingRate, RefueledEnergyOnPath[0][j]);
            return TimeDifference;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingTheDepotDirectly()
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingTheDepotDirectly(j);
                string constraint_name = "Time_Regulation_Following_TheDepot_to_node_" + j.ToString() + "_directly";
                allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingTheDepotDirectly(int j)
        {
            double totalArcTravelTime = TravelTime(TheDepot, preprocessedSites[j]); //travel time only (from the depot)
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            TimeDifference.AddTerm(-1.0 * totalArcTravelTime, X[0][j]);
            return TimeDifference;
        }

        void AddConstraint_ArrivalEnergyRegulationFollowingACustomerAndRefuel()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    double M = preprocessedSites[j].DeltaMax - preprocessedSites[i].DeltaMin;
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        ILinearNumExpr ArrivalSOEDifference = CreateCoreOf_ArrivalEnergyRegulationFollowingACustomerAndRefuel(i, j, r, M);
                        string constraint_name = "SOE_Regulation_Following_Customer_" + i.ToString() + "_to_node_" + j.ToString() + "_through_refueling_path_" + r.ToString();
                        allConstraints_list.Add(AddLe(ArrivalSOEDifference, M, constraint_name));
                    }
                }
        }
        private ILinearNumExpr CreateCoreOf_ArrivalEnergyRegulationFollowingACustomerAndRefuel(int i, int j, int r, double M)
        {
            ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
            double energyConsumption = nondominatedRPsExceptDirectArcs[i, j][r].TotalEnergyConsumption;
            ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[j]);
            ArrivalSOELimit.AddTerm(-1.0, ArrivalSOE[i]);
            ArrivalSOELimit.AddTerm(energyConsumption, Y[i][j][r]);
            ArrivalSOELimit.AddTerm(-1.0, RefueledEnergyOnPath[i][j]);
            ArrivalSOELimit.AddTerm(M, Y[i][j][r]);
            return ArrivalSOELimit;
        }

        void AddConstraint_ArrivalEnergyRegulationFollowingACustomerDirectly()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    double M = preprocessedSites[j].DeltaMax - preprocessedSites[i].DeltaMin;
                    ILinearNumExpr ArrivalSOEDifference = CreateCoreOf_ArrivalEnergyRegulationFollowingACustomerDirectly(i, j, M);
                    string constraint_name = "SOE_Regulation_Following_Customer_" + i.ToString() + "_to_node_" + j.ToString() + "_directly";
                    allConstraints_list.Add(AddLe(ArrivalSOEDifference, M, constraint_name));
                }
        }
        private ILinearNumExpr CreateCoreOf_ArrivalEnergyRegulationFollowingACustomerDirectly(int i, int j, double M)
        {
            ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
            double energyConsumption = EnergyConsumption(preprocessedSites[i], preprocessedSites[j], VehicleCategories.EV);
            ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[j]);
            ArrivalSOELimit.AddTerm(-1.0, ArrivalSOE[i]);
            ArrivalSOELimit.AddTerm(energyConsumption, X[i][j]);
            ArrivalSOELimit.AddTerm(M, X[i][j]);
            return ArrivalSOELimit;
        }

        void AddConstraint_ArrivalEnergyRegulationFollowingTheDepotAndRefuel()
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr ArrivalSOEDifference = CreateCoreOf_Constraint_ArrivalEnergyRegulationFollowingTheDepotAndRefuel(j);
                string constraint_name = "SOE_Regulation_Following_TheDepot_to_node_" + j.ToString() + "_through_a_refueling_path";
                allConstraints_list.Add(AddLe(ArrivalSOEDifference, batteryCapacity, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_ArrivalEnergyRegulationFollowingTheDepotAndRefuel(int j)
        {
            ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
            ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[j]);
            for (int r = 0; r < nondominatedRPsExceptDirectArcs[0, j].Count; r++)
            {
                double energyConsumption = nondominatedRPsExceptDirectArcs[0, j][r].TotalEnergyConsumption;
                ArrivalSOELimit.AddTerm(energyConsumption, Y[0][j][r]);
            }
            ArrivalSOELimit.AddTerm(-1.0, RefueledEnergyOnPath[0][j]);
            return ArrivalSOELimit;
        }

        void AddConstraint_ArrivalEnergyRegulationFollowingTheDepotDirectly()
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr ArrivalSOEDifference = CreateCoreOf_ArrivalEnergyRegulationFollowingTheDepotDirectly(j);
                string constraint_name = "SOE_Regulation_Following_TheDepot_to_node_" + j.ToString() + "_directly";
                allConstraints_list.Add(AddLe(ArrivalSOEDifference, (BatteryCapacity(VehicleCategories.EV)), constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_ArrivalEnergyRegulationFollowingTheDepotDirectly(int j)
        {
            ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
            double energyConsumption = EnergyConsumption(TheDepot, preprocessedSites[j], VehicleCategories.EV);
            ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[j]);
            ArrivalSOELimit.AddTerm(energyConsumption, X[0][j]);
            return ArrivalSOELimit;
        }

        void AddConstraint_MinimumArrivalEnergyAtACustomerBeforeRefuel()
        {
            for (int i = 1; i < numNonESNodes; i++)
            {
                ILinearNumExpr MinimumArrivalSOEAtOrigin = CreateCoreOf_MinimumArrivalEnergyAtACustomerBeforeRefuel(i);
                string constraint_name = "Minimum_Arrival_SOE_At_Customer_" + i.ToString() + "_through_a_refueling_path";
                allConstraints_list.Add(AddGe(MinimumArrivalSOEAtOrigin, 0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_MinimumArrivalEnergyAtACustomerBeforeRefuel(int i)
        {
            ILinearNumExpr MinimumArrivalSOEAtOrigin = LinearNumExpr();
            MinimumArrivalSOEAtOrigin.AddTerm(1.0, ArrivalSOE[i]);
            for (int j = 0; j < numNonESNodes; j++)
                for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                {
                    double firstArcEnergyConsumption = nondominatedRPsExceptDirectArcs[i, j][r].FirstArcEnergyConsumption;
                    MinimumArrivalSOEAtOrigin.AddTerm(-1.0 * firstArcEnergyConsumption, Y[i][j][r]);
                }
            return MinimumArrivalSOEAtOrigin;
        }

        void AddConstraint_MinimumArrivalEnergyAtACustomerBeforeDirectlyLeaving()
        {
            for (int i = 1; i < numNonESNodes; i++)
            {
                ILinearNumExpr MinimumArrivalSOEAtOrigin = CreateCoreOf_MinimumArrivalEnergyAtACustomerBeforeDirectlyLeaving(i);
                string constraint_name = "Minimum_Arrival_SOE_At_Customer_" + i.ToString() + "_before_directly_leaving";
                allConstraints_list.Add(AddGe(MinimumArrivalSOEAtOrigin, 0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_MinimumArrivalEnergyAtACustomerBeforeDirectlyLeaving(int i)
        {
            ILinearNumExpr MinimumArrivalSOEAtOrigin = LinearNumExpr();
            MinimumArrivalSOEAtOrigin.AddTerm(1.0, ArrivalSOE[i]);
            for (int j = 0; j < numNonESNodes; j++)
            {
                double energyConsumption = EnergyConsumption(preprocessedSites[i], preprocessedSites[j], VehicleCategories.EV);
                MinimumArrivalSOEAtOrigin.AddTerm(-1.0 * energyConsumption, X[i][j]);
            }
            return MinimumArrivalSOEAtOrigin;
        }

        void AddConstraint_MinimumArrivalEnergyFollowingTheDepotAndRefuel()
        {
            ILinearNumExpr MinimumArrivalSOEAtOrigin = LinearNumExpr();
            for (int j = 1; j < numNonESNodes; j++)
                for (int r = 0; r < nondominatedRPsExceptDirectArcs[0, j].Count; r++)
                {
                    double firstArcEnergyConsumption = nondominatedRPsExceptDirectArcs[0, j][r].FirstArcEnergyConsumption;
                    MinimumArrivalSOEAtOrigin.AddTerm(-1.0 * firstArcEnergyConsumption, Y[0][j][r]);
                }
            string constraint_name = "Minimum_Arrival_SOE_At_A_Customer_following_theDepot_through_a_refueling_path";
            allConstraints_list.Add(AddLe(MinimumArrivalSOEAtOrigin, batteryCapacity, constraint_name));
        }

        void AddConstraint_MaximumArrivalEnergyAtANodeAfterRefuel()
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr MaximumArrivalSOEAtDestination = CreateCoreOf_MaximumArrivalEnergyAtANodeAfterRefuel(j);
                string constraint_name = "Minimum_Arrival_SOE_At_a_Node_" + j.ToString() + "_following_a_refueling_path";
                allConstraints_list.Add(AddLe(MaximumArrivalSOEAtDestination, batteryCapacity, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_MaximumArrivalEnergyAtANodeAfterRefuel(int j)
        {
            ILinearNumExpr MaximumArrivalSOEAtDestination = LinearNumExpr();
            MaximumArrivalSOEAtDestination.AddTerm(1.0, ArrivalSOE[j]);
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                {
                    double lastArcEnergyConsumption = nondominatedRPsExceptDirectArcs[i, j][r].LastArcEnergyConsumption;
                    MaximumArrivalSOEAtDestination.AddTerm(lastArcEnergyConsumption, Y[i][j][r]);
                }
            return MaximumArrivalSOEAtDestination;
        }

        void AddConstraint_MaximumArrivalEnergyAtANodeWhenArrivedDirectly()
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr MaximumArrivalSOEAtDestination = CreateCoreOf_MaximumArrivalEnergyAtANodeWhenArrivedDirectly(j);
                string constraint_name = "Minimum_Arrival_SOE_At_a_Node_" + j.ToString() + "_following_a_refueling_path";
                allConstraints_list.Add(AddLe(MaximumArrivalSOEAtDestination, batteryCapacity, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_MaximumArrivalEnergyAtANodeWhenArrivedDirectly(int j)
        {
            ILinearNumExpr MaximumArrivalSOEAtDestination = LinearNumExpr();
            MaximumArrivalSOEAtDestination.AddTerm(1.0, ArrivalSOE[j]);
            for (int i = 1; i < numNonESNodes; i++)
            {
                double energyConsumption = EnergyConsumption(preprocessedSites[i], preprocessedSites[j], VehicleCategories.EV);
                MaximumArrivalSOEAtDestination.AddTerm(energyConsumption, X[i][j]);
            }
            return MaximumArrivalSOEAtDestination;
        }

        void AddConstraint_TotalTravelTime()
        {
            totalTravelTimeConstraintIndex = allConstraints_list.Count;

            if (RHS_forNodeCoverage != null)
            {
                ILinearNumExpr TotalTravelTime = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site sTo = preprocessedSites[j];
                        TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j]);
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                        {
                            TotalTravelTime.AddTerm(nondominatedRPsExceptDirectArcs[i, j][r].TotalTravelTime, Y[i][j][r]);
                        }
                    }
                string constraint_name = "Total_Travel_Time";
                double totalServiceTime = 0;
                for (int i = 0; i < numNonESNodes; i++)
                    totalServiceTime += RHS_forNodeCoverage[i] * preprocessedSites[i].ServiceDuration;
                double rhs = theProblemModel.CRD.TMax - totalServiceTime;
                allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));
            }
        }

        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {

            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();

            outcome.AddRange(GetVehicleSpecificRoutes(VehicleCategories.EV));

            return outcome;
        }
        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory = VehicleCategories.EV)
        {
            GetDecisionVariableValues();
            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            List<List<string>> listOfNonDepotSiteIDs;
            allRoutesESVisits = new List<IndividualRouteESVisits>();
            listOfNonDepotSiteIDs = GetListOfNonDepotSiteIDsEV();           
            int count = 0;
            foreach (List<string> NDSIDs in listOfNonDepotSiteIDs)
            {
                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, NDSIDs, allRoutesESVisits[count]));
                count++;
            }
            return outcome;
        }

        public void GetDecisionVariableValues()
        {
            int[,] length3 = new int[numNonESNodes, numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    length3[i, j] = nondominatedRPsExceptDirectArcs[i, j].Count;

            x_ij_Values = new double[numNonESNodes, numNonESNodes];
            y_ijp_Values = new double[numNonESNodes, numNonESNodes][];
            DeltaValues = new double[numNonESNodes];
            TauValues = new double[numNonESNodes];
            EpsilonValues = new double[numNonESNodes, numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                DeltaValues[i] = GetValue(ArrivalSOE[i]);
                TauValues[i] = GetValue(ArrivalTime[i]);

                for (int j = 0; j < numNonESNodes; j++)
                {
                    y_ijp_Values[i, j] = new double[numNondominatedRPs[i, j]];
                    x_ij_Values[i, j] = GetValue(X[i][j]);
                    for (int r = 0; r < numNondominatedRPs[i, j]; r++)
                        y_ijp_Values[i, j][r] = GetValue(Y[i][j][r]);
                }
            }
            //WriteDecisionVariables();

        }
        void WriteDecisionVariables()
        {
            var pathX = @"C:\Users\tha810\Documents\Xvariables.txt";
            var pathY = @"C:\Users\tha810\Documents\Yvariables.txt";
            var pathD = @"C:\Users\tha810\Documents\Dvariables.txt";
            var pathT = @"C:\Users\tha810\Documents\Tvariables.txt";
            var sw = new System.IO.StreamWriter(pathX);

            for (int i = 0; i < numNonESNodes; i++)
            {
                for (int j = 0; j < numNonESNodes; j++)
                {
                    sw.Write(preprocessedSites[i].ID + "\t" + preprocessedSites[j].ID + "\t");
                    sw.WriteLine(x_ij_Values[i, j].ToString());
                }
            }

            sw.Close();
            sw = new System.IO.StreamWriter(pathY);

            for (int i = 0; i < numNonESNodes; i++)
            {
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int p = 0; p < numNondominatedRPs[i, j]; p++)
                    {
                        sw.Write(preprocessedSites[i].ID + "\t" + preprocessedSites[j].ID + "\t");
                        for (int r = 0; r < 5; r++)
                        {
                            if (r < nondominatedRPsExceptDirectArcs[i, j][p].RefuelingStops.Count)
                                sw.Write(nondominatedRPsExceptDirectArcs[i, j][p].RefuelingStops[r].ID + "\t");
                            else
                                sw.Write("\t");
                        }
                        sw.WriteLine(y_ijp_Values[i, j][p].ToString());
                    }
                }
            }
            sw.Close();
            sw = new System.IO.StreamWriter(pathD);
            for (int i = 0; i < numNonESNodes; i++)
            {
                sw.WriteLine(preprocessedSites[i].ID + "\t" + DeltaValues[i].ToString());
            }
            sw.Close();
            sw = new System.IO.StreamWriter(pathT);
            for (int i = 0; i < numNonESNodes; i++)
            {
                sw.WriteLine(preprocessedSites[i].ID + "\t" + TauValues[i].ToString());
            }
            sw.Close();
            Console.WriteLine("data written to file");
        }
        List<List<string>> GetListOfNonDepotSiteIDsEV()
        {
            List<List<string>> outcome = new List<List<string>>();
            List<string> singleRoute;
            IndividualRouteESVisits routesESVisits;
            List<List<string>> allActiveEVarcs = new List<List<string>>();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                        if (y_ijp_Values[i, j][r] >= 0.5)
                        {
                            List<string> activeArc = new List<string>();
                            activeArc.Add(preprocessedSites[i].ID);
                            for (int p = 0; p < nondominatedRPsExceptDirectArcs[i, j][r].RefuelingStops.Count; p++)
                            {
                                activeArc.Add(nondominatedRPsExceptDirectArcs[i, j][r].RefuelingStops[p].ID);
                            }
                            activeArc.Add(preprocessedSites[j].ID);
                            allActiveEVarcs.Add(activeArc);
                        }
                    if (x_ij_Values[i, j] >= 0.5)
                    {
                        List<string> activeArc = new List<string>();
                        activeArc.Add(preprocessedSites[i].ID);
                        activeArc.Add(preprocessedSites[j].ID);
                        allActiveEVarcs.Add(activeArc);
                    }
                }

            List<List<string>> tempActiveArcs = new List<List<string>>(allActiveEVarcs);
            string lastID = theProblemModel.SRD.GetSingleDepotID();
            while (tempActiveArcs.Count > 0)
            {
                singleRoute = new List<string>();
                routesESVisits = new IndividualRouteESVisits();
                do
                {
                    for (int i = 0; i < tempActiveArcs.Count; i++)
                    {
                        if (tempActiveArcs[i][0] == lastID)
                        {
                            if (tempActiveArcs[i].Count > 2) //there is at least one ES
                            {
                                routesESVisits.AddRange(GetIndividualESVisit(tempActiveArcs[i]));
                            }
                            tempActiveArcs[i].RemoveAt(0);
                            singleRoute.AddRange(tempActiveArcs[i]);
                            lastID = singleRoute.Last();
                            tempActiveArcs.RemoveAt(i);
                            break;
                        }
                    }
                } while (lastID != theProblemModel.SRD.GetSingleDepotID());
                singleRoute.RemoveAt(singleRoute.Count - 1);
                outcome.Add(singleRoute);
                allRoutesESVisits.Add(routesESVisits);
                lastID = theProblemModel.SRD.GetSingleDepotID();
            }
            return outcome;
        }
        List<IndividualESVisitDataPackage> GetIndividualESVisit(List<string> activeArc)
        {
            List<string> tempArc = new List<string>(activeArc);
            List<IndividualESVisitDataPackage> outcome = new List<IndividualESVisitDataPackage>();
            string from = tempArc.First();
            string to = tempArc.Last();
            tempArc.RemoveAt(0);
            tempArc.RemoveAt(tempArc.Count - 1);
            double timeSpentEnRoute = 0;
            for (int i = 0; i < numNonESNodes; i++)
                if (preprocessedSites[i].ID == from)
                {
                    for (int j = 0; j < numNonESNodes; j++)
                        if (preprocessedSites[j].ID == to)
                        {
                            timeSpentEnRoute = GetValue(ArrivalTime[j]) - ServiceDuration(theProblemModel.SRD.GetSiteByID(from)) - GetValue(ArrivalTime[i]);
                            double travelTime = TravelTime(theProblemModel.SRD.GetSiteByID(from), theProblemModel.SRD.GetSiteByID(tempArc.First())) + TravelTime(theProblemModel.SRD.GetSiteByID(tempArc.Last()), theProblemModel.SRD.GetSiteByID(to));
                            while (tempArc.Count > 1)
                            {
                                travelTime += TravelTime(theProblemModel.SRD.GetSiteByID(tempArc[0]), theProblemModel.SRD.GetSiteByID(tempArc[1]));
                                tempArc.RemoveAt(0);
                            }
                            timeSpentEnRoute -= travelTime;
                            break;
                        }
                    break;
                }
            for (int k = 1; k < activeArc.Count - 1; k++)
                outcome.Add(new IndividualESVisitDataPackage(activeArc[k], (timeSpentEnRoute / (activeArc.Count - 2)) / RechargingRate(theProblemModel.SRD.GetSiteByID(activeArc[k]))));
            return outcome;
        }

        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }
        public override string GetModelName()
        {
            return "ETSP VP with RPs";
        }       
        public override void RefineDecisionVariables(CustomerSet cS)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                        {
                            Y[i][j][r].UB = 1.0;
                        }
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[j, i].Count; r++)
                        {
                            Y[j][i][r].UB = 1.0;
                        }
                        X[i][j].UB = 1.0;
                        X[j][i].UB = 1.0;
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                        {
                            Y[i][j][r].UB = 0.0;
                        }
                        for (int r = 0; r < nondominatedRPsExceptDirectArcs[j, i].Count; r++)
                        {
                            Y[j][i][r].UB = 0.0;
                        }
                        X[i][j].UB = 0.0;
                        X[j][i].UB = 0.0;
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();
            RefineTotalTravelTimeConstraints();
        }
        void RefineRightHandSidesOfCustomerVisitationConstraints()
        {
            int c = 0;

            for (int j = 1; j < numNonESNodes; j++)
            {
                if (RHS_forNodeCoverage[j] == 1)
                {
                    allConstraints_array[c].UB = RHS_forNodeCoverage[j];
                    allConstraints_array[c].LB = RHS_forNodeCoverage[j];
                }
                else//RHS_forNodeCoverage[j] == 0
                {
                    allConstraints_array[c].LB = RHS_forNodeCoverage[j];
                    allConstraints_array[c].UB = RHS_forNodeCoverage[j];
                }
                c++;
            }

        }
        void RefineTotalTravelTimeConstraints()
        {
            double totalServiceTime = 0.0;
            for (int j = 1; j < numNonESNodes; j++)
                if (RHS_forNodeCoverage[j] == 1)
                    totalServiceTime += preprocessedSites[j].ServiceDuration;
            allConstraints_array[totalTravelTimeConstraintIndex].UB = theProblemModel.CRD.TMax - totalServiceTime;
        }
        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            throw new NotImplementedException();
        }
        void AddKnownSolutionForTesting_Route1()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        Y[i][j][r].UB = 0.0;
                        Y[i][j][r].LB = 0.0;
                    }

                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;

                }
            //First tour
            X[0][2].UB = 1.0;
            X[0][2].LB = 1.0;

            X[2][16].UB = 1.0;
            X[2][16].LB = 1.0;

            X[16][0].UB = 1.0;
            X[16][0].LB = 1.0;
        }
        void AddKnownSolutionForTesting_Route2()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        Y[i][j][r].UB = 0.0;
                        Y[i][j][r].LB = 0.0;
                    }

                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;

                }

            //Second tour
            X[0][3].UB = 1.0;
            X[0][3].LB = 1.0;

            Y[3][9][0].UB = 1.0;
            Y[3][9][0].LB = 1.0;

            X[9][0].UB = 1.0;
            X[9][0].LB = 1.0;
        }
        void AddKnownSolutionForTesting_Route3()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        Y[i][j][r].UB = 0.0;
                        Y[i][j][r].LB = 0.0;
                    }

                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;

                }

            //Third tour
            X[0][4].UB = 1.0;
            X[0][4].LB = 1.0;

            X[4][8].UB = 1.0;
            X[4][8].LB = 1.0;

            X[8][18].UB = 1.0;
            X[8][18].LB = 1.0;

            X[18][13].UB = 1.0;
            X[18][13].LB = 1.0;

            X[13][12].UB = 1.0;
            X[13][12].LB = 1.0;

            X[12][0].UB = 1.0;
            X[12][0].LB = 1.0;
        }
        void AddKnownSolutionForTesting_Route4()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        Y[i][j][r].UB = 0.0;
                        Y[i][j][r].LB = 0.0;
                    }

                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;

                }
            //Fourth tour 
            X[0][7].UB = 1.0;
            X[0][7].LB = 1.0;

            X[7][6].UB = 1.0;
            X[7][6].LB = 1.0;

            X[6][10].UB = 1.0;
            X[6][10].LB = 1.0;

            X[10][0].UB = 1.0;
            X[10][0].LB = 1.0;
        }
        void AddKnownSolutionForTesting_Route5()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        Y[i][j][r].UB = 0.0;
                        Y[i][j][r].LB = 0.0;
                    }

                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;

                }
            //Fifth tour 
            X[0][11].UB = 1.0;
            X[0][11].LB = 1.0;

            X[11][20].UB = 1.0;
            X[11][20].LB = 1.0;

            X[20][14].UB = 1.0;
            X[20][14].LB = 1.0;

            X[14][1].UB = 1.0;
            X[14][1].LB = 1.0;

            Y[1][0][1].UB = 1.0;
            Y[1][0][1].LB = 1.0;
        }
        void AddKnownSolutionForTesting_Route6()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    for (int r = 0; r < nondominatedRPsExceptDirectArcs[i, j].Count; r++)
                    {
                        Y[i][j][r].UB = 0.0;
                        Y[i][j][r].LB = 0.0;
                    }

                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;

                }
            //Sixth tour 
            X[0][2].UB = 1.0;
            X[0][2].LB = 1.0;

            X[2][4].UB = 1.0;
            X[2][4].LB = 1.0;

            Y[4][3][0].UB = 1.0;
            Y[4][3][0].LB = 1.0;

            X[3][1].UB = 1.0;
            X[3][1].LB = 1.0;

            X[1][0].UB = 1.0;
            X[1][0].LB = 1.0;
        }

    }
}

