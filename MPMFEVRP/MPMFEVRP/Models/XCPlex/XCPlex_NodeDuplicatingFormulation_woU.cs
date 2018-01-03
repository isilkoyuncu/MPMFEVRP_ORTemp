using ILOG.Concert;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_NodeDuplicatingFormulation_woU : XCPlexVRPBase
    {
        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        //DVs, upper and lower bounds
        INumVar[][][] X; double[][][] X_LB, X_UB;

        //Auxiliaries, upper and lower bounds
        INumVar[] Epsilon;
        INumVar[] Delta; 
        INumVar[] T; 

        int firstCustomerVisitationConstraintIndex=-1;
        int totalTravelTimeConstraintIndex = -1;

        IndividualRouteESVisits singleRouteESvisits;
        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

        public XCPlex_NodeDuplicatingFormulation_woU() { } //Empty constructor
        public XCPlex_NodeDuplicatingFormulation_woU(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam){ } //XCPlex VRP Constructor

        protected override void DefineDecisionVariables()
        {
            PreprocessSites(theProblemModel.Lambda * theProblemModel.GetNumVehicles(VehicleCategories.EV));//lambda*nEVs duplicates of each ES node, and a single copy of each other node will be places in the array preprocessedSWAVs
            SetMinAndMaxValuesOfModelSpecificVariables();

            allVariables_list = new List<INumVar>();

            //dvs: X_ijv
            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, NumPreprocessedSites, NumPreprocessedSites, numVehCategories, out X);
            //auxiliaries (Epsilon_j, Delta_j, T_j)
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, NumPreprocessedSites, out Epsilon);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, NumPreprocessedSites, out Delta);
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, NumPreprocessedSites, out T);
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXVariablesTo0();
        }
        void SetUndesiredXVariablesTo0()
        {
            //No arc from a node to itself
            for (int j = 0; j < NumPreprocessedSites; j++)
                for (int v = 0; v < numVehCategories; v++)
                    X[j][j][v].UB = 0.0;
            //No arc from one ES to another
            for (int i = FirstESNodeIndex; i <= LastESNodeIndex; i++)
                for (int j = FirstESNodeIndex; j <= LastESNodeIndex; j++)
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X[i][j][v].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > vehicle battery capacity
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        if (EnergyConsumption(sFrom, sTo, vehicleCategories[v]) > BatteryCapacity(vehicleCategories[v]))
                            X[i][j][v].UB = 0.0;
                }
            }
            //No arc from depot to its duplicate
            for (int v = 0; v < numVehCategories; v++)
                for (int j = FirstESNodeIndex; j <= LastESNodeIndex; j++)
                    if ((preprocessedSites[j].X == TheDepot.X) && (preprocessedSites[j].Y == TheDepot.Y))//Comparing X and Y coordinates to those of the depot makes sure that the ES at hand corresponds to the one at the depot!
                    {
                        X[0][j][v].UB = 0.0;
                        X[j][0][v].UB = 0.0;
                    }
            //No arc from or to an ES node can be traversed by a GDV
            for (int v = 0; v < numVehCategories; v++)
                if (vehicleCategories[v] == VehicleCategories.GDV)
                    for (int i = 0; i < NumPreprocessedSites; i++)
                        for (int j = FirstESNodeIndex; j <= LastESNodeIndex; j++)
                        {
                            X[i][j][v].UB = 0.0;
                            X[j][i][v].UB = 0.0;
                        }
        }
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {
            X_LB = new double[NumPreprocessedSites][][];
            X_UB = new double[NumPreprocessedSites][][];

            RHS_forNodeCoverage = new double[NumPreprocessedSites];

            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                X_LB[i] = new double[NumPreprocessedSites][];
                X_UB[i] = new double[NumPreprocessedSites][];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    X_LB[i][j] = new double[numVehCategories];
                    X_UB[i][j] = new double[numVehCategories];
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X_LB[i][j][v] = 0.0;
                        X_UB[i][j][v] = 1.0;
                    }
                }
                RHS_forNodeCoverage[i] = 1.0;
            }                
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i < numNodes; i++)\nfor (int j = 0; j < numNodes; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nX[i][j][v]"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\nepsilon[j]\n"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\ndelta[j]\n"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\nT[j]\n";
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
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: prize collection
            for (int i = 0; i < NumPreprocessedSites; i++)
                for (int j = 0; j < NumPreprocessedSites; j++)
            {
                Site s = preprocessedSites[j];
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(Prize(s, vehicleCategories[v]), X[i][j][v]);
            }
            //Second term: distance-based costs
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Third term: vehicle fixed costs
            for (int j = 0; j < NumPreprocessedSites; j++)
                for (int v = 0; v < numVehCategories; v++)
                objFunction.AddTerm(-1.0 * GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT)
            {
                //First term: distance-based costs
                for (int i = 0; i < NumPreprocessedSites; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int j = 0; j < NumPreprocessedSites; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        for (int v = 0; v < numVehCategories; v++)
                            objFunction.AddTerm(Distance(sFrom, sTo), X[i][j][v]);
                    }
                }
            }
            else
            {
                //First term: distance-based costs
                for (int i = 0; i < NumPreprocessedSites; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int j = 0; j < NumPreprocessedSites; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        for (int v = 0; v < numVehCategories; v++)
                            objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                    }
                }
                //Second term: vehicle fixed costs
                for (int j = 0; j < NumPreprocessedSites; j++)
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);

            }//Now adding the objective function to the model
            AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_NumberOfVisitsPerCustomerNode();//1
            AddConstraint_NumberOfEvVisitsPerESNode();//2
            AddConstraint_NoGDVVisitToESNodes();//3
            AddConstraint_IncomingXTotalEqualsOutgoingXTotal();//4
            AddConstraint_MaxNumberOfVehiclesPerCategory();//5
            AddConstraint_MaxEnergyGainAtNonDepotSite();//6

            if (rechargingDuration_status==RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
                AddConstraint_DepartureSOCFromNode_PartialRecharging();//7-8
            else
            {
                AddConstraint_DepartureSOCFromCustomerNode();//7
                AddConstraint_DepartureSOCFromESNode_FullRecharging();//8
            }

            AddConstraint_SOCRegulationFollowingNondepot();//9
            AddConstraint_SOCRegulationFollowingDepot();//10
            AddConstraint_TimeRegulationFollowingACustomerVisit();//11

            if(rechargingDuration_status==RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                AddConstraint_TimeRegulationFollowingAnESVisit_FixedRechargingDuration();//12
            else
                AddConstraint_TimeRegulationFollowingAnESVisit_VariableRechargingDuration();//12

            AddConstraint_ArrivalTimeLimits();
            AddConstraint_TotalTravelTime();
            AddConstraint_TimeFeasibilityOfTwoConsecutiveArcs();
            //AddConstraint_EnergyFeasibilityOfTwoConsecutiveArcs();
            //AddConstraint_EnergyConservation();
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }

        void AddConstraint_NumberOfVisitsPerCustomerNode()//1
        {
            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

            for (int j = FirstCustomerNodeIndex; j <= LastCustomerNodeIndex; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheCustomerNode = LinearNumExpr();
                for (int i = 0; i < NumPreprocessedSites; i++)
                    for (int v = 0; v < numVehCategories; v++)
                        NumberOfVehiclesVisitingTheCustomerNode.AddTerm(1.0, X[i][j][v]);

                string constraint_name;

                if (xCplexParam.TSP)//TODO: I don't think we should be checking for "TSP" separately, isn't it the same thing as "exactly once"?
                {
                    constraint_name = "The_customer_node_" + j.ToString()+"_must_be_visited";
                    allConstraints_list.Add(AddEq(NumberOfVehiclesVisitingTheCustomerNode, RHS_forNodeCoverage[j], constraint_name));
                }
                else
                {
                    if (theProblemModel.CoverConstraintType == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
                    {
                        constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
                        allConstraints_list.Add(AddEq(NumberOfVehiclesVisitingTheCustomerNode, 1.0, constraint_name));
                    }
                    else
                    {
                        constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheCustomerNode, 1.0, constraint_name));
                    }
                }
                NumberOfVehiclesVisitingTheCustomerNode.Clear();
            }
        }
        void AddConstraint_NumberOfEvVisitsPerESNode()//2
        {
            for (int j = FirstESNodeIndex; j <= LastESNodeIndex; j++)
            {
                ILinearNumExpr NumberOfEvVisitToTheESNode = LinearNumExpr();
                for (int i = 0; i < NumPreprocessedSites; i++)
                    NumberOfEvVisitToTheESNode.AddTerm(1.0, X[i][j][vIndex_EV]);

                string constraint_name = "At_most_one_EV_can_visit_the_ES_node_" + j.ToString();
                allConstraints_list.Add(AddLe(NumberOfEvVisitToTheESNode, 1.0, constraint_name));
                NumberOfEvVisitToTheESNode.Clear();
            }
        }
        void AddConstraint_NoGDVVisitToESNodes()//3
        {
            for (int j = FirstESNodeIndex; j <= LastESNodeIndex; j++)
            {
                ILinearNumExpr NumberOfGDVsVisitingTheNode = LinearNumExpr();
                for (int i = 0; i < NumPreprocessedSites; i++)
                    NumberOfGDVsVisitingTheNode.AddTerm(1.0, X[i][j][vIndex_GDV]);

                string constraint_name = "No_GDV_can_visit_the_ES_node_" + j.ToString();
                allConstraints_list.Add(AddEq(NumberOfGDVsVisitingTheNode, 0.0, constraint_name));
                NumberOfGDVsVisitingTheNode.Clear();
            }
        }
        void AddConstraint_IncomingXTotalEqualsOutgoingXTotal()//4
        {
            for (int j = 0; j < NumPreprocessedSites; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusOutgoingXTotal = LinearNumExpr();
                    for (int i = 0; i < NumPreprocessedSites; i++)
                        IncomingXTotalMinusOutgoingXTotal.AddTerm(1.0, X[i][j][v]);
                    for (int k = 0; k < NumPreprocessedSites; k++)
                        IncomingXTotalMinusOutgoingXTotal.AddTerm(-1.0, X[j][k][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_vehicles";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusOutgoingXTotal, 0.0, constraint_name));
                    IncomingXTotalMinusOutgoingXTotal.Clear();
                }
        }
        void AddConstraint_MaxNumberOfVehiclesPerCategory()//5
        {
            for (int v = 0; v < numVehCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                for (int j = 1; j < NumPreprocessedSites; j++)
                    NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][j][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_node_0_cannot_exceed_" + numVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, numVehicles[v], constraint_name));
                NumberOfVehiclesPerCategoryOutgoingFromTheDepot.Clear();
            }
        }
        void AddConstraint_MaxEnergyGainAtNonDepotSite()//6
        {
            for (int j = 1; j < NumPreprocessedSites; j++)
            {
                ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
                EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < NumPreprocessedSites; i++)
                    EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][vIndex_EV]);
                string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
                allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));
                EnergyGainAtNonDepotSite.Clear();
            }
        }
        void AddConstraint_DepartureSOCFromCustomerNode()//7
        {
            for (int j = FirstCustomerNodeIndex; j <= LastCustomerNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < NumPreprocessedSites; i++)
                    DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV)-minValue_Delta[j]), X[i][j][vIndex_EV]);
                string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));
                DepartureSOCFromCustomer.Clear();
            }
        }
        void AddConstraint_DepartureSOCFromESNode_FullRecharging()//8
        {
            for (int j = FirstESNodeIndex; j <= LastESNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                DepartureSOCFromES.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < NumPreprocessedSites; i++)
                    DepartureSOCFromES.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j][vIndex_EV]);
                string constraint_name = "Departure_SOC_From_ES_" + j.ToString();
                allConstraints_list.Add(AddEq(DepartureSOCFromES, minValue_Delta[j], constraint_name));
                DepartureSOCFromES.Clear();
            }
        }
        void AddConstraint_DepartureSOCFromNode_PartialRecharging()//8 Only in VP case
        {
            for (int j = 0; j < NumPreprocessedSites; j++)
            {
                ILinearNumExpr DepartureSOCFromNode = LinearNumExpr();
                DepartureSOCFromNode.AddTerm(1.0, Delta[j]);
                DepartureSOCFromNode.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < NumPreprocessedSites; i++)
                    DepartureSOCFromNode.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j][vIndex_EV]);
                string constraint_name = "Departure_SOC_From_ES_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromNode, minValue_Delta[j], constraint_name));
                DepartureSOCFromNode.Clear();
            }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()//9
        {
            for (int i = 1; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, Delta[j]);
                    SOCDifference.AddTerm(-1.0, Delta[i]);
                    SOCDifference.AddTerm(-1.0, Epsilon[i]);
                    SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV) + BigDelta[i][j], X[i][j][vIndex_EV]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, BigDelta[i][j], constraint_name));
                    SOCDifference.Clear();
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()//10
        {
            for (int j = 0; j < NumPreprocessedSites; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                SOCDifference.AddTerm(EnergyConsumption(TheDepot, sTo,VehicleCategories.EV), X[0][j][vIndex_EV]);
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
                SOCDifference.Clear();
            }
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()//11
        {
            for (int i = FirstCustomerNodeIndex; i <= LastCustomerNodeIndex; i++)
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < numVehCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j][v]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                    TimeDifference.Clear();
                }
        }
        void AddConstraint_TimeRegulationFollowingAnESVisit_FixedRechargingDuration()//12 
        {
            for (int i = FirstESNodeIndex; i <= LastESNodeIndex; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    string constraint_name;
                    TimeDifference.AddTerm(-1.0 * ((BatteryCapacity(VehicleCategories.EV) / RechargingRate(sFrom)) + TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j][vIndex_EV]);
                    constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                    TimeDifference.Clear();
                }
            }
        }
        void AddConstraint_TimeRegulationFollowingAnESVisit_VariableRechargingDuration()//12 Only if recharging duration is variable
        {
            for (int i = FirstESNodeIndex; i <= LastESNodeIndex; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    string constraint_name;
                    TimeDifference.AddTerm(-1.0 / RechargingRate(sFrom), Epsilon[i]);
                    TimeDifference.AddTerm(-1.0 * (TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j][vIndex_EV]);
                    constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                    TimeDifference.Clear();
                }
            }
        }
        void AddConstraint_ArrivalTimeLimits()//13
        {
            for (int j = 1; j < NumPreprocessedSites; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int i = 0; i < NumPreprocessedSites; i++)
                    for (int v = 0; v < numVehCategories; v++)
                        TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), X[i][j][v]);
                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, maxValue_T[j], constraint_name));
                TimeDifference.Clear();
            }
        }
        void AddConstraint_TotalTravelTime()//14
        {
            totalTravelTimeConstraintIndex = allConstraints_list.Count;

            ILinearNumExpr TotalTravelTime = LinearNumExpr();
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j][v]);
                }
            }
            string constraint_name = "Total_Travel_Time";
            double rhs = (numVehicles[vIndex_EV]+numVehicles[vIndex_GDV]) * theProblemModel.CRD.TMax;
            rhs -= theProblemModel.SRD.GetTotalCustomerServiceTime();
            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));
            TotalTravelTime.Clear();
        }
        void AddConstraint_TimeFeasibilityOfTwoConsecutiveArcs()//15
        {
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site from = preprocessedSites[i];
                for (int k = 0; k < NumPreprocessedSites; k++)
                {
                    Site through = preprocessedSites[k];
                    for (int j = 0; j < NumPreprocessedSites; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (i != j && j != k && i != k)
                            if (from.SiteType == SiteTypes.Customer && through.SiteType == SiteTypes.Customer && to.SiteType == SiteTypes.Customer)
                                if (minValue_T[i] + ServiceDuration(from)+ TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
                                {
                                    ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
                                    string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                                    allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                    TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                }
                    }
                }
            }
        }
        void AddConstraint_EnergyFeasibilityOfTwoConsecutiveArcs()//16
        {
            for (int j = FirstCustomerNodeIndex; j <= LastCustomerNodeIndex; j++)
            {
                Site through = preprocessedSites[j];
                ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                for (int i = FirstESNodeIndex; i <= LastESNodeIndex; i++)
                {
                    Site ES = preprocessedSites[i];
                    EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(EnergyConsumption(ES, through, VehicleCategories.EV), X[i][j][vIndex_EV]);
                    EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(EnergyConsumption(through, ES, VehicleCategories.EV), X[j][i][vIndex_EV]);
                }
                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(-1.0, Epsilon[j]);
                string constraint_name = "No_arc_through_node_" + j.ToString();
                allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, BatteryCapacity(VehicleCategories.EV), constraint_name));
                EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
            }
        }
        void AddConstraint_EnergyConservation()//17
        {
            ILinearNumExpr EnergyConservation = LinearNumExpr();
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    EnergyConservation.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV), X[i][j][vIndex_EV]);
                    EnergyConservation.AddTerm(-1.0*maxValue_Epsilon[j], X[i][j][vIndex_EV]);
                }
            }
            string constraint_name = "Energy conservation";
            allConstraints_list.Add(AddLe(EnergyConservation, numVehicles[vIndex_EV] * BatteryCapacity(VehicleCategories.EV), constraint_name));
            EnergyConservation.Clear();
        }



        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            List<string> activeX = new List<string>();
            List<string> allX = new List<string>();
            for (int i = 0; i < NumPreprocessedSites; i++)
                for (int j = 0; j < NumPreprocessedSites; j++)
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        allX.Add(preprocessedSites[i].ID + "," + preprocessedSites[j].ID + "," + v.ToString() + "->" + GetValue(X[i][j][v]).ToString());
                        if (GetValue(X[i][j][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            activeX.Add(preprocessedSites[i].ID + "," + preprocessedSites[j].ID + "," + v.ToString()+"->"+ GetValue(X[i][j][v]).ToString());
                    }

            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            foreach (VehicleCategories vc in vehicleCategories)
                outcome.AddRange(GetVehicleSpecificRoutes(vc));
            return outcome;
        }
        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory)
        {
            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);//Pulling the vehicle infor from the Problem Model. Not exactly flexible, but works as long as we have only two categories of vehicles and no more than one of each
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            int counter = 0;
            foreach (List<string> nonDepotSiteIDs in GetListsOfNonDepotSiteIDs(vehicleCategory))
            {
                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs, allRoutesESVisits[counter]));
                counter++;
            }
            return outcome;
        }
        List<List<string>> GetListsOfNonDepotSiteIDs(VehicleCategories vehicleCategory)
        {
            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

            //We first determine the route start points
            List<int> firstSiteIndices = new List<int>();
            for (int j = 0; j < NumPreprocessedSites; j++)
                if (GetValue(X[0][j][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    firstSiteIndices.Add(j);
                }
            //Then, populate the whole routes (actually, not routes yet)
            List<List<string>> outcome = new List<List<string>>();
            foreach (int firstSiteIndex in firstSiteIndices)
            {
                outcome.Add(GetNonDepotSiteIDs(firstSiteIndex, vehicleCategory));
            }
            return outcome;
        }
        List<string> GetNonDepotSiteIDs(int firstSiteIndex, VehicleCategories vehicleCategory)
        {
            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
            List<string> outcome = new List<string>();
            if (GetValue(X[0][firstSiteIndex][vc_int]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
            {
                throw new System.Exception("XCPlex_NodeDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndex,vehicleCategory) pair that doesn't correspond to a flow from the depot!");
            }
            string currentSiteID = preprocessedSites[firstSiteIndex].ID;
            int currentSiteIndex = firstSiteIndex;
            int nextSiteIndex = -1;
            singleRouteESvisits = new IndividualRouteESVisits();
            do
            {
                outcome.Add(currentSiteID);
                if (preprocessedSites[currentSiteIndex].SiteType == SiteTypes.ExternalStation)
                    singleRouteESvisits.Add(GetIndividualESVisit(currentSiteIndex));
                nextSiteIndex = GetNextSiteIndex(currentSiteIndex, vc_int);
                if (preprocessedSites[nextSiteIndex].ID == TheDepot.ID)
                {
                    allRoutesESVisits.Add(singleRouteESvisits);
                    return outcome;
                }
                currentSiteIndex = nextSiteIndex;
                currentSiteID = preprocessedSites[currentSiteIndex].ID;
            }
            while (currentSiteID != TheDepot.ID);
            allRoutesESVisits.Add(singleRouteESvisits);
            return outcome;

        }
        IndividualESVisitDataPackage GetIndividualESVisit(int i)
        {
            Site ES = preprocessedSites[i];
            if (ES.SiteType != SiteTypes.ExternalStation)
                throw new System.Exception("Given site is not an ES.");
            else
                return new IndividualESVisitDataPackage(ES.ID, GetValue(Epsilon[i]) / RechargingRate(ES), preprocessedESSiteIndex: i);
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        int GetNextSiteIndex(int currentSiteIndex, int vc_int)
        {
            for (int nextSiteIndex = 0; nextSiteIndex < NumPreprocessedSites; nextSiteIndex++)
                if (GetValue(X[currentSiteIndex][nextSiteIndex][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    return nextSiteIndex;
                }
            throw new System.Exception("Flow ended before returning to the depot!");
        }

        public override string GetModelName()
        {
            return "Node Duplicating wo U";
        }

        public override void RefineDecisionVariables(CustomerSet cS)
        {
            RHS_forNodeCoverage = new double[NumPreprocessedSites];
            int VCIndex = (int)xCplexParam.VehCategory;
            for (int i = 0; i < NumPreprocessedSites; i++)
                for (int j = FirstCustomerNodeIndex; j <= LastCustomerNodeIndex; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();

            allConstraints_array[totalTravelTimeConstraintIndex].UB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);

        }
        void RefineRightHandSidesOfCustomerVisitationConstraints()
        {
            int c = firstCustomerVisitationConstraintIndex;

            for (int j = FirstCustomerNodeIndex; j <= LastCustomerNodeIndex; j++)
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
    }
}
