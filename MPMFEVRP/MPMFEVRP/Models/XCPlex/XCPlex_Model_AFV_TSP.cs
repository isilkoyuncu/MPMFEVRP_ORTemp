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
            AddConstraint_IncomingXTotalEqualsOutgoingXTotal();//2
            AddConstraint_DepartingNumberOfEVs();

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
        void AddConstraint_NumberOfVisitsPerCustomerNode()
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
        void AddConstraint_IncomingXTotalEqualsOutgoingXTotal()
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr IncomingXYTotalMinusOutgoingXYTotal = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                {
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
        void AddConstraint_DepartingNumberOfEVs()
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
        void AddConstraint_SOCRegulationThroughRP()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (allNondominatedRPs[i, j].Count > 0)
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            ILinearNumExpr SOCDifference = LinearNumExpr();
                            SOCDifference.AddTerm(1.0, Delta[j]);
                            SOCDifference.AddTerm(allNondominatedRPs[i, j][r].LastArcEnergyConsumption, X[i][j][r]);
                            string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
                        }
        }
        void AddConstraint_SOCRegulationFollowingDepot()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (allNondominatedRPs[i, j].Count == 0)
                        {
                            ILinearNumExpr SOCDifference = LinearNumExpr();
                            SOCDifference.AddTerm(1.0, Delta[j]);
                            SOCDifference.AddTerm(allNondominatedRPs[i, j][r].LastArcEnergyConsumption, X[i][j][r]);
                            string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
                        }


            for (int j = 0; j < numNonESNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                for (int r = 0; r < allNondominatedRPs[0, j].Count; r++)
                {
                    SOCDifference.AddTerm(EnergyConsumption(ES, sTo, VehicleCategories.EV), X[0][j][r]);
                }
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
            }
        }
        void AddConstraint_TimeRegulationThroughAnESVisit_FixedTimeRecharging() 
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
        
        void AddConstraint_ArrivalTimeLimits()//14
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {
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

                for (int r = 0; r < allNondominatedRPs[0,j].Count; r++)
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

