﻿using ILOG.Concert;
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
    //TODO get all sites here and then use the mapping we coded

    public class XCPlex_NodeDuplicatingFormulation : XCPlexBase
    {
        RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        int numDuplicatedNodes;
        int numVehCategories;
        double[] minValue_T, maxValue_T, minValue_delta, maxValue_delta, minValue_epsilon, maxValue_epsilon;
        Site theDepot;

        //TODO: Make sure that we don't need the following anymore, and then delete them
        int firstESNodeIndex, lastESNodeIndex, firstCustomerNodeIndex, lastCustomerNodeIndex;//There is a single depot and it is always node #0

        INumVar[][][] X;
        INumVar[][] U;
        INumVar[] T;
        INumVar[] delta;
        INumVar[] epsilon;

        public XCPlex_NodeDuplicatingFormulation(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam)
        {
            rechargingDuration_status = theProblemModel.RechargingDuration_status;
            numVehCategories = base.theProblemModel.VRD.NumVehicleCategories;
            if (numVehCategories <= vehicleCategories.Length) { throw new System.Exception("XCPlex_NodeDuplicatingFormulation number of VehicleCategories are different than problemModel.VRD.NumVehicleCategories"); }
        }
        protected override void DefineDecisionVariables()
        {
            DuplicateAndOrganizeSites();
            SetMinAndMaxValuesOfAuxiliaryVariables();//TODO: Expand this (and rename) to all variables, not just the aux

            allVariables_list = new List<INumVar>();
            //TODO avoid using array definition of decision variables, use individual definitions with string names generated JIT and then delete the loops below.
            AddThreeDimensionalDecisionVariable("X", 0.0, 1.0, NumVarType.Int, numDuplicatedNodes, numDuplicatedNodes, numVehCategories);
            AddTwoDimensionalDecisionVariable("U", 0.0, 1.0, NumVarType.Int, numDuplicatedNodes, numVehCategories);
            AddOneDimensionalDecisionVariable("T", 0.0, 1.0, NumVarType.Int, numDuplicatedNodes);
            AddOneDimensionalDecisionVariable("Delta", 0.0, 1.0, NumVarType.Int, numDuplicatedNodes);
            AddOneDimensionalDecisionVariable("Epsilon", 0.0, 1.0, NumVarType.Int, numDuplicatedNodes);

            /*****************************************************************************************/
            ////X
            //string[][][] X_name = new string[numDuplicatedNodes][][];
            //X = new INumVar[numDuplicatedNodes][][];
            //for (int i = 0; i < numDuplicatedNodes; i++)
            //{
            //    X_name[i] = new string[numDuplicatedNodes][];
            //    X[i] = new INumVar[numDuplicatedNodes][];
            //    for (int j = 0; j < numDuplicatedNodes; j++)
            //    {
            //        X_name[i][j] = new string[numVehCategories];
            //        X[i][j] = new INumVar[numVehCategories];
            //        for (int v = 0; v < numVehCategories; v++)
            //        {
            //            X_name[i][j][v] = "X_(" + i.ToString() + "," + j.ToString() + "," + v.ToString() + ")";
            //            X[i][j][v] = NumVar(0, 1, variable_type, X_name[i][j][v]);
            //            allVariables_list.Add(X[i][j][v]);
            //        }//for v
            //    }//for j
            //}//for i
            ///*****************************************************************************************/
            ////U
            //string[][] U_name = new string[numDuplicatedNodes][];
            //U = new INumVar[numDuplicatedNodes][];
            //for (int j = 0; j < numDuplicatedNodes; j++)
            //{
            //    U_name[j] = new string[numVehCategories];
            //    U[j] = new INumVar[numVehCategories];
            //    for (int v = 0; v < numVehCategories; v++)
            //    {
            //        U_name[j][v] = "U_(" + j.ToString() + "," + v.ToString() + ")";
            //        if (originalSites[j].SiteType == SiteTypes.Depot)
            //            //TODO U upper bound: if depot num vehicles ow 1.
            //            U[j][v] = NumVar(0, theProblemModel.GetNumVehicles(vehicleCategories[v]), variable_type, U_name[j][v]);
            //        else
            //            U[j][v] = NumVar(0, 1, variable_type, U_name[j][v]);
            //        allVariables_list.Add(U[j][v]);
            //    }//for v
            //}//for j
            // /*****************************************************************************************/

            ////auxiliaries (T, delta, epsilon)

            //T = new INumVar[numDuplicatedNodes];
            //string[] T_name = new string[numDuplicatedNodes];
            //for (int j = 0; j < numDuplicatedNodes; j++)
            //{
            //    T_name[j] = "T_(" + j.ToString() + ")";
            //    T[j] = NumVar(minValue_T[j], maxValue_T[j], NumVarType.Float, T_name[j]);
            //    allVariables_list.Add(T[j]);
            //}
            //delta = new INumVar[numDuplicatedNodes];
            //string[] delta_name = new string[numDuplicatedNodes];
            //for (int j = 0; j < numDuplicatedNodes; j++)
            //{
            //    delta_name[j] = "delta_(" + j.ToString() + ")";
            //    delta[j] = NumVar(minValue_delta[j], maxValue_delta[j], NumVarType.Float, delta_name[j]);
            //    allVariables_list.Add(delta[j]);
            //}
            //epsilon = new INumVar[numDuplicatedNodes];
            //string[] epsilon_name = new string[numDuplicatedNodes];
            //for (int j = 0; j < numDuplicatedNodes; j++)
            //{
            //    epsilon_name[j] = "epsilon_(" + j.ToString() + ")";
            //    epsilon[j] = NumVar(minValue_epsilon[j], maxValue_epsilon[j], NumVarType.Float, epsilon_name[j]);
            //    allVariables_list.Add(epsilon[j]);
            //}
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXVariablesTo0();
        }
        void DuplicateAndOrganizeSites()
        {
            Site[] originalSites = theProblemModel.SRD.GetAllSitesArray();
            int numCustomers = theProblemModel.SRD.NumCustomers;
            int numDuplicationsForeachES = theProblemModel.CRD.Lambda * theProblemModel.NumVehicles[0];
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
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                    X[j][j][v].UB = 0.0;
            //No arc from one ES to another
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                    for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                        if (EnergyConsumption(sFrom, sTo, vehicleCategories[v]) > 1)
                            X[i][j][v].UB = 0.0;
                }
            }
            //No arc from depot to its duplicate
            for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    if((preprocessedSites[j].X==theDepot.X)&&(preprocessedSites[j].Y== theDepot.Y))//Comparing X and Y coordinates to those of the depot makes sures that the ES at hand corresponds to the one at the depot!
                    {
                        X[0][j][v].UB = 0.0;
                        X[j][0][v].UB = 0.0;
                    }
            //No arc from or to an ES node can be traversed by a GDV
            for (int i = 0; i < numDuplicatedNodes; i++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                        if (vehicleCategories[v] == VehicleCategories.GDV)
                        {
                            X[i][j][v].UB = 0.0;
                            X[j][i][v].UB = 0.0;
                        }
        }
        void SetMinAndMaxValuesOfAuxiliaryVariables()
        {
            minValue_T = new double[numDuplicatedNodes];
            maxValue_T = new double[numDuplicatedNodes];
            minValue_delta = new double[numDuplicatedNodes];
            maxValue_delta = new double[numDuplicatedNodes];
            minValue_epsilon = new double[numDuplicatedNodes];
            maxValue_epsilon = new double[numDuplicatedNodes];

            for (int j = 0; j < numDuplicatedNodes; j++)
            {
                Site s = preprocessedSites[j];
                minValue_T[j] = TravelTime(theDepot, s);
                maxValue_T[j] = theProblemModel.CRD.TMax - TravelTime(s, theDepot);
                if(s.SiteType == SiteTypes.Customer)
                    maxValue_T[j] -= ServiceDuration(s);

                //TODO Fine-tune the min and max values of delta
                minValue_delta[j] = 0.0;
                maxValue_delta[j] = 1.0;

                minValue_epsilon[j] = 0.0;
                //TODO: Use the utility function instead!
                //if(s.SiteType == SiteTypes.Customer)
                //    maxValue_epsilon[j] = Math.Min(1.0, ServiceDuration(j) * Math.Min(RechargingRate(j),theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate)/theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity);
                //else
                //maxValue_epsilon[j] = 1.0;
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
            for (int j = 0; j < numDuplicatedNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(-1.0 * GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);
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
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
            for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                for(int v=0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
            for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                        TimeDifference.AddTerm(-1.0 / RechargingRate(sFrom), epsilon[i]);
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
                    SOCDifference.AddTerm(1.0, delta[j]);
                    SOCDifference.AddTerm(-1.0, delta[i]);
                    SOCDifference.AddTerm(-1.0, epsilon[i]);
                    for (int v = 0; v < numVehCategories; v++)
                        if (vehicleCategories[v] == VehicleCategories.EV)
                            SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, vehicleCategories[v]) + maxValue_delta[j] - minValue_delta[i], X[i][j][v]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, maxValue_delta[j] - minValue_delta[i], constraint_name));
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()
        {
            for (int j = 0; j < numDuplicatedNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, delta[j]);
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                RechargeAtCustomer.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
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
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                DepartureSOCFromES.AddTerm(1.0, delta[j]);
                DepartureSOCFromES.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
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

        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
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
            int vc = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

            //We first determine the route start points
            List<string> firstSites = new List<string>();
            for (int j = 0; j < numDuplicatedNodes; j++)
                if (GetValue(X[0][j][vc]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                    firstSites.Add(preprocessedSites[j].ID);
            //Then, populate the whole routes (actually, not routes yet)
            List<List<string>> outcome = new List<List<string>>();
            foreach (string firstSiteID in firstSites)
                outcome.Add(GetNonDepotSiteIDs(firstSiteID, vehicleCategory));
            return outcome;
        }
        List<string> GetNonDepotSiteIDs(string firstSiteID, VehicleCategories vehicleCategory)
        {
            int vc = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
            List<string> outcome = new List<string>();
            for (int j=0;j<numDuplicatedNodes;j++)
                if(preprocessedSites[j].ID== firstSiteID)
                {
                    if (GetValue(X[0][j][vc]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                    {
                        throw new System.Exception("XCPlex_NodeDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteID,vehicleCategory) pair that doesn't correspond to a flow from the depot!");
                    }
                    string currentSiteID = firstSiteID;
                    int currentSiteIndex = j;
                    string nextSiteID = "";
                    do
                    {
                        outcome.Add(currentSiteID);
                        for(int nextSiteIndex = 0; nextSiteIndex<numDuplicatedNodes;nextSiteIndex++)
                            if(GetValue(X[currentSiteIndex][nextSiteIndex][vc])>= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            {
                                currentSiteIndex = nextSiteIndex;
                                currentSiteID = preprocessedSites[currentSiteIndex].ID;
                                continue;
                            }
                        if (currentSiteID == outcome.Last())
                            throw new System.Exception("Flow ended before returning to the depot!");
                    }
                    while (nextSiteID != theDepot.ID);
                }
            return outcome;

        }

        public void RefineDecisionVariables(CustomerSet CS)
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
            throw new NotImplementedException();
            //return new RouteBasedSolution(theProblemModel, GetXVariablesSetTo1());
        }

        double Distance(Site from, Site to)
        {
            return theProblemModel.SRD.GetDistance(from.ID, to.ID);
        }
        double EnergyConsumption(Site from, Site to, VehicleCategories vehicleCategory)
        {
            if (vehicleCategory == VehicleCategories.GDV)
                return 0;
            return theProblemModel.SRD.GetEVEnergyConsumption(from.ID, to.ID);
        }
        double TravelTime(Site from, Site to)
        {
            return theProblemModel.SRD.GetTravelTime(from.ID, to.ID);
        }
        double ServiceDuration(Site site)
        {
            return site.ServiceDuration;
        }
        double RechargingRate(Site site)
        {
            return site.RechargingRate;
        }
        double Prize(Site site, VehicleCategories vehicleCategory)
        {
            return site.GetPrize(vehicleCategory);
        }

        double GetVarCostPerMile(VehicleCategories vehicleCategory)
        {
            return theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).VariableCostPerMile;
        }
        double GetVehicleFixedCost(VehicleCategories vehicleCategory)
        {
            return theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).FixedCost;
        }
    }
}
