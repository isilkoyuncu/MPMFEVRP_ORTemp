using ILOG.Concert;
using MPMFEVRP.Domains.AlgorithmDomain;
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
    public class XCPlex_NodeDuplicatingFormulation : XCPlexVRPBase
    {
        int numDuplicatedNodes;
          
        Site theDepot;

        int firstESNodeIndex, lastESNodeIndex, firstCustomerNodeIndex, lastCustomerNodeIndex;//There is a single depot and it is always node #0

        INumVar[][][] X; double[][][] X_LB, X_UB;
        INumVar[][] U; double[][] U_LB, U_UB;
        INumVar[] T;
        INumVar[] Delta;
        INumVar[] Epsilon;
        public XCPlex_NodeDuplicatingFormulation() { }
        public XCPlex_NodeDuplicatingFormulation(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam){}

        protected override void DefineDecisionVariables()
        {
            DuplicateAndOrganizeSites();
            SetMinAndMaxValuesOfCommonVariables();
            SetMinAndMaxValuesOfModelSpecificVariables();
            minValue_Delta = SetDeltaMinViaLabelSetting(maxValue_Epsilon);
            maxValue_Delta = SetDeltaMaxViaLabelSetting(maxValue_Epsilon);

            allVariables_list = new List<INumVar>();
            //dvs: X_ijv and U_jv
            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numDuplicatedNodes, numDuplicatedNodes, numVehCategories,out X);
            AddTwoDimensionalDecisionVariable("U", U_LB, U_UB, NumVarType.Int, numDuplicatedNodes, numVehCategories, out U);
            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numDuplicatedNodes, out T);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numDuplicatedNodes, out Delta);
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numDuplicatedNodes, out Epsilon);
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXVariablesTo0();
        }
        void DuplicateAndOrganizeSites()
        {
            Site[] originalSites = theProblemModel.SRD.GetAllSitesArray();
            int numCustomers = theProblemModel.SRD.NumCustomers;
            int numDuplicationsForeachES = theProblemModel.Lambda * theProblemModel.NumVehicles[0];
            numDuplicatedNodes = 1 + (numDuplicationsForeachES * theProblemModel.SRD.NumES) + numCustomers;
            preprocessedSites = new Site[numDuplicatedNodes];
            depots = new List<Site>();
            customers = new List<Site>();
            externalStations = new List<Site>();
            int nodeCounter = 0;
            foreach(Site s in originalSites)
            {
                switch (s.SiteType)
                {
                    case SiteTypes.Depot:
                        preprocessedSites[nodeCounter++] = s;
                        depots.Add(s);
                        break;
                    case SiteTypes.ExternalStation:
                        if (firstESNodeIndex == 0)
                            firstESNodeIndex = nodeCounter;
                        for (int i = 0; i < numDuplicationsForeachES; i++)
                        {
                            preprocessedSites[nodeCounter++] = s;
                            externalStations.Add(s);
                        }
                        lastESNodeIndex = nodeCounter - 1;
                        break;
                    case SiteTypes.Customer:
                        if (firstCustomerNodeIndex == 0)
                            firstCustomerNodeIndex = nodeCounter;
                        lastCustomerNodeIndex = nodeCounter;
                        preprocessedSites[nodeCounter++] = s;
                        customers.Add(s);
                        break;
                    default:
                        throw new System.Exception("Site type incompatible!");
                }
            }
            theDepot = depots[0];
        }
        void SetUndesiredXVariablesTo0()
        {
            //No arc from a node to itself
            for (int j = 0; j < numDuplicatedNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                    X[j][j][v].UB = 0.0;
            //No arc from one ES to another
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X[i][j][v].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > 1
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        if (EnergyConsumption(sFrom, sTo, vehicleCategories[v]) > 1)
                            X[i][j][v].UB = 0.0;
                }
            }
            //No arc from depot to its duplicate
            for (int v = 0; v < numVehCategories; v++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    if((preprocessedSites[j].X==theDepot.X)&&(preprocessedSites[j].Y== theDepot.Y))//Comparing X and Y coordinates to those of the depot makes sures that the ES at hand corresponds to the one at the depot!
                    {
                        X[0][j][v].UB = 0.0;
                        X[j][0][v].UB = 0.0;
                    }
            //No arc from or to an ES node can be traversed by a GDV
            for (int i = 0; i < numDuplicatedNodes; i++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < numVehCategories; v++)
                        if (vehicleCategories[v] == VehicleCategories.GDV)
                        {
                            X[i][j][v].UB = 0.0;
                            X[j][i][v].UB = 0.0;
                        }
        }
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {
            X_LB = new double[numDuplicatedNodes][][];
            X_UB = new double[numDuplicatedNodes][][];
            U_LB = new double[numDuplicatedNodes][];
            U_UB = new double[numDuplicatedNodes][];

            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                X_LB[i] = new double[numDuplicatedNodes][];
                X_UB[i] = new double[numDuplicatedNodes][];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site s = preprocessedSites[j];
                    X_LB[i][j] = new double[numVehCategories];
                    X_UB[i][j] = new double[numVehCategories];
                    U_LB[j] = new double[numVehCategories];
                    U_UB[j] = new double[numVehCategories];
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X_LB[i][j][v] = 0.0;
                        X_UB[i][j][v] = 1.0;
                        if (s.SiteType == SiteTypes.Depot)
                            U_UB[j][v] = theProblemModel.GetNumVehicles(vehicleCategories[v]);
                        else
                            U_UB[j][v] = 1.0;
                        U_LB[j][v] = 0.0;
                    }
               }
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
            if (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                AddMaxTypeObjectiveFunction();
            else
                AddMinTypeObjectiveFunction();
        }
        void AddMaxTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: prize collection
            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
            {
                Site s = preprocessedSites[j];
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(Prize(s, vehicleCategories[v]), U[j][v]);
            }
            //Second term: distance-based costs
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Third term: vehicle fixed costs
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(-1.0 * GetVehicleFixedCost(vehicleCategories[v]), U[0][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //Second term: distance-based costs
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Third term: vehicle fixed costs
            for (int j = 0; j < numDuplicatedNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);            
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
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                    for (int i = 0; i < numDuplicatedNodes; i++)
                        IncomingXTotalMinusU.AddTerm(1.0, X[i][0][v]);
                    IncomingXTotalMinusU.AddTerm(-1.0, U[0][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_depot_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_OutgoingXfromDepotTotalEqualsU()
        {
            for (int v = 0; v < numVehCategories; v++)
            {
                ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                for (int k = 0; k < numDuplicatedNodes; k++)
                    OutgoingXTotalMinusU.AddTerm(1.0, X[0][k][v]);
                OutgoingXTotalMinusU.AddTerm(-1.0, U[0][v]);
                string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_depot_equals_the_U_variable";
                allConstraints_list.Add(AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
            }
        }
        void AddConstraint_IncomingXTotalEqualsU()
        {
            for(int j=1; j < numDuplicatedNodes; j++)
                for(int v=0; v < numVehCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                    for (int i = 0; i < numDuplicatedNodes; i++)
                        IncomingXTotalMinusU.AddTerm(1.0, X[i][j][v]);
                    IncomingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_OutgoingXTotalEqualsU()
        {
            for (int j = 1; j < numDuplicatedNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                    for (int k = 0; k < numDuplicatedNodes; k++)
                        OutgoingXTotalMinusU.AddTerm(1.0, X[j][k][v]);
                    OutgoingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                    allConstraints_list.Add(AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
                }
        }
        void AddConstraint_NumberOfVisitsPerNode()
        {
            for (int j = 1; j < numDuplicatedNodes; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < numVehCategories; v++)
                    NumberOfVehiclesVisitingTheNode.AddTerm(1.0, U[j][v]);

                string constraint_name;

                if ((theProblemModel.CoverConstraintType == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)&&(j >= firstCustomerNodeIndex) && (j <= lastCustomerNodeIndex))
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
                for (int v = 0; v < numVehCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.GDV)
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
                numVehicles = theProblemModel.NumVehicles;
            }
            for (int v = 0; v < numVehCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                for (int k = 0; k < numDuplicatedNodes; k++)
                    NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][k][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_node_0_cannot_exceed_" + numVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, numVehicles[v], constraint_name));
            }
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()
        {
            for (int i = firstCustomerNodeIndex; i <= lastCustomerNodeIndex; i++)
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < numVehCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom,sTo) + (maxValue_T[i] - minValue_T[j])), X[i][j][v]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                }
        }
        void AddConstraint_TimeRegulationFollowingAnESVisit() // TODO make sure all the other constraints are the same as formulation
        {
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < numVehCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (TravelTime(sFrom, sTo) + (maxValue_T[i] - minValue_T[j] + 1.0 / RechargingRate(sFrom))), X[i][j][v]);
                    // Here we decide whether recharging duration is fixed or depends on the arrival SOC
                    if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                    {
                        // If the recharging duration is fixed, then it is enough for RHS to be Tmax 
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                    }
                    else
                    {
                        // If the recharging duration depends on the arrival SOC, then we need to add the possible maximum duration to the RHS
                        TimeDifference.AddTerm(-1.0 / RechargingRate(sFrom), Epsilon[i]);
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j] + 1.0 / RechargingRate(sFrom)), constraint_name));
                    }
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()
        {
            for (int i = 1; i < numDuplicatedNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, Delta[j]);
                    SOCDifference.AddTerm(-1.0, Delta[i]);
                    SOCDifference.AddTerm(-1.0, Epsilon[i]);
                    for (int v = 0; v < numVehCategories; v++)
                        if (vehicleCategories[v] == VehicleCategories.EV)
                            SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, vehicleCategories[v]) + maxValue_Delta[j] - minValue_Delta[i], X[i][j][v]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, maxValue_Delta[j] - minValue_Delta[i], constraint_name));
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()
        {
            for (int j = 0; j < numDuplicatedNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
                        SOCDifference.AddTerm(EnergyConsumption(theDepot, sTo, vehicleCategories[v]), X[0][j][v]);
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, 1.0, constraint_name));
            }
        }
        void AddConstraint_MaxRechargeAtCustomerNode()
        {
            for (int j = firstCustomerNodeIndex; j <=lastCustomerNodeIndex ; j++)
            {
                ILinearNumExpr RechargeAtCustomer = LinearNumExpr();
                RechargeAtCustomer.AddTerm(1.0, Epsilon[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
                        RechargeAtCustomer.AddTerm(-1.0 * maxValue_Epsilon[j], U[j][v]);
                string constraint_name = "Max_Recharge_At_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(RechargeAtCustomer, 0.0, constraint_name));
            }
        }
        void AddConstraint_MaxDepartureSOCFromCustomerNode()
        {
            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
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
                DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                DepartureSOCFromES.AddTerm(1.0, Epsilon[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
                        DepartureSOCFromES.AddTerm(-1.0 * maxValue_Epsilon[j], U[j][v]);
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

        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            foreach (VehicleCategories vc in vehicleCategories)
                outcome.AddRange(GetVehicleSpecificRoutes(vc));
            return outcome;
        }
        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory)
        {
            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);//Pulling the vehicle infor from the Problem Model. Not exactly flexible, but works as long as we have only two categories of vehicles and no more than one of each
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            foreach (List<string> nonDepotSiteIDs in GetListsOfNonDepotSiteIDs(vehicleCategory))
                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs));
            return outcome;
        }
        List<List<string>> GetListsOfNonDepotSiteIDs(VehicleCategories vehicleCategory)
        {
            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

            //We first determine the route start points
            List<int> firstSiteIndices = new List<int>();
            for (int j = 0; j < numDuplicatedNodes; j++)
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
            do
            {
                outcome.Add(currentSiteID);
                nextSiteIndex = GetNextSiteIndex(currentSiteIndex, vc_int);
                if (preprocessedSites[nextSiteIndex].ID == theDepot.ID)
                {
                    return outcome;
                }
                currentSiteIndex = nextSiteIndex;
                currentSiteID = preprocessedSites[currentSiteIndex].ID;
            }
            while (currentSiteID != theDepot.ID);

            return outcome;

        }
        public override void RefineDecisionVariables(CustomerSet CS)
        {
            int VCIndex = (int)xCplexParam.VehCategory;
            
            for (int j=firstCustomerNodeIndex; j<=lastCustomerNodeIndex; j++)
            {
                if(CS.Customers.Contains(preprocessedSites[j].ID))
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
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        int GetNextSiteIndex(int currentSiteIndex, int vc_int)
        {
            for (int nextSiteIndex = 0; nextSiteIndex < numDuplicatedNodes; nextSiteIndex++)
                if (GetValue(X[currentSiteIndex][nextSiteIndex][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    return nextSiteIndex;
                }
            throw new System.Exception("Flow ended before returning to the depot!");
        }

        public override string GetModelName()
        {
            return "Node Duplicating";
        }
    }
}
