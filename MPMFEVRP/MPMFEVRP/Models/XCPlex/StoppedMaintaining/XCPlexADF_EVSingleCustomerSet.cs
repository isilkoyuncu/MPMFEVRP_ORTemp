﻿//using ILOG.Concert;
//using MPMFEVRP.Domains.ProblemDomain;
//using MPMFEVRP.Domains.SolutionDomain;
//using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.Solutions;
//using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
//using MPMFEVRP.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace MPMFEVRP.Models.XCPlex
//{
//    public class XCPlexADF_EVSingleCustomerSet : XCPlexVRPBase
//    {
//        int numNonESNodes;
//        int numCustomers, numES;

//        int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
//        int totalTravelTimeConstraintIndex = -1;
//        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

//        bool addTotalNumberOfActiveArcsCut = false;

//        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

//        INumVar[][] X; double[][] X_LB, X_UB;
//        INumVar[][][] Y; double[][][] Y_LB, Y_UB;
//        INumVar[] Epsilon;
//        INumVar[] Delta;
//        INumVar[] T;

//        protected double[][][] BigTVarRecharge;

//        IndividualRouteESVisits singleRouteESvisits;
//        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

//        public XCPlexADF_EVSingleCustomerSet() { }
     
//        public XCPlexADF_EVSingleCustomerSet(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
//            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
//        {
//        }

//        protected override void DefineDecisionVariables()
//        {
//            PreprocessSites();
//            numCustomers = theProblemModel.SRD.NumCustomers;
//            numNonESNodes = numCustomers + 1;
//            numES = theProblemModel.SRD.NumES;
//            SetMinAndMaxValuesOfModelSpecificVariables();

//            allVariables_list = new List<INumVar>();

//            //dvs: X_ijv and Y_irj
//            AddTwoDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, out X);
//            AddThreeDimensionalDecisionVariable("Y", Y_LB, Y_UB, NumVarType.Int, numNonESNodes, numES, numNonESNodes, out Y);

//            //auxiliaries (T_j, Delta_j, Epsilon_j)
//            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out T);
//            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numNonESNodes, out Delta);
//            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numNonESNodes, out Epsilon);

//            //All variables defined
//            allVariables_array = allVariables_list.ToArray();
//            //Now we need to set some to the variables to 0
//            SetUndesiredXYVariablesTo0();
//        }

//        void SetUndesiredXYVariablesTo0()
//        {
//            //TODO: Why do we have to set the following here? Wouldn't they more naturally fit wherever we calculate other auxiliary variable bounds?
//            T[0].LB = theProblemModel.CRD.TMax;
//            T[0].UB = theProblemModel.CRD.TMax;
//            Epsilon[0].LB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
//            Epsilon[0].UB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;

//            //NewPerspectiveOnArcDominationAndAuxiliaryVariableBounds();

//            //No arc from a node to itself
//            for (int j = 0; j < numNonESNodes; j++)
//            {
//                    X[j][j].UB = 0.0;

//                for (int r = 0; r < numES; r++)
//                    Y[j][r][j].UB = 0.0;
//            }
//            //No arc from depot to its duplicate
//            for (int j = 0; j < numNonESNodes; j++)
//                for (int r = 0; r < numES; r++)
//                    if ((ExternalStations[r].X == TheDepot.X) && (ExternalStations[r].Y == TheDepot.Y))//Comparing X and Y coordinates to those of the depot makes sures that the ES at hand corresponds to the one at the depot!
//                    {
//                        Y[0][r][j].UB = 0.0;
//                        Y[j][r][0].UB = 0.0;
//                    }
//            //No arc from a node to another if energy consumption is > capacity
//            for (int i = 0; i < numNonESNodes; i++)
//            {
//                Site sFrom = preprocessedSites[i];
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site sTo = preprocessedSites[j];
//                    if (EnergyConsumption(sFrom, sTo, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV))
//                        X[i][j].UB = 0.0;
//                }
//            }

//            //No arc from a node to an ES or from an ES to a node if energy consumption is > capacity
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int r = 0; r < numES; r++)
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sFrom = preprocessedSites[i];
//                        Site ES = ExternalStations[r];
//                        Site sTo = preprocessedSites[j];
//                        if (EnergyConsumption(sFrom, ES, VehicleCategories.EV) > Math.Min(maxValue_Delta[i] + maxValue_Epsilon[i], BatteryCapacity(VehicleCategories.EV)) ||
//                            EnergyConsumption(ES, sTo, VehicleCategories.EV) > theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity)
//                            Y[i][r][j].UB = 0.0;
//                    }

//            //No YArc if it's not more beneficial
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int j = 0; j < numNonESNodes; j++)
//                    if (X[i][j].UB == 1.0)//The arc has not been eliminated for some other reason
//                    {
//                        Site sFrom = preprocessedSites[i];
//                        Site sTo = preprocessedSites[j];
//                        double directEnergyConsumption = EnergyConsumption(sFrom, sTo, VehicleCategories.EV);
//                        for (int r = 0; r < numES; r++)
//                            if (Y[i][r][j].UB == 1.0)//The arc has not been eliminated for some other reason
//                            {
//                                Site ES = ExternalStations[r];
//                                double toESEnergyConsumption = EnergyConsumption(sFrom, ES, VehicleCategories.EV);
//                                if (toESEnergyConsumption > directEnergyConsumption)
//                                {
//                                    double directArrivalSOE_wMinDelta = minValue_Delta[i] + maxValue_Epsilon[i] - directEnergyConsumption;
//                                    double fromESArrivalSOE = BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(ES, sTo, VehicleCategories.EV);
//                                    if (directArrivalSOE_wMinDelta >= fromESArrivalSOE) //No benefit in going through ES
//                                        Y[i][r][j].UB = 0.0;
//                                }
//                            }
//                    }

