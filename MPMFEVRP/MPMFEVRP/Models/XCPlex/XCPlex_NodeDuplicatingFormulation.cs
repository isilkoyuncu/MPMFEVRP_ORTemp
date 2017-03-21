﻿using System;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_NodeDuplicatingFormulation : XCPlexBase
    {
        int numNodes;
        int[] nodeToOriginalSiteNumberMap;
        int firstESNodeIndex, lastESNodeIndex, firstCustomerNodeIndex, lastCustomerNodeIndex;//There is a single depot and it is always node #0
        double[] minValue_T, maxValue_T, minValue_delta, maxValue_delta, minValue_epsilon, maxValue_epsilon;

        INumVar[][][] X;
        INumVar[][] U;
        INumVar[] T;
        INumVar[] delta;
        INumVar[] epsilon;

        public XCPlex_NodeDuplicatingFormulation(ProblemToAlgorithm fromProblem, AlgorithmToXCPlex fromAlgorithm)
            : base(fromProblem, fromAlgorithm)
        {
        }
        protected override void DefineDecisionVariables()
        {
            duplicateAndMapNodes();
            allVariables_list = new List<INumVar>();
            //X
            string[][][] X_name = new string[numNodes][][];
            X = new INumVar[numNodes][][];
            for (int i = 0; i < numNodes; i++)
            {
                X_name[i] = new string[numNodes][];
                X[i] = new INumVar[numNodes][];
                for (int j = 0; j < numNodes; j++)
                {
                    X_name[i][j] = new string[problemModel.NumVehicleCategories];
                    X[i][j] = new INumVar[problemModel.NumVehicleCategories];
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    {
                        X_name[i][j][v] = "X_(" + i.ToString() + "," + j.ToString() + "," + v.ToString() + ")";
                        X[i][j][v] = NumVar(0, 1, variable_type, X_name[i][j][v]);
                        allVariables_list.Add(X[i][j][v]);
                    }//for v
                }//for j
            }//for i
            //U
            string[][] U_name = new string[numNodes][];
            U = new INumVar[numNodes][];
            for (int j = 0; j < numNodes; j++)
            {
                U_name[j] = new string[problemModel.NumVehicleCategories];
                U[j] = new INumVar[problemModel.NumVehicleCategories];
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                {
                    U_name[j][v] = "U_(" + j.ToString() + "," + v.ToString() + ")";
                    U[j][v] = NumVar(0, getUUpperBound(j, v), variable_type, U_name[j][v]);
                    allVariables_list.Add(U[j][v]);
                }//for v
            }//for j
            //auxiliaries (T, delta, epsilon)
            setMinAndMaxValuesOfAuxiliaryVariables();
            T = new INumVar[numNodes];
            string[] T_name = new string[numNodes];
            for (int j = 0; j < numNodes; j++)
            {
                T_name[j] = "T_(" + j.ToString() + ")";
                T[j] = NumVar(minValue_T[j], maxValue_T[j], NumVarType.Float, T_name[j]);
                allVariables_list.Add(T[j]);
            }
            delta = new INumVar[numNodes];
            string[] delta_name = new string[numNodes];
            for (int j = 0; j < numNodes; j++)
            {
                delta_name[j] = "delta_(" + j.ToString() + ")";
                delta[j] = NumVar(minValue_delta[j], maxValue_delta[j], NumVarType.Float, delta_name[j]);
                allVariables_list.Add(delta[j]);
            }
            epsilon = new INumVar[numNodes];
            string[] epsilon_name = new string[numNodes];
            for (int j = 0; j < numNodes; j++)
            {
                epsilon_name[j] = "epsilon_(" + j.ToString() + ")";
                epsilon[j] = NumVar(minValue_epsilon[j], maxValue_epsilon[j], NumVarType.Float, epsilon_name[j]);
                allVariables_list.Add(epsilon[j]);
            }
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            setUndesiredXVariablesTo0();
        }
        void duplicateAndMapNodes()
        {
            int numCustomers = problemModel.NumCustomers;
            int numES = problemModel.Lambda * problemModel.NumVehicles[0] * problemModel.NumES;
            numNodes = 1 + numCustomers + numES;

            nodeToOriginalSiteNumberMap = new int[numNodes];
            int nodeCounter = 0;
            for (int orgSiteIndex = 0; orgSiteIndex < problemModel.SiteArray.Length; orgSiteIndex++)
            {
                switch (problemModel.SiteArray[orgSiteIndex].SiteType)
                {
                    case SiteTypes.Depot:
                        nodeToOriginalSiteNumberMap[nodeCounter++] = orgSiteIndex;
                        break;
                    case SiteTypes.Customer:
                        if (firstCustomerNodeIndex == 0)
                            firstCustomerNodeIndex = nodeCounter;
                        lastCustomerNodeIndex = nodeCounter;
                        nodeToOriginalSiteNumberMap[nodeCounter++] = orgSiteIndex;
                        break;
                    case SiteTypes.ExternalStation:
                        if (firstESNodeIndex == 0)
                            firstESNodeIndex = nodeCounter;
                        for (int i = 0; i < problemModel.Lambda * problemModel.NumVehicles[0]; i++)
                            nodeToOriginalSiteNumberMap[nodeCounter++] = orgSiteIndex;
                        lastESNodeIndex = nodeCounter - 1;
                        break;
                    default:
                        throw new System.Exception("Site type incompatible!");
                }
            }
        }
        int getUUpperBound(int j, int v)
        {
            if (j == 0)
                return problemModel.NumVehicles[v];
            else
                return 1;
        }
        void setUndesiredXVariablesTo0()
        {
            //No arc from a node to itself
            for (int j = 0; j < numNodes; j++)
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    X[j][j][v].UB = 0.0;
            //No arc from one ES to another
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    {
                        X[i][j][v].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > 1
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        if (problemModel.VehicleArray[v].Category == VehicleCategories.EV)
                        {
                            if (problemModel.EnergyConsumption[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j], v] > 1)
                                X[i][j][v].UB = 0.0;
                        }
            //No arc from depot to its duplicate
            for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    if (nodeToOriginalSiteNumberMap[j] == 1)
                    {
                        X[0][j][v].UB = 0.0;
                        X[j][0][v].UB = 0.0;
                    }
            //No arc between a non-ES node and an ES node can be traversed by a GDV
            for (int i = 0; i < numNodes; i++)
            {
                if (problemModel.SiteArray[nodeToOriginalSiteNumberMap[i]].SiteType == SiteTypes.ExternalStation)
                    continue;
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        if (problemModel.VehicleArray[v].Category == VehicleCategories.GDV)
                        {
                            X[i][j][v].UB = 0.0;
                            X[j][i][v].UB = 0.0;
                        }
            }
        }
        void setMinAndMaxValuesOfAuxiliaryVariables()
        {
            minValue_T = new double[numNodes];
            maxValue_T = new double[numNodes];
            minValue_delta = new double[numNodes];
            maxValue_delta = new double[numNodes];
            minValue_epsilon = new double[numNodes];
            maxValue_epsilon = new double[numNodes];

            for (int j = 0; j < numNodes; j++)
            {
                minValue_T[j] = problemModel.TimeConsumption[0, nodeToOriginalSiteNumberMap[j]];
                maxValue_T[j] = problemModel.TMax - problemModel.TimeConsumption[nodeToOriginalSiteNumberMap[j], 0];
                if (problemModel.SiteArray[nodeToOriginalSiteNumberMap[j]].SiteType == SiteTypes.Customer)
                    maxValue_T[j] -= problemModel.SiteArray[nodeToOriginalSiteNumberMap[j]].ServiceDuration;

                //TODO Fine-tune the min and max values of delta
                minValue_delta[j] = 0.0;
                maxValue_delta[j] = 1.0;

                minValue_epsilon[j] = 0.0;
                if (problemModel.SiteArray[nodeToOriginalSiteNumberMap[j]].SiteType == SiteTypes.Customer)
                    maxValue_epsilon[j] = Math.Min(1.0, problemModel.SiteArray[nodeToOriginalSiteNumberMap[j]].ServiceDuration * Math.Min(problemModel.SiteArray[nodeToOriginalSiteNumberMap[j]].RechargingRate,problemModel.VehicleArray[0].MaxChargingRate)/problemModel.VehicleArray[0].BatteryCapacity);
                else
                    maxValue_epsilon[j] = 1.0;
            }
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i < numNodes; i++)\nfor (int j = 0; j < numNodes; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nX[i][j][v]"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nU[j][v]\n"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\nT[j]\n"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\ndelta[j]\n"
                + "then\n"
                + "for (int j = 0; j < numNodes; j++)\nepsilon[j]\n";
        }

        protected override void AddTheObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: prize collection
            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    objFunction.AddTerm(problemModel.SiteArray[nodeToOriginalSiteNumberMap[j]].Prize[v], U[j][v]);
            //Second term: distance-based costs
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        objFunction.AddTerm(-1.0 * problemModel.VehicleArray[v].VariableCostPerMile * problemModel.Distance[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j]], X[i][j][v]);
            //Third term: vehicle fixed costs
            for (int j = 0; j < numNodes; j++)
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    objFunction.AddTerm(-1.0 * problemModel.VehicleArray[v].FixedCost, X[0][j][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_IncomingXtoDepotTotalEqualsU();
            AddConstraint_OutgoingXfromDepotTotalEqualsU();
            AddConstraint_IncomingXTotalEqualsU();
            AddConstraint_OutgoingXTotalEqualsU();
            AddConstraint_AtMostOneVisitPerNode();
            AddConstraint_NoGDVVisitToESNodes();
            AddConstraint_MaxNumberOfVehiclesPerCategory();
            AddConstraint_TimeRegulationFollowingACustomerVisit();
            AddConstraint_TimeRegulationFollowingAnESVisit();
            AddConstraint_SOCRegulationFollowingNondepot();
            AddConstraint_SOCRegulationFollowingDepot();
            AddConstraint_MaxRechargeAtCustomerNode();
            AddConstraint_MaxDepartureSOCFromCustomerNode();
            AddConstraint_DepartureSOCFromESNode();
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_IncomingXtoDepotTotalEqualsU()
        {
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                    for (int i = 0; i < numNodes; i++)
                        IncomingXTotalMinusU.AddTerm(1.0, X[i][0][v]);
                    IncomingXTotalMinusU.AddTerm(-1.0, U[0][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_depot_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_OutgoingXfromDepotTotalEqualsU()
        {
            for (int v = 0; v < problemModel.NumVehicleCategories; v++)
            {
                ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                for (int i = 0; i < numNodes; i++)
                    IncomingXTotalMinusU.AddTerm(1.0, X[i][0][v]);
                IncomingXTotalMinusU.AddTerm(-1.0, U[0][v]);
                string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_depot_equals_the_U_variable";
                allConstraints_list.Add(AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
            }
        }
        void AddConstraint_IncomingXTotalEqualsU()
        {
            for(int j=1; j < numNodes; j++)
                for(int v=0; v < problemModel.NumVehicleCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                    for (int i = 0; i < numNodes; i++)
                        IncomingXTotalMinusU.AddTerm(1.0, X[i][j][v]);
                    IncomingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_OutgoingXTotalEqualsU()
        {
            for (int j = 1; j < numNodes; j++)
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                {
                    ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                    for (int k = 0; k < numNodes; k++)
                        OutgoingXTotalMinusU.AddTerm(1.0, X[j][k][v]);
                    OutgoingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_AtMostOneVisitPerNode()
        {
            for (int j = 1; j < numNodes; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    NumberOfVehiclesVisitingTheNode.AddTerm(1.0, U[j][v]);
                string constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheNode, 1.0, constraint_name));
            }
        }
        void AddConstraint_NoGDVVisitToESNodes()
        {
            for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
            {
                ILinearNumExpr NumberOfGDVsVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    if (problemModel.VehicleArray[v].Category == VehicleCategories.GDV)
                        NumberOfGDVsVisitingTheNode.AddTerm(1.0, U[j][v]);
                string constraint_name = "No_GDV_can_visit_the_ES_node_" + j.ToString();
                allConstraints_list.Add(AddEq(NumberOfGDVsVisitingTheNode, 0.0, constraint_name));
            }
        }
        void AddConstraint_MaxNumberOfVehiclesPerCategory()
        {
            for (int v = 0; v < problemModel.NumVehicleCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                for (int k = 0; k < numNodes; k++)
                    NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][k][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_node_0_cannot_exceed_" + problemModel.NumVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, problemModel.NumVehicles[v], constraint_name));
            }
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()
        {
            for (int i = firstCustomerNodeIndex; i <= lastCustomerNodeIndex; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (problemModel.SiteArray[nodeToOriginalSiteNumberMap[i]].ServiceDuration + problemModel.TimeConsumption[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j]] + (maxValue_T[i] - minValue_T[j])), X[i][j][v]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                }
        }
        void AddConstraint_TimeRegulationFollowingAnESVisit()
        {
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (problemModel.TimeConsumption[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j]] + (maxValue_T[i] - minValue_T[j] + 1.0/problemModel.SiteArray[nodeToOriginalSiteNumberMap[i]].RechargingRate)), X[i][j][v]);
                    TimeDifference.AddTerm(1.0 / problemModel.SiteArray[nodeToOriginalSiteNumberMap[i]].RechargingRate, delta[i]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()
        {
            for (int i = 1; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, delta[j]);
                    SOCDifference.AddTerm(-1.0, delta[i]);
                    SOCDifference.AddTerm(-1.0, epsilon[i]);
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        if (problemModel.VehicleArray[v].Category == VehicleCategories.EV)
                            SOCDifference.AddTerm(problemModel.EnergyConsumption[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j], v] + maxValue_delta[j] - minValue_delta[i], X[i][j][v]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, maxValue_delta[j] - minValue_delta[i], constraint_name));
                }
        }
        void AddConstraint_SOCRegulationFollowingDepot()
        {
            for (int j = 0; j < numNodes; j++)
            {
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, delta[j]);
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    if (problemModel.VehicleArray[v].Category == VehicleCategories.EV)
                        SOCDifference.AddTerm(problemModel.EnergyConsumption[nodeToOriginalSiteNumberMap[0], nodeToOriginalSiteNumberMap[j], v], X[0][j][v]);
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, 1.0, constraint_name));
            }
        }
        void AddConstraint_MaxRechargeAtCustomerNode()
        {
            for (int j = firstCustomerNodeIndex; j <=lastCustomerNodeIndex ; j++)
            {
                ILinearNumExpr RechargeAtCustomer = LinearNumExpr();
                RechargeAtCustomer.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    if (problemModel.VehicleArray[v].Category == VehicleCategories.EV)
                        RechargeAtCustomer.AddTerm(-1.0 * maxValue_epsilon[j], U[j][v]);
                string constraint_name = "Max_Recharge_At_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(RechargeAtCustomer, 0.0, constraint_name));
            }
        }
        void AddConstraint_MaxDepartureSOCFromCustomerNode()
        {
            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    if (problemModel.VehicleArray[v].Category == VehicleCategories.EV)
                        DepartureSOCFromCustomer.AddTerm(-1.0, U[j][v]);
                string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, 0.0, constraint_name));
            }
        }
        void AddConstraint_DepartureSOCFromESNode()
        {
            for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                DepartureSOCFromES.AddTerm(1.0, delta[j]);
                DepartureSOCFromES.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                    if (problemModel.VehicleArray[v].Category == VehicleCategories.EV)
                        DepartureSOCFromES.AddTerm(-1.0 * maxValue_epsilon[j], U[j][v]);
                string constraint_name = "Departure_SOC_From_ES_" + j.ToString();
                allConstraints_list.Add(AddEq(DepartureSOCFromES, 0.0, constraint_name));
            }
        }


        public List<Tuple<int,int,int>> GetXVariablesSetTo1()
        {
            if (solutionStatus != XCPlexSolutionStatus.Optimal)
                return null;
            List < Tuple <int, int, int>> outcome = new List<Tuple<int, int, int>>();
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.NumVehicleCategories; v++)
                        if (GetValue(X[i][j][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            outcome.Add(new Tuple<int, int, int>(nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j], v));
            return outcome;
        }

        public override NewCompleteSolution GetCompleteSolution()
        {
            return new NewCompleteSolution(problemModel, GetXVariablesSetTo1());
        }

        protected override void AddTheObjectiveFunction()
        {
            throw new NotImplementedException();
        }

        protected override void AddAllConstraints()
        {
            throw new NotImplementedException();
        }
    }
}
