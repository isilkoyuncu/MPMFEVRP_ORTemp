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
    public class XCPlex_Model_AFV_TSP : XCPlexVRPBase
    {
        RefuelingPathGenerator rpg;
        List<string> GDV_optRouteIDs;
        List<SiteWithAuxiliaryVariables> GDV_optRoute;

        RefuelingPathList[,] allNondominatedRPs;

        int numNonESNodes;
        int numCustomers;

        int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
        int totalTravelTimeConstraintIndex = -1;
        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

        bool addTotalNumberOfActiveArcsCut = false;

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        INumVar[][][] X; double[][][] X_LB, X_UB; //[i=1,..,numNonESNodes][j=1,..,numNonESNodes][r=1,..,numNondominatedRPs(i,j)], defined for each possible arc in between nonESNodes i and j, (Binary)
        INumVar[][][] W; double[][][] W_LB, W_UB; //[i=1,..,numNonESNodes][j=1,..,numNonESNodes][r=1,..,numNondominatedRPs(i,j)], defined for each possible arc in between nonESNodes i and j, (Continuous)
        INumVar[][][] U; double[][][] U_LB, U_UB; //[i=1,..,numNonESNodes][j=1,..,numNonESNodes][r=1,..,numNondominatedRPs(i,j)], defined for each possible arc in between nonESNodes i and j, (Continuous)

        INumVar[] Epsilon;
        INumVar[] Delta;
        INumVar[] T;

        //INumVar[] EnergySlack; double[] EnergySlack_LB, EnergySlack_UB;//Defined for each nonESNode
        //INumVar[] TimeSlack; double[] TimeSlack_LB, TimeSlack_UB;//Defined for each nonESNode

        //IndividualRouteESVisits singleRouteESvisits;
        //List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

        public XCPlex_Model_AFV_TSP() { }

        public XCPlex_Model_AFV_TSP(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
            //Do not uncomment these. They are here to show which methods are being implemented in the base
            
            ////Model Specific Initialization
            //Initialize()

            ////now we are ready to put the model together and then solve it
            ////Define the variables
            //DefineDecisionVariables()

            ////Objective function
            //AddTheObjectiveFunction()

            ////Constraints
            //AddAllConstraints()

            ////Cplex parameters
            //SetCplexParameters()

            ////output variables
            //InitializeOutputVariables()
        }

        protected override void DefineDecisionVariables()
        {
            PreprocessSites();
            numCustomers = theProblemModel.SRD.NumCustomers;
            numNonESNodes = numCustomers+1; //customers + thedepot
            rpg = new RefuelingPathGenerator(theProblemModel);
            SetNondominatedRPsBetweenODPairs();
            SetMinAndMaxValuesOfModelSpecificVariables();
            

            allVariables_list = new List<INumVar>();

            int[,] length3 = new int[numNonESNodes, numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    length3[i, j] = allNondominatedRPs[i, j].Count;

            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, length3, out X);
            AddThreeDimensionalDecisionVariable("W", W_LB, W_UB, NumVarType.Float, numNonESNodes, numNonESNodes, length3, out W);
            AddThreeDimensionalDecisionVariable("U", U_LB, U_UB, NumVarType.Float, numNonESNodes, numNonESNodes, length3, out U);

            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out T);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numNonESNodes, out Delta);
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numNonESNodes, out Epsilon);

            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXYVariablesTo0();
        }

        void SetNondominatedRPsBetweenODPairs()
        {
            allNondominatedRPs = new RefuelingPathList[numNonESNodes, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++) {
                    allNondominatedRPs[i, j] = rpg.GenerateNonDominatedBetweenODPair(preprocessedSites[i], preprocessedSites[j], ExternalStations, theProblemModel.SRD);
                }
        }
        void SetUndesiredXYVariablesTo0()
        {
            //No arc from a node to itself
                for (int j = 0; j < numNonESNodes; j++)
                        X[j][j][0].UB = 0.0;
        }
        
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {           
            X_LB = new double[numNonESNodes][][];
            X_UB = new double[numNonESNodes][][];
            W_LB = new double[numNonESNodes][][];
            W_UB = new double[numNonESNodes][][];
            U_LB = new double[numNonESNodes][][];
            U_UB = new double[numNonESNodes][][];

            RHS_forNodeCoverage = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                X_LB[i] = new double[numNonESNodes][];
                X_UB[i] = new double[numNonESNodes][];
                W_LB[i] = new double[numNonESNodes][];
                W_UB[i] = new double[numNonESNodes][];
                U_LB[i] = new double[numNonESNodes][];
                U_UB[i] = new double[numNonESNodes][];

                for (int j = 0; j < numNonESNodes; j++)
                {
                    int numRPS = allNondominatedRPs[i, j].Count;
                    X_LB[i][j] = new double[numRPS];
                    X_UB[i][j] = new double[numRPS];
                    W_LB[i][j] = new double[numRPS];
                    W_UB[i][j] = new double[numRPS];
                    U_LB[i][j] = new double[numRPS];
                    U_UB[i][j] = new double[numRPS];
                  
                    for (int r = 0; r < numRPS; r++)
                    {
                        X_LB[i][j][r] = 0.0;
                        X_UB[i][j][r] = 1.0;

                        W_LB[i][j][r] = 0.0;
                        W_UB[i][j][r] = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity* allNondominatedRPs[i,j][r].RefuelingStops.Count;

                        if (theProblemModel.RechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                        {
                            U_LB[i][j][r] = allNondominatedRPs[i, j][r].TotalTime;
                            U_UB[i][j][r] = allNondominatedRPs[i, j][r].TotalTime;
                        }
                        else
                            throw new System.Exception("We are only concerned with FF policy for now!");

                        //The following may be useful when we allow VP policy
                        //U_UB[i][j][r] = W_UB[i][j][r] * theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
                    }
                }
                RHS_forNodeCoverage[i] = 1.0;
            }

            T[0].LB = theProblemModel.CRD.TMax;
            T[0].UB = theProblemModel.CRD.TMax;
            Epsilon[0].LB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
            Epsilon[0].UB = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "Need to rewrite this!";
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
            throw new NotImplementedException();
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT)//TODO: This code was written just to save the day, must be reconsidered in relation to the problem model's objective function calculation method
            {
                for (int i = 0; i < numNonESNodes; i++)
                {
                    Site sFrom = preprocessedSites[i];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        for(int r=0; r< allNondominatedRPs[i, j].Count; r++)
                        objFunction.AddTerm(allNondominatedRPs[i, j][r].TotalDistance, X[i][j][r]);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
                //The old code was repetitive of the distance-based cost calculation given above and it was only capable of multiplying the VMt by the vehicle's $/mile coeff. We can and shoulddo much better than that, for example combining distance- and time-based costs together. Will solve when it's needed :)
            }
            //Now adding the objective function to the model
            objective = AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_NumberOfVisitsPerCustomerNode();//1
            AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV();//2
            AddConstraint_DepartingNumberOfEVs();
            AddConstraint_MaxEnergyGainAtNonDepotSite();//6
            AddConstraint_DepartureSOCFromCustomerNode();//7
            AddConstraint_ArrivalSOE();//8
            AddConstraint_ArrivalSOCToESNodeLB();//9b
            AddConstraint_SOCRegulationFollowingNondepot();//10
            AddConstraint_SOCRegulationFollowingDepot();//11
            AddConstraint_TimeRegulationFollowingACustomerVisit();//12
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
                AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging();//13a
                AddConstraint_DepartureSOCFromESNodeLB();//9
            }
            AddConstraint_TimeRegulationFromDepotThroughAnESVisit();//13c
            AddConstraint_ArrivalTimeLimits();//14
            AddConstraint_TotalTravelTime();//15

            addTotalNumberOfActiveArcsCut = false;
            if (addTotalNumberOfActiveArcsCut)
                AddCut_TotalNumberOfActiveArcs();

            //Some additional cuts
            //AddAllCuts();

            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_NumberOfVisitsPerCustomerNode() //1
        {
            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
            {
                ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                {
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j][r]);
                }
                string constraint_name;

                switch (customerCoverageConstraint)
                {
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
                        constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
                        allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
                        constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode invoked for CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce, which must not happen for a VRP!");
                    default:
                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
                }

            }
        }
        void AddConstraint_IncomingXYTotalEqualsOutgoingXYTotalforEV()//2
        {
            for (int j = 0; j < numNonESNodes; j++)
            {

                ILinearNumExpr IncomingXYTotalMinusOutgoingXYTotal = LinearNumExpr();

                for (int i = 0; i < numNonESNodes; i++)
                {

                    IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, Xold[i][j]);
                    IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, Xold[j][i]);
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    {
                        IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, X[i][j][r]);
                        IncomingXYTotalMinusOutgoingXYTotal.AddTerm(-1.0, X[j][i][r]);
                    }

                }
                string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
                allConstraints_list.Add(AddEq(IncomingXYTotalMinusOutgoingXYTotal, 0.0, constraint_name));

            }
        }
        void AddConstraint_DepartingNumberOfEVs()//4
        {
            ILinearNumExpr NumberOfEVsOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < numNonESNodes; j++)
            {
                for (int r = 0; r < allNondominatedRPs[0,j].Count; r++)
                    NumberOfEVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j][r]);

            }
            if (customerCoverageConstraint == CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
            {
                string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_1";
                allConstraints_list.Add(AddLe(NumberOfEVsOutgoingFromTheDepot, 1.0, constraint_name));
            }
            else if (customerCoverageConstraint == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
            {
                string constraint_name = "Number_of_EVs_outgoing_from_node_0_cannot_exceed_1";
                allConstraints_list.Add(AddEq(NumberOfEVsOutgoingFromTheDepot, 1.0, constraint_name));
            }
            else
                throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
        }
        void AddConstraint_MaxEnergyGainAtNonDepotSite()//6
        {
            for (int j = 1; j < numNonESNodes; j++)
            {

                ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
                EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {

                    EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], Xold[i][j]);
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][r]);

                }
                string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
                allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));

            }
        }
        void AddConstraint_DepartureSOCFromCustomerNode()//7
        {
            for (int j = 1; j <= numCustomers; j++)//Index 0 is the depot
            {

                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {

                    DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), Xold[i][j]);
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j][r]);

                }
                string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));

            }
        }
        void AddConstraint_ArrivalSOE()//8
        {
            for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
            {
                Site from = ExternalStations[];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site to = preprocessedSites[j];
                    ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                    DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                    for (int i = 0; i < numNonESNodes; i++)
                        DepartureSOCFromES.AddTerm(EnergyConsumption(from, to, VehicleCategories.EV), X[i][j][r]);
                    string constraint_name = "Departure_SOC(UB)_From_ES_" + r.ToString() + "_going_to_customer_" + j.ToString();
                    allConstraints_list.Add(AddLe(DepartureSOCFromES, BatteryCapacity(VehicleCategories.EV), constraint_name));

                }
            }
        }
        void AddConstraint_DepartureSOCFromESNodeLB()//9
        {
            for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
            {
                Site ES = ExternalStations[];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site to = preprocessedSites[j];
                    ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                    DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                    for (int i = 0; i < numNonESNodes; i++)
                        DepartureSOCFromES.AddTerm((EnergyConsumption(ES, to, VehicleCategories.EV) - BatteryCapacity(VehicleCategories.EV) + minValue_Delta[j]), X[i][j][r]);
                    string constraint_name = "Departure_SOC(LB)_From_ES_" + r.ToString() + "_going_to_" + j.ToString();
                    allConstraints_list.Add(AddGe(DepartureSOCFromES, minValue_Delta[j], constraint_name));

                }
            }
        }
        void AddConstraint_ArrivalSOCToESNodeLB()//9b
        {
            for (int j = 0; j < numNonESNodes; j++)
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                {
                    Site from = preprocessedSites[j];
                    Site ES = ExternalStations[];
                    ILinearNumExpr ArrivalSOCFromES = LinearNumExpr();
                    ArrivalSOCFromES.AddTerm(1.0, Delta[j]);
                    ArrivalSOCFromES.AddTerm(1.0, Epsilon[j]);
                    for (int k = 0; k < numNonESNodes; k++)
                        ArrivalSOCFromES.AddTerm(-1.0 * EnergyConsumption(from, ES, VehicleCategories.EV), X[j][k][r]);
                    string constraint_name = "Arrival_SOC(LB)_To_ES_" + r.ToString() + "_from_" + j.ToString();
                    allConstraints_list.Add(AddGe(ArrivalSOCFromES, 0.0, constraint_name));

                }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()//10
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
                    SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV) + BigDelta[i][j], Xold[i][j]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, BigDelta[i][j], constraint_name));
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()//11
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                SOCDifference.AddTerm(EnergyConsumption(TheDepot, sTo, VehicleCategories.EV), Xold[0][j]);
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                {
                    Site ES = ExternalStations[];
                    SOCDifference.AddTerm(EnergyConsumption(ES, sTo, VehicleCategories.EV), X[0][j][r]);
                }
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
            }
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()//12
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, sTo) + BigT[i][j]), Xold[i][j]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                }
        }
        void AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging()//13a Only if recharging is full (FF) 
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[];
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[i][j]), X[i][j][r]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationThroughAnESVisits_VariableTimeRecharging()//13b Only in VF, VP cases
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[];
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
                        double timeSpentDueToEnergyConsumption = (EnergyConsumption(sFrom, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES);
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + travelDuration + timeSpentDueToEnergyConsumption + BigTVarRecharge[i][j][r]), X[i][j][r]);
                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (BigTVarRecharge[i][j][r]), constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationThroughAnESVisits_VariableFullTimeRecharging()//13b Only in VF, VP cases
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site ES = ExternalStations[];
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        double travelDuration = TravelTime(sFrom, ES) + TravelTime(ES, sTo);
                        double timeSpentDueToEnergyConsumption = (BatteryCapacity(VehicleCategories.EV) + EnergyConsumption(sFrom, ES, VehicleCategories.EV)) / RechargingRate(ES);
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (BigTVarRecharge[i][j][r]), X[i][j][r]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        double rhs = -1.0 * BigTVarRecharge[i][j][r] + ServiceDuration(sFrom);
                        allConstraints_list.Add(AddGe(TimeDifference, rhs, constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationFromDepotThroughAnESVisit()//13c
        {
            for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site ES = ExternalStations[];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);

                    // Here we decide whether recharging duration is fixed or depends on the arrival SOC
                    if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                    {
                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + (BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), X[0][j][r]);
                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j], constraint_name));
                    }
                    else
                    {
                        TimeDifference.AddTerm(-1.0 * (TravelTime(TheDepot, ES) + ((EnergyConsumption(TheDepot, ES, VehicleCategories.EV) + EnergyConsumption(ES, sTo, VehicleCategories.EV)) / RechargingRate(ES)) + TravelTime(ES, sTo) + BigT[0][j]), X[0][j][r]);
                        TimeDifference.AddTerm(-1.0 / RechargingRate(ES), Delta[j]);
                        string constraint_name = "Time_Regulation_from_depot_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[0][j] - BatteryCapacity(VehicleCategories.EV) / RechargingRate(ES), constraint_name));
                    }
                }
        }
        void AddConstraint_ArrivalTimeLimits()//14
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {
                    TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), Xold[i][j]);
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), X[i][j][r]);
                }
                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, maxValue_T[j], constraint_name));
            }
        }
        void AddConstraint_TotalTravelTime()//15
        {
            totalTravelTimeConstraintIndex = allConstraints_list.Count;

            ILinearNumExpr TotalTravelTime = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), Xold[i][j]);
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    {
                        Site ES = ExternalStations[];
                        TotalTravelTime.AddTerm((TravelTime(sFrom, ES) + TravelTime(ES, sTo)), X[i][j][r]);
                    }
                }

            string constraint_name = "Total_Travel_Time";
            double rhs = theProblemModel.CRD.TMax;
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
                    if (Xold[i][k].UB == 0.0)//The direct arc (from,through) has already been marked infeasible
                        continue;
                    for (int j = 0; j < numNonESNodes; j++)//This was starting at 1
                    {
                        Site to = preprocessedSites[j];
                        if (Xold[k][j].UB == 0.0)//The direct arc (through,to) has already been marked infeasible
                            continue;
                        if (i != j && j != k && i != k)
                        {
                            ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                            if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
                            {

                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[i][k]);
                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[k][j]);

                                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                                {
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][r]);
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][r]);
                                }
                                string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                                allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                            }
                            else if (rechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                            {
                                for (int r1 = 0; r1 < allNondominatedRPs[i,j].Count; r1++)
                                {
                                    Site ES1 = ExternalStations[r1];
                                    double fixedChargeTimeAtES1 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES1.RechargingRate;
                                    if (fixedChargeTimeAtES1 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//Not even one visit through ES is allowed
                                    {

                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[i][k]);
                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[k][j]);

                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][r1][k]);
                                        TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][r1][j]);
                                        string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString() + "_through_ES_" + r1.ToString();
                                        allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                        TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                    }
                                    else
                                    {
                                        for (int r2 = 0; r2 < allNondominatedRPs[i,j].Count; r2++)
                                            if (r2 != r1)
                                            {
                                                Site ES2 = ExternalStations[r2];
                                                double fixedChargeTimeAtES2 = theProblemModel.VRD.GetVehiclesOfCategory(VehicleCategories.EV)[0].BatteryCapacity / ES2.RechargingRate;
                                                if (fixedChargeTimeAtES1 + fixedChargeTimeAtES2 + minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])//ES1 was fine by itself but not together with ES2
                                                {
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][r1][k]);
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][r1][j]);
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][r2][k]);
                                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][r2][j]);
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
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[i][k]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[k][j]);
                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
                        }
                    }
                }

                //First from ES, then direct
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                {
                    Site ES = ExternalStations[];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (EnergyConsumption(ES, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j] + maxValue_Epsilon[k])
                        {
                            for (int i = 0; i < numNonESNodes; i++)
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][r]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[k][j]);
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
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    {
                        Site ES = ExternalStations[];
                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, ES, VehicleCategories.EV) > maxValue_Delta[i] + maxValue_Epsilon[k])
                        {
                            for (int j = 0; j < numNonESNodes; j++)
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][r]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, Xold[i][k]);
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
            for (int r1 = 0; r1 < allNondominatedRPs[i,j].Count; r1++)
            {
                Site ES1 = ExternalStations[r1];
                for (int j = 1; j < numNonESNodes; j++)
                {
                    Site customer = preprocessedSites[j];
                    for (int r2 = 0; r2 < allNondominatedRPs[i,j].Count; r2++)
                    {
                        Site ES2 = ExternalStations[r2];
                        if (EnergyConsumption(ES1, customer, VehicleCategories.EV) + EnergyConsumption(customer, ES2, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV) + maxValue_Epsilon[j])//This didn't have the maxValue_Epsilon in it and hence it ignored the SOE gain at ISs
                        {
                            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                            for (int i = 0; i < numNonESNodes; i++)
                            {
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][r1][j]);
                                EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[j][r2][i]);
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

            int nActiveArcs_EV = 1 + allNondominatedRPs[i,j].Count;
            ILinearNumExpr totalArcFlow_EV = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    totalArcFlow_EV.AddTerm(1.0, Xold[i][j]);
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    {
                        totalArcFlow_EV.AddTerm(1.0, X[i][j][r]);
                    }
                }
            string constraintName_EV = "Number_of_active_EV_arcs_cannot_exceed_" + nActiveArcs_EV.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow_EV, nActiveArcs_EV, constraintName_EV));
        }
        void AddCut_EnergyConservation()//17
        {
            ILinearNumExpr EnergyConservation = LinearNumExpr();
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                {
                    Site through = ExternalStations[];
                    for (int j = 0; j < NumPreprocessedSites; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        EnergyConservation.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV), Xold[i][j]);
                        EnergyConservation.AddTerm(-1.0 * maxValue_Epsilon[j], Xold[i][j]);
                        EnergyConservation.AddTerm(EnergyConsumption(sFrom, through, VehicleCategories.EV) + EnergyConsumption(through, sTo, VehicleCategories.EV), X[i][j][r]);
                        EnergyConservation.AddTerm(-1.0 * BatteryCapacity(VehicleCategories.EV), X[i][j][r]);
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
            outcome.AddRange(GetVehicleSpecificRoutes(VehicleCategories.EV));
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
            GetDecisionVariableValues();

            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

            //We first determine the route start points
            List<List<int>> listOfFirstSiteIndices = new List<List<int>>();
            for (int j = 0; j < numNonESNodes; j++)
                if (GetValue(Xold[0][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    listOfFirstSiteIndices.Add(new List<int>() { j });
                }
            if (vehicleCategory != VehicleCategories.GDV)
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                        if (GetValue(X[0][j][r]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
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
            double[,] xValues = new double[numNonESNodes, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    xValues[i, j] = GetValue(Xold[i][j]);//CONSULT (w/ Isil): Why only 0 when xValues is defined over all numVehCategories? IK: This was just debugging purposes, since EMH does not have any GDVs, I only wrote [0].
            double[,,] yValues = new double[numNonESNodes, allNondominatedRPs[i,j].Count, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                    for (int j = 0; j < numNonESNodes; j++)
                        yValues[i, r, j] = GetValue(X[i][j][r]);

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
                if (GetValue(Xold[0][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to an X-flow from the depot!");
                }
            if (firstSiteIndices.Count == 2)
                if (GetValue(X[0][firstSiteIndices.First()][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
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
                if (GetValue(Xold[currentSiteIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    return new List<int>() { nextCustomerIndex };
                }
            if (vehicleCategory == VehicleCategories.EV)
                for (int nextESIndex = 0; nextESIndex < allNondominatedRPs[i,j].Count; nextESIndex++)
                    for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
                        if (GetValue(X[currentSiteIndex][nextESIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            return new List<int>() { nextESIndex, nextCustomerIndex };
            throw new System.Exception("Flow ended before returning to the depot!");
        }
        IndividualESVisitDataPackage GetIndividualESVisit(int i, int r, int j)
        {
            Site from = preprocessedSites[i];
            Site ES = ExternalStations[];
            Site to = preprocessedSites[j];

            double timeSpentInES = GetValue(T[j]) - TravelTime(from, ES) - ServiceDuration(from) - GetValue(T[i]) - TravelTime(ES, to);
            return new IndividualESVisitDataPackage(ES.ID, timeSpentInES / RechargingRate(ES), preprocessedESSiteIndex: r);
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        public void RefineDecisionVariables(CustomerSet cS, bool preserveCustomerVisitSequence)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            GDV_optRouteIDs = cS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs;
            GDV_optRoute = GetGDVoptRouteSWAVs();

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        Xold[i][j].UB = 1.0;
                        Xold[j][i].UB = 1.0;
                        for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        {
                            X[i][j][r].UB = 1.0;
                            X[j][i][r].UB = 1.0;

                        }
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        Xold[i][j].UB = 0.0;
                        Xold[j][i].UB = 0.0;
                        for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        {
                            X[i][j][r].UB = 0.0;
                            X[j][i][r].UB = 0.0;

                        }
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();
            RefineRHSofTotalTravelConstraints(cS);
            allRoutesESVisits.Clear();
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
        void RefineRHSofTotalTravelConstraints(CustomerSet cS)
        {
            int c = 0;
            if (customerCoverageConstraint != CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
            {
                c = totalTravelTimeConstraintIndex;
                allConstraints_array[c].UB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
                //allConstraints_array[c].LB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
            }
            if (addTotalNumberOfActiveArcsCut)
            {
                c = totalNumberOfActiveArcsConstraintIndex;
                allConstraints_array[c].UB = theProblemModel.CRD.TMax + cS.NumberOfCustomers;
                //allConstraints_array[c].LB = theProblemModel.CRD.TMax + cS.NumberOfCustomers;
            }
        }

        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            throw new NotImplementedException();
        }
        public override void RefineDecisionVariables(CustomerSet cS)
        {
            throw new NotImplementedException();
        }
        List<SiteWithAuxiliaryVariables> GetGDVoptRouteSWAVs()
        {
            List<SiteWithAuxiliaryVariables> output = new List<SiteWithAuxiliaryVariables>();
            foreach (string id in GDV_optRouteIDs)
                output.Add(theProblemModel.SRD.GetSWAVByID(id));
            return output;
        }
        RefuelingPathList GetAllNonDominatedRefuelingPathsForGDV_OptRoute(VehicleSpecificRouteOptimizationOutcome vsroo_GDV)
        {
            RefuelingPathList outcome = new RefuelingPathList();
            List<string> listOfVisitedSiteIncludingDepotIDs = vsroo_GDV.VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs;
            SiteWithAuxiliaryVariables from = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs.First());
            for (int i = 1; i < listOfVisitedSiteIncludingDepotIDs.Count; i++)
            {
                SiteWithAuxiliaryVariables to = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
                outcome.AddRange(theProblemModel.PDP.PopulateRefuelingPathsBetween(rpg, from, to));
                from = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
            }
            return outcome;
        }
        RefuelingPathList GetAllNonDominatedRefuelingPaths()
        {
            RefuelingPathList outcome = new RefuelingPathList();
            SiteWithAuxiliaryVariables from = theProblemModel.SRD.AllOriginalSWAVs.First();
            for (int i = 1; i < listOfVisitedSiteIncludingDepotIDs.Count; i++)
            {
                SiteWithAuxiliaryVariables to = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
                outcome.AddRange(theProblemModel.PDP.PopulateRefuelingPathsBetween(rpg, from, to));
                from = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
            }
            return outcome;
        }

        public override string GetModelName()
        {
            return "New AFV TSP Solver";
        }
    }
}