//            //Between two YArcs, check for domination and kill the dominated one
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int j = 0; j < numNonESNodes; j++)
//                    for (int r1 = 0; r1 < numES; r1++)
//                    {
//                        if (Y[i][r1][j].UB == 0.0)
//                            continue;
//                        for (int r2 = 0; r2 < numES; r2++)
//                        {
//                            if (r2 == r1)
//                                continue;
//                            if (Y[i][r2][j].UB == 0.0)
//                                continue;
//                            int dom = Dominates(i, j, r1, r2);
//                            if (dom == 1)
//                                Y[i][r2][j].UB = 0.0;
//                            if (dom == 2)
//                                Y[i][r1][j].UB = 0.0;
//                        }
//                    }
//        }       
//        /// <summary>
//        /// Returns 0, 1, or 2 based on the comparison of two YArcs
//        /// </summary>
//        /// <param name="nonES1"></param>
//        /// <param name="nonES2"></param>
//        /// <param name="r1"></param>
//        /// <param name="r2"></param>
//        /// <returns>1 if the first one dominates, 2 if the second dominates, 0 if both are nondominated</returns>
//        int Dominates(int nonES1, int nonES2, int r1, int r2)
//        {
//            Site from = preprocessedSites[nonES1];
//            Site to = preprocessedSites[nonES2];
//            Site ES1 = ExternalStations[r1];
//            Site ES2 = ExternalStations[r2];
//            bool ES1isNotDominated = false;
//            bool ES2isNotDominated = false;

//            //Who has the shortest first leg is not dominated
//            int sign = Math.Sign(Distance(from, ES1) - Distance(from, ES2));
//            if (sign != 0)
//            {
//                if (sign == -1)
//                    ES1isNotDominated = true;
//                else
//                    ES2isNotDominated = true;
//            }

//            //Who has the shortest second leg is not dominated
//            sign = Math.Sign(Distance(ES1, to) - Distance(ES2, to));
//            if (sign != 0)
//            {
//                if (sign == -1)
//                    ES1isNotDominated = true;
//                else
//                    ES2isNotDominated = true;
//            }

//            if (ES1isNotDominated && ES2isNotDominated)
//                return 0;

//            //Who has the overall shortest time including FF refuel is not dominated
//            sign = Math.Sign(TravelTime(from, ES1) + TravelTime(ES1, to) + BatteryCapacity(VehicleCategories.EV) / ES1.RechargingRate - (TravelTime(from, ES2) + TravelTime(ES2, to) + BatteryCapacity(VehicleCategories.EV) / ES2.RechargingRate));
//            if (sign != 0)
//            {
//                if (sign == -1)
//                    ES1isNotDominated = true;
//                else
//                    ES2isNotDominated = true;
//            }

//            if (ES1isNotDominated && ES2isNotDominated)
//                return 0;
//            if (ES1isNotDominated)
//                return 1;
//            else
//                return 2;
//        }
//        void SetMinAndMaxValuesOfModelSpecificVariables()
//        {
//            X_LB = new double[numNonESNodes][];
//            X_UB = new double[numNonESNodes][];
//            Y_LB = new double[numNonESNodes][][];
//            Y_UB = new double[numNonESNodes][][];
//            BigTVarRecharge = new double[numNonESNodes][][];

//            RHS_forNodeCoverage = new double[numNonESNodes];

//            for (int i = 0; i < numNonESNodes; i++)
//            {
//                Y_LB[i] = new double[numES][];
//                Y_UB[i] = new double[numES][];
//                BigTVarRecharge[i] = new double[numES][];

//                for (int r = 0; r < numES; r++)
//                {
//                    Y_LB[i][r] = new double[numNonESNodes];
//                    Y_UB[i][r] = new double[numNonESNodes];
//                    BigTVarRecharge[i][r] = new double[numNonESNodes];
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Y_LB[i][r][j] = 0.0;
//                        Y_UB[i][r][j] = 1.0;
//                        BigTVarRecharge[i][r][j] = maxValue_T[i] - minValue_T[j] + TravelTime(preprocessedSites[i], ExternalStations[r]) + TravelTime(ExternalStations[r], preprocessedSites[j]) + (BatteryCapacity(VehicleCategories.EV) + EnergyConsumption(preprocessedSites[i], ExternalStations[r], VehicleCategories.EV)) / RechargingRate(ExternalStations[r]);
//                    }
//                }
//                RHS_forNodeCoverage[i] = 1.0;
//            }

