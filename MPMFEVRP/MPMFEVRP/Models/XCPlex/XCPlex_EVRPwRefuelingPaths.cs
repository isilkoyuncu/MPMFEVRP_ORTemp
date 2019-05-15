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
    public class XCPlex_EVRPwRefuelingPaths : XCPlexVRPBase
    {
        int numCustomers, numES, numNonESNodes;
        RefuelingPathGenerator rpg;
        RefuelingPathList rpl;
        AllPairsShortestPaths apss;

        INumVar[][][][] X; double[][][][] X_LB, X_UB; //X_ijvp=1 if a vehicle v travels from i to j using refueling path p, for v=GDV p is always 0
        INumVar[][][] U; double[][][] U_LB, U_UB; //U_ijp total travel time from i to j including the refueling time on refueling path p
        INumVar[] Epsilon; double[] Epsilon_LB, Epsilon_UB; //Epsilon_j=energy gained at customer j if customer j is visited by an AFV, Epsilon_LB otherwise
        INumVar[][][] EpsilonR; double[][][] EpsilonR_LB, EpsilonR_UB; //Epsilon_ijp=energy gained on refueling path p, from i to j, if p=0 then EpsilonR=LB.
        INumVar[] Delta; double[] Delta_LB, Delta_UB; //Delta_j=departure SOC at customer j, Delta_LB otherwise 
        INumVar[] T; double[] T_LB, T_UB;//T_j=remaining duration of the workday when departing from customer j, Delta_LB otherwise

        public XCPlex_EVRPwRefuelingPaths() { }
        public XCPlex_EVRPwRefuelingPaths(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            :base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
        }
        protected override void DefineDecisionVariables()
        {
            PreprocessSites();
            numCustomers = theProblemModel.SRD.NumCustomers;
            numES = theProblemModel.SRD.NumES;
            numNonESNodes = numCustomers + 1;

            SetNondominatedRefuelingPaths();

            SetMinAndMaxValuesOfModelSpecificVariables();
            allVariables_list = new List<INumVar>();

            //dvs: X_ipjv
            AddFourDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, out X);

            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out T);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numNonESNodes, out Delta);
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numNonESNodes, out Epsilon);

            //All variables defined
            allVariables_array = allVariables_list.ToArray();

            //Now we need to set some to the variables to 0

        }

        void SetNondominatedRefuelingPaths()
        {
            apss = new AllPairsShortestPaths(theProblemModel.SRD.Distance, theProblemModel.SRD.GetAllIDs().ToArray());
            
            rpg = new RefuelingPathGenerator(apss);
            rpl = new RefuelingPathList();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    rpg.GenerateSingleESNonDominatedBetweenODPair(preprocessedSites[i], preprocessedSites[j], ExternalStations, theProblemModel.SRD);

                    //rpl.AddRange(rpg.GenerateNonDominatedBetweenODPair(preprocessedSites[i], preprocessedSites[j], ExternalStations, theProblemModel.SRD, 0, numES));
                }

        }
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {
            //Four dimensional
            X_LB = new double[numNonESNodes][][][];//Xijvp: (i,j) in nonESNodes, p in refuelingPaths(i,j), v in vehicleCategories
            X_UB = new double[numNonESNodes][][][];
            
            //Three dimensional
            U_LB = new double[numNonESNodes][][];//Uijp: i,j in nonESNodes, p in refuelingPaths(i,j)
            U_UB = new double[numNonESNodes][][];
            EpsilonR_LB = new double[numNonESNodes][][];//Epsilon_ijp: i,j in nonESNodes, p in refuelingPaths(i,j)
            EpsilonR_UB = new double[numNonESNodes][][];

            for (int i = 0; i < numNonESNodes; i++)
            {
                X_LB[i] = new double[numNonESNodes][][];
                X_UB[i] = new double[numNonESNodes][][];
                
                U_LB[i] = new double[numNonESNodes][];
                U_UB[i] = new double[numNonESNodes][];
                EpsilonR_LB[i] = new double[numNonESNodes][];
                EpsilonR_UB[i] = new double[numNonESNodes][];

                for (int j = 0; j < numNonESNodes; j++)
                {
                    X_LB[i][j] = new double[numVehCategories][];
                    X_UB[i][j] = new double[numVehCategories][];

                    int l_rp = 3;// refuelingPathSet[i][j].Length;
                    X_LB[i][j][0] = new double[l_rp];
                    X_UB[i][j][0] = new double[l_rp];
                    X_LB[i][j][1] = new double[1]; //Length is 1 since no other arc is possible for a GDV other than a direct arc.
                    X_LB[i][j][1] = new double[1];

                    U_LB[i][j] = new double[l_rp];
                    U_UB[i][j] = new double[l_rp];
                    EpsilonR_LB[i][j] = new double[l_rp];
                    EpsilonR_UB[i][j] = new double[l_rp];

                    for (int p = 0; p < l_rp; p++)
                    {
                        X_LB[i][j][0][p] = 0.0;
                        X_UB[i][j][0][p] = 1.0;
                        U_LB[i][j][p] = 0;
                        U_UB[i][j][p] = theProblemModel.SRD.GetSingleDepotSite().DueDate;
                        EpsilonR_LB[i][j][p] = 0;
                        EpsilonR_UB[i][j][p] = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
                    }

                }
                
            }
        }

        //protected override void AddAllConstraints()
        //{
        //    allConstraints_list = new List<IRange>();
        //    //Now adding the constraints one (family) at a time
        //    AddConstraint_NumberOfVisitsPerCustomerNode();//1
        //    AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV();//2
        //    AddConstraint_IncomingXTotalEqualsOutgoingXTotalforGDV();//3

        //    if ((xCplexParam.TSP) || (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT))//This is the case for both TSP and Orienteering models encountered. For the broader model, we don't impose any constraints. MinimizeVMT objective check is made to simply understand the EMH problems.
        //        AddConstraint_MaxNumberOfGDVs();//5

        //    if (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
        //    {
        //        AddConstraint_MaxNumberOfEVs();//4
        //        //AddConstraint_MaxNumberOfGDVs();//5
        //    }
        //    else //Minimize
        //    {
        //        AddConstraint_MaxNumberOfEVs();//4

        //        AddConstraint_MinNumberOfVehicles();//4-5 b                
        //    }
        //    AddConstraint_MaxEnergyGainAtNonDepotSite();//6
        //    AddConstraint_DepartureSOCFromCustomerNode();//7
        //    AddConstraint_DepartureSOCFromESNodeUB();//8
        //    AddConstraint_ArrivalSOCToESNodeLB();//9b
        //    AddConstraint_SOCRegulationFollowingNondepot();//10
        //    AddConstraint_SOCRegulationFollowingDepot();//11
        //    AddConstraint_TimeRegulationFollowingACustomerVisit();//12
        //    if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
        //    {
        //        AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13c
        //    }
        //    else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
        //    {
        //        AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging();//13
        //        AddConstraint_DepartureSOCFromESNodeLB();//9
        //    }
        //    else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
        //    {
        //        AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging();//13a
        //        AddConstraint_DepartureSOCFromESNodeLB();//9
        //    }
        //    AddConstraint_TimeRegulationFromDepotThroughAnESVisit();//13c
        //    AddConstraint_ArrivalTimeLimits();//14
        //    AddConstraint_TotalTravelTime();//15

        //    //Some additional cuts
        //    //AddAllCuts();

        //    //All constraints added
        //    allConstraints_array = allConstraints_list.ToArray();
        //}

        //void AddConstraint_NumberOfVisitsPerCustomerNode() //1
        //{
        //    firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

        //    for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
        //    {
        //        ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            for (int v = 0; v < numVehCategories; v++)
        //                IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j][v]);

        //            for (int r = 0; r < numES; r++)
        //                IncomingXandYToCustomerNodes.AddTerm(1.0, Y[i][r][j]);
        //        }
        //        string constraint_name;

        //        switch (customerCoverageConstraint)
        //        {
        //            case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
        //                constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
        //                allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
        //                break;
        //            case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
        //                constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
        //                allConstraints_list.Add(AddLe(IncomingXandYToCustomerNodes, 1.0, constraint_name));
        //                break;
        //            case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
        //                throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode invoked for CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce, which must not happen for a VRP!");
        //            default:
        //                throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
        //        }

        //    }
        //}
        //void AddConstraint_NumberOfVisitsPerCustomerNode2() //1
        //{
        //    firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

        //    for (int i = 1; i <= numCustomers; i++)//Index 0 is the depot
        //    {
        //        ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            for (int v = 0; v < numVehCategories; v++)
        //                IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j][v]);

        //            for (int r = 0; r < numES; r++)
        //                IncomingXandYToCustomerNodes.AddTerm(1.0, Y[i][r][j]);
        //        }
        //        string constraint_name;

        //        if (xCplexParam.TSP || theProblemModel.CoverConstraintType == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
        //        {
        //            constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + i.ToString();
        //            allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[i], constraint_name));
        //        }
        //        else
        //        {
        //            constraint_name = "At_most_one_vehicle_can_visit_node_" + i.ToString();
        //            allConstraints_list.Add(AddLe(IncomingXandYToCustomerNodes, 1.0, constraint_name));
        //        }
        //    }
        //}
        //void AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV()//2
        //{
        //    for (int j = 0; j < numNonESNodes; j++)
        //    {
        //        ILinearNumExpr IncomingXYTotalMinusOutgoingXYTotal = LinearNumExpr();

        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, X[i][j][vIndex_EV]);
        //            IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, X[j][i][vIndex_EV]);
        //            for (int r = 0; r < numES; r++)
        //            {
        //                IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, Y[i][r][j]);
        //                IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, Y[j][r][i]);
        //            }
        //        }
        //        string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
        //        allConstraints_list.Add(AddEq(IncomingXYTotalMinusOutgoingXYTotal, 0.0, constraint_name));

        //    }
        //}
        //void AddConstraint_IncomingXTotalEqualsOutgoingXTotalforGDV() //3
        //{
        //    for (int j = 0; j < numNonESNodes; j++)
        //    {
        //        ILinearNumExpr IncomingXTotalEqualsOutgoingXTotal = LinearNumExpr();
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            IncomingXTotalEqualsOutgoingXTotal.AddTerm(1.0, X[i][j][vIndex_GDV]);
        //            IncomingXTotalEqualsOutgoingXTotal.AddTerm(-1.0, X[j][i][vIndex_GDV]);
        //        }
        //        string constraint_name = "Number_of_GDVs_incoming_to_node_" + j.ToString() + "_equals_to_the_outgoing_GDVs";
        //        allConstraints_list.Add(base.AddEq(IncomingXTotalEqualsOutgoingXTotal, 0.0, constraint_name));
        //    }
        //}
        //void AddConstraint_MaxNumberOfEVs()//4
        //{
        //    ILinearNumExpr NumberOfEVsOutgoingFromTheDepot = LinearNumExpr();
        //    for (int j = 1; j < numNonESNodes; j++)
        //    {
        //        NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j][vIndex_EV]);
        //        for (int r = 0; r < numES; r++)
        //            NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, Y[0][r][j]);
        //    }
        //    string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_EV].ToString();
        //    allConstraints_list.Add(AddLe(NumberOfEVsOutgoingFromTheDepot, numVehicles[vIndex_EV], constraint_name));
        //}
        //void AddConstraint_MaxNumberOfGDVs()//5
        //{
        //    ILinearNumExpr NumberOfGDVsOutgoingFromTheDepot = LinearNumExpr();
        //    for (int j = 1; j < numNonESNodes; j++)
        //    {
        //        NumberOfGDVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j][vIndex_GDV]);
        //    }
        //    string constraint_name = "Number_of_GDVs_outgoing_from_node_0_cannot_exceed_" + numVehicles[vIndex_GDV].ToString();
        //    allConstraints_list.Add(AddLe(NumberOfGDVsOutgoingFromTheDepot, numVehicles[vIndex_GDV], constraint_name));
        //}
        //void AddConstraint_MinNumberOfVehicles() //4-5 b
        //{
        //    if (numVehicles.Sum() < minNumVeh)
        //        return;
        //    ILinearNumExpr NumberOfVehiclesOutgoingFromTheDepot = LinearNumExpr();
        //    for (int j = 1; j < numNonESNodes; j++)
        //    {
        //        for (int v = 0; v < numVehCategories; v++)
        //            NumberOfVehiclesOutgoingFromTheDepot.AddTerm(1.0, X[0][j][v]);
        //        for (int r = 0; r < numES; r++)
        //            NumberOfVehiclesOutgoingFromTheDepot.AddTerm(1.0, Y[0][r][j]);
        //    }
        //    string constraint_name = "Number_of_vehicles_outgoing_from_node_0_must_be_greater_than_or_equal_to_" + (minNumVeh).ToString();
        //    allConstraints_list.Add(AddGe(NumberOfVehiclesOutgoingFromTheDepot, minNumVeh, constraint_name));
        //}
        //void AddConstraint_MaxEnergyGainAtNonDepotSite()//6
        //{
        //    for (int j = 1; j < numNonESNodes; j++)
        //    {
        //        ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
        //        EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][vIndex_EV]);
        //            for (int r = 0; r < numES; r++)
        //                EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], Y[i][r][j]);
        //        }
        //        string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
        //        allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));
        //    }
        //}
        //void AddConstraint_DepartureSOCFromCustomerNode()//7
        //{
        //    for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
        //    {
        //        ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
        //        DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
        //        DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j][vIndex_EV]);
        //            for (int r = 0; r < numES; r++)
        //                DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), Y[i][r][j]);
        //        }
        //        string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
        //        allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));
        //    }
        //}
        //void AddConstraint_DepartureSOCFromESNodeUB()//8
        //{
        //    for (int r = 0; r < numES; r++)
        //    {
        //        Site from = ExternalStations[r];
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            Site to = preprocessedSites[j];
        //            ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
        //            DepartureSOCFromES.AddTerm(1.0, Delta[j]);
        //            for (int i = 0; i < numNonESNodes; i++)
        //                DepartureSOCFromES.AddTerm(EnergyConsumption(from, to, VehicleCategories.EV), Y[i][r][j]);
        //            string constraint_name = "Departure_SOC(UB)_From_ES_" + r.ToString() + "_going_to_customer_" + j.ToString();
        //            allConstraints_list.Add(AddLe(DepartureSOCFromES, BatteryCapacity(VehicleCategories.EV), constraint_name));

        //        }
        //    }
        //}
        //void AddConstraint_DepartureSOCFromESNodeLB()//9
        //{
        //    for (int r = 0; r < numES; r++)
        //    {
        //        Site ES = ExternalStations[r];
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            Site to = preprocessedSites[j];
        //            ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
        //            DepartureSOCFromES.AddTerm(1.0, Delta[j]);
        //            for (int i = 0; i < numNonESNodes; i++)
        //                DepartureSOCFromES.AddTerm((EnergyConsumption(ES, to, VehicleCategories.EV) - BatteryCapacity(VehicleCategories.EV) + minValue_Delta[j]), Y[i][r][j]);
        //            string constraint_name = "Departure_SOC(LB)_From_ES_" + r.ToString() + "_going_to_" + j.ToString();
        //            allConstraints_list.Add(AddGe(DepartureSOCFromES, minValue_Delta[j], constraint_name));

        //        }
        //    }
        //}
        //void AddConstraint_ArrivalSOCToESNodeLB()//9b
        //{
        //    for (int j = 0; j < numNonESNodes; j++)
        //        for (int r = 0; r < numES; r++)
        //        {
        //            Site from = preprocessedSites[j];
        //            Site ES = ExternalStations[r];
        //            ILinearNumExpr ArrivalSOCFromES = LinearNumExpr();
        //            ArrivalSOCFromES.AddTerm(1.0, Delta[j]);
        //            ArrivalSOCFromES.AddTerm(1.0, Epsilon[j]);
        //            for (int k = 0; k < numNonESNodes; k++)
        //                ArrivalSOCFromES.AddTerm(-1.0 * EnergyConsumption(from, ES, VehicleCategories.EV), Y[j][r][k]);
        //            string constraint_name = "Arrival_SOC(LB)_To_ES_" + r.ToString() + "_from_" + j.ToString();
        //            allConstraints_list.Add(AddGe(ArrivalSOCFromES, 0.0, constraint_name));

        //        }
        //}
        //void AddConstraint_SOCRegulationFollowingNondepot()//10
        //{
        //    for (int i = 1; i < numNonESNodes; i++)
        //    {
        //        Site sFrom = preprocessedSites[i];
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            Site sTo = preprocessedSites[j];
        //            ILinearNumExpr SOCDifference = LinearNumExpr();
        //            SOCDifference.AddTerm(1.0, Delta[j]);
        //            SOCDifference.AddTerm(-1.0, Delta[i]);
        //            SOCDifference.AddTerm(-1.0, Epsilon[i]);
        //            SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV) + BigDelta[i][j], X[i][j][vIndex_EV]);
        //            string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
        //            allConstraints_list.Add(AddLe(SOCDifference, BigDelta[i][j], constraint_name));
        //        }
        //    }
        //}
        //void AddConstraint_SOCRegulationFollowingDepot()//11
        //{
        //    for (int j = 0; j < numNonESNodes; j++)
        //    {
        //        Site sTo = preprocessedSites[j];
        //        ILinearNumExpr SOCDifference = LinearNumExpr();
        //        SOCDifference.AddTerm(1.0, Delta[j]);
        //        SOCDifference.AddTerm(EnergyConsumption(TheDepot, sTo, VehicleCategories.EV), X[0][j][vIndex_EV]);
        //        for (int r = 0; r < numES; r++)
        //        {
        //            Site ES = ExternalStations[r];
        //            SOCDifference.AddTerm(EnergyConsumption(ES, sTo, VehicleCategories.EV), Y[0][r][j]);
        //        }
        //        string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
        //        allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
        //    }
        //}
        //void AddConstraint_TimeRegulationFollowingACustomerVisit()//12
        //{
        //    for (int i = 1; i < numNonESNodes; i++)
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            Site sFrom = preprocessedSites[i];
        //            Site sTo = preprocessedSites[j];
        //            ILinearNumExpr TimeDifference = LinearNumExpr();
        //            TimeDifference.AddTerm(1.0, T[j]);
        //            TimeDifference.AddTerm(-1.0, T[i]);
        //            for (int v = 0; v < numVehCategories; v++)
        //                TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j][v]);
        //            string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
        //            allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
        //        }
        //}
        //void AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging()//13a Only if recharging is full (FF) 
        //{
        //    for (int i = 1; i < numNonESNodes; i++)
        //        for (int r = 0; r < numES; r++)
        //            for (int j = 0; j < numNonESNodes; j++)
        //            {
        //                Site sFrom = preprocessedSites[i];
        //                Site ES = ExternalStations[r];
        //                Site sTo = preprocessedSites[j];
        //                ILinearNumExpr TimeDifference = LinearNumExpr();
        //                TimeDifference.AddTerm(1.0, T[j]);
        //                TimeDifference.AddTerm(-1.0, T[i]);
        //                TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[i][j]), Y[i][r][j]);
        //                string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
        //            }
        //}
        //void AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging()//13b Only in VF, VP cases
        //{
        //    for (int i = 1; i < numNonESNodes; i++)
        //        for (int r = 0; r < numES; r++)
        //            for (int j = 0; j < numNonESNodes; j++)
        //            {
        //                Site sFrom = preprocessedSites[i];
        //                Site ES = ExternalStations[r];
        //                Site sTo = preprocessedSites[j];
        //                ILinearNumExpr TimeDifference = LinearNumExpr();
        //                double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
        //                double timeSpentDueToEnergyConsumption = (EnergyConsumption(sFrom, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES);
        //                TimeDifference.AddTerm(1.0, T[j]);
        //                TimeDifference.AddTerm(-1.0, T[i]);
        //                TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + travelDuration + timeSpentDueToEnergyConsumption + BigTVarRecharge[i][r][j]), Y[i][r][j]);
        //                TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
        //                TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
        //                TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
        //                string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (BigTVarRecharge[i][r][j]), constraint_name));
        //            }
        //}
        //void AddConstraint_TimeRegulationThroughAnESVisits_VariableFullTimeRecharging()//13b Only in VF, VP cases
        //{
        //    for (int i = 1; i < numNonESNodes; i++)
        //        for (int r = 0; r < numES; r++)
        //            for (int j = 0; j < numNonESNodes; j++)
        //            {
        //                Site sFrom = preprocessedSites[i];
        //                Site ES = ExternalStations[r];
        //                Site sTo = preprocessedSites[j];
        //                ILinearNumExpr TimeDifference = LinearNumExpr();
        //                double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
        //                double timeSpentDueToEnergyConsumption = (BatteryCapacity(VehicleCategories.EV) + EnergyConsumption(sFrom, ES, VehicleCategories.EV)) / RechargingRate(ES);
        //                TimeDifference.AddTerm(1.0, T[j]);
        //                TimeDifference.AddTerm(-1.0, T[i]);
        //                TimeDifference.AddTerm(-1.0 * (BigTVarRecharge[i][r][j]), Y[i][r][j]);
        //                TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
        //                TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
        //                string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                double rhs = -1.0 * BigTVarRecharge[i][r][j] + ServiceDuration(sFrom);
        //                allConstraints_list.Add(AddGe(TimeDifference, rhs, constraint_name));
        //            }
        //}
        //void AddConstraint_TimeRegulationFromDepotThroughAnESVisit()//13c
        //{
        //    for (int r = 0; r < numES; r++)
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            Site ES = ExternalStations[r];
        //            Site sTo = preprocessedSites[j];
        //            ILinearNumExpr TimeDifference = LinearNumExpr();
        //            TimeDifference.AddTerm(1.0, T[j]);

        //            // Here we decide whether recharging duration is fixed or depends on the arrival SOC
        //            if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
        //            {
        //                TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
        //                string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j], constraint_name));
        //            }
        //            else
        //            {
        //                TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + ((EnergyConsumption(TheDepot, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), Y[0][r][j]);
        //                TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
        //                string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
        //                allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j] - BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES), constraint_name));
        //            }
        //        }
        //}
        //void AddConstraint_ArrivalTimeLimits()//14
        //{
        //    for (int j = 1; j < numNonESNodes; j++)
        //    {
        //        ILinearNumExpr TimeDifference = LinearNumExpr();
        //        TimeDifference.AddTerm(1.0, T[j]);
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            for (int v = 0; v < numVehCategories; v++)
        //                TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), X[i][j][v]);
        //            for (int r = 0; r < numES; r++)
        //                TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), Y[i][r][j]);
        //        }
        //        string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
        //        allConstraints_list.Add(AddGe(TimeDifference, maxValue_T[j], constraint_name));
        //    }
        //}
        //void AddConstraint_TotalTravelTime()//15
        //{
        //    totalTravelTimeConstraintIndex = allConstraints_list.Count;

        //    ILinearNumExpr TotalTravelTime = LinearNumExpr();
        //    for (int i = 0; i < numNonESNodes; i++)
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            Site sFrom = preprocessedSites[i];
        //            Site sTo = preprocessedSites[j];
        //            for (int v = 0; v < numVehCategories; v++)
        //                TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j][v]);
        //            for (int r = 0; r < numES; r++)
        //            {
        //                Site ES = ExternalStations[r];
        //                TotalTravelTime.AddTerm((TravelTime(sFrom, ES) + TravelTime(ES, sTo)), Y[i][r][j]);
        //            }
        //        }

        //    string constraint_name = "Total_Travel_Time";
        //    double rhs = (xCplexParam.TSP ? 1 : theProblemModel.GetNumVehicles(VehicleCategories.EV) + theProblemModel.GetNumVehicles(VehicleCategories.GDV)) * theProblemModel.CRD.TMax;
        //    if (customerCoverageConstraint != CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
        //        rhs -= theProblemModel.SRD.GetTotalCustomerServiceTime();
        //    allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));

        //}

        //void AddAllCuts()
        //{
        //    //AddCut_DeltaTime();
        //    AddCut_TimeFeasibilityOfTwoConsecutiveArcs();
        //    AddCut_EnergyFeasibilityOfCustomerBetweenTwoES();
        //    AddCut_TotalNumberOfActiveArcs();
        //}
        //void AddCut_TimeFeasibilityOfTwoConsecutiveArcs()//16
        //{
        //    for (int i = 0; i < numNonESNodes; i++)//This was starting at 1
        //    {
        //        Site from = preprocessedSites[i];
        //        for (int k = 1; k < numNonESNodes; k++)
        //        {
        //            Site through = preprocessedSites[k];
        //            if (X[i][k][0].UB == 0.0)//The direct arc (from,through) has already been marked infeasible
        //                continue;
        //            for (int j = 0; j < numNonESNodes; j++)//This was starting at 1
        //            {
        //                Site to = preprocessedSites[j];
        //                if (X[k][j][0].UB == 0.0)//The direct arc (through,to) has already been marked infeasible
        //                    continue;
        //                if (i != j && j != k && i != k)
        //                {
        //                    ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
        //                    if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
        //                    {
        //                        for (int v = 0; v < numVehCategories; v++)
        //                        {
        //                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][v]);
        //                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][v]);
        //                        }
        //                        for (int r = 0; r < numES; r++)
        //                        {
        //                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r][k]);
        //                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r][j]);
        //                        }
        //                        string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
        //                        allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                        TimeFeasibilityOfTwoConsecutiveArcs.Clear();
        //                    }
        //                    else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
        //                    {
        //                        for (int r1 = 0; r1 < numES; r1++)
        //                        {
        //                            Site ES1 = ExternalStations[r1];
        //                            double fixedChargeTimeAtES1 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES1.RechargingRate;
        //                            if (fixedChargeTimeAtES1 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//Not even one visit through ES is allowed
        //                            {
        //                                for (int v = 0; v < numVehCategories; v++)
        //                                {
        //                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][v]);
        //                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][v]);
        //                                }
        //                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][k]);
        //                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r1][j]);
        //                                string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ES_" + r1.ToString();
        //                                allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                                TimeFeasibilityOfTwoConsecutiveArcs.Clear();
        //                            }
        //                            else
        //                            {
        //                                for (int r2 = 0; r2 < numES; r2++)
        //                                    if (r2 != r1)
        //                                    {
        //                                        Site ES2 = ExternalStations[r2];
        //                                        double fixedChargeTimeAtES2 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES2.RechargingRate;
        //                                        if (fixedChargeTimeAtES1 + fixedChargeTimeAtES2 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//ES1 was fine by itself but not together with ES2
        //                                        {
        //                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][k]);
        //                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r1][j]);
        //                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r2][k]);
        //                                            TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r2][j]);
        //                                            string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ESs_" + r1.ToString() + "_and_" + r2.ToString();
        //                                            allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                                            TimeFeasibilityOfTwoConsecutiveArcs.Clear();
        //                                        }
        //                                    }
        //                            }
        //                        }
        //                    }
        //                }//if (i != j && j != k && i != k)
        //            }
        //        }
        //    }
        //}
        //void AddCut_EnergyFeasibilityOfTwoConsecutiveArcs()//17
        //{
        //    ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
        //    for (int k = 1; k < numNonESNodes; k++)
        //    {
        //        Site through = preprocessedSites[k];

        //        //Both direct
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            Site from = preprocessedSites[i];
        //            for (int j = 0; j < numNonESNodes; j++)
        //            {
        //                Site to = preprocessedSites[j];
        //                if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > maxValue_Delta[i] - minValue_Delta[j] + maxValue_Epsilon[k])//This was ignoring maxValue_Epsilon[k]. It was also ignoring delta bounds and was just using battery capacity (BatteryCapacity(VehicleCategories.EV)) 
        //                {
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
        //                    string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
        //                    allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
        //                }
        //            }
        //        }

        //        //First from ES, then direct
        //        for (int r = 0; r < numES; r++)
        //        {
        //            Site ES = ExternalStations[r];
        //            for (int j = 0; j < numNonESNodes; j++)
        //            {
        //                Site to = preprocessedSites[j];
        //                if (EnergyConsumption(ES, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j] + maxValue_Epsilon[k])
        //                {
        //                    for (int i = 0; i < numNonESNodes; i++)
        //                        EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r][k]);
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
        //                    string constraint_name = "No_arc_from_ES_" + r.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
        //                    allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
        //                }
        //            }
        //        }

        //        //First direct, then to ES
        //        for (int i = 0; i < numNonESNodes; i++)
        //        {
        //            Site from = preprocessedSites[i];
        //            for (int r = 0; r < numES; r++)
        //            {
        //                Site ES = ExternalStations[r];
        //                if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, ES, VehicleCategories.EV) > maxValue_Delta[i] + maxValue_Epsilon[k])
        //                {
        //                    for (int j = 0; j < numNonESNodes; j++)
        //                        EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[k][r][j]);
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
        //                    string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "_to_ES_" + r.ToString();
        //                    allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
        //                }
        //            }
        //        }
        //    }
        //}
        //void AddCut_EnergyFeasibilityOfCustomerBetweenTwoES()
        //{
        //    for (int r1 = 0; r1 < numES; r1++)
        //    {
        //        Site ES1 = ExternalStations[r1];
        //        for (int j = 1; j < numNonESNodes; j++)
        //        {
        //            Site customer = preprocessedSites[j];
        //            for (int r2 = 0; r2 < numES; r2++)
        //            {
        //                Site ES2 = ExternalStations[r2];
        //                if (EnergyConsumption(ES1, customer, VehicleCategories.EV) + EnergyConsumption(customer, ES2, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) + maxValue_Epsilon[j])//This didn't have the maxValue_Epsilon in it and hence it ignored the SOE gain at ISs
        //                {
        //                    ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
        //                    for (int i = 0; i < numNonESNodes; i++)
        //                    {
        //                        EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[i][r1][j]);
        //                        EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Y[j][r2][i]);
        //                    }
        //                    string constraint_name = "No_arc_from_ES_node_" + r1.ToString() + "_through_customer_" + j.ToString() + "to_ES_node_" + r2.ToString();
        //                    allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
        //                    EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
        //                }

        //            }
        //        }
        //    }
        //}
        //void AddCut_TotalNumberOfActiveArcs()
        //{
        //    totalNumberOfActiveArcsConstraintIndex = allConstraints_list.Count;

        //    int nActiveArcs = numVehicles[0] + numVehicles[1] + numCustomers + numES;
        //    int nActiveArcs_EV = numVehicles[vIndex_EV] + numCustomers + numES;
        //    int nActiveArcs_GDV = numVehicles[vIndex_GDV] + numCustomers;
        //    ILinearNumExpr totalArcFlow = LinearNumExpr();
        //    ILinearNumExpr totalArcFlow_EV = LinearNumExpr();
        //    ILinearNumExpr totalArcFlow_GDV = LinearNumExpr();
        //    for (int i = 0; i < numNonESNodes; i++)
        //        for (int j = 0; j < numNonESNodes; j++)
        //        {
        //            for (int v = 0; v < 2; v++)
        //                totalArcFlow.AddTerm(1.0, X[i][j][v]);
        //            totalArcFlow_EV.AddTerm(1.0, X[i][j][vIndex_EV]);
        //            totalArcFlow_GDV.AddTerm(1.0, X[i][j][vIndex_GDV]);
        //            for (int r = 0; r < numES; r++)
        //            {
        //                totalArcFlow.AddTerm(1.0, Y[i][r][j]);
        //                totalArcFlow_EV.AddTerm(1.0, Y[i][r][j]);
        //            }
        //        }
        //    string constraintName_overall = "Total_number_of_active_arcs_cannot_exceed_" + nActiveArcs.ToString();
        //    allConstraints_list.Add(AddLe(totalArcFlow, (double)nActiveArcs, constraintName_overall));
        //    string constraintName_EV = "Number_of_active_EV_arcs_cannot_exceed_" + nActiveArcs_EV.ToString();
        //    allConstraints_list.Add(AddLe(totalArcFlow_EV, (double)nActiveArcs_EV, constraintName_EV));
        //    string constraintName_GDV = "Number_of_active_GDV_arcs_cannot_exceed_" + nActiveArcs_GDV.ToString();
        //    allConstraints_list.Add(AddLe(totalArcFlow_GDV, (double)nActiveArcs_GDV, constraintName_GDV));
        //}
        //void AddCut_EnergyConservation()//17
        //{
        //    ILinearNumExpr EnergyConservation = LinearNumExpr();
        //    for (int i = 0; i < NumPreprocessedSites; i++)
        //    {
        //        Site sFrom = preprocessedSites[i];
        //        for (int r = 0; r < numES; r++)
        //        {
        //            Site through = ExternalStations[r];
        //            for (int j = 0; j < NumPreprocessedSites; j++)
        //            {
        //                Site sTo = preprocessedSites[j];
        //                EnergyConservation.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV), X[i][j][vIndex_EV]);
        //                EnergyConservation.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][vIndex_EV]);
        //                EnergyConservation.AddTerm(EnergyConsumption(sFrom, through, VehicleCategories.EV) + EnergyConsumption(through, sTo, VehicleCategories.EV), Y[i][r][j]);
        //                EnergyConservation.AddTerm(-1.0 * BatteryCapacity(VehicleCategories.EV), Y[i][r][j]);
        //            }
        //        }
        //    }
        //    string constraint_name = "Energy conservation";
        //    allConstraints_list.Add(AddLe(EnergyConservation, numVehicles[vIndex_EV] * BatteryCapacity(VehicleCategories.EV), constraint_name));
        //    EnergyConservation.Clear();
        //}

        //void AddCut_DeltaTime()
        //{
        //    double energyConsumptionPerMinute = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).ConsumptionRate;
        //    for (int j = 1; j < numNonESNodes; j++)
        //    {
        //        ILinearNumExpr DeltaTimeRelationship = LinearNumExpr();

        //        DeltaTimeRelationship.AddTerm(1.0, T[j]);
        //        DeltaTimeRelationship.AddTerm(1.0 / energyConsumptionPerMinute, Delta[j]);

        //        string constraint_name = "DeltaTimeRelationship_" + j.ToString();
        //        allConstraints_list.Add(AddGe(DeltaTimeRelationship, (BatteryCapacity(VehicleCategories.EV) / energyConsumptionPerMinute), constraint_name));
        //    }
        //}


        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            throw new NotImplementedException();
        }

        public override string GetDescription_AllVariables_Array()
        {
            throw new NotImplementedException();
        }

        public override string GetModelName()
        {
            return "EVRP w Refueling Paths";
        }

        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            throw new NotImplementedException();
        }

        public override void RefineDecisionVariables(CustomerSet cS)
        {
            throw new NotImplementedException();
        }

        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            throw new NotImplementedException();
        }

        protected override void AddAllConstraints()
        {
            throw new NotImplementedException();
        }

        protected override void AddTheObjectiveFunction()
        {
            throw new NotImplementedException();
        }

        
    }
}
