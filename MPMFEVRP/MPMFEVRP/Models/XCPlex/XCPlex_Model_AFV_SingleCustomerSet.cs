using ILOG.Concert;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_Model_AFV_SingleCustomerSet : XCPlexVRPBase
    {
        int numNonESNodes;
        int numCustomers;

        RefuelingPathGenerator rpg;
        RefuelingPathList[,] allNondominatedRPs;

        double evPerMileCost = 0.5;
        int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
        int totalTravelTimeConstraintIndex = -1;
        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV
        int objUpperBoundConstraintIndex = -1;
        int objLowerBoundConstraintIndex = -1;
        double objRHS = double.MaxValue;
        double objRHS_LB = 0.0;

        bool addTotalNumberOfActiveArcsCut = false;

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        INumVar[][][] X; double[][][] X_LB, X_UB;
        INumVar[] ArrivalSOE;
        INumVar[] ArrivalTime;

        IndividualRouteESVisits singleRouteESvisits;

        //The following are extracted decision variables. They are public for only testing purposes. Never use these or manipulate these outside of this class.
        //The output of this solver should never be the values of these decision variables but rather the vehicle specific routes.
        public double[] DeltaValues;
        public double[] TValues;
        double[,][] xValues;
        public XCPlex_Model_AFV_SingleCustomerSet() { }

        public XCPlex_Model_AFV_SingleCustomerSet(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
            //Do not uncomment these. They are here to show which methods are being implemented in the base

            //Initialize()
            //DefineDecisionVariables()
            //AddTheObjectiveFunction()
            //AddAllConstraints()
            //SetCplexParameters()
            //InitializeOutputVariables()
        }

        protected override void DefineDecisionVariables()
        {
            numCustomers = theProblemModel.SRD.NumCustomers;
            numNonESNodes = numCustomers + 1;
            SpecialPreprocessSites();

            int[,] length3 = new int[numNonESNodes, numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    length3[i, j] = allNondominatedRPs[i, j].Count;

            SetMinAndMaxValuesOfModelSpecificVariables();
            allVariables_list = new List<INumVar>();

            //dvs: X_ijv
            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, length3, out X);

            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("ArrivalTime", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out ArrivalTime);
            AddOneDimensionalDecisionVariable("ArrivalSOE", minValue_Delta, maxValue_Delta, NumVarType.Float, numNonESNodes, out ArrivalSOE);

            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXYVariablesTo0();
        }
        protected void SpecialPreprocessSites()
        {
            preprocessedSites = theProblemModel.SRD.GetAllNonESSWAVsList().ToArray();
            rpg = new RefuelingPathGenerator(theProblemModel);
            SetNondominatedRPsBetweenODPairs();
            SetFirstAndLastNodeIndices();
        }
        void SetNondominatedRPsBetweenODPairs()
        {
            allNondominatedRPs = new RefuelingPathList[numNonESNodes, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    allNondominatedRPs[i, j] = rpg.GenerateNonDominatedBetweenODPairIK(preprocessedSites[i], preprocessedSites[j], theProblemModel.SRD);
                }
        }
        void SetUndesiredXYVariablesTo0()
        {
            ArrivalTime[0].LB = theProblemModel.CRD.TMax;
            ArrivalTime[0].UB = theProblemModel.CRD.TMax;

            ArrivalSOE[0].LB = 0.0;
            ArrivalSOE[0].UB = 0.0;
        }
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {
            //Make sure you invoke this method when preprocessedSWAVs have been created!
            minValue_Delta = new double[numNonESNodes];
            maxValue_Delta = new double[numNonESNodes];
            minValue_T = new double[numNonESNodes];
            maxValue_T = new double[numNonESNodes];

            X_LB = new double[numNonESNodes][][];
            X_UB = new double[numNonESNodes][][];

            RHS_forNodeCoverage = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                minValue_Delta[i] = preprocessedSites[i].DeltaMin;
                maxValue_Delta[i] = preprocessedSites[i].DeltaMax;
                minValue_T[i] = preprocessedSites[i].TES;
                maxValue_T[i] = preprocessedSites[i].TLS;

                X_LB[i] = new double[numNonESNodes][];
                X_UB[i] = new double[numNonESNodes][];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    int numRPS = allNondominatedRPs[i, j].Count;
                    X_LB[i][j] = new double[numRPS];
                    X_UB[i][j] = new double[numRPS];
                    for (int r = 0; r < numRPS; r++)
                    {
                        X_LB[i][j][r] = 0.0;
                        X_UB[i][j][r] = 1.0;
                    }
                }
                RHS_forNodeCoverage[i] = 1.0;
            }
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
            throw new NotImplementedException("This model works with only vmt minimization objective for now.");
            //ILinearNumExpr objFunction = LinearNumExpr();           
            ////First term: prize collection
            //    for (int i = 0; i < numNonESNodes; i++)
            //        for (int j = 0; j < numNonESNodes; j++)
            //            for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
            //                objFunction.AddTerm(allNondominatedRPs[i, j][r].Destination.Prize[vIndex_EV], X[i][j][r]);

            ////Second term: distance-based costs from customer to customer through an ES
            //for (int i = 0; i < numNonESNodes; i++)
            //    for (int j = 0; j < numNonESNodes; j++)
            //        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
            //            objFunction.AddTerm(-1.0 *allNondominatedRPs[i, j][r].TotalDistance* GetVarCostPerMile(vehicleCategories[vIndex_EV]), X[i][j][r]);
            
            ////Now adding the objective function to the model
            //objective = AddMaximize(objFunction);
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT)
            {
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                            objFunction.AddTerm(allNondominatedRPs[i, j][r].TotalDistance, X[i][j][r]);
            }
            else
            {
                throw new NotImplementedException();
                //The old code was repetitive of the distance-based cost calculation given above and it was only capable of multiplying the VMt by the vehicle's $/mile coeff. We can and shoulddo much better than that, for example combining distance- and time-based costs together. Will solve when it's needed :)
            }
                //Now adding the objective function to the model
                objective = AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time

            AddConstraint_NumberOfVisitsPerNode();//1
            AddConstraint_IncomingXTotalEqualsOutgoingXTotal();//2
            AddConstraint_DepartingNumberOfVehicles();//3
            AddConstraint_ArrivalTimeLimitsAtOrigin();//4
            AddConstraint_ArrivalTimeLimitsAtDestination();//5
            AddConstraint_ArrivalTimeRegulationFollowingTheDepot();//7
            AddConstraint_TimeRegulationFollowingACustomerVisit();//6
            AddConstraint_TotalTravelTime();//8
            AddConstraint_ArrivalEnergyRegulationFollowingTheDepot();//10
            AddConstraint_ArrivalEnergyRegulationFollowingACustomer();//13
            AddConstraint_RegulateArrivalSOEAtOrigin();//9
            AddConstraint_RegulateArrivalSOEAtDestinationThroughRP();//11
            AddConstraint_RegulateArrivalSOEAtDestinationDirect();//12

            AddConstraint_MinimizeVMTObjectiveUB();
            AddConstraint_MinimizeVMTObjectiveLB();

            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }

        void AddConstraint_NumberOfVisitsPerNode() //1
        {
            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;
            for (int j = 1; j < numNonESNodes; j++)//Index 0 is the depot
            {
                ILinearNumExpr IncomingXToCustomerNodes = CreateCoreOf_Constraint_NumberOfVisitsPerCustomerNode(j);

                string constraint_name = "Exactly_"+ RHS_forNodeCoverage[j].ToString()+ "_vehicle_must_visit_the_customer_node_" + j.ToString();
                allConstraints_list.Add(AddEq(IncomingXToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
            }
        }
        public ILinearNumExpr CreateCoreOf_Constraint_NumberOfVisitsPerCustomerNode(int j)
        {
            ILinearNumExpr IncomingXToCustomerNodes = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    IncomingXToCustomerNodes.AddTerm(1.0, X[i][j][r]);
            return IncomingXToCustomerNodes;
        }

        void AddConstraint_IncomingXTotalEqualsOutgoingXTotal()//2
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr IncomingXTotalMinusOutgoingXTotal = CreateCoreOf_Constraint_IncomingXTotalEqualsOutgoingXTotal(j);

                string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
                allConstraints_list.Add(AddEq(IncomingXTotalMinusOutgoingXTotal, 0.0, constraint_name));
            }

        }
        public ILinearNumExpr CreateCoreOf_Constraint_IncomingXTotalEqualsOutgoingXTotal(int j)
        {
            ILinearNumExpr IncomingXTotalMinusOutgoingXTotal = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    IncomingXTotalMinusOutgoingXTotal.AddTerm(1.0, X[i][j][r]);

            for (int k = 0; k < numNonESNodes; k++)
                for (int r = 0; r < allNondominatedRPs[j, k].Count; r++)
                    IncomingXTotalMinusOutgoingXTotal.AddTerm(-1.0, X[j][k][r]);
            return IncomingXTotalMinusOutgoingXTotal;
        }

        void AddConstraint_DepartingNumberOfVehicles()//3
        {
            ILinearNumExpr NumberOfAFVsOutgoingFromTheDepot = CreateCoreOf_Constraint_DepartingNumberOfVehicles();

            string constraint_name = "Number_of_AFVs_outgoing_from_node_0_cannot_exceed_1";
            allConstraints_list.Add(AddEq(NumberOfAFVsOutgoingFromTheDepot, 1.0, constraint_name));
        }
        public ILinearNumExpr CreateCoreOf_Constraint_DepartingNumberOfVehicles()
        {
            ILinearNumExpr NumberOfAFVsOutgoingFromTheDepot = LinearNumExpr();
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i,0].Count; r++)
                    NumberOfAFVsOutgoingFromTheDepot.AddTerm(1.0, X[i][0][r]);
            return NumberOfAFVsOutgoingFromTheDepot;
        }

        void AddConstraint_ArrivalTimeLimitsAtOrigin()//4
        {
            ILinearNumExpr TimeDifference;
            for (int i = 1; i < numNonESNodes; i++)
            {
                TimeDifference = CreateCoreOf_Constraint_ArrivalTimeLimits(i);
                string constraint_name = "Arrival_Time_Limit_at_node_" + i.ToString();
                allConstraints_list.Add(AddLe(TimeDifference, theProblemModel.SRD.GetSingleDepotSite().DueDate, constraint_name));
            }
        }
        public ILinearNumExpr CreateCoreOf_Constraint_ArrivalTimeLimits(int i)
        {
            double singleRefDuration = BatteryCapacity(VehicleCategories.EV) / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[i]);
            for (int j = 0; j < numNonESNodes; j++)
                for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                {
                    double refuelingDuration = allNondominatedRPs[i, j][r].RefuelingStops.Count * singleRefDuration;
                    double serviceDuration = preprocessedSites[j].ServiceDuration;
                    double totalTime = refuelingDuration + serviceDuration + allNondominatedRPs[i, j][r].TotalTravelTime;
                    TimeDifference.AddTerm(totalTime, X[i][j][r]);
                }
            return TimeDifference;
        }

        void AddConstraint_ArrivalTimeLimitsAtDestination()//5
        {
            ILinearNumExpr TimeDifference;
            for (int j = 0; j < numNonESNodes; j++)
            {
                TimeDifference = CreateCoreOf_Constraint_ArrivalTimeLimitsAtDestination(j);
                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
            }
        }
        public ILinearNumExpr CreateCoreOf_Constraint_ArrivalTimeLimitsAtDestination(int j)
        {
            double singleRefDuration = BatteryCapacity(VehicleCategories.EV) / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i, j].Count; r++) 
                {
                    double refuelingDuration = allNondominatedRPs[i, j][r].RefuelingStops.Count * singleRefDuration;
                    double serviceDuration = preprocessedSites[j].ServiceDuration;
                    double totalTime = refuelingDuration + serviceDuration + allNondominatedRPs[i, j][r].TotalTravelTime;
                    TimeDifference.AddTerm(-1.0 * (preprocessedSites[i].TES + totalTime), X[i][j][r]);
                }
            return TimeDifference;
        }

        void AddConstraint_TimeRegulationFollowingACustomerVisit()//6
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    {
                        ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingACustomerVisit(i, j, r);
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, (preprocessedSites[j].TES - preprocessedSites[i].TLS), constraint_name));
                    }
        }
        public ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingACustomerVisit(int i, int j, int r)
        {
            double singleRefDuration = BatteryCapacity(VehicleCategories.EV) / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
            double refuelingDuration = allNondominatedRPs[i, j][r].RefuelingStops.Count * singleRefDuration;
            double serviceDuration = preprocessedSites[i].ServiceDuration;
            double totalArcTime = refuelingDuration + serviceDuration + allNondominatedRPs[i, j][r].TotalTravelTime;

            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            TimeDifference.AddTerm(-1.0, ArrivalTime[i]);
            TimeDifference.AddTerm(-1.0 * (totalArcTime - (preprocessedSites[j].TES - preprocessedSites[i].TLS)), X[i][j][r]);
            return TimeDifference;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingTheDepot()//7
        {
            for (int j = 1; j < numNonESNodes; j++)
                for (int r = 0; r < allNondominatedRPs[0, j].Count; r++)
                {
                    double singleRefDuration = BatteryCapacity(VehicleCategories.EV) / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
                    double refuelingDuration = allNondominatedRPs[0, j][r].RefuelingStops.Count * singleRefDuration;
                    double totalArcTime = refuelingDuration + allNondominatedRPs[0, j][r].TotalTravelTime;

                    ILinearNumExpr TimeFlow = LinearNumExpr();
                    TimeFlow.AddTerm(1.0, ArrivalTime[j]);
                    TimeFlow.AddTerm(-1.0 * (totalArcTime - preprocessedSites[j].TES), X[0][j][r]);
                    string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeFlow, preprocessedSites[j].TES, constraint_name));
                }
        }

        void AddConstraint_TotalTravelTime()//8
        {
            double singleRefDuration = BatteryCapacity(VehicleCategories.EV) / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
            totalTravelTimeConstraintIndex = allConstraints_list.Count;
            ILinearNumExpr TotalTravelTime = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    {
                        double refuelingDuration = allNondominatedRPs[i, j][r].RefuelingStops.Count * singleRefDuration;
                        double serviceDuration = preprocessedSites[j].ServiceDuration;
                        double totalTime = refuelingDuration + serviceDuration+ allNondominatedRPs[i, j][r].TotalTravelTime;
                        TotalTravelTime.AddTerm(totalTime, X[i][j][r]);
                    }

            string constraint_name = "Total_Travel_Time";
            double rhs = theProblemModel.CRD.TMax;
            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));
        }

        void AddConstraint_RegulateArrivalSOEAtOrigin()//9
        {
            for (int i = 1; i < numNonESNodes; i++)
            {
                ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
                ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[i]);

                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        ArrivalSOELimit.AddTerm(-1.0 * allNondominatedRPs[i, j][r].FirstArcEnergyConsumption, X[i][j][r]);

                string constraint_name = "Arrival_Time_Limit_at_node_" + i.ToString();
                allConstraints_list.Add(AddGe(ArrivalSOELimit, 0.0, constraint_name));
            }
        }

        void AddConstraint_ArrivalEnergyRegulationFollowingTheDepot()//10
        {
            for (int j = 1; j < numNonESNodes; j++)
                if (allNondominatedRPs[0, j].Count == 1)
                    if (allNondominatedRPs[0, j][0].RefuelingStops.Count==0)
                    {
                        ILinearNumExpr EnergyFlow = LinearNumExpr();
                        EnergyFlow.AddTerm(1.0, ArrivalSOE[j]);
                        EnergyFlow.AddTerm(allNondominatedRPs[0, j][0].TotalEnergyConsumption, X[0][j][0]);
                        string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(EnergyFlow, BatteryCapacity(VehicleCategories.EV), constraint_name));
                    }
        }
        void AddConstraint_RegulateArrivalSOEAtDestinationThroughRP()//11
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
                ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[j]);
                for (int i = 0; i < numNonESNodes; i++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        if (allNondominatedRPs[i, j][r].RefuelingStops.Count > 0)
                            ArrivalSOELimit.AddTerm(allNondominatedRPs[i, j][r].LastArcEnergyConsumption, X[i][j][r]);

                string constraint_name = "Arrival_Energy_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddLe(ArrivalSOELimit, BatteryCapacity(VehicleCategories.EV), constraint_name));

            }
        }

        void AddConstraint_RegulateArrivalSOEAtDestinationDirect()//12
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr ArrivalSOELimit = LinearNumExpr();
                ArrivalSOELimit.AddTerm(1.0, ArrivalSOE[j]);

                for (int i = 0; i < numNonESNodes; i++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        if (allNondominatedRPs[i, j][r].RefuelingStops.Count == 0)
                            ArrivalSOELimit.AddTerm((allNondominatedRPs[i, j][r].TotalEnergyConsumption + BatteryCapacity(VehicleCategories.EV) - preprocessedSites[i].DeltaMax), X[i][j][r]);

                string constraint_name = "Arrival_Energy_Limit_through_direct_arc_at_node_" + j.ToString();
                allConstraints_list.Add(AddLe(ArrivalSOELimit, BatteryCapacity(VehicleCategories.EV), constraint_name));

            }
        }
        
        void AddConstraint_ArrivalEnergyRegulationFollowingACustomer()//13
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        if (allNondominatedRPs[i, j][r].RefuelingStops.Count == 0)
                        {
                            ILinearNumExpr EnergyFlow = LinearNumExpr();
                            EnergyFlow.AddTerm(1.0, ArrivalSOE[j]);
                            EnergyFlow.AddTerm(-1.0, ArrivalSOE[i]);
                            EnergyFlow.AddTerm((allNondominatedRPs[i, j][0].TotalEnergyConsumption + preprocessedSites[j].DeltaMax - preprocessedSites[i].DeltaMin), X[i][j][0]);
                            string constraint_name = "Arrival_Energy_Regulation_from_node_" + i.ToString() +"_to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(EnergyFlow, preprocessedSites[j].DeltaMax - preprocessedSites[i].DeltaMin, constraint_name));
                        }
                    
        }

        void AddConstraint_MinimizeVMTObjectiveUB()
        {
            objUpperBoundConstraintIndex = allConstraints_list.Count;
            ILinearNumExpr ObjUB = LinearNumExpr();

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        ObjUB.AddTerm(allNondominatedRPs[i, j][r].TotalDistance, X[i][j][r]);
            string constraint_name = "Minimize VMT Objective Upper Bound";
            allConstraints_list.Add(AddLe(ObjUB, objRHS, constraint_name));
        }

        void AddConstraint_MinimizeVMTObjectiveLB()
        {
            objLowerBoundConstraintIndex = allConstraints_list.Count;
            ILinearNumExpr ObjLB = LinearNumExpr();

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        ObjLB.AddTerm(allNondominatedRPs[i, j][r].TotalDistance, X[i][j][r]);
            string constraint_name = "Minimize VMT Objective Lower Bound";
            allConstraints_list.Add(AddGe(ObjLB, objRHS_LB, constraint_name));
        }

        void AddConstraint_ArrivalTimeLimitsOLD()
        {
            ILinearNumExpr TimeDifference;
            for (int j = 1; j < numNonESNodes; j++)
            {
                TimeDifference = CreateCoreOf_Constraint_ArrivalTimeLimitsOLD(j);
                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, preprocessedSites[j].TLS, constraint_name));
            }
        }
        public ILinearNumExpr CreateCoreOf_Constraint_ArrivalTimeLimitsOLD(int j)
        {
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    TimeDifference.AddTerm((preprocessedSites[j].TLS - preprocessedSites[j].TES), X[i][j][r]);

            return TimeDifference;
        }


        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            outcome.AddRange(GetVehicleSpecificRoutes());
            return outcome;
        }
        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory = VehicleCategories.EV)
        {
            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            List<string> nonDepotSiteIDs = GetNonDepotSiteIDs();
            outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs));
            return outcome;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstSiteIndices"></param>Can contain one or two elements. If one, it's for an X arc; if two, it's for a Y arc; nothing else is possible!
        /// <param name="vehicleCategory"></param>Obviously if GDV, there won't be any Y arcs
        /// <returns></returns>
        List<string> GetNonDepotSiteIDs()
        {
            GetDecisionVariableValues();
            List<string> outcome = new List<string>();

            List<List<string>> allActiveArcs = new List<List<string>>();

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        if (xValues[i, j][r] >= 0.5)
                        {
                            List<string> activeArc = new List<string>();
                            activeArc.Add(preprocessedSites[i].ID);
                            for (int p = 0; p < allNondominatedRPs[i, j][r].RefuelingStops.Count; p++)
                            {
                                activeArc.Add(allNondominatedRPs[i, j][r].RefuelingStops[p].ID);
                            }
                            activeArc.Add(preprocessedSites[j].ID);
                            allActiveArcs.Add(activeArc);
                        }
            List<List<string>> tempActiveArcs = new List<List<string>>(allActiveArcs);
            string lastID = theProblemModel.SRD.GetSingleDepotID();         
            while (tempActiveArcs.Count>0)
            {
                for(int i=0;i<tempActiveArcs.Count; i++)
                {
                    if(tempActiveArcs[i][0]==lastID)
                    {
                        tempActiveArcs[i].RemoveAt(0);
                        outcome.AddRange(tempActiveArcs[i]);
                        lastID = outcome.Last();
                        tempActiveArcs.RemoveAt(i);
                        break;
                    }
                }
            }
            outcome.RemoveAt(outcome.Count - 1);
            return outcome;
        }
        public void GetDecisionVariableValues()
        {
            int[,] length3 = new int[numNonESNodes, numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    length3[i, j] = allNondominatedRPs[i, j].Count;

            //System.IO.StreamWriter sw = new System.IO.StreamWriter("routes.txt");
            xValues = new double[numNonESNodes, numNonESNodes][];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    xValues[i, j] = new double[length3[i, j]];
                    for (int r = 0; r < length3[i, j]; r++)
                        xValues[i, j][r] = GetValue(X[i][j][r]);
                }
            DeltaValues = new double[numNonESNodes];
            for (int j = 0; j < numNonESNodes; j++)
                DeltaValues[j] = GetValue(ArrivalSOE[j]);
            TValues = new double[numNonESNodes];
            for (int j = 0; j < numNonESNodes; j++)
                TValues[j] = GetValue(ArrivalTime[j]);

            //This is only for testing, do not use this function while executing the code.
            List<string> routeIds = new List<string>();
            List<string> routeIndices = new List<string>();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < length3[i, j]; r++)
                        if (xValues[i, j][r] >= 0.5)
                        {
                            string s = preprocessedSites[i].ID + "-";
                            string s2 = i.ToString() + "_";
                            for (int p = 0; p < allNondominatedRPs[i, j][r].RefuelingStops.Count; p++)
                            {
                                s = s + allNondominatedRPs[i, j][r].RefuelingStops[p].ID + "-";
                                s2 = s2 + "r" + r.ToString() + "_";
                            }
                            s = s + preprocessedSites[j].ID;
                            s2 = s2 + j.ToString();
                            routeIds.Add(s);
                            routeIndices.Add(s2);
                        }

        }

        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        public override string GetModelName()
        {
            return "GDV Optimize Single Customer Set";
        }
        public void RefineDecisionVariables(CustomerSet cS, bool preserveCustomerVisitSequence, VehicleSpecificRoute vsr_GDV)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 1.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 1.0;
                        }
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 0.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 0.0;
                        }
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();

            //If we want to preserve the sequence, we must have a gdv optimal route!
            if (preserveCustomerVisitSequence)
            {
                //if (cS.GetVehicleSpecificRouteOptimizationStatus(VehicleCategories.GDV) != VehicleSpecificRouteOptimizationStatus.Optimized)
                //    throw new System.Exception("RefineDecisionVariables of afv solver for single customer set wants to keep the route order but it is not given one!");
                VehicleSpecificRoute vsr = vsr_GDV;
                List<int[]> activeArcs = GetOrderedCustomerIndices(vsr);
                foreach(int[] arc in activeArcs)
                    for(int i=0; i<numNonESNodes; i++)
                        for(int j=0; j<numNonESNodes; j++)
                        {
                            if(i!=arc[0]||j!=arc[1])
                            {
                                for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                                    X[i][j][r].UB = 0.0;
                            }
                        }
            }
            if(vsr_GDV!=null)
            {
                objRHS_LB = vsr_GDV.GetVehicleMilesTraveled();
                int c = objLowerBoundConstraintIndex;
                allConstraints_array[c].LB = objRHS_LB;
            }
        }
        public void RefineDecisionVariables(CustomerSet cS, bool preserveCustomerVisitSequence)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 1.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 1.0;
                        }
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 0.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 0.0;
                        }
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();
        }

        List<int[]> GetOrderedCustomerIndices(VehicleSpecificRoute vsr)
        {
            List<int[]> outcome = new List<int[]>();
            List<string> visitedSiteIDs = vsr.ListOfVisitedNonDepotSiteIDs;
            int[] indices = new int[2];//GDV arcs must have two elements 
            int from = -1;
            for (int i=0; i<preprocessedSites.Length; i++)
                if (visitedSiteIDs[0] == preprocessedSites[i].ID)
                {
                    from = i;
                }
            for (int k=1; k<visitedSiteIDs.Count; k++)
                for (int j = 0; j < preprocessedSites.Length; j++)
                    if (visitedSiteIDs[k] == preprocessedSites[j].ID)
                    {
                        int to = j;
                        indices[0] = from;
                        indices[1] = to;
                        from = to;
                    }
            
            return outcome;
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
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 1.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 1.0;
                        }
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 0.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 0.0;
                        }
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();
            //singleRouteESvisits.Clear();
        }
        void RefineRightHandSidesOfCustomerVisitationConstraints()
        {
            int c = firstCustomerVisitationConstraintIndex;

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

        public void RefineDecisionVariables(CustomerSet cS, double AFV_vmt)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 1.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 1.0;
                        }
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            X[i][j][r].UB = 0.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 0.0;
                        }
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();
            allConstraints_array[objUpperBoundConstraintIndex].UB = double.MaxValue;
            allConstraints_array[objUpperBoundConstraintIndex].LB = 0.0;
            if (objUpperBoundConstraintIndex > -1)
            {
                allConstraints_array[objUpperBoundConstraintIndex].UB = AFV_vmt;
                allConstraints_array[objUpperBoundConstraintIndex].LB = AFV_vmt;
            }
        }

        //void RefineRHSofTotalTravelConstraints(CustomerSet cS)
        //{
        //    int c = 0;
        //    if (customerCoverageConstraint != CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
        //    {
        //        c = totalTravelTimeConstraintIndex;
        //        allConstraints_array[c].UB = theProblemModel.CRD.TMax - 30.0 * RHS_forNodeCoverage.Sum();//theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
        //        //allConstraints_array[c].LB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
        //    }
        //    if (addTotalNumberOfActiveArcsCut)
        //    {
        //        c = totalNumberOfActiveArcsConstraintIndex;
        //        allConstraints_array[c].UB = theProblemModel.CRD.TMax + cS.NumberOfCustomers;
        //        //allConstraints_array[c].LB = allConstraints_array[c].LB + cS.NumberOfCustomers;
        //    }
        //}
        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            throw new NotImplementedException();
        }
    }
}