//            for (int i = 0; i < numNonESNodes; i++)
//            {
//                X_LB[i] = new double[numNonESNodes];
//                X_UB[i] = new double[numNonESNodes];
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    X_LB[i][j] = 0.0;
//                    X_UB[i][j] = 1.0;
//                }
//            }
//        }
//        public override string GetDescription_AllVariables_Array()
//        {
//            return
//                "for (int i = 0; i <= numCustomers; i++)\nfor (int j = 0; j <= numCustomers; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nX[i][j][v]"
//                + "then\n"
//                + "for (int i = 0; i <= numCustomers; i++)\nfor (int r = 0; r < numES; r++)\nfor (int j = 0; j <= numCustomers; j++)\nY[i][r][j]"
//                + "then\n"
//                + "for (int j = 0; j <= numCustomers; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nU[j][v]\n"
//                + "then\n"
//                + "for (int j = 0; j <= numCustomers; j++)\nT[j]\n"
//                + "then\n"
//                + "for (int j = 0; j <= numCustomers; j++)\ndelta[j]\n"
//                + "then\n"
//                + "for (int j = 0; j <= numCustomers; j++)\nepsilon[j]\n";
//        }
//        protected override void AddTheObjectiveFunction()
//        {
//            if (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
//                AddMaxTypeObjectiveFunction();
//            else
//                AddMinTypeObjectiveFunction();
//        }
//        void AddMaxTypeObjectiveFunction()
//        {
//            ILinearNumExpr objFunction = LinearNumExpr();
//            //First term: prize collection
//            for (int j = 0; j < numNonESNodes; j++)
//            {
//                Site s = preprocessedSites[j];
//                if (s.SiteType == SiteTypes.Customer)
//                    for (int i = 0; i < numNonESNodes; i++)
//                    {
//                        objFunction.AddTerm(Prize(s, vehicleCategories[vIndex_EV]), X[i][j]);
//                        for (int r = 0; r < numES; r++)
//                            objFunction.AddTerm(Prize(s, vehicleCategories[vIndex_EV]), Y[i][r][j]);
//                    }
//            }
//            //Second term Part I: distance-based costs from customer to customer directly
//            for (int i = 0; i < numNonESNodes; i++)
//            {
//                Site sFrom = preprocessedSites[i];
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site sTo = preprocessedSites[j];
//                    objFunction.AddTerm(-1.0 * GetVarCostPerMile(vehicleCategories[vIndex_EV]) * Distance(sFrom, sTo), X[i][j]);
//                }
//            }
//            //Second term Part II: distance-based costs from customer to customer through an ES
//            for (int i = 0; i < numNonESNodes; i++)
//            {
//                Site sFrom = preprocessedSites[i];
//                for (int r = 0; r < numES; r++)
//                {
//                    Site ES = ExternalStations[r];
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sTo = preprocessedSites[j];
//                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES) + Distance(ES, sTo)), Y[i][r][j]);
//                    }
//                }
//            }
//            //Third term: vehicle fixed costs
//            for (int j = 0; j < numNonESNodes; j++)
//            {
//                    objFunction.AddTerm(-1.0 * GetVehicleFixedCost(vehicleCategories[vIndex_EV]), X[0][j]);
//                for (int r = 0; r < numES; r++)
//                    objFunction.AddTerm(-1.0 * GetVehicleFixedCost(vehicleCategories[vIndex_EV]), Y[0][r][j]);
//            }
//            //Now adding the objective function to the model
//            objective = AddMaximize(objFunction);
//        }
//        void AddMinTypeObjectiveFunction()
//        {
//            ILinearNumExpr objFunction = LinearNumExpr();
//            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT)//TODO: This code was written just to save the day, must be reconsidered in relation to the problem model's objective function calculation method
//            {
//                //Second term Part I: distance-based costs from customer to customer directly
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    Site sFrom = preprocessedSites[i];
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sTo = preprocessedSites[j];
//                        objFunction.AddTerm(Distance(sFrom, sTo), X[i][j]);
//                    }
//                }
//                //Second term Part II: distance-based costs from customer to customer through an ES
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    Site sFrom = preprocessedSites[i];
//                    for (int r = 0; r < numES; r++)
//                    {
//                        Site ES = ExternalStations[r];
//                        for (int j = 0; j < numNonESNodes; j++)
//                        {
//                            Site sTo = preprocessedSites[j];
//                            objFunction.AddTerm(Distance(sFrom, ES) + Distance(ES, sTo), Y[i][r][j]);
//                        }
//                    }
//                }
//            }
//            else
//            {
//                //Second term Part I: distance-based costs from customer to customer directly
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    Site sFrom = preprocessedSites[i];
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sTo = preprocessedSites[j];
//                            objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[vIndex_EV]) * Distance(sFrom, sTo), X[i][j]);
//                    }
//                }
//                //Second term Part II: distance-based costs from customer to customer through an ES
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    Site sFrom = preprocessedSites[i];
//                    for (int r = 0; r < numES; r++)
//                    {
//                        Site ES = ExternalStations[r];
//                        for (int j = 0; j < numNonESNodes; j++)
//                        {
//                            Site sTo = preprocessedSites[j];
//                            objFunction.AddTerm(GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES) + Distance(ES, sTo)), Y[i][r][j]);
//                        }
//                    }
//                }
//                //Third term: vehicle fixed costs
//                for (int j = 0; j < numNonESNodes; j++)
//                        objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[vIndex_EV]), X[0][j]);
//            }
//            //Now adding the objective function to the model
//            objective = AddMinimize(objFunction);
//        }
//        protected override void AddAllConstraints()
//        {
//            allConstraints_list = new List<IRange>();
//            //Now adding the constraints one (family) at a time
//            AddConstraint_NumberOfVisitsPerCustomerNode();//1
//            AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV();//2
//            AddConstraint_DepartingNumberOfEVs();
//            AddConstraint_MaxEnergyGainAtNonDepotSite();//6
//            AddConstraint_DepartureSOCFromCustomerNode();//7
//            AddConstraint_DepartureSOCFromESNodeUB();//8
//            AddConstraint_ArrivalSOCToESNodeLB();//9b
//            AddConstraint_SOCRegulationFollowingNondepot();//10
//            AddConstraint_SOCRegulationFollowingDepot();//11
//            AddConstraint_TimeRegulationFollowingACustomerVisit();//12
//            if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
//            {
//                AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13c
//            }
//            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
//            {
//                AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13
//                AddConstraint_DepartureSOCFromESNodeLB();//9
//            }
//            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
//            {
//                AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging();//13a
//                AddConstraint_DepartureSOCFromESNodeLB();//9
//            }
//            AddConstraint_TimeRegulationFromDepotThroughAnESVisit();//13c
//            AddConstraint_ArrivalTimeLimits();//14
//            AddConstraint_TotalTravelTime();//15

//            addTotalNumberOfActiveArcsCut = false;
//            if(addTotalNumberOfActiveArcsCut)
//                AddCut_TotalNumberOfActiveArcs();

//            //Some additional cuts
//            //AddAllCuts();

//            AddCut_SymmetryBreak();

//            //All constraints added
//            allConstraints_array = allConstraints_list.ToArray();
//        }
//        void AddCut_SymmetryBreak()
//        {
//            ILinearNumExpr SymmetryBreak = LinearNumExpr();
//            for (int i = 1; i < NumPreprocessedSites; i++)
//            {
//                Site site = preprocessedSites[i];
//                SymmetryBreak.AddTerm(1.0 * Distance(site, TheDepot), X[i][0]);
//                SymmetryBreak.AddTerm(-1.0 * Distance(TheDepot, site), X[0][i]);
//                for (int r = 0; r < numES; r++)
//                {
//                    Site through = ExternalStations[r];
//                    SymmetryBreak.AddTerm(1.0 * (Distance(site, through) + Distance(through, TheDepot)), Y[i][r][0]);
//                    SymmetryBreak.AddTerm(-1.0 * (Distance(TheDepot, through) + Distance(through, site)), Y[0][r][i]);
//                }
//            }
//            string constraint_name = "Symmetry break";
//            allConstraints_list.Add(AddGe(SymmetryBreak, 0.0, constraint_name));
//            SymmetryBreak.Clear();
//        }
//        void AddConstraint_NumberOfVisitsPerCustomerNode() //1
//        {
//            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

