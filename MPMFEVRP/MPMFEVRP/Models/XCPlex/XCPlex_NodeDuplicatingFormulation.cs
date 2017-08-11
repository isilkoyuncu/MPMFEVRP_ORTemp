using System;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Interfaces;


namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_NodeDuplicatingFormulation : XCPlexBase
    {
        RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        int numNodes;
        int[] nodeToOriginalSiteNumberMap;
        int firstESNodeIndex, lastESNodeIndex, firstCustomerNodeIndex, lastCustomerNodeIndex;//There is a single depot and it is always node #0
        double[] minValue_T, maxValue_T, minValue_delta, maxValue_delta, minValue_epsilon, maxValue_epsilon;

        INumVar[][][] X;
        INumVar[][] U;
        INumVar[] T;
        INumVar[] delta;
        INumVar[] epsilon;

        public XCPlex_NodeDuplicatingFormulation(ProblemModelBase problemModel, XCPlexParameters xCplexParam)
            : base(problemModel, xCplexParam)
        {
            rechargingDuration_status = problemModel.RechargingDuration_status;
        }
        protected override void DefineDecisionVariables()
        {
            DuplicateAndMapNodes();
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
                    X_name[i][j] = new string[problemModel.VRD.NumVehicleCategories];
                    X[i][j] = new INumVar[problemModel.VRD.NumVehicleCategories];
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
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
                U_name[j] = new string[problemModel.VRD.NumVehicleCategories];
                U[j] = new INumVar[problemModel.VRD.NumVehicleCategories];
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    U_name[j][v] = "U_(" + j.ToString() + "," + v.ToString() + ")";
                    U[j][v] = NumVar(0, GetUpperBound(j, v), variable_type, U_name[j][v]);
                    allVariables_list.Add(U[j][v]);
                }//for v
            }//for j
            //auxiliaries (T, delta, epsilon)
            SetMinAndMaxValuesOfAuxiliaryVariables();
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
            SetUndesiredXVariablesTo0();
        }
        void DuplicateAndMapNodes()
        {
            int numCustomers = problemModel.SRD.NumCustomers;
            int numES = problemModel.CRD.Lambda * problemModel.VRD.NumVehicles[0] * problemModel.SRD.NumES;
            numNodes = 1 + numCustomers + numES;

            nodeToOriginalSiteNumberMap = new int[numNodes];
            int nodeCounter = 0;
            for (int orgSiteIndex = 0; orgSiteIndex < problemModel.SRD.SiteArray.Length; orgSiteIndex++)
            {
                switch (problemModel.SRD.SiteArray[orgSiteIndex].SiteType)
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
                        for (int i = 0; i < problemModel.CRD.Lambda * problemModel.VRD.NumVehicles[0]; i++)
                            nodeToOriginalSiteNumberMap[nodeCounter++] = orgSiteIndex;
                        lastESNodeIndex = nodeCounter - 1;
                        break;
                    default:
                        throw new System.Exception("Site type incompatible!");
                }
            }
        }
        int GetUpperBound(int j, int v)
        {
            if (j == 0)
                return problemModel.VRD.NumVehicles[v];
            else
                return 1;
        }
        void SetUndesiredXVariablesTo0()
        {
            //No arc from a node to itself
            for (int j = 0; j < numNodes; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    X[j][j][v].UB = 0.0;
            //No arc from one ES to another
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    {
                        X[i][j][v].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > 1
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                        {
                            if (EnergyConsumption(i,j,v) > 1)
                                X[i][j][v].UB = 0.0;
                        }
            //No arc from depot to its duplicate
            for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    if (nodeToOriginalSiteNumberMap[j] == 1)
                    {
                        X[0][j][v].UB = 0.0;
                        X[j][0][v].UB = 0.0;
                    }
            //No arc between a non-ES node and an ES node can be traversed by a GDV
            for (int i = 0; i < numNodes; i++)
            {
                if (problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[i]].SiteType == SiteTypes.ExternalStation)
                    continue;
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.GDV)
                        {
                            X[i][j][v].UB = 0.0;
                            X[j][i][v].UB = 0.0;
                        }
            }
        }
        void SetMinAndMaxValuesOfAuxiliaryVariables()
        {
            minValue_T = new double[numNodes];
            maxValue_T = new double[numNodes];
            minValue_delta = new double[numNodes];
            maxValue_delta = new double[numNodes];
            minValue_epsilon = new double[numNodes];
            maxValue_epsilon = new double[numNodes];

            for (int j = 0; j < numNodes; j++)
            {
                minValue_T[j] = TravelTime(0,j);
                maxValue_T[j] = problemModel.CRD.TMax - TravelTime(j,0);
                if (problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[j]].SiteType == SiteTypes.Customer)
                    maxValue_T[j] -= ServiceDuration(j);

                //TODO Fine-tune the min and max values of delta
                minValue_delta[j] = 0.0;
                maxValue_delta[j] = 1.0;

                minValue_epsilon[j] = 0.0;
                if (problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[j]].SiteType == SiteTypes.Customer)
                    maxValue_epsilon[j] = Math.Min(1.0, ServiceDuration(j) * Math.Min(RechargingRate(j),problemModel.VRD.VehicleArray[0].MaxChargingRate)/problemModel.VRD.VehicleArray[0].BatteryCapacity);//TODO: Use the utility function instead!
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
            if (problemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                AddMaxTypeObjectiveFunction();
            else
                AddMinTypeObjectiveFunction();
        }
        void AddMaxTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: prize collection
            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    objFunction.AddTerm(Prize(j,v), U[j][v]);
            //Second term: distance-based costs
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        objFunction.AddTerm(-1.0 * problemModel.VRD.VehicleArray[v].VariableCostPerMile * Distance(i,j), X[i][j][v]);
            //Third term: vehicle fixed costs
            for (int j = 0; j < numNodes; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    objFunction.AddTerm(-1.0 * problemModel.VRD.VehicleArray[v].FixedCost, X[0][j][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //Second term: distance-based costs
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        objFunction.AddTerm(problemModel.VRD.VehicleArray[v].VariableCostPerMile * Distance(i,j), X[i][j][v]);
            //Third term: vehicle fixed costs
            for (int j = 0; j < numNodes; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    objFunction.AddTerm(problemModel.VRD.VehicleArray[v].FixedCost, X[0][j][v]);
            //Now adding the objective function to the model
            AddMinimize(objFunction);
        }

        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_IncomingXtoDepotTotalEqualsU();
            AddConstraint_OutgoingXfromDepotTotalEqualsU();
            AddConstraint_IncomingXTotalEqualsU();
            AddConstraint_OutgoingXTotalEqualsU();
            AddConstraint_NumberOfVisitsPerNode();
            AddConstraint_NoGDVVisitToESNodes();
            AddConstraint_MaxNumberOfVehiclesPerCategory();
            AddConstraint_TimeRegulationFollowingACustomerVisit();
            AddConstraint_TimeRegulationFollowingAnESVisit();

            AddConstraint_SOCRegulationFollowingNondepot();
            AddConstraint_DepartureSOCFromESNode();

            AddConstraint_SOCRegulationFollowingDepot();
            AddConstraint_MaxRechargeAtCustomerNode();
            AddConstraint_MaxDepartureSOCFromCustomerNode();
            
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_IncomingXtoDepotTotalEqualsU()
        {
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
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
            for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
            {
                ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                for (int k = 0; k < numNodes; k++)
                    OutgoingXTotalMinusU.AddTerm(1.0, X[0][k][v]);
                OutgoingXTotalMinusU.AddTerm(-1.0, U[0][v]);
                string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_depot_equals_the_U_variable";
                allConstraints_list.Add(AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
            }
        }
        void AddConstraint_IncomingXTotalEqualsU()
        {
            for(int j=1; j < numNodes; j++)
                for(int v=0; v < problemModel.VRD.NumVehicleCategories; v++)
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
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                    for (int k = 0; k < numNodes; k++)
                        OutgoingXTotalMinusU.AddTerm(1.0, X[j][k][v]);
                    OutgoingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_NumberOfVisitsPerNode()
        {
            for (int j = 1; j < numNodes; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    NumberOfVehiclesVisitingTheNode.AddTerm(1.0, U[j][v]);

                string constraint_name;

                if ((problemModel.CoverConstraintType == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)&&(j >= firstCustomerNodeIndex) && (j <= lastCustomerNodeIndex))
                {
                    constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
                    allConstraints_list.Add(AddEq(NumberOfVehiclesVisitingTheNode, 1.0, constraint_name));
                }
                else
                {
                    constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheNode, 1.0, constraint_name));
                }
            }
        }
        void AddConstraint_NoGDVVisitToESNodes()
        {
            for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
            {
                ILinearNumExpr NumberOfGDVsVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.GDV)
                        NumberOfGDVsVisitingTheNode.AddTerm(1.0, U[j][v]);
                string constraint_name = "No_GDV_can_visit_the_ES_node_" + j.ToString();
                allConstraints_list.Add(AddEq(NumberOfGDVsVisitingTheNode, 0.0, constraint_name));
            }
        }
        void AddConstraint_MaxNumberOfVehiclesPerCategory()
        {
            int[] numVehicles;
            if (xCplexParam.TSP)
            {
                switch (xCplexParam.VehCategory)
                {
                    case VehicleCategories.EV:
                        numVehicles = new int[] { 1, 0 };
                        break;
                    case VehicleCategories.GDV:
                        numVehicles = new int[] { 0, 1 };
                        break;
                    default:
                        throw new System.Exception("Vehicle Category unrecognized!!!");
                }
            }
            else//not TSP
            {
                numVehicles = problemModel.VRD.NumVehicles;
            }
            for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                for (int k = 0; k < numNodes; k++)
                    NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][k][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_node_0_cannot_exceed_" + numVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, numVehicles[v], constraint_name));
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
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(i) + TravelTime(i,j) + (maxValue_T[i] - minValue_T[j])), X[i][j][v]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                }
        }
        void AddConstraint_TimeRegulationFollowingAnESVisit() // TODO make sure all the other constraints are the same as formulation
        {
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (TravelTime(i,j)+ (maxValue_T[i] - minValue_T[j] + 1.0/RechargingRate(i))), X[i][j][v]);
                    // Here we decide whether recharging duration is fixed or depends on the arrival SOC
                    if (rechargingDuration_status==RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                    {
                        // If the recharging duration is fixed, then it is enough for RHS to be Tmax 
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                    }
                    else
                    {
                        // If the recharging duration depends on the arrival SOC, then we need to add the possible maximum duration to the RHS
                        TimeDifference.AddTerm(-1.0 / RechargingRate(i), epsilon[i]);
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j] + 1.0 / RechargingRate(i)), constraint_name));
                    }
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
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                            SOCDifference.AddTerm(EnergyConsumption(i,j,v) + maxValue_delta[j] - minValue_delta[i], X[i][j][v]);
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
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                        SOCDifference.AddTerm(EnergyConsumption(0, j, v), X[0][j][v]);
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
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
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
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
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
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                        DepartureSOCFromES.AddTerm(-1.0 * maxValue_epsilon[j], U[j][v]);
                string constraint_name = "Departure_SOC_From_ES_" + j.ToString();
                if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
                {
                    allConstraints_list.Add(AddLe(DepartureSOCFromES, 0.0, constraint_name));
                }
                else
                {
                    allConstraints_list.Add(AddEq(DepartureSOCFromES, 0.0, constraint_name));
                }
            }
        }

        public List<Tuple<int,int,int>> GetXVariablesSetTo1()
        {
            //if (solutionStatus != XCPlexSolutionStatus.Optimal)
            //    return null;
            List < Tuple <int, int, int>> outcome = new List<Tuple<int, int, int>>();
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (GetValue(X[i][j][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            outcome.Add(new Tuple<int, int, int>(nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j], v));
            return outcome;
        }
        public List<double> GetDeltaVariables()
        {
            if (solutionStatus != XCPlexSolutionStatus.Optimal)
                return null;
            List<double> outcome = new List<double>();
            for (int i = 0; i < numNodes; i++)
                outcome.Add(GetValue(delta[i]));
            return outcome;
        }

        public void RefineDecisionVariables(CustomerSet CS)
        {
            int VCIndex = (int)xCplexParam.VehCategory;
            
            for (int j=firstCustomerNodeIndex; j<=lastCustomerNodeIndex; j++)
            {
                if(CS.Customers.Contains(problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[j]].ID))
                {
                    U[j][VCIndex].LB = 1.0;
                    U[j][VCIndex].UB = 1.0;
                    U[j][1 - VCIndex].LB = 0.0;
                    U[j][1 - VCIndex].UB = 0.0;
                }
                else
                {
                    U[j][VCIndex].LB = 0.0;
                    U[j][VCIndex].UB = 0.0;
                    U[j][1 - VCIndex].LB = 0.0;
                    U[j][1 - VCIndex].UB = 0.0;
                }
            }
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(problemModel, GetXVariablesSetTo1());
        }

        double Distance(int i, int j)
        {
            return problemModel.SRD.Distance[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j]];
        }
        double EnergyConsumption(int i, int j, int v)
        {
            return problemModel.SRD.EnergyConsumption[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j], v];
        }
        double TravelTime(int i, int j)
        {
            return problemModel.SRD.TimeConsumption[nodeToOriginalSiteNumberMap[i], nodeToOriginalSiteNumberMap[j]];
        }
        double ServiceDuration(int j)
        {
            return problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[j]].ServiceDuration;
        }
        double RechargingRate(int j)
        {
            return problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[j]].RechargingRate;
        }
        double Prize(int j, int v)
        {
            return problemModel.SRD.SiteArray[nodeToOriginalSiteNumberMap[j]].Prize[v];
        }
    }
}
