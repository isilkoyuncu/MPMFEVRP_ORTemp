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
    public class XCPlex_LeggieriAttempt : XCPlexVRPBase
    {
        int numNonESNodes;
        int numCustomers, numES;

        int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
        int totalTravelTimeConstraintIndex = -1;
        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        INumVar[][][] X; double[][][] X_LB, X_UB;
        INumVar[][][] Y; double[][][] Y_LB, Y_UB;
        INumVar[][] Z; double[][] Z_LB, Z_UB;
        //INumVar[][] W; double[][] W_LB, W_UB;
        INumVar[] Epsilon;
        INumVar[] Delta;
        INumVar[] T;

        protected double[][][] BigTVarRecharge;

        IndividualRouteESVisits singleRouteESvisits;
        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

        public XCPlex_LeggieriAttempt() { }
        public XCPlex_LeggieriAttempt(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam)
        {
        }

        protected override void DefineDecisionVariables()
        {
            PreprocessSites();
            numCustomers = theProblemModel.SRD.NumCustomers;
            numES = theProblemModel.SRD.NumES;
            numNonESNodes = numCustomers + 1;
            SetMinAndMaxValuesOfModelSpecificVariables();

            allVariables_list = new List<INumVar>();

            //dvs: X_ijv and Y_irj
            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, numVehCategories, out X);
            AddThreeDimensionalDecisionVariable("Y", Y_LB, Y_UB, NumVarType.Int, numNonESNodes, numES, numNonESNodes, out Y);

            //dvs: Z_ij and W_ij
            AddTwoDimensionalDecisionVariable("Z", Z_LB, Z_UB, NumVarType.Int, numNonESNodes, numNonESNodes, out Z);
            //AddTwoDimensionalDecisionVariable("W", W_LB, W_UB, NumVarType.Int, numNonESNodes, numNonESNodes, out W);

            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out T);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numNonESNodes, out Delta);
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numNonESNodes, out Epsilon);

            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXYVariablesTo0();
            //rig_IKfromNDFEMH();
        }
        void rig_IKfromNDFEMH()
        {

            X[0][4][0].LB = 1.0;
            X[4][10][0].LB = 1.0;
            X[10][0][0].LB = 1.0;

            X[0][6][0].LB = 1.0;
            X[6][15][0].LB = 1.0;
            Y[15][2][3].LB = 1.0;
            X[3][13][0].LB = 1.0;
            X[13][12][0].LB = 1.0;
            X[12][5][0].LB = 1.0;
            X[5][0][0].LB = 1.0;

            X[0][8][0].LB = 1.0;
            X[8][19][0].LB = 1.0;
            X[19][14][0].LB = 1.0;
            X[14][7][0].LB = 1.0;
            Y[7][3][11].LB = 1.0;
            X[11][18][0].LB = 1.0;
            X[18][9][0].LB = 1.0;
            X[9][0][0].LB = 1.0;

            X[0][20][0].LB = 1.0;
            X[20][16][0].LB = 1.0;
            X[16][2][0].LB = 1.0;
            Y[2][3][17].LB = 1.0;
            X[17][1][0].LB = 1.0;

        }
        void rig_IK()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X[i][j][v].LB = 0.0;
                        X[i][j][v].UB = 0.0;
                    }
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Y[i][r][j].LB = 0.0;
                        Y[i][r][j].UB = 0.0;
                    }
            IKTestsToDelete ikTest = new IKTestsToDelete();
            List<List<int>> routes = ikTest.Routes;
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    int from = routes[0].IndexOf(i);
                    if (from >= 0)
                        if (routes[0].ElementAt(from + 1) == j)
                        {
                            X[i][j][0].LB = 1.0;
                            X[i][j][0].UB = 1.0;
                        }
                }
            for (int rt = 1; rt < routes.Count; rt++)
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        int from = routes[rt].IndexOf(i);
                        if (from >= 0)
                            if (routes[rt].ElementAt(from + 1) == j)
                            {
                                X[i][j][1].LB = 1.0;
                                X[i][j][1].UB = 1.0;
                            }
                    }
        }
        void SetUndesiredXYVariablesTo0()
        {
            T[0].LB = theProblemModel.CRD.TMax;
            T[0].UB = theProblemModel.CRD.TMax;
            Epsilon[0].LB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
            Epsilon[0].UB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;

            //No arc from a node to itself
            for (int j = 0; j < numNonESNodes; j++)
            {
                for (int v = 0; v < numVehCategories; v++)
                    X[j][j][v].UB = 0.0;

                for (int r = 0; r < numES; r++)
                    Y[j][r][j].UB = 0.0;
            }
            //No arc from depot to its duplicate
            for (int j = 0; j < numNonESNodes; j++)
                for (int r = 0; r < numES; r++)
                    if ((ExternalStations[r].X == TheDepot.X) && (ExternalStations[r].Y == TheDepot.Y))//Comparing X and Y coordinates to those of the depot makes sures that the ES at hand corresponds to the one at the depot!
                    {
                        Y[0][r][j].UB = 0.0;
                        Y[j][r][0].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > capacity
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    if (EnergyConsumption(sFrom, sTo, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV))
                        X[i][j][vIndex_EV].UB = 0.0;
                }
            }

            //No arc from a node to an ES or from an ES to a node if energy consumption is > capacity
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[r];
                        Site sTo = preprocessedSites[j];
                        if (EnergyConsumption(sFrom, ES, VehicleCategories.EV) > Math.Min(maxValue_Delta[i] + maxValue_Epsilon[i], BatteryCapacity(VehicleCategories.EV)) ||
                            EnergyConsumption(ES, sTo, VehicleCategories.EV) > theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity)
                            Y[i][r][j].UB = 0.0;
                    }

            //No YArc if it's not more beneficial
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (X[i][j][vIndex_EV].UB == 1.0)//The arc has not been eliminated for some other reason
                    {
                        Site sFrom = preprocessedSites[i];
                        Site sTo = preprocessedSites[j];
                        double directEnergyConsumption = EnergyConsumption(sFrom, sTo, VehicleCategories.EV);
                        for (int r = 0; r < numES; r++)
                            if (Y[i][r][j].UB == 1.0)//The arc has not been eliminated for some other reason
                            {
                                Site ES = ExternalStations[r];
                                double toESEnergyConsumption = EnergyConsumption(sFrom, ES, VehicleCategories.EV);
                                if (toESEnergyConsumption > directEnergyConsumption)
                                {
                                    double directArrivalSOE = Math.Min(maxValue_Delta[i] + maxValue_Epsilon[i], BatteryCapacity(VehicleCategories.EV)) - directEnergyConsumption;
                                    double fromESArrivalSOE = BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(ES, sTo, VehicleCategories.EV);
                                    if (directArrivalSOE >= fromESArrivalSOE)//No benefit in going through ES
                                        Y[i][r][j].UB = 0.0;
                                }
                            }
                    }

            //Between two YArcs, check for domination and kill the dominated one
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r1 = 0; r1 < numES; r1++)
                    {
                        if (Y[i][r1][j].UB == 0.0)
                            continue;
                        for (int r2 = 0; r2 < numES; r2++)
                        {
                            if (r2 == r1)
                                continue;
                            if (Y[i][r2][j].UB == 0.0)
                                continue;
                            int dom = Dominates(i, j, r1, r2);
                            if (dom == 1)
                                Y[i][r2][j].UB = 0.0;
                            if (dom == 2)
                                Y[i][r1][j].UB = 0.0;
                        }
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
            Site ES1 = ExternalStations[r1];
            Site ES2 = ExternalStations[r2];
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
            X_LB = new double[numNonESNodes][][];
            X_UB = new double[numNonESNodes][][];
            Y_LB = new double[numNonESNodes][][];
            Y_UB = new double[numNonESNodes][][];
            Z_LB = new double[numNonESNodes][];
            Z_UB = new double[numNonESNodes][];
            //W_LB = new double[numNonESNodes][];
            //W_UB = new double[numNonESNodes][];
            BigTVarRecharge = new double[numNonESNodes][][];

            RHS_forNodeCoverage = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                Y_LB[i] = new double[numES][];
                Y_UB[i] = new double[numES][];
                BigTVarRecharge[i] = new double[numES][];

                for (int r = 0; r < numES; r++)
                {
                    Y_LB[i][r] = new double[numNonESNodes];
                    Y_UB[i][r] = new double[numNonESNodes];
                    BigTVarRecharge[i][r] = new double[numNonESNodes];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Y_LB[i][r][j] = 0.0;
                        Y_UB[i][r][j] = 1.0;
                        BigTVarRecharge[i][r][j] = maxValue_T[i] - minValue_T[j] + TravelTime(preprocessedSites[i], ExternalStations[r]) + TravelTime(ExternalStations[r], preprocessedSites[j]) + (BatteryCapacity(VehicleCategories.EV) + EnergyConsumption(preprocessedSites[i], ExternalStations[r], VehicleCategories.EV)) / RechargingRate(ExternalStations[r]);
                    }
                }
                RHS_forNodeCoverage[i] = 1.0;
            }

            for (int i = 0; i < numNonESNodes; i++)
            {
                X_LB[i] = new double[numNonESNodes][];
                X_UB[i] = new double[numNonESNodes][];
                Z_LB[i] = new double[numNonESNodes];
                Z_UB[i] = new double[numNonESNodes];
                //W_LB[i] = new double[numNonESNodes];
                //W_UB[i] = new double[numNonESNodes];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    X_LB[i][j] = new double[numVehCategories];
                    X_UB[i][j] = new double[numVehCategories];
                    Z_LB[i][j] = 0.0;
                    Z_UB[i][j] = 1.0;
                    //W_LB[i][j] = 0.0;
                    //W_UB[i][j] = 1.0;
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X_LB[i][j][v] = 0.0;
                        X_UB[i][j][v] = 1.0;
                    }
                }
            }
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i <= numCustomers; i++)\nfor (int j = 0; j <= numCustomers; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nX[i][j][v]"
                + "then\n"
                + "for (int i = 0; i <= numCustomers; i++)\nfor (int r = 0; r < numES; r++)\nfor (int j = 0; j <= numCustomers; j++)\nY[i][r][j]"
                + "then\n"
                + "for (int j = 0; j <= numCustomers; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nU[j][v]\n"
                + "then\n"
                + "for (int j = 0; j <= numCustomers; j++)\nT[j]\n"
                + "then\n"
                + "for (int j = 0; j <= numCustomers; j++)\ndelta[j]\n"
                + "then\n"
                + "for (int j = 0; j <= numCustomers; j++)\nepsilon[j]\n";
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
            for (int j = 0; j < numNonESNodes; j++)
            {
                Site s = preprocessedSites[j];
                if (s.SiteType == SiteTypes.Customer)
                    for (int i = 0; i < numNonESNodes; i++)
                        for (int v = 0; v < numVehCategories; v++)
                            objFunction.AddTerm(Prize(s, vehicleCategories[v]), X[i][j][v]);
            }
            //Second term Part I: distance-based costs from customer to customer directly
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Second term Part II: distance-based costs from customer to customer through an ES
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int r = 0; r < numES; r++)
                {
                    Site ES = ExternalStations[r];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES) + Distance(ES, sTo)), Y[i][r][j]);
                    }
                }
            }
            //Third term: vehicle fixed costs
            for (int j = 0; j < numNonESNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(GetVehicleFixedCost(base.vehicleCategories[v]), X[0][j][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT)//TODO: This code was written just to save the day, must be reconsidered in relation to the problem model's objective function calculation method
            {
                //Second term Part I: distance-based costs from customer to customer directly
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        for (int v = 0; v < numVehCategories; v++)
                            objFunction.AddTerm(Distance(sFrom, sTo), X[i][j][v]);
                    }
                }
                //Second term Part II: distance-based costs from customer to customer through an ES
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int r = 0; r < numES; r++)
                    {
                        Site ES = ExternalStations[r];
                        for (int j = 0; j < numNonESNodes; j++)
                        {
                            Site sTo = preprocessedSites[j];
                            objFunction.AddTerm(Distance(sFrom, ES) + Distance(ES, sTo), Y[i][r][j]);
                        }
                    }
                }
            }
            else
            {
                //Second term Part I: distance-based costs from customer to customer directly
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        for (int v = 0; v < numVehCategories; v++)
                            objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                    }
                }
                //Second term Part II: distance-based costs from customer to customer through an ES
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int r = 0; r < numES; r++)
                    {
                        Site ES = ExternalStations[r];
                        for (int j = 0; j < numNonESNodes; j++)
                        {
                            Site sTo = preprocessedSites[j];
                            objFunction.AddTerm(GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES) + Distance(ES, sTo)), Y[i][r][j]);
                        }
                    }
                }
                //Third term: vehicle fixed costs
                for (int j = 0; j < numNonESNodes; j++)
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);
            }
            //Now adding the objective function to the model
            AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            //rig_IKfromNDFEMH();

            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_SetEVArcs();//1
            //AddConstraint_SetGDVArcs();//2
            AddConstraint_NumberOfIncomingVehiclesPerCustomerNode();//3
            AddConstraint_IncomingEVTotalEqualsOutgoingEVTotal();//4
            AddConstraint_IncomingGDVTotalEqualsOutgoingGDVTotal();//5
            //AddConstraint_MaxNumberOfEVs();//6
            AddConstraint_MaxNumberOfGDvs();//7
            AddConstraint_MinNumberOfVehicles();//8
            AddConstraint_MaxEnergyGainAtNonDepotSite();//9
            AddConstraint_DepartureSOCFromCustomerNode();//10
            AddConstraint_DepartureSOCFromESNodeUB();//11
            AddConstraint_ArrivalSOCToESNodeLB();//12
            AddConstraint_SOCRegulationFollowingNondepot();//13
            AddConstraint_SOCRegulationFollowingDepot();//14
            AddConstraint_TimeRegulationFollowingACustomerVisit();//15
            if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
            {
                AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13c
            }
            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
            {
                AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13
                AddConstraint_DepartureSOCFromESNodeLB();//9
            }
            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
            {
                AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging();//16
                AddConstraint_DepartureSOCFromESNodeLB();//17
            }
            AddConstraint_TimeRegulationFromDepotThroughAnESVisit();//18
            AddConstraint_ArrivalTimeLimits();//19
            AddConstraint_TotalTravelTime();//20

            //Some additional cuts
            AddAllCuts();

            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_SetEVArcs() //1
        {
            for (int j = 0; j < numNonESNodes; j++)//Index 0 is the depot
            {
                for (int i = 0; i < numNonESNodes; i++)
                {
                    ILinearNumExpr SetZVariables = LinearNumExpr();
                    SetZVariables.AddTerm(1.0, Z[i][j]);

                    SetZVariables.AddTerm(-1.0, X[i][j][vIndex_EV]);

                    for (int r = 0; r < numES; r++)
                        SetZVariables.AddTerm(-1.0, Y[i][r][j]);
                    string constraint_name = "Nodes_" + i.ToString() + "_" + j.ToString() + "_are_visited_or_not_by_an_EV";
                    allConstraints_list.Add(AddEq(SetZVariables, 0.0, constraint_name));
                }
            }
        }
        //void AddConstraint_SetGDVArcs() //2
        //{
        //    for (int j = 0; j < numNonESNodes; j++)//Index 0 is the depot
        //    {
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            ILinearNumExpr SetWVariables = LinearNumExpr();
        //            SetWVariables.AddTerm(1.0, W[i][j]);

        //            SetWVariables.AddTerm(-1.0, X[i][j][vIndex_GDV]);

        //            string constraint_name = "Nodes_" + i.ToString() + "_" + j.ToString() + "_are_visited_or_not_by_a_GDV";
        //            allConstraints_list.Add(AddEq(SetWVariables, 0.0, constraint_name));
        //        }
        //    }
        //}
        void AddConstraint_NumberOfIncomingVehiclesPerCustomerNode() //3
        {
            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
            {
                ILinearNumExpr IncomingZandWToCustomerNodes = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                {
                    IncomingZandWToCustomerNodes.AddTerm(1.0, Z[i][j]);
                    IncomingZandWToCustomerNodes.AddTerm(1.0, X[i][j][vIndex_GDV]);
                }
                string constraint_name = "Exactly_one_vehicle_must_arrive_the_customer_node_" + j.ToString();
                allConstraints_list.Add(AddEq(IncomingZandWToCustomerNodes, 1.0, constraint_name));
            }
        }
        void AddConstraint_IncomingEVTotalEqualsOutgoingEVTotal() //4
        {
            for (int j = 0; j < numNonESNodes; j++)//Index 0 is the depot
            {
                ILinearNumExpr OutgoingZandWToCustomerNodes = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                {
                    OutgoingZandWToCustomerNodes.AddTerm(1.0, Z[i][j]);
                    OutgoingZandWToCustomerNodes.AddTerm(-1.0, Z[j][i]);
                }
                string constraint_name = "Exactly_one_vehicle_must_arrive_the_customer_node_" + j.ToString();
                allConstraints_list.Add(AddEq(OutgoingZandWToCustomerNodes, 0.0, constraint_name));
            }
        }
        void AddConstraint_IncomingGDVTotalEqualsOutgoingGDVTotal() //5
        {
            for (int i = 0; i < numNonESNodes; i++)//Index 0 is the depot
            {
                ILinearNumExpr OutgoingZandWToCustomerNodes = LinearNumExpr();
                for (int j = 0; j < numNonESNodes; j++)
                {
                    OutgoingZandWToCustomerNodes.AddTerm(1.0, X[i][j][vIndex_GDV]);
                    OutgoingZandWToCustomerNodes.AddTerm(-1.0, X[j][i][vIndex_GDV]);
                }
                string constraint_name = "Exactly_one_vehicle_must_arrive_the_customer_node_" + i.ToString();
                allConstraints_list.Add(AddEq(OutgoingZandWToCustomerNodes, 0.0, constraint_name));
            }
        }
        void AddConstraint_MaxNumberOfEVs()//6
        {
            ILinearNumExpr NumberOfEVsOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < numNonESNodes; j++)
            {
                NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, Z[0][j]);
            }
            string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_EV].ToString();
            allConstraints_list.Add(AddLe(NumberOfEVsOutgoingFromTheDepot, numVehicles[vIndex_EV], constraint_name));
        }
        void AddConstraint_MaxNumberOfGDvs()//7
        {
            ILinearNumExpr NumberOfGDVsOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < numNonESNodes; j++)
            {
                NumberOfGDVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j][vIndex_GDV]);
            }
            string constraint_name = "Number_of_GDVs_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_GDV].ToString();
            allConstraints_list.Add(AddLe(NumberOfGDVsOutgoingFromTheDepot, numVehicles[vIndex_GDV], constraint_name));
        }
        void AddConstraint_MinNumberOfVehicles() //8
        {
            if (numVehicles.Sum() < minNumVeh)
                return;
            ILinearNumExpr NumberOfVehiclesOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < numNonESNodes; j++)
            {
                NumberOfVehiclesOutgoingFromTheDepot.AddTerm(1.0, Z[0][j]);
                NumberOfVehiclesOutgoingFromTheDepot.AddTerm(1.0, X[0][j][vIndex_GDV]);
            }
            string constraint_name = "Number_of_vehicles_outgoing_from_node_0_must_be_greater_than_or_equal_to_" + (minNumVeh).ToString();
            allConstraints_list.Add(AddGe(NumberOfVehiclesOutgoingFromTheDepot, minNumVeh, constraint_name));
        }
        void AddConstraint_MaxEnergyGainAtNonDepotSite()//9 //28
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
                EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {
                    EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], Z[i][j]);
                }
                string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
                allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));
            }
        }
        void AddConstraint_DepartureSOCFromCustomerNode()//10 //29
        {
            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {
                    DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), Z[i][j]);
                }
                string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));
            }
        }
        void AddConstraint_DepartureSOCFromESNodeUB()//11 //30
        {
            for (int r = 0; r < numES; r++)
            {
                Site from = ExternalStations[r];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site to = preprocessedSites[j];
                    ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                    DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                    for (int i = 0; i < numNonESNodes; i++)
                        DepartureSOCFromES.AddTerm(EnergyConsumption(from, to, VehicleCategories.EV), Y[i][r][j]);
                    string constraint_name = "Departure_SOC(UB)_From_ES_" + r.ToString() + "_going_to_customer_" + j.ToString();
                    allConstraints_list.Add(AddLe(DepartureSOCFromES, BatteryCapacity(VehicleCategories.EV), constraint_name));

                }
            }
        }
        void AddConstraint_DepartureSOCFromESNodeLB()//17 //31
        {
            for (int r = 0; r < numES; r++)
            {
                Site ES = ExternalStations[r];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site to = preprocessedSites[j];
                    ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                    DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                    for (int i = 0; i < numNonESNodes; i++)
                        DepartureSOCFromES.AddTerm((EnergyConsumption(ES, to, VehicleCategories.EV) - BatteryCapacity(VehicleCategories.EV) + minValue_Delta[j]), Y[i][r][j]);
                    string constraint_name = "Departure_SOC(LB)_From_ES_" + r.ToString() + "_going_to_" + j.ToString();
                    allConstraints_list.Add(AddGe(DepartureSOCFromES, minValue_Delta[j], constraint_name));

                }
            }
        }
        void AddConstraint_ArrivalSOCToESNodeLB()//12 //none
        {
            for (int j = 0; j < numNonESNodes; j++)
                for (int r = 0; r < numES; r++)
                {
                    Site from = preprocessedSites[j];
                    Site ES = ExternalStations[r];
                    ILinearNumExpr ArrivalSOCFromES = LinearNumExpr();
                    ArrivalSOCFromES.AddTerm(1.0, Delta[j]);
                    ArrivalSOCFromES.AddTerm(1.0, Epsilon[j]);
                    for (int k = 0; k < numNonESNodes; k++)
                        ArrivalSOCFromES.AddTerm(-1.0 * EnergyConsumption(from, ES, VehicleCategories.EV), Y[j][r][k]);
                    string constraint_name = "Arrival_SOC(LB)_To_ES_" + r.ToString() + "_from_" + j.ToString();
                    allConstraints_list.Add(AddGe(ArrivalSOCFromES, 0.0, constraint_name));

                }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()//13 //32
        {
            for (int i = 1; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, Delta[j]);
                    SOCDifference.AddTerm(-1.0, Delta[i]);
                    SOCDifference.AddTerm(-1.0, Epsilon[i]);
                    SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV) + BigDelta[i][j], X[i][j][vIndex_EV]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, BigDelta[i][j], constraint_name));
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()//14 //33
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                SOCDifference.AddTerm(EnergyConsumption(TheDepot, sTo, VehicleCategories.EV), X[0][j][vIndex_EV]);
                for (int r = 0; r < numES; r++)
                {
                    Site ES = ExternalStations[r];
                    SOCDifference.AddTerm(EnergyConsumption(ES, sTo, VehicleCategories.EV), Y[0][r][j]);
                }
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
            }
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()//15 //34
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
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
                }
        }
        void AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging()//16 //35 Only if recharging is full (FF) 
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[r];
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[i][j]), Y[i][r][j]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging()//13b Only in VF, VP cases
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[r];
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
                        double timeSpentDueToEnergyConsumption = (EnergyConsumption(sFrom, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES);
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + travelDuration + timeSpentDueToEnergyConsumption + BigTVarRecharge[i][r][j]), Y[i][r][j]);
                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (BigTVarRecharge[i][r][j]), constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationThroughAnESVisits_VariableFullTimeRecharging()//13b Only in VF, VP cases
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[r];
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
                        double timeSpentDueToEnergyConsumption = (BatteryCapacity(VehicleCategories.EV) + EnergyConsumption(sFrom, ES, VehicleCategories.EV)) / RechargingRate(ES);
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (BigTVarRecharge[i][r][j]), Y[i][r][j]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        double rhs = -1.0 * BigTVarRecharge[i][r][j] + ServiceDuration(sFrom);
                        allConstraints_list.Add(AddGe(TimeDifference, rhs, constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationFromDepotThroughAnESVisit()//18 //none
        {
            for (int r = 0; r < numES; r++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site ES = ExternalStations[r];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);

                    // Here we decide whether recharging duration is fixed or depends on the arrival SOC
                    if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                    {
                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j], constraint_name));
                    }
                    else
                    {
                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + ((EnergyConsumption(TheDepot, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j] - BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES), constraint_name));
                    }
                }
        }
        void AddConstraint_ArrivalTimeLimits()//19 //none
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {
                    for (int v = 0; v < numVehCategories; v++)
                        TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), X[i][j][v]);
                    for (int r = 0; r < numES; r++)
                        TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), Y[i][r][j]);
                }
                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, maxValue_T[j], constraint_name));
            }
        }
        void AddConstraint_TotalTravelTime()//20 //none
        {
            totalTravelTimeConstraintIndex = allConstraints_list.Count;

            ILinearNumExpr TotalTravelTime = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j][v]);
                    for (int r = 0; r < numES; r++)
                    {
                        Site ES = ExternalStations[r];
                        TotalTravelTime.AddTerm((TravelTime(sFrom, ES) + TravelTime(ES, sTo)), Y[i][r][j]);
                    }
                }

            string constraint_name = "Total_Travel_Time";
            double rhs = (xCplexParam.TSP ? 1 : theProblemModel.GetNumVehicles(VehicleCategories.EV) + theProblemModel.GetNumVehicles(VehicleCategories.GDV)) * theProblemModel.CRD.TMax;
            rhs -= theProblemModel.SRD.GetTotalCustomerServiceTime();
            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));

        }


        void AddAllCuts()
        {
            AddCut_TimeFeasibilityOfTwoConsecutiveArcs();
            AddCut_EnergyFeasibilityOfCustomerBetweenTwoES();
            AddCut_TotalNumberOfActiveArcs();
        }
        void AddCut_TimeFeasibilityOfTwoConsecutiveArcs()//16
        {
            for (int i = 0; i < numNonESNodes; i++)//This was starting at 1
            {
                Site from = preprocessedSites[i];
                for (int k = 1; k < numNonESNodes; k++)
                {
                    Site through = preprocessedSites[k];
                    if (X[i][k][0].UB == 0.0)//The direct arc (from,through) has already been marked infeasible
                        continue;
                    for (int j = 0; j < numNonESNodes; j++)//This was starting at 1
                    {
                        Site to = preprocessedSites[j];
                        if (X[k][j][0].UB == 0.0)//The direct arc (through,to) has already been marked infeasible
                            continue;
                        if (i != j && j != k && i != k)
                        {
                            ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                            if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
                            {
                                for (int v = 0; v < numVehCategories; v++)
                                {
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][v]);
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][v]);
                                }
                                for (int r = 0; r < numES; r++)
                                {
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r][k]);
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r][j]);
                                }
                                string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                                allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                            }
                            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                            {
                                for (int r1 = 0; r1 < numES; r1++)
                                {
                                    Site ES1 = ExternalStations[r1];
                                    double fixedChargeTimeAtES1 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES1.RechargingRate;
                                    if (fixedChargeTimeAtES1 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//Not even one visit through ES is allowed
                                    {
                                        for (int v = 0; v < numVehCategories; v++)
                                        {
                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][v]);
                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][v]);
                                        }
                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][k]);
                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r1][j]);
                                        string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ES_" + r1.ToString();
                                        allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                        TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                    }
                                    else
                                    {
                                        for (int r2 = 0; r2 < numES; r2++)
                                            if (r2 != r1)
                                            {
                                                Site ES2 = ExternalStations[r2];
                                                double fixedChargeTimeAtES2 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES2.RechargingRate;
                                                if (fixedChargeTimeAtES1 + fixedChargeTimeAtES2 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//ES1 was fine by itself but not together with ES2
                                                {
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][k]);
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r1][j]);
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r2][k]);
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r2][j]);
                                                    string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ESs_" + r1.ToString() + "_and_" + r2.ToString();
                                                    allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                                    TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                                }
                                            }
                                    }
                                }
                            }
                        }//if (i != j && j != k && i != k)
                    }
                }
            }
        }
        void AddCut_EnergyFeasibilityOfTwoConsecutiveArcs()//17
        {
            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
            for (int k = 1; k < numNonESNodes; k++)
            {
                Site through = preprocessedSites[k];

                //Both direct
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site from = preprocessedSites[i];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > maxValue_Delta[i] - minValue_Delta[j] + maxValue_Epsilon[k])//This was ignoring maxValue_Epsilon[k]. It was also ignoring delta bounds and was just using battery capacity (BatteryCapacity(VehicleCategories.EV)) 
                        {
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
                        }
                    }
                }

                //First from ES, then direct
                for (int r = 0; r < numES; r++)
                {
                    Site ES = ExternalStations[r];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (EnergyConsumption(ES, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j] + maxValue_Epsilon[k])
                        {
                            for (int i = 0; i < numNonESNodes; i++)
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r][k]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
                            string constraint_name = "No_arc_from_ES_" + r.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
                        }
                    }
                }

                //First direct, then to ES
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site from = preprocessedSites[i];
                    for (int r = 0; r < numES; r++)
                    {
                        Site ES = ExternalStations[r];
                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, ES, VehicleCategories.EV) > maxValue_Delta[i] + maxValue_Epsilon[k])
                        {
                            for (int j = 0; j < numNonESNodes; j++)
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r][j]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "_to_ES_" + r.ToString();
                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
                        }
                    }
                }
            }
        }
        void AddCut_EnergyFeasibilityOfCustomerBetweenTwoES()
        {
            for (int r1 = 0; r1 < numES; r1++)
            {
                Site ES1 = ExternalStations[r1];
                for (int j = 1; j < numNonESNodes; j++)
                {
                    Site customer = preprocessedSites[j];
                    for (int r2 = 0; r2 < numES; r2++)
                    {
                        Site ES2 = ExternalStations[r2];
                        if (EnergyConsumption(ES1, customer, VehicleCategories.EV) + EnergyConsumption(customer, ES2, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) + maxValue_Epsilon[j])//This didn't have the maxValue_Epsilon in it and hence it ignored the SOE gain at ISs
                        {
                            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                            for (int i = 0; i < numNonESNodes; i++)
                            {
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][j]);
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[j][r2][i]);
                            }
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

            int nActiveArcs = numVehicles[0] + numVehicles[1] + numCustomers;
            int nActiveArcs_EV = numVehicles[vIndex_EV] + numCustomers;
            int nActiveArcs_GDV = numVehicles[vIndex_GDV] + numCustomers;
            ILinearNumExpr totalArcFlow = LinearNumExpr();
            ILinearNumExpr totalArcFlow_EV = LinearNumExpr();
            ILinearNumExpr totalArcFlow_GDV = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    totalArcFlow.AddTerm(1.0, X[i][j][vIndex_GDV]);
                    totalArcFlow.AddTerm(1.0, Z[i][j]);

                    totalArcFlow_EV.AddTerm(1.0, Z[i][j]);
                    totalArcFlow_GDV.AddTerm(1.0, X[i][j][vIndex_GDV]);
                }
            string constraintName_overall = "Total_number_of_active_arcs_cannot_exceed_" + nActiveArcs.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow, (double)nActiveArcs, constraintName_overall));
            string constraintName_EV = "Number_of_active_EV_arcs_cannot_exceed_" + nActiveArcs_EV.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow_EV, (double)nActiveArcs_EV, constraintName_EV));
            string constraintName_GDV = "Number_of_active_GDV_arcs_cannot_exceed_" + nActiveArcs_GDV.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow_GDV, (double)nActiveArcs_GDV, constraintName_GDV));
        }
        void AddCut_EnergyConservation()//17
        {
            ILinearNumExpr EnergyConservation = LinearNumExpr();
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int r = 0; r < numES; r++)
                {
                    Site through = ExternalStations[r];
                    for (int j = 0; j < NumPreprocessedSites; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        EnergyConservation.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV), X[i][j][vIndex_EV]);
                        EnergyConservation.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][vIndex_EV]);
                        EnergyConservation.AddTerm(EnergyConsumption(sFrom, through, VehicleCategories.EV) + EnergyConsumption(through, sTo, VehicleCategories.EV), Y[i][r][j]);
                        EnergyConservation.AddTerm(-1.0 * BatteryCapacity(VehicleCategories.EV), Y[i][r][j]);
                    }
                }
            }
            string constraint_name = "Energy conservation";
            allConstraints_list.Add(AddLe(EnergyConservation, numVehicles[vIndex_EV] * BatteryCapacity(VehicleCategories.EV), constraint_name));
            EnergyConservation.Clear();
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
            //TODO: Delete the following after debugging. Update on 11/10/17: Is this still relevant?
            //GetDecisionVariableValues();

            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

            //We first determine the route start points
            List<List<int>> listOfFirstSiteIndices = new List<List<int>>();
            for (int j = 0; j < numNonESNodes; j++)
                if (GetValue(X[0][j][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    listOfFirstSiteIndices.Add(new List<int>() { j });
                }
            if (vehicleCategory != VehicleCategories.GDV)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                        if (GetValue(Y[0][r][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                        {
                            listOfFirstSiteIndices.Add(new List<int>() { r, j });
                        }
            //Then, populate the whole routes (actually, not routes yet)
            List<List<string>> outcome = new List<List<string>>();
            foreach (List<int> firstSiteIndices in listOfFirstSiteIndices)
            {
                outcome.Add(GetNonDepotSiteIDs(firstSiteIndices, vehicleCategory));
            }
            return outcome;
        }
        public void GetDecisionVariableValues()
        {
            //System.IO.StreamWriter sw = new System.IO.StreamWriter("routes.txt");
            double[,,] xValues = new double[numNonESNodes, numNonESNodes, numVehCategories];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    xValues[i, j, 0] = GetValue(X[i][j][0]);//CONSULT (w/ Isil): Why only 0 when xValues is defined over all numVehCategories? IK: This was just debugging purposes, since EMH does not have any GDVs, I only wrote [0].
            double[,,] yValues = new double[numNonESNodes, numES, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                        yValues[i, r, j] = GetValue(Y[i][r][j]);

            double[] epsilonValues = new double[numNonESNodes];
            for (int j = 0; j < numNonESNodes; j++)
                epsilonValues[j] = GetValue(Epsilon[j]);
            double[] deltaValues = new double[numNonESNodes];
            for (int j = 0; j < numNonESNodes; j++)
                deltaValues[j] = GetValue(Delta[j]);
            double[] TValues = new double[numNonESNodes];
            for (int j = 0; j < numNonESNodes; j++)
                TValues[j] = GetValue(T[j]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstSiteIndices"></param>Can contain one or two elements. If one, it's for an X arc; if two, it's for a Y arc; nothing else is possible!
        /// <param name="vehicleCategory"></param>Obviously if GDV, there won't be any Y arcs
        /// <returns></returns>
        List<string> GetNonDepotSiteIDs(List<int> firstSiteIndices, VehicleCategories vehicleCategory)
        {
            if ((firstSiteIndices.Count > 2) || ((vehicleCategory == VehicleCategories.GDV) && (firstSiteIndices.Count > 1)))
                throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called with too many firstSiteIndices!");

            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
            List<string> outcome = new List<string>();

            if (firstSiteIndices.Count == 1)
                if (GetValue(X[0][firstSiteIndices.Last()][vc_int]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to an X-flow from the depot!");
                }
            if (firstSiteIndices.Count == 2)
                if (GetValue(Y[0][firstSiteIndices.First()][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to a Y-flow from the depot!");
                }

            List<int> currentSiteIndices = firstSiteIndices;
            List<int> nextSiteIndices;
            singleRouteESvisits = new IndividualRouteESVisits();
            int i = 0, r = 0, j = 0;
            do
            {
                j = currentSiteIndices.Last();
                if (currentSiteIndices.Count == 2)
                {
                    outcome.Add(ExternalStations[currentSiteIndices.First()].ID);
                    r = currentSiteIndices.First();
                    singleRouteESvisits.Add(GetIndividualESVisit(i, r, j));
                }
                outcome.Add(preprocessedSites[currentSiteIndices.Last()].ID);

                nextSiteIndices = GetNextSiteIndices(currentSiteIndices.Last(), vehicleCategory);
                i = currentSiteIndices.Last();
                if (preprocessedSites[nextSiteIndices.Last()].ID == TheDepot.ID)
                {
                    if (nextSiteIndices.Count == 2)
                    {
                        r = nextSiteIndices.First();
                        j = 0;
                        outcome.Add(ExternalStations[nextSiteIndices.First()].ID);
                        singleRouteESvisits.Add(GetIndividualESVisit(i, r, j));
                    }
                    allRoutesESVisits.Add(singleRouteESvisits);
                    return outcome;
                }
                currentSiteIndices = nextSiteIndices;
            }
            while (preprocessedSites[currentSiteIndices.Last()].ID != TheDepot.ID);

            allRoutesESVisits.Add(singleRouteESvisits);
            return outcome;
        }
        List<int> GetNextSiteIndices(int currentSiteIndex, VehicleCategories vehicleCategory)
        {
            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
            for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
                if (GetValue(X[currentSiteIndex][nextCustomerIndex][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    return new List<int>() { nextCustomerIndex };
                }
            if (vehicleCategory == VehicleCategories.EV)
                for (int nextESIndex = 0; nextESIndex < numES; nextESIndex++)
                    for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
                        if (GetValue(Y[currentSiteIndex][nextESIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            return new List<int>() { nextESIndex, nextCustomerIndex };
            throw new System.Exception("Flow ended before returning to the depot!");
        }
        IndividualESVisitDataPackage GetIndividualESVisit(int i, int r, int j)
        {
            Site from = preprocessedSites[i];
            Site ES = ExternalStations[r];
            Site to = preprocessedSites[j];

            double timeSpentInES = GetValue(T[j]) - TravelTime(from, ES) - ServiceDuration(from) - GetValue(T[i]) - TravelTime(ES, to);
            return new IndividualESVisitDataPackage(ES.ID, timeSpentInES / RechargingRate(ES), preprocessedESSiteIndex: r);
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        public override string GetModelName()
        {
            return "Arc Duplicating without U";
        }
        public override void RefineDecisionVariables(CustomerSet cS)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            int VCIndex = (int)xCplexParam.VehCategory;
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
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

            if (totalTravelTimeConstraintIndex >= 0)
                allConstraints_array[totalTravelTimeConstraintIndex].UB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
            if (totalNumberOfActiveArcsConstraintIndex >= 0)
            {
                allConstraints_array[totalNumberOfActiveArcsConstraintIndex].UB = (double)(cS.NumberOfCustomers + 1);
                if (numVehicles[vIndex_GDV] == 0)//EV_TSP
                {
                    allConstraints_array[totalNumberOfActiveArcsConstraintIndex + 1].UB = (double)(cS.NumberOfCustomers + 1);
                    allConstraints_array[totalNumberOfActiveArcsConstraintIndex + 2].UB = 0.0;
                }
                else//GDV-TSP
                {
                    allConstraints_array[totalNumberOfActiveArcsConstraintIndex + 1].UB = 0.0;
                    allConstraints_array[totalNumberOfActiveArcsConstraintIndex + 2].UB = (double)(cS.NumberOfCustomers + 1);
                }
            }
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

    }
}