//            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
//            {
//                ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j]);

//                    for (int r = 0; r < numES; r++)
//                        IncomingXandYToCustomerNodes.AddTerm(1.0, Y[i][r][j]);
//                }
//                string constraint_name;

//                switch (customerCoverageConstraint)
//                {
//                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
//                        constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
//                        allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
//                        break;
//                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
//                        constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
//                        allConstraints_list.Add(AddLe(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
//                        break;
//                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
//                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode invoked for CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce, which must not happen for a VRP!");
//                    default:
//                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
//                }

//            }
//        }
//        void AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV()//2
//        {
//            for (int j = 0; j < numNonESNodes; j++)
//            {
                
//                    ILinearNumExpr IncomingXYTotalMinusOutgoingXYTotal = LinearNumExpr();

//                    for (int i = 0; i < numNonESNodes; i++)
//                    {
                        
//                            IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, X[i][j]);
//                            IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, X[j][i]);
//                            for (int r = 0; r < numES; r++)
//                            {
//                                IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, Y[i][r][j]);
//                                IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, Y[j][r][i]);
//                            }
                        
//                    }
//                    string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
//                    allConstraints_list.Add(AddEq(IncomingXYTotalMinusOutgoingXYTotal, 0.0, constraint_name));
               
//            }
//        }
//        void AddConstraint_DepartingNumberOfEVs()//4
//        {
//            ILinearNumExpr NumberOfEVsOutgoingFromTheDepot = LinearNumExpr();
//            for (int j = 1; j < numNonESNodes; j++)
//            {
                
//                    NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j]);
//                    for (int r = 0; r < numES; r++)
//                        NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, Y[0][r][j]);
                
//            }
//            if (customerCoverageConstraint == CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
//            {
//                string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_1";
//                allConstraints_list.Add(AddLe(NumberOfEVsOutgoingFromTheDepot, 1.0, constraint_name));
//            }
//            else if (customerCoverageConstraint == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
//            {
//                string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_1";
//                allConstraints_list.Add(AddEq(NumberOfEVsOutgoingFromTheDepot, 1.0, constraint_name));
//            }
//            else
//                throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
//        }
//        void AddConstraint_MaxEnergyGainAtNonDepotSite()//6
//        {
//            for (int j = 1; j < numNonESNodes; j++)
//            {
                
//                    ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
//                    EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
//                    for (int i = 0; i < numNonESNodes; i++)
//                    {
                        
//                            EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j]);
//                            for (int r = 0; r < numES; r++)
//                                EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], Y[i][r][j]);
                        
//                    }
//                    string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
//                    allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));
                
//            }
//        }
//        void AddConstraint_DepartureSOCFromCustomerNode()//7
//        {
//            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
//            {
                
//                    ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
//                    DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
//                    DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
//                    for (int i = 0; i < numNonESNodes; i++)
//                    {
                        
//                            DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j]);
//                            for (int r = 0; r < numES; r++)
//                                DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), Y[i][r][j]);
                        
//                    }
//                    string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
//                    allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));
                
//            }
//        }
//        void AddConstraint_DepartureSOCFromESNodeUB()//8
//        {
//            for (int r = 0; r < numES; r++)
//            {
//                Site from = ExternalStations[r];
//                for (int j = 0; j < numNonESNodes; j++)
//                {
                    
//                        Site to = preprocessedSites[j];
//                        ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
//                        DepartureSOCFromES.AddTerm(1.0, Delta[j]);
//                        for (int i = 0; i < numNonESNodes; i++)
//                            DepartureSOCFromES.AddTerm(EnergyConsumption(from, to, VehicleCategories.EV), Y[i][r][j]);
//                        string constraint_name = "Departure_SOC(UB)_From_ES_" + r.ToString() + "_going_to_customer_" + j.ToString();
//                        allConstraints_list.Add(AddLe(DepartureSOCFromES, BatteryCapacity(VehicleCategories.EV), constraint_name));
                    
//                }
//            }
//        }
//        void AddConstraint_DepartureSOCFromESNodeLB()//9
//        {
//            for (int r = 0; r < numES; r++)
//            {
//                Site ES = ExternalStations[r];
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site to = preprocessedSites[j];
//                    ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
//                    DepartureSOCFromES.AddTerm(1.0, Delta[j]);
//                    for (int i = 0; i < numNonESNodes; i++)
//                        DepartureSOCFromES.AddTerm((EnergyConsumption(ES, to, VehicleCategories.EV) - BatteryCapacity(VehicleCategories.EV) + minValue_Delta[j]), Y[i][r][j]);
//                    string constraint_name = "Departure_SOC(LB)_From_ES_" + r.ToString() + "_going_to_" + j.ToString();
//                    allConstraints_list.Add(AddGe(DepartureSOCFromES, minValue_Delta[j], constraint_name));

//                }
//            }
//        }
//        void AddConstraint_ArrivalSOCToESNodeLB()//9b
//        {
//            for (int j = 0; j < numNonESNodes; j++)
//                for (int r = 0; r < numES; r++)
//                {
//                    Site from = preprocessedSites[j];
//                    Site ES = ExternalStations[r];
//                    ILinearNumExpr ArrivalSOCFromES = LinearNumExpr();
//                    ArrivalSOCFromES.AddTerm(1.0, Delta[j]);
//                    ArrivalSOCFromES.AddTerm(1.0, Epsilon[j]);
//                    for (int k = 0; k < numNonESNodes; k++)
//                        ArrivalSOCFromES.AddTerm(-1.0 * EnergyConsumption(from, ES, VehicleCategories.EV), Y[j][r][k]);
//                    string constraint_name = "Arrival_SOC(LB)_To_ES_" + r.ToString() + "_from_" + j.ToString();
//                    allConstraints_list.Add(AddGe(ArrivalSOCFromES, 0.0, constraint_name));

