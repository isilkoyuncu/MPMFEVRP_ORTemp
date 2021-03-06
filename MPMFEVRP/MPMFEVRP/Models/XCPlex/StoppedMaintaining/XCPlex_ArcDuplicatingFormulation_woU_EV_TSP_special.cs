﻿//using ILOG.Concert;
//using MPMFEVRP.Domains.ProblemDomain;
//using MPMFEVRP.Domains.SolutionDomain;
//using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.Solutions;
//using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace MPMFEVRP.Models.XCPlex
//{
//    public class XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special : XCPlexVRPBase
//    {
//        int numNonESNodes;
//        int numCustomers, numES;

//        int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
//        int totalTravelTimeConstraintIndex = -1;
//        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

//        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets


//        INumVar[][] X; double[][] X_LB, X_UB;
//        INumVar[][][] Y; double[][][] Y_LB, Y_UB;
//        INumVar[] Epsilon;
//        INumVar[] Delta;
//        INumVar[] T;
//        public XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special() { }
//        public XCPlex_ArcDuplicatingFormulation_woU_EV_TSP_special(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
//            : base(theProblemModel, xCplexParam)
//        {
//        }

       // protected override void DefineDecisionVariables()
       //{
//            PreprocessSites();
//            numCustomers = theProblemModel.SRD.NumCustomers;
//            numES = theProblemModel.SRD.NumES;
//            numNonESNodes = numCustomers + 1;
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
 //       }
        //        void SetUndesiredXYVariablesTo0()
        //        {
        //            T[0].LB = theProblemModel.CRD.TMax;
        //            T[0].UB = theProblemModel.CRD.TMax;
        //            Epsilon[0].LB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
        //            Epsilon[0].UB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;

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
        //                VehicleCategories vehicleCategory = vehicleCategories[vIndex_EV];
        //                for (int i = 0; i < numNonESNodes; i++)
        //                {
        //                    Site sFrom = preprocessedSites[i];
        //                    for (int j = 0; j < numNonESNodes; j++)
        //                    {
        //                        Site sTo = preprocessedSites[j];
        //                        if (EnergyConsumption(sFrom, sTo, vehicleCategory) > theProblemModel.VRD.GetVehiclesOfCategory(vehicleCategory)[0].BatteryCapacity)
        //                            X[i][j].UB = 0.0;
        //                    }
        //                }

        //            //No arc from a node to an ES or from an ES to a node if energy consumption is > capacity
        //            for (int i = 0; i < numNonESNodes; i++)
        //                for (int r = 0; r < numES; r++)
        //                    for (int j = 0; j < numNonESNodes; j++)
        //                    {
        //                        Site sFrom = preprocessedSites[i];
        //                        Site ES = ExternalStations[r];
        //                        Site sTo = preprocessedSites[j];
        //                        if (EnergyConsumption(sFrom, ES, VehicleCategories.EV) > theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity ||
        //                            EnergyConsumption(ES, sTo, VehicleCategories.EV) > theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity)
        //                            Y[i][r][j].UB = 0.0;
        //                    }
        //        }
        //        void SetMinAndMaxValuesOfModelSpecificVariables()
        //        {
        //            X_LB = new double[numNonESNodes][];
        //            X_UB = new double[numNonESNodes][];
        //            Y_LB = new double[numNonESNodes][][];
        //            Y_UB = new double[numNonESNodes][][];
        //            RHS_forNodeCoverage = new double[numNonESNodes];


        //            for (int i = 0; i < numNonESNodes; i++)
        //            {
        //                Y_LB[i] = new double[numES][];
        //                Y_UB[i] = new double[numES][];
        //                for (int r = 0; r < numES; r++)
        //                {
        //                    Y_LB[i][r] = new double[numNonESNodes];
        //                    Y_UB[i][r] = new double[numNonESNodes];
        //                    for (int j = 0; j < numNonESNodes; j++)
        //                    {
        //                        Y_LB[i][r][j] = 0.0;
        //                        Y_UB[i][r][j] = 1.0;
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
        //                "for (int i = 0; i <= numCustomers; i++)\nfor (int j = 0; j <= numCustomers; j++)\nX[i][j][v]"
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
        //                            objFunction.AddTerm(Prize(s, vehicleCategories[vIndex_EV]), X[i][j]);
        //            }
        //            //Second term Part I: distance-based costs from customer to customer directly
        //            for (int i = 0; i < numNonESNodes; i++)
        //            {
        //                Site sFrom = preprocessedSites[i];
        //                for (int j = 0; j < numNonESNodes; j++)
        //                {
        //                    Site sTo = preprocessedSites[j];
        //                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(vehicleCategories[vIndex_EV]) * Distance(sFrom, sTo), X[i][j]);
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
        //                    objFunction.AddTerm(GetVehicleFixedCost(base.vehicleCategories[vIndex_EV]), X[0][j]);
        //            //Now adding the objective function to the model
        //            AddMaximize(objFunction);
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
        //                            objFunction.AddTerm(Distance(sFrom, sTo), X[i][j]);
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
        //                        for (int j = 0; j < numVehCategories; j++)
        //                        {
        //                            Site sTo = preprocessedSites[j];
        //                            objFunction.AddTerm(GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES) + Distance(ES, sTo)), Y[i][r][j]);
        //                        }
        //                    }
        //                }
        //                //Third term: vehicle fixed costs
        //                for (int j = 0; j < numVehCategories; j++)
        //                        objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[vIndex_EV]), X[0][j]);
        //            }
        //            //Now adding the objective function to the model
        //            AddMinimize(objFunction);
        //        }
        //        protected override void AddAllConstraints()
        //        {
        //            allConstraints_list = new List<IRange>();
        //            //Now adding the constraints one (family) at a time
        //            AddConstraint_NumberOfVisitsPerCustomerNode();//1
        //            AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV();//2
        //            AddConstraint_MaxNumberOfEVs();//4
        //            AddConstraint_MaxEnergyGainAtNonDepotSite();//6
        //            AddConstraint_DepartureSOCFromCustomerNode();//7
        //            AddConstraint_DepartureSOCFromESNodeUB();//8
        //            AddConstraint_DepartureSOCFromESNodeLB();//9
        //            AddConstraint_ArrivalSOCToESNodeLB();//9b
        //            AddConstraint_SOCRegulationFollowingNondepot();//10
        //            AddConstraint_SOCRegulationFollowingDepot();//11
        //            AddConstraint_TimeRegulationFollowingACustomerVisit();//12
        //            if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
        //            {
        //                AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13b
        //            }
        //            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
        //            {
        //                AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13b
        //                AddConstraint_DepartureSOCFromESNodeLB();//9
        //            }
        //            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
        //            {
        //                AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging();//13a
        //                AddConstraint_DepartureSOCFromESNodeLB();//9
        //            }
        //            AddConstraint_TimeRegulationFromDepotThroughAnESVisit();//13b
        //            AddConstraint_ArrivalTimeLimits();//14
        //            AddConstraint_TotalTravelTime();//15
        //            //AddConstraint_TimeFeasibilityOfTwoConsecutiveArcs();//16
        //            //AddConstraint_EnergyFeasibilityOfTwoConsecutiveArcs();//17
        //            //AddConstraint_EnergyFeasibilityOfCustomerBetweenTwoES();//18
        //            //AddConstraint_TotalNumberOfActiveArcs();

        //            //All constraints added
        //            allConstraints_array = allConstraints_list.ToArray();
        //        }

        //        void AddConstraint_NumberOfVisitsPerCustomerNode() //1
        //        {
        //            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

        //            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
        //            {
        //                ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
        //                for (int i = 0; i < numNonESNodes; i++)
        //                {
        //                        IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j]);

        //                    for (int r = 0; r < numES; r++)
        //                        IncomingXandYToCustomerNodes.AddTerm(1.0, Y[i][r][j]);
        //                }
        //                string constraint_name;

        //                if (xCplexParam.TSP || theProblemModel.CoverConstraintType == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
        //                {
        //                    constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
        //                    allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
        //                }
        //                else
        //                {
        //                    constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
        //                    allConstraints_list.Add(AddLe(IncomingXandYToCustomerNodes, 1.0, constraint_name));
        //                }
        //            }
        //        }
        //        void AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV()//2
        //        {
        //            for (int j = 0; j < numNonESNodes; j++)
        //            {
        //                ILinearNumExpr IncomingXYTotalMinusOutgoingXYTotal = LinearNumExpr();

        //                for (int i = 0; i < numNonESNodes; i++)
        //                {
        //                    IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, X[i][j]);
        //                    IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, X[j][i]);
        //                    for (int r = 0; r < numES; r++)
        //                    {
        //                        IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, Y[i][r][j]);
        //                        IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, Y[j][r][i]);
        //                    }
        //                }
        //                string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
        //                allConstraints_list.Add(AddEq(IncomingXYTotalMinusOutgoingXYTotal, 0.0, constraint_name));

        //            }
        //        }
        //        void AddConstraint_MaxNumberOfEVs()//4
        //        {
        //            ILinearNumExpr NumberOfEVsOutgoingFromTheDepot = LinearNumExpr();
        //            for (int j = 1; j < numNonESNodes; j++)
        //            {
        //                NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j]);
        //                for (int r = 0; r < numES; r++)
        //                    NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, Y[0][r][j]);
        //            }
        //            string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_EV].ToString();
        //            allConstraints_list.Add(AddLe(NumberOfEVsOutgoingFromTheDepot, numVehicles[vIndex_EV], constraint_name));
        //        }
        //        void AddConstraint_MaxEnergyGainAtNonDepotSite()//6
        //        {
        //            for (int j = 1; j < numNonESNodes; j++)
        //            {
        //                ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
        //                EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
        //                for (int i = 0; i < numNonESNodes; i++)
        //                {
        //                    EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j]);
        //                    for (int r = 0; r < numES; r++)
        //                        EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], Y[i][r][j]);
        //                }
        //                string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
        //                allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));
        //            }
        //        }
        //        void AddConstraint_DepartureSOCFromCustomerNode()//7
        //        {
        //            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
        //            {
        //                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
        //                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
        //                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
        //                for (int i = 0; i < numNonESNodes; i++)
        //                {
        //                    DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j]);
        //                    for (int r = 0; r < numES; r++)
        //                        DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), Y[i][r][j]);
        //                }
        //                string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
        //                allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));
        //            }
        //        }
        //        void AddConstraint_DepartureSOCFromESNodeUB()//8
        //        {
        //            for (int r = 0; r < numES; r++)
        //            {
        //                Site from = ExternalStations[r];
        //                for (int j = 0; j < numNonESNodes; j++)
        //                {
        //                    Site to = preprocessedSites[j];
        //                    ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
        //                    DepartureSOCFromES.AddTerm(1.0, Delta[j]);
        //                    for (int i = 0; i < numNonESNodes; i++)
        //                        DepartureSOCFromES.AddTerm(EnergyConsumption(from, to, VehicleCategories.EV), Y[i][r][j]);
        //                    string constraint_name = "Departure_SOC(UB)_From_ES_" + r.ToString() + "_going_to_" + j.ToString();
        //                    allConstraints_list.Add(AddLe(DepartureSOCFromES, BatteryCapacity(VehicleCategories.EV), constraint_name));

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
        //        void AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging()//13a Only if recharging time is fixed (FF) 
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
        //        void AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging()//13b Only if recharing time is variable (VF, VP) cases
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
        //                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, ES) + TravelTime(ES, sTo) + (EnergyConsumption(sFrom, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV) / RechargingRate(ES)) + BigT[i][j]), Y[i][r][j]);
        //                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
        //                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
        //                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
        //                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (BigT[i][j] + (EnergyConsumption(sFrom, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV) / RechargingRate(ES))), constraint_name));
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
        //                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + (EnergyConsumption(TheDepot, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
        //                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
        //                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[0]);
        //                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[0]);
        //                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j], constraint_name));
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
        //            double rhs = (xCplexParam.TSP ? 1 : theProblemModel.GetNumVehicles(VehicleCategories.EV) + theProblemModel.GetNumVehicles(VehicleCategories.GDV)) * theProblemModel.CRD.TMax;
        //            rhs -= theProblemModel.SRD.GetTotalCustomerServiceTime();
        //            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));

        //        }
        //        void AddConstraint_TimeFeasibilityOfTwoConsecutiveArcs()//16
        //        {
        //                for (int i = 1; i < numNonESNodes; i++)
        //                {
        //                    Site from = preprocessedSites[i];
        //                    for (int k = 1; k < numNonESNodes; k++)
        //                    {
        //                        Site through = preprocessedSites[k];
        //                        for (int j = 1; j < numNonESNodes; j++)
        //                        {
        //                            Site to = preprocessedSites[j];
        //                            if (i != j && j != k && i != k)
        //                                if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
        //                                {
        //                                    ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
        //                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
        //                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);
        //                                    string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
        //                                    allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                                    TimeFeasibilityOfTwoConsecutiveArcs.Clear();
        //                                }
        //                        }
        //                    }
        //                }
        //        }
        //        void AddConstraint_EnergyFeasibilityOfTwoConsecutiveArcs()//17
        //        {
        //            for (int i = 0; i < numNonESNodes; i++)
        //            {
        //                Site from = preprocessedSites[i];
        //                for (int k = 0; k < numNonESNodes; k++)
        //                {
        //                    Site through = preprocessedSites[k];
        //                    for (int j = 0; j < numNonESNodes; j++)
        //                    {
        //                        Site to = preprocessedSites[j];
        //                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV))
        //                        {
        //                            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
        //                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
        //                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);
        //                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
        //                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        void AddConstraint_EnergyFeasibilityOfCustomerBetweenTwoES()//18
        //        {
        //            for (int r1 = 0; r1 < numES; r1++)
        //            {
        //                Site ES1 = ExternalStations[r1];
        //                for (int j = 0; j < numNonESNodes; j++)
        //                {
        //                    Site customer = preprocessedSites[j];
        //                    for (int r2 = 0; r2 < numES; r2++)
        //                    {
        //                        Site ES2 = ExternalStations[r2];
        //                        if (EnergyConsumption(ES1, customer, VehicleCategories.EV) + EnergyConsumption(customer, ES2, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV))
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
        //        void AddConstraint_TotalNumberOfActiveArcs()
        //        {
        //            totalNumberOfActiveArcsConstraintIndex = allConstraints_list.Count;

        //            int nActiveArcs = numVehicles[0] + numVehicles[1] + numCustomers;
        //            ILinearNumExpr totalArcFlow = LinearNumExpr();
        //            for (int i = 0; i < numNonESNodes; i++)
        //                for (int j = 0; j < numNonESNodes; j++)
        //                {
        //                        totalArcFlow.AddTerm(1.0, X[i][j]);
        //                    for (int r = 0; r < numES; r++)
        //                    {
        //                        totalArcFlow.AddTerm(1.0, Y[i][r][j]);
        //                    }
        //                }
        //            string constraintName_overall = "Total_number_of_active_arcs_cannot_exceed_" + nActiveArcs.ToString();
        //            allConstraints_list.Add(AddLe(totalArcFlow, (double)nActiveArcs, constraintName_overall));
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
        //            foreach (List<string> nonDepotSiteIDs in GetListsOfNonDepotSiteIDs(vehicleCategory))
        //                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs));
        //            return outcome;
        //        }
        //        List<List<string>> GetListsOfNonDepotSiteIDs(VehicleCategories vehicleCategory)
        //        {
        //            //TODO: Delete the following after debugging
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
        //            double[,] xValues = new double[numNonESNodes, numNonESNodes];
        //            for (int i = 0; i < numNonESNodes; i++)
        //                for (int j = 0; j < numNonESNodes; j++)
        //                    xValues[i, j] = GetValue(X[i][j]);//TODO: Why only 0 when xValues is defined over all numVehCategories?
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

        //            do
        //            {
        //                if (currentSiteIndices.Count == 2)
        //                    outcome.Add(ExternalStations[currentSiteIndices.First()].ID);
        //                outcome.Add(preprocessedSites[currentSiteIndices.Last()].ID);

        //                nextSiteIndices = GetNextSiteIndices(currentSiteIndices.Last(), vehicleCategory);
        //                if (preprocessedSites[nextSiteIndices.Last()].ID == TheDepot.ID)
        //                {
        //                    if (nextSiteIndices.Count == 2)
        //                        outcome.Add(ExternalStations[nextSiteIndices.First()].ID);
        //                    return outcome;
        //                }
        //                currentSiteIndices = nextSiteIndices;
        //            }
        //            while (preprocessedSites[currentSiteIndices.Last()].ID != TheDepot.ID);

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

        //        public override SolutionBase GetCompleteSolution(Type SolutionType)
        //        {
        //            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        //        }

        //        public override string GetModelName()
        //        {
        //            return "ADF without U EV TSP";
        //        }

        //        public override void RefineDecisionVariables(CustomerSet cS)
        //        {
        //            RHS_forNodeCoverage = new double[numNonESNodes];
        //            int VCIndex = (int)xCplexParam.VehCategory;
        //            for (int i = 0; i < numNonESNodes; i++)
        //                for (int j = 1; j < numNonESNodes; j++)
        //                {
        //                    if (cS.Customers.Contains(preprocessedSites[j].ID))
        //                    {
        //                        RHS_forNodeCoverage[j] = 1.0;
        //                    }
        //                    else
        //                    {
        //                        RHS_forNodeCoverage[j] = 0.0;
        //                    }
        //                }
        //            RefineRightHandSidesOfCustomerVisitationConstraints();

        //            if (totalTravelTimeConstraintIndex >= 0)
        //                allConstraints_array[totalTravelTimeConstraintIndex].UB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
        //            if (totalNumberOfActiveArcsConstraintIndex >= 0)
        //            {
        //                allConstraints_array[totalNumberOfActiveArcsConstraintIndex].UB = (double)(cS.NumberOfCustomers + 1);
        //            }
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

//        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

