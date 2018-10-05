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
    public class XCPlex_NodeDuplicatingFormulation_woU : XCPlexVRPBase
    {
        int numCustomers, numES;

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        //DVs, upper and lower bounds
        INumVar[][][] X; double[][][] X_LB, X_UB;

        //Auxiliaries, upper and lower bounds
        INumVar[] Epsilon;
        INumVar[] Delta;
        INumVar[] T;

        int firstCustomerVisitationConstraintIndex = -1;
        int totalTravelTimeConstraintIndex = -1;

        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

        IndividualRouteESVisits singleRouteESvisits;
        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

        public XCPlex_NodeDuplicatingFormulation_woU() { } //Empty constructor
        public XCPlex_NodeDuplicatingFormulation_woU(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam)
        {
            if (xCplexParam.TSP)//This is just pre-cautionary, the right input for a TSP model should already specify "ExactlyOnce"
                customerCoverageConstraint = CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce;
        } //XCPlex VRP Constructor
        public XCPlex_NodeDuplicatingFormulation_woU(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
        }

        protected override void DefineDecisionVariables()
        {
            PreprocessSites(theProblemModel.Lambda * theProblemModel.GetNumVehicles(VehicleCategories.EV));//lambda*nEVs duplicates of each ES node, and a single copy of each other node will be places in the array preprocessedSWAVs
            numCustomers = theProblemModel.SRD.NumCustomers;
            numES = theProblemModel.SRD.NumES;

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
            Rig_IKEMH10();
        }
        void Rig_IKEMH10()
        {
            X[0][12 + 24][0].LB = 1.0;
            X[12 + 24][6 + 24][0].LB = 1.0;
            X[6 + 24][3 + 24][0].LB = 1.0;
            X[3 + 24][1 + 24][0].LB = 1.0;
            X[1 + 24][16 + 24][0].LB = 1.0;
            X[16 + 24][19 + 24][0].LB = 1.0;
            X[19 + 24][0][0].LB = 1.0;

            X[0][14 + 24][0].LB = 1.0;
            X[14 + 24][7 + 24][0].LB = 1.0;
            X[7 + 24][11 + 24][0].LB = 1.0;
            X[11 + 24][17 + 24][0].LB = 1.0;
            X[17 + 24][10 + 24][0].LB = 1.0;
            X[10 + 24][0][0].LB = 1.0;

            X[0][15 + 24][0].LB = 1.0;
            X[15 + 24][18 + 24][0].LB = 1.0;
            X[18 + 24][4 + 24][0].LB = 1.0;
            X[4 + 24][5 + 24][0].LB = 1.0;
            X[5 + 24][13 + 24][0].LB = 1.0;
            X[13 + 24][0][0].LB = 1.0;

            X[0][24][0].LB = 1.0;
            X[24][2 + 24][0].LB = 1.0;
            X[2 + 24][20 + 24][0].LB = 1.0;
            X[20 + 24][8 + 24][0].LB = 1.0;
            X[8 + 24][9 + 24][0].LB = 1.0;
            X[9 + 24][0][0].LB = 1.0;
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
        /// <summary>
        /// Returns 0, 1, or 2 based on the comparison of two YArcs
        /// </summary>
        /// <param name="nonES1"></param>
        /// <param name="nonES2"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>1 if the first one dominates, 2 if the second dominates, 0 if both are nondominated</returns>
        int Dominates(int nonES1, int nonES2, int r1, int r2)
        {
            Site from = preprocessedSites[nonES1];
            Site to = preprocessedSites[nonES2];
            Site ES1 = preprocessedSites[r1];
            Site ES2 = preprocessedSites[r2];
            bool ES1isNotDominated = false;
            bool ES2isNotDominated = false;

            //Who has the shortest first leg is not dominated
            int sign = Math.Sign(Distance(from, ES1) - Distance(from, ES2));
            if (sign != 0)
            {
                if (sign == -1)
                    ES1isNotDominated = true;
                else
                    ES2isNotDominated = true;
            }

            //Who has the shortest second leg is not dominated
            sign = Math.Sign(Distance(ES1, to) - Distance(ES2, to));
            if (sign != 0)
            {
                if (sign == -1)
                    ES1isNotDominated = true;
                else
                    ES2isNotDominated = true;
            }

            if (ES1isNotDominated && ES2isNotDominated)
                return 0;

            //Who has the shortest overall distance is not dominated
            sign = Math.Sign(Distance(from, ES1) + Distance(ES1, to) - Distance(from, ES2) - Distance(ES2, to));
            if (sign != 0)
            {
                if (sign == -1)
                    ES1isNotDominated = true;
                else
                    ES2isNotDominated = true;
            }

            if (ES1isNotDominated && ES2isNotDominated)
                return 0;

            //Who has the overall shortest time including FF refuel is not dominated
            sign = Math.Sign(TravelTime(from, ES1) + TravelTime(ES1, to) + BatteryCapacity(VehicleCategories.EV) / ES1.RechargingRate - (TravelTime(from, ES2) + TravelTime(ES2, to) + BatteryCapacity(VehicleCategories.EV) / ES2.RechargingRate));
            if (sign != 0)
            {
                if (sign == -1)
                    ES1isNotDominated = true;
                else
                    ES2isNotDominated = true;
            }

            if (ES1isNotDominated && ES2isNotDominated)
                return 0;
            if (ES1isNotDominated)
                return 1;
            else
                return 2;
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
            objective = AddMaximize(objFunction);
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
            objective = AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            //AddConstraint_EliminateDominatedESVisits();
            AddConstraint_NumberOfVisitsPerCustomerNode();//1
            AddConstraint_NumberOfEvVisitsPerESNode();//2
            AddConstraint_NoGDVVisitToESNodes();//3
            AddConstraint_IncomingXTotalEqualsOutgoingXTotal();//4

            if ((xCplexParam.TSP) || (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT))//This is the case for both TSP and Orienteering models encountered. For the broader model, we don't impose any constraints. MinimizeVMT objective check is made to simply understand the EMH problems.
                AddConstraint_MaxNumberOfGDVs();

            if (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
            {
                AddConstraint_MaxNumberOfEVs();//5
                AddConstraint_MaxNumberOfGDVs();
            }
            else //Minimize
            {
                AddConstraint_MinNumberOfVehicles();//5 b
            }

            AddConstraint_MaxEnergyGainAtNonDepotSite();//6

            if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
                AddConstraint_DepartureSOCFromNode_PartialRecharging();//7-8
            else
            {
                AddConstraint_DepartureSOCFromCustomerNode();//7
                AddConstraint_DepartureSOCFromESNode_FullRecharging();//8
            }

            AddConstraint_SOCRegulationFollowingNondepot();//9
            AddConstraint_SOCRegulationFollowingDepot();//10
            AddConstraint_TimeRegulationFollowingACustomerVisit();//11

            if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                AddConstraint_TimeRegulationFollowingAnESVisit_FixedRechargingDuration();//12
            else
                AddConstraint_TimeRegulationFollowingAnESVisit_VariableRechargingDuration();//12

            AddConstraint_ArrivalTimeLimits();
            AddConstraint_TotalTravelTime();

            //Some additional cuts
            //AddAllCuts();

            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_EliminateDominatedESVisits()
        {
            //Between two ESs, check for domination and kill the dominated one
            for (int i = 0; i < NumPreprocessedSites; i++)
                for (int j = 0; j < NumPreprocessedSites; j++)
                    if (preprocessedSites[i].SiteType != SiteTypes.ExternalStation && preprocessedSites[j].SiteType != SiteTypes.ExternalStation)
                        for (int r1 = FirstESNodeIndex; r1 <= LastESNodeIndex; r1++)
                        {
                            if (X[i][r1][vIndex_EV].UB + X[r1][j][vIndex_EV].UB <= 1.0)
                                continue;
                            for (int r2 = FirstESNodeIndex; r2 <= LastESNodeIndex; r2++)
                            {
                                if (r2 == r1)
                                    continue;
                                if (X[i][r2][vIndex_EV].UB + X[r2][j][vIndex_EV].UB <= 1.0)
                                    continue;
                                ILinearNumExpr EliminateDominatedESVisits = LinearNumExpr();
                                string constraint_name="";
                                int dom = Dominates(i, j, r1, r2);
                                if (dom == 1)
                                {
                                    EliminateDominatedESVisits.AddTerm(1.0, X[i][r2][vIndex_EV]);
                                    EliminateDominatedESVisits.AddTerm(1.0, X[r2][j][vIndex_EV]);
                                    constraint_name = "The_ES_node_" + r2.ToString() + "_is_dominated_by_ES_node_" + r1.ToString() + "_from_node_"+ i.ToString() + "_to_node_" + j.ToString();
                                    allConstraints_list.Add(AddLe(EliminateDominatedESVisits, 1.0, constraint_name));
                                }
                                if (dom == 2)
                                {
                                    EliminateDominatedESVisits.AddTerm(1.0, X[i][r1][vIndex_EV]);
                                    EliminateDominatedESVisits.AddTerm(1.0, X[r1][j][vIndex_EV]);
                                    constraint_name = "The_ES_node_" + r1.ToString() + "_is_dominated_by_ES_node_" + r2.ToString() + "_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                                    allConstraints_list.Add(AddLe(EliminateDominatedESVisits, 1.0, constraint_name));
                                }
                                
                                EliminateDominatedESVisits.Clear();
                            }
                        }
            
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

                switch (customerCoverageConstraint)
                {
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
                        constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
                        allConstraints_list.Add(AddEq(NumberOfVehiclesVisitingTheCustomerNode, 1.0, constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
                        constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheCustomerNode, 1.0, constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode invoked for CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce, which must not happen for a VRP!");
                    default:
                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
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
        void AddConstraint_MaxNumberOfEVs()//5
        {
            ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < NumPreprocessedSites; j++)
                NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][j][vIndex_EV]);
            string constraint_name = "Number_of_Vehicles_of_category_" + vIndex_EV.ToString() + "_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_EV].ToString();
            allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, numVehicles[vIndex_EV], constraint_name));
            NumberOfVehiclesPerCategoryOutgoingFromTheDepot.Clear();
        }
        void AddConstraint_MaxNumberOfGDVs()//5
        {
            ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < NumPreprocessedSites; j++)
                NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][j][vIndex_GDV]);
            string constraint_name = "Number_of_Vehicles_of_category_" + vIndex_GDV.ToString() + "_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_GDV].ToString();
            allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, numVehicles[vIndex_GDV], constraint_name));
            NumberOfVehiclesPerCategoryOutgoingFromTheDepot.Clear();
        }
        void AddConstraint_MinNumberOfVehicles() //4-5 b
        {
            if (numVehicles.Sum() < minNumVeh)
                return;
            ILinearNumExpr NumberOfVehiclesOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < NumPreprocessedSites; j++)
            {
                for (int v = 0; v < numVehCategories; v++)
                    NumberOfVehiclesOutgoingFromTheDepot.AddTerm(1.0, X[0][j][v]);
            }
            string constraint_name = "Number_of_vehicles_outgoing_from_node_0_must_be_greater_than_" + (minNumVeh).ToString();
            allConstraints_list.Add(AddGe(NumberOfVehiclesOutgoingFromTheDepot, minNumVeh, constraint_name));
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
        void AddConstraint_TimeRegulationFollowingDepot()//000
        {
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sFrom = preprocessedSites[0];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    for (int v = 0; v < numVehCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (TravelTime(sFrom, sTo) + BigT[0][j]), X[0][j][v]);
                    string constraint_name = "Time_Regulation_from_depot_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j], constraint_name));
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
            if (customerCoverageConstraint != CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
                rhs -= theProblemModel.SRD.GetTotalCustomerServiceTime();
            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));
            TotalTravelTime.Clear();
        }

        void AddAllCuts()
        {
            AddCut_TimeFeasibilityOfTwoConsecutiveArcs();
            AddCut_EnergyFeasibilityOfCustomerBetweenTwoES();
            AddCut_TotalNumberOfActiveArcs();
        }
        void AddCut_TimeFeasibilityOfTwoConsecutiveArcs()
        {
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site from = preprocessedSites[i];
                for (int k = 0; k < NumPreprocessedSites; k++)
                {
                    Site through = preprocessedSites[k];
                    if (X[i][k][0].UB == 0.0)//The direct arc (from,through) has already been marked infeasible
                        continue;
                    for (int j = 0; j < NumPreprocessedSites; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (X[k][j][0].UB == 0.0)//The direct arc (through,to) has already been marked infeasible
                            continue;
                        if (i != j && j != k && i != k)
                            if (from.SiteType == SiteTypes.Customer && through.SiteType == SiteTypes.Customer && to.SiteType == SiteTypes.Customer)
                            {
                                ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                                if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
                                {
                                    for (int v = 0; v < numVehCategories; v++)
                                    {
                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][v]);
                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][v]);
                                    }
                                    
                                    string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                                    allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                    TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                }
                                else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                                {
                                    for (int r1 = FirstESNodeIndex; r1 <= LastESNodeIndex; r1++)
                                    {
                                        Site ES1 = preprocessedSites[r1];
                                        double fixedChargeTimeAtES1 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES1.RechargingRate;
                                        if (fixedChargeTimeAtES1 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//Not even one visit through ES is allowed
                                        {
                                            for (int v = 0; v < numVehCategories; v++)
                                            {
                                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][v]);
                                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][v]);
                                            }
                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(0.5, X[i][r1][vIndex_EV]);
                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(0.5, X[r1][k][vIndex_EV]);
                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(0.5, X[k][r1][vIndex_EV]);
                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(0.5, X[r1][j][vIndex_EV]);
                                            string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ES_" + r1.ToString();
                                            allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.5, constraint_name));
                                            TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                        }
                                        else
                                        {
                                            for (int r2 = FirstESNodeIndex; r2 <= LastESNodeIndex; r2++)
                                                if (r2 != r1)
                                                {
                                                    Site ES2 = preprocessedSites[r2];
                                                    double fixedChargeTimeAtES2 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES2.RechargingRate;
                                                    if (fixedChargeTimeAtES1 + fixedChargeTimeAtES2 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//ES1 was fine by itself but not together with ES2
                                                    {
                                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][r1][vIndex_EV]);
                                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[r1][k][vIndex_EV]);
                                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][r2][vIndex_EV]);
                                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[r2][j][vIndex_EV]);
                                                        string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ESs_" + r1.ToString() + "_and_" + r2.ToString();
                                                        allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 3.0, constraint_name));
                                                        TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                                    }
                                                }
                                        }
                                    }
                                }
                            }
                    }
                }
            }
        }
        void AddCut_EnergyFeasibilityOfCustomerBetweenTwoES()
        {
            for (int r1 = FirstESNodeIndex; r1 <= LastESNodeIndex; r1++)
            {
                Site ES1 = preprocessedSites[r1];
                for (int j = FirstCustomerNodeIndex; j <= LastCustomerNodeIndex; j++)
                {
                    Site customer = preprocessedSites[j];
                    for (int r2 = FirstESNodeIndex; r2 <= LastESNodeIndex; r2++)
                    {
                        Site ES2 = preprocessedSites[r2];
                        if (EnergyConsumption(ES1, customer, VehicleCategories.EV) + EnergyConsumption(customer, ES2, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) + maxValue_Epsilon[j])//This didn't have the maxValue_Epsilon in it and hence it ignored the SOE gain at ISs
                        {
                            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();

                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[r1][j][vIndex_EV]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[j][r2][vIndex_EV]);

                            string constraint_name = "No_arc_from_ES_node_" + r1.ToString() + "_through_customer_" + j.ToString() + "to_ES_node_" + r2.ToString();
                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
                        }

                    }
                }
            }
        }
        void AddCut_TotalNumberOfActiveArcs()
        {
            totalNumberOfActiveArcsConstraintIndex = allConstraints_list.Count;

            int nActiveArcs = numVehicles[0]* theProblemModel.Lambda + numVehicles[1] + numCustomers;
            int nActiveArcs_EV = numVehicles[vIndex_EV] * theProblemModel.Lambda + numCustomers;
            int nActiveArcs_GDV = numVehicles[vIndex_GDV] + numCustomers;
            ILinearNumExpr totalArcFlow = LinearNumExpr();
            ILinearNumExpr totalArcFlow_EV = LinearNumExpr();
            ILinearNumExpr totalArcFlow_GDV = LinearNumExpr();
            for (int i = 0; i <NumPreprocessedSites; i++)
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    for (int v = 0; v < 2; v++)
                        totalArcFlow.AddTerm(1.0, X[i][j][v]);
                    if (preprocessedSites[i].SiteType == SiteTypes.ExternalStation || preprocessedSites[j].SiteType == SiteTypes.ExternalStation)
                    {
                        totalArcFlow_EV.AddTerm(1.0, X[i][j][vIndex_EV]);
                    }
                    else
                    {
                        totalArcFlow_GDV.AddTerm(1.0, X[i][j][vIndex_GDV]);
                        totalArcFlow_EV.AddTerm(1.0, X[i][j][vIndex_EV]);
                    }
                }
            string constraintName_overall = "Total_number_of_active_arcs_cannot_exceed_" + nActiveArcs.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow, (double)nActiveArcs, constraintName_overall));
            string constraintName_EV = "Number_of_active_EV_arcs_cannot_exceed_" + nActiveArcs_EV.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow_EV, (double)nActiveArcs_EV, constraintName_EV));
            string constraintName_GDV = "Number_of_active_GDV_arcs_cannot_exceed_" + nActiveArcs_GDV.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow_GDV, (double)nActiveArcs_GDV, constraintName_GDV));
        }

        void AddCut_EnergyFeasibilityOfTwoConsecutiveArcs()
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
        void AddCut_EnergyConservation()
        {
            ILinearNumExpr EnergyConservation = LinearNumExpr();
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    EnergyConservation.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV), X[i][j][vIndex_EV]);
                    EnergyConservation.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][vIndex_EV]);
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
            List<double> Tvalues = new List<double>();
            List<double> DeltaValues = new List<double>();
            List<double> EpsilonValues = new List<double>();

            for (int i = 0; i < NumPreprocessedSites; i++)
                for (int j = 0; j < NumPreprocessedSites; j++)
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        allX.Add(preprocessedSites[i].ID + "," + preprocessedSites[j].ID + "," + v.ToString() + "->" + GetValue(X[i][j][v]).ToString());
                        if (GetValue(X[i][j][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            activeX.Add(preprocessedSites[i].ID + "," + preprocessedSites[j].ID + "," + v.ToString()+"->"+ GetValue(X[i][j][v]).ToString());
                    }
            for (int j = 1; j < NumPreprocessedSites; j++)
            {
                Tvalues.Add(GetValue(T[j]));
                DeltaValues.Add(GetValue(Delta[j]));
                EpsilonValues.Add(GetValue(Epsilon[j]));
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

        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            Remove(objective);
            AddMinCostObjectiveFunction(customerCoverageConstraintShadowPrices);
        }
        void AddMinCostObjectiveFunction(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: distance-based costs
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        if (sFrom.SiteType == SiteTypes.Customer)
                            objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo) - customerCoverageConstraintShadowPrices[sFrom.ID], X[i][j][v]);
                        else
                            objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Second term: vehicle fixed costs
            for (int j = 0; j < NumPreprocessedSites; j++)
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);

            //Now adding the objective function to the model
            objective = AddMinimize(objFunction);
        }

    }
}