//                }
//        }
//        void AddConstraint_SOCRegulationFollowingNondepot()//10
//        {
//            for (int i = 1; i < numNonESNodes; i++)
//            {
//                Site sFrom = preprocessedSites[i];
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site sTo = preprocessedSites[j];
//                    ILinearNumExpr SOCDifference = LinearNumExpr();
//                    SOCDifference.AddTerm(1.0, Delta[j]);
//                    SOCDifference.AddTerm(-1.0, Delta[i]);
//                    SOCDifference.AddTerm(-1.0, Epsilon[i]);
//                    SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV) + BigDelta[i][j], X[i][j]);
//                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
//                    allConstraints_list.Add(AddLe(SOCDifference, BigDelta[i][j], constraint_name));
//                }
//            }
//        }
//        void AddConstraint_SOCRegulationFollowingDepot()//11
//        {
//            for (int j = 0; j < numNonESNodes; j++)
//            {
//                Site sTo = preprocessedSites[j];
//                ILinearNumExpr SOCDifference = LinearNumExpr();
//                SOCDifference.AddTerm(1.0, Delta[j]);
//                SOCDifference.AddTerm(EnergyConsumption(TheDepot, sTo, VehicleCategories.EV), X[0][j]);
//                for (int r = 0; r < numES; r++)
//                {
//                    Site ES = ExternalStations[r];
//                    SOCDifference.AddTerm(EnergyConsumption(ES, sTo, VehicleCategories.EV), Y[0][r][j]);
//                }
//                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
//                allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
//            }
//        }
//        void AddConstraint_TimeRegulationFollowingACustomerVisit()//12
//        {
//            for (int i = 1; i < numNonESNodes; i++)
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site sFrom = preprocessedSites[i];
//                    Site sTo = preprocessedSites[j];
//                    ILinearNumExpr TimeDifference = LinearNumExpr();
//                    TimeDifference.AddTerm(1.0, T[j]);
//                    TimeDifference.AddTerm(-1.0, T[i]);
//                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j]);
//                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
//                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
//                }
//        }
//        void AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging()//13a Only if recharging is full (FF) 
//        {
//            for (int i = 1; i < numNonESNodes; i++)
//                for (int r = 0; r < numES; r++)
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sFrom = preprocessedSites[i];
//                        Site ES = ExternalStations[r];
//                        Site sTo = preprocessedSites[j];
//                        ILinearNumExpr TimeDifference = LinearNumExpr();
//                        TimeDifference.AddTerm(1.0, T[j]);
//                        TimeDifference.AddTerm(-1.0, T[i]);
//                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[i][j]), Y[i][r][j]);
//                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
//                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
//                    }
//        }
//        void AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging()//13b Only in VF, VP cases
//        {
//            for (int i = 1; i < numNonESNodes; i++)
//                for (int r = 0; r < numES; r++)
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sFrom = preprocessedSites[i];
//                        Site ES = ExternalStations[r];
//                        Site sTo = preprocessedSites[j];
//                        ILinearNumExpr TimeDifference = LinearNumExpr();
//                        double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
//                        double timeSpentDueToEnergyConsumption = (EnergyConsumption(sFrom, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES);
//                        TimeDifference.AddTerm(1.0, T[j]);
//                        TimeDifference.AddTerm(-1.0, T[i]);
//                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + travelDuration + timeSpentDueToEnergyConsumption + BigTVarRecharge[i][r][j]), Y[i][r][j]);
//                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
//                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
//                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
//                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
//                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (BigTVarRecharge[i][r][j]), constraint_name));
//                    }
//        }
//        void AddConstraint_TimeRegulationThroughAnESVisits_VariableFullTimeRecharging()//13b Only in VF, VP cases
//        {
//            for (int i = 1; i < numNonESNodes; i++)
//                for (int r = 0; r < numES; r++)
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site sFrom = preprocessedSites[i];
//                        Site ES = ExternalStations[r];
//                        Site sTo = preprocessedSites[j];
//                        ILinearNumExpr TimeDifference = LinearNumExpr();
//                        double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
//                        double timeSpentDueToEnergyConsumption = (BatteryCapacity(VehicleCategories.EV) + EnergyConsumption(sFrom, ES, VehicleCategories.EV)) / RechargingRate(ES);
//                        TimeDifference.AddTerm(1.0, T[j]);
//                        TimeDifference.AddTerm(-1.0, T[i]);
//                        TimeDifference.AddTerm(-1.0 * (BigTVarRecharge[i][r][j]), Y[i][r][j]);
//                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
//                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
//                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
//                        double rhs = -1.0 * BigTVarRecharge[i][r][j] + ServiceDuration(sFrom);
//                        allConstraints_list.Add(AddGe(TimeDifference, rhs, constraint_name));
//                    }
//        }
//        void AddConstraint_TimeRegulationFromDepotThroughAnESVisit()//13c
//        {
//            for (int r = 0; r < numES; r++)
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site ES = ExternalStations[r];
//                    Site sTo = preprocessedSites[j];
//                    ILinearNumExpr TimeDifference = LinearNumExpr();
//                    TimeDifference.AddTerm(1.0, T[j]);

//                    // Here we decide whether recharging duration is fixed or depends on the arrival SOC
//                    if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
//                    {
//                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
//                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
//                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j], constraint_name));
//                    }
//                    else
//                    {
//                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + ((EnergyConsumption(TheDepot, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
//                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
//                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
//                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j] - BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES), constraint_name));
//                    }
//                }
//        }
//        void AddConstraint_ArrivalTimeLimits()//14
//        {
//            for (int j = 1; j < numNonESNodes; j++)
//            {
//                ILinearNumExpr TimeDifference = LinearNumExpr();
//                TimeDifference.AddTerm(1.0, T[j]);
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                        TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), X[i][j]);
//                    for (int r = 0; r < numES; r++)
//                        TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), Y[i][r][j]);
//                }
//                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
//                allConstraints_list.Add(AddGe(TimeDifference, maxValue_T[j], constraint_name));
//            }
//        }
//        void AddConstraint_TotalTravelTime()//15
//        {
//            totalTravelTimeConstraintIndex = allConstraints_list.Count;

//            ILinearNumExpr TotalTravelTime = LinearNumExpr();
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    Site sFrom = preprocessedSites[i];
//                    Site sTo = preprocessedSites[j];
//                        TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j]);
//                    for (int r = 0; r < numES; r++)
//                    {
//                        Site ES = ExternalStations[r];
//                        TotalTravelTime.AddTerm((TravelTime(sFrom, ES) + TravelTime(ES, sTo)), Y[i][r][j]);
//                    }
//                }

//            string constraint_name = "Total_Travel_Time";
//            double rhs = theProblemModel.CRD.TMax;
//            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));

//        }

//        void AddAllCuts()
//        {
//            AddCut_TimeFeasibilityOfTwoConsecutiveArcs();
//            AddCut_EnergyFeasibilityOfCustomerBetweenTwoES();
//            AddCut_TotalNumberOfActiveArcs();
//        }
//        void AddCut_TimeFeasibilityOfTwoConsecutiveArcs()//16
//        {
//            for (int i = 0; i < numNonESNodes; i++)//This was starting at 1
//            {
//                Site from = preprocessedSites[i];
//                for (int k = 1; k < numNonESNodes; k++)
//                {
//                    Site through = preprocessedSites[k];
//                    if (X[i][k].UB == 0.0)//The direct arc (from,through) has already been marked infeasible
//                        continue;
//                    for (int j = 0; j < numNonESNodes; j++)//This was starting at 1
//                    {
//                        Site to = preprocessedSites[j];
//                        if (X[k][j].UB == 0.0)//The direct arc (through,to) has already been marked infeasible
//                            continue;
//                        if (i != j && j != k && i != k)
//                        {
//                            ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
//                            if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
//                            {
                                
//                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
//                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);

//                                for (int r = 0; r < numES; r++)
//                                {
//                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r][k]);
//                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r][j]);
//                                }
//                                string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
//                                allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                                TimeFeasibilityOfTwoConsecutiveArcs.Clear();
//                            }
//                            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
//                            {
//                                for (int r1 = 0; r1 < numES; r1++)
//                                {
//                                    Site ES1 = ExternalStations[r1];
//                                    double fixedChargeTimeAtES1 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES1.RechargingRate;
//                                    if (fixedChargeTimeAtES1 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//Not even one visit through ES is allowed
//                                    {
                                        
//                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
//                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);
                                        
//                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][k]);
//                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r1][j]);
//                                        string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ES_" + r1.ToString();
//                                        allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                                        TimeFeasibilityOfTwoConsecutiveArcs.Clear();
//                                    }
//                                    else
//                                    {
//                                        for (int r2 = 0; r2 < numES; r2++)
//                                            if (r2 != r1)
//                                            {
//                                                Site ES2 = ExternalStations[r2];
//                                                double fixedChargeTimeAtES2 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES2.RechargingRate;
//                                                if (fixedChargeTimeAtES1 + fixedChargeTimeAtES2 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//ES1 was fine by itself but not together with ES2
//                                                {
//                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][k]);
//                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r1][j]);
//                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r2][k]);
//                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r2][j]);
//                                                    string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ESs_" + r1.ToString() + "_and_" + r2.ToString();
//                                                    allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                                                    TimeFeasibilityOfTwoConsecutiveArcs.Clear();
//                                                }
//                                            }
//                                    }
//                                }
//                            }
//                        }//if (i != j && j != k && i != k)
//                    }
//                }
//            }
//        }
//        void AddCut_EnergyFeasibilityOfTwoConsecutiveArcs()//17
//        {
//            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
//            for (int k = 1; k < numNonESNodes; k++)
//            {
//                Site through = preprocessedSites[k];

//                //Both direct
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    Site from = preprocessedSites[i];
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site to = preprocessedSites[j];
//                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > maxValue_Delta[i] - minValue_Delta[j] + maxValue_Epsilon[k])//This was ignoring maxValue_Epsilon[k]. It was also ignoring delta bounds and was just using battery capacity (BatteryCapacity(VehicleCategories.EV)) 
//                        {
//                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
//                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);
//                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
//                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
//                        }
//                    }
//                }

//                //First from ES, then direct
//                for (int r = 0; r < numES; r++)
//                {
//                    Site ES = ExternalStations[r];
//                    for (int j = 0; j < numNonESNodes; j++)
//                    {
//                        Site to = preprocessedSites[j];
//                        if (EnergyConsumption(ES, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j] + maxValue_Epsilon[k])
//                        {
//                            for (int i = 0; i < numNonESNodes; i++)
//                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r][k]);
//                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);
//                            string constraint_name = "No_arc_from_ES_" + r.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
//                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
//                        }
//                    }
//                }

//                //First direct, then to ES
//                for (int i = 0; i < numNonESNodes; i++)
//                {
//                    Site from = preprocessedSites[i];
//                    for (int r = 0; r < numES; r++)
//                    {
//                        Site ES = ExternalStations[r];
//                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, ES, VehicleCategories.EV) > maxValue_Delta[i] + maxValue_Epsilon[k])
//                        {
//                            for (int j = 0; j < numNonESNodes; j++)
//                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r][j]);
//                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
//                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "_to_ES_" + r.ToString();
//                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
//                        }
//                    }
//                }
//            }
//        }
//        void AddCut_EnergyFeasibilityOfCustomerBetweenTwoES()
//        {
//            for (int r1 = 0; r1 < numES; r1++)
//            {
//                Site ES1 = ExternalStations[r1];
//                for (int j = 1; j < numNonESNodes; j++)
//                {
//                    Site customer = preprocessedSites[j];
//                    for (int r2 = 0; r2 < numES; r2++)
//                    {
//                        Site ES2 = ExternalStations[r2];
//                        if (EnergyConsumption(ES1, customer, VehicleCategories.EV) + EnergyConsumption(customer, ES2, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) + maxValue_Epsilon[j])//This didn't have the maxValue_Epsilon in it and hence it ignored the SOE gain at ISs
//                        {
//                            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
//                            for (int i = 0; i < numNonESNodes; i++)
//                            {
//                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][j]);
//                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[j][r2][i]);
//                            }
//                            string constraint_name = "No_arc_from_ES_node_" + r1.ToString() + "_through_customer_" + j.ToString() + "to_ES_node_" + r2.ToString();
//                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
//                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
//                        }

//                    }
//                }
//            }
//        }
//        void AddCut_TotalNumberOfActiveArcs()
//        {
//            totalNumberOfActiveArcsConstraintIndex = allConstraints_list.Count;

//            int nActiveArcs_EV = 1 + numES;
//            ILinearNumExpr totalArcFlow_EV = LinearNumExpr();
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int j = 0; j < numNonESNodes; j++)
//                {
//                    totalArcFlow_EV.AddTerm(1.0, X[i][j]);
//                    for (int r = 0; r < numES; r++)
//                    {
//                        totalArcFlow_EV.AddTerm(1.0, Y[i][r][j]);
//                    }
//                }
//            string constraintName_EV = "Number_of_active_EV_arcs_cannot_exceed_" + nActiveArcs_EV.ToString();
//            allConstraints_list.Add(AddLe(totalArcFlow_EV, nActiveArcs_EV, constraintName_EV));
//        }
//        void AddCut_EnergyConservation()//17
//        {
//            ILinearNumExpr EnergyConservation = LinearNumExpr();
//            for (int i = 0; i < NumPreprocessedSites; i++)
//            {
//                Site sFrom = preprocessedSites[i];
//                for (int r = 0; r < numES; r++)
//                {
//                    Site through = ExternalStations[r];
//                    for (int j = 0; j < NumPreprocessedSites; j++)
//                    {
//                        Site sTo = preprocessedSites[j];
//                        EnergyConservation.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV), X[i][j]);
//                        EnergyConservation.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j]);
//                        EnergyConservation.AddTerm(EnergyConsumption(sFrom, through, VehicleCategories.EV) + EnergyConsumption(through, sTo, VehicleCategories.EV), Y[i][r][j]);
//                        EnergyConservation.AddTerm(-1.0 * BatteryCapacity(VehicleCategories.EV), Y[i][r][j]);
//                    }
//                }
//            }
//            string constraint_name = "Energy conservation";
//            allConstraints_list.Add(AddLe(EnergyConservation, numVehicles[vIndex_EV] * BatteryCapacity(VehicleCategories.EV), constraint_name));
//            EnergyConservation.Clear();
//        }

//        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
//        {
//            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
//            outcome.AddRange(GetVehicleSpecificRoutes(VehicleCategories.EV));
//            return outcome;
//        }
//        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory)
//        {
//            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);//Pulling the vehicle infor from the Problem Model. Not exactly flexible, but works as long as we have only two categories of vehicles and no more than one of each
//            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
//            int counter = 0;
//            foreach (List<string> nonDepotSiteIDs in GetListsOfNonDepotSiteIDs(vehicleCategory))
//            {
//                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs, allRoutesESVisits[counter]));
//                counter++;
//            }
//            return outcome;
//        }
//        List<List<string>> GetListsOfNonDepotSiteIDs(VehicleCategories vehicleCategory)
//        {
//            //TODO: Delete the following after debugging. Update on 11/10/17: Is this still relevant?
//            GetDecisionVariableValues();

//            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

//            //We first determine the route start points
//            List<List<int>> listOfFirstSiteIndices = new List<List<int>>();
//            for (int j = 0; j < numNonESNodes; j++)
//                if (GetValue(X[0][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
//                {
//                    listOfFirstSiteIndices.Add(new List<int>() { j });
//                }
//            if (vehicleCategory != VehicleCategories.GDV)
//                for (int r = 0; r < numES; r++)
//                    for (int j = 0; j < numNonESNodes; j++)
//                        if (GetValue(Y[0][r][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
//                        {
//                            listOfFirstSiteIndices.Add(new List<int>() { r, j });
//                        }
//            //Then, populate the whole routes (actually, not routes yet)
//            List<List<string>> outcome = new List<List<string>>();
//            foreach (List<int> firstSiteIndices in listOfFirstSiteIndices)
//            {
//                outcome.Add(GetNonDepotSiteIDs(firstSiteIndices, vehicleCategory));
//            }
//            return outcome;
//        }
//        public void GetDecisionVariableValues()
//        {
//            //System.IO.StreamWriter sw = new System.IO.StreamWriter("routes.txt");
//            double[,] xValues = new double[numNonESNodes, numNonESNodes];
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int j = 0; j < numNonESNodes; j++)
//                    xValues[i, j] = GetValue(X[i][j]);//CONSULT (w/ Isil): Why only 0 when xValues is defined over all numVehCategories? IK: This was just debugging purposes, since EMH does not have any GDVs, I only wrote [0].
//            double[,,] yValues = new double[numNonESNodes, numES, numNonESNodes];
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int r = 0; r < numES; r++)
//                    for (int j = 0; j < numNonESNodes; j++)
//                        yValues[i, r, j] = GetValue(Y[i][r][j]);

//            double[] epsilonValues = new double[numNonESNodes];
//            for (int j = 0; j < numNonESNodes; j++)
//                epsilonValues[j] = GetValue(Epsilon[j]);
//            double[] deltaValues = new double[numNonESNodes];
//            for (int j = 0; j < numNonESNodes; j++)
//                deltaValues[j] = GetValue(Delta[j]);
//            double[] TValues = new double[numNonESNodes];
//            for (int j = 0; j < numNonESNodes; j++)
//                TValues[j] = GetValue(T[j]);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="firstSiteIndices"></param>Can contain one or two elements. If one, it's for an X arc; if two, it's for a Y arc; nothing else is possible!
//        /// <param name="vehicleCategory"></param>Obviously if GDV, there won't be any Y arcs
//        /// <returns></returns>
//        List<string> GetNonDepotSiteIDs(List<int> firstSiteIndices, VehicleCategories vehicleCategory)
//        {
//            if ((firstSiteIndices.Count > 2) || ((vehicleCategory == VehicleCategories.GDV) && (firstSiteIndices.Count > 1)))
//                throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called with too many firstSiteIndices!");

//            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
//            List<string> outcome = new List<string>();

//            if (firstSiteIndices.Count == 1)
//                if (GetValue(X[0][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
//                {
//                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to an X-flow from the depot!");
//                }
//            if (firstSiteIndices.Count == 2)
//                if (GetValue(Y[0][firstSiteIndices.First()][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
//                {
//                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to a Y-flow from the depot!");
//                }

//            List<int> currentSiteIndices = firstSiteIndices;
//            List<int> nextSiteIndices;
//            singleRouteESvisits = new IndividualRouteESVisits();
//            int i = 0, r = 0, j = 0;
//            do
//            {
//                j = currentSiteIndices.Last();
//                if (currentSiteIndices.Count == 2)
//                {
//                    outcome.Add(ExternalStations[currentSiteIndices.First()].ID);
//                    r = currentSiteIndices.First();
//                    singleRouteESvisits.Add(GetIndividualESVisit(i, r, j));
//                }
//                outcome.Add(preprocessedSites[currentSiteIndices.Last()].ID);

//                nextSiteIndices = GetNextSiteIndices(currentSiteIndices.Last(), vehicleCategory);
//                i = currentSiteIndices.Last();
//                if (preprocessedSites[nextSiteIndices.Last()].ID == TheDepot.ID)
//                {
//                    if (nextSiteIndices.Count == 2)
//                    {
//                        r = nextSiteIndices.First();
//                        j = 0;
//                        outcome.Add(ExternalStations[nextSiteIndices.First()].ID);
//                        singleRouteESvisits.Add(GetIndividualESVisit(i, r, j));
//                    }
//                    allRoutesESVisits.Add(singleRouteESvisits);
//                    return outcome;
//                }
//                currentSiteIndices = nextSiteIndices;
//            }
//            while (preprocessedSites[currentSiteIndices.Last()].ID != TheDepot.ID);

//            allRoutesESVisits.Add(singleRouteESvisits);
//            return outcome;
//        }
//        List<int> GetNextSiteIndices(int currentSiteIndex, VehicleCategories vehicleCategory)
//        {
//            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
//            for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
//                if (GetValue(X[currentSiteIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
//                {
//                    return new List<int>() { nextCustomerIndex };
//                }
//            if (vehicleCategory == VehicleCategories.EV)
//                for (int nextESIndex = 0; nextESIndex < numES; nextESIndex++)
//                    for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
//                        if (GetValue(Y[currentSiteIndex][nextESIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
//                            return new List<int>() { nextESIndex, nextCustomerIndex };
//            throw new System.Exception("Flow ended before returning to the depot!");
//        }
//        IndividualESVisitDataPackage GetIndividualESVisit(int i, int r, int j)
//        {
//            Site from = preprocessedSites[i];
//            Site ES = ExternalStations[r];
//            Site to = preprocessedSites[j];

//            double timeSpentInES = GetValue(T[j]) - TravelTime(from, ES) - ServiceDuration(from) - GetValue(T[i]) - TravelTime(ES, to);
//            return new IndividualESVisitDataPackage(ES.ID, timeSpentInES / RechargingRate(ES), preprocessedESSiteIndex: r);
//        }
//        public override SolutionBase GetCompleteSolution(Type SolutionType)
//        {
//            return new RouteBasedSolution(GetVehicleSpecificRoutes());
//        }

//        public override string GetModelName()
//        {
//            return "EV Optimize Single Customer Set";
//        }
//        public override void RefineDecisionVariables(CustomerSet cS)
//        {
//            RHS_forNodeCoverage = new double[numNonESNodes];
//            for (int i = 0; i < numNonESNodes; i++)
//                for (int j = 1; j < numNonESNodes; j++)
//                {
//                    if (cS.Customers.Contains(preprocessedSites[j].ID))
//                    {
//                        RHS_forNodeCoverage[j] = 1.0;
//                        X[i][j].UB = 1.0;
//                        X[j][i].UB = 1.0;
//                        for (int r = 0; r < numES; r++)
//                        {
//                            Y[i][r][j].UB = 1.0;
//                            Y[j][r][i].UB = 1.0;

//                        }
//                    }
//                    else
//                    {
//                        RHS_forNodeCoverage[j] = 0.0;
//                        X[i][j].UB = 0.0;
//                        X[j][i].UB = 0.0;
//                        for (int r = 0; r < numES; r++)
//                        {
//                            Y[i][r][j].UB = 0.0;
//                            Y[j][r][i].UB = 0.0;

//                        }
//                    }
//                }
//            RefineRightHandSidesOfCustomerVisitationConstraints();
//            RefineRHSofTotalTravelConstraints(cS);
//            allRoutesESVisits.Clear();
//        }
//        void RefineRightHandSidesOfCustomerVisitationConstraints()
//        {
//            int c = firstCustomerVisitationConstraintIndex;

//            for (int j = 1; j < numNonESNodes; j++)
//            {
//                if (RHS_forNodeCoverage[j] == 1)
//                {
//                    allConstraints_array[c].UB = RHS_forNodeCoverage[j];
//                    allConstraints_array[c].LB = RHS_forNodeCoverage[j];
//                }
//                else//RHS_forNodeCoverage[j] == 0
//                {
//                    allConstraints_array[c].LB = RHS_forNodeCoverage[j];
//                    allConstraints_array[c].UB = RHS_forNodeCoverage[j];
//                }
//                c++;
//            }

//        }
//        void RefineRHSofTotalTravelConstraints(CustomerSet cS)
//        {
//            int c=0;
//            if (customerCoverageConstraint != CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
//            {
//                c = totalTravelTimeConstraintIndex;
//                allConstraints_array[c].UB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
//                //allConstraints_array[c].LB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
//            }
//            if (addTotalNumberOfActiveArcsCut)
//            {
//                c = totalNumberOfActiveArcsConstraintIndex;
//                allConstraints_array[c].UB = theProblemModel.CRD.TMax + cS.NumberOfCustomers;
//                //allConstraints_array[c].LB = theProblemModel.CRD.TMax + cS.NumberOfCustomers;
//            }
//        }

//        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

