using ILOG.Concert;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_NodeDuplicatingFormulation_woU : XCPlexVRPBase
    {
        int numDuplicatedNodes;
        int numCustomers;

        int firstESNodeIndex, lastESNodeIndex, firstCustomerNodeIndex, lastCustomerNodeIndex;
        Site theDepot; //There is a single depot

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        //DVs, upper and lower bounds
        INumVar[][][] X; double[][][] X_LB, X_UB;

        //Auxiliaries, upper and lower bounds
        INumVar[] Epsilon; double[] minValue_Epsilon, maxValue_Epsilon;
        INumVar[] Delta; double[] minValue_Delta, maxValue_Delta; double[][] BigDelta;
        INumVar[] T; double[] minValue_T, maxValue_T; double[][] BigT;

        int firstCustomerVisitationConstraintIndex=-1;
        int totalTravelTimeConstraintIndex = -1;

        Dictionary<string, double> eSStayDurations = new Dictionary<string, double>();

        public XCPlex_NodeDuplicatingFormulation_woU() { } //Empty constructor
        public XCPlex_NodeDuplicatingFormulation_woU(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam){ } //XCPlex VRP Constructor

        protected override void DefineDecisionVariables()
        {
            DuplicateAndOrganizeSites();
            SetMinAndMaxValuesOfAllVariables();
            SetBigMvalues();

            allVariables_list = new List<INumVar>();

            //dvs: X_ijv
            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numDuplicatedNodes, numDuplicatedNodes, numVehCategories, out X);
            //auxiliaries (Epsilon_j, Delta_j, T_j)
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numDuplicatedNodes, out Epsilon);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numDuplicatedNodes, out Delta);
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numDuplicatedNodes, out T);
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXVariablesTo0();
        }
        void DuplicateAndOrganizeSites()
        {
            Site[] originalSites = theProblemModel.SRD.GetAllSitesArray();
            numCustomers = theProblemModel.SRD.NumCustomers;
            int numDuplicationsForeachES = theProblemModel.Lambda * theProblemModel.GetNumVehicles(VehicleCategories.EV);
            numDuplicatedNodes = 1 + (numDuplicationsForeachES * theProblemModel.SRD.NumES) + numCustomers;
            preprocessedSites = new Site[numDuplicatedNodes];
            depots = new List<Site>();
            customers = new List<Site>();
            externalStations = new List<Site>();
            int nodeCounter = 0;
            foreach (Site s in originalSites)
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
            //No arc from a node to another if energy consumption is > vehicle battery capacity
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        if (EnergyConsumption(sFrom, sTo, vehicleCategories[v]) > BatteryCapacity(vehicleCategories[v]))
                            X[i][j][v].UB = 0.0;
                }
            }
            //No arc from depot to its duplicate
            for (int v = 0; v < numVehCategories; v++)
                for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                    if ((preprocessedSites[j].X == theDepot.X) && (preprocessedSites[j].Y == theDepot.Y))//Comparing X and Y coordinates to those of the depot makes sure that the ES at hand corresponds to the one at the depot!
                    {
                        X[0][j][v].UB = 0.0;
                        X[j][0][v].UB = 0.0;
                    }
            //No arc from or to an ES node can be traversed by a GDV
            for (int v = 0; v < numVehCategories; v++)
                if (vehicleCategories[v] == VehicleCategories.GDV)
                    for (int i = 0; i < numDuplicatedNodes; i++)
                        for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
                        {
                            X[i][j][v].UB = 0.0;
                            X[j][i][v].UB = 0.0;
                        }
        }
        void SetMinAndMaxValuesOfAllVariables()
        {
            X_LB = new double[numDuplicatedNodes][][];
            X_UB = new double[numDuplicatedNodes][][];

            minValue_Epsilon = new double[numDuplicatedNodes];
            maxValue_Epsilon = new double[numDuplicatedNodes];
            minValue_Delta = new double[numDuplicatedNodes];
            maxValue_Delta = new double[numDuplicatedNodes];
            minValue_T = new double[numDuplicatedNodes];
            maxValue_T = new double[numDuplicatedNodes];

            RHS_forNodeCoverage = new double[numDuplicatedNodes];

            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                X_LB[i] = new double[numDuplicatedNodes][];
                X_UB[i] = new double[numDuplicatedNodes][];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site s = preprocessedSites[j];
                    X_LB[i][j] = new double[numVehCategories];
                    X_UB[i][j] = new double[numVehCategories];
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        X_LB[i][j][v] = 0.0;
                        X_UB[i][j][v] = 1.0;
                    }

                    //UB - LB on Epsilon
                    minValue_Epsilon[j] = 0.0;

                    if (s.SiteType == SiteTypes.Customer)
                        maxValue_Epsilon[j] = Math.Min(BatteryCapacity(VehicleCategories.EV), s.ServiceDuration * RechargingRate(s)); //Utils.Calculators.MaxSOCGainAtSite(s, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV), maxStayDuration: s.ServiceDuration);
                    else //TODO: Unit test the following utility function. It should give us MaxSOCGainAtSite s with EV.
                        maxValue_Epsilon[j] = BatteryCapacity(VehicleCategories.EV); //TODO fine tune this. It needs to be the same as the paper.

                    //TODO Fine-tune the min and max values of delta using label setting algorithm
                    minValue_Delta[j] = 0.0;
                    maxValue_Delta[j] = BatteryCapacity(VehicleCategories.EV);

                    minValue_T[j] = TravelTime(theDepot, s);
                    maxValue_T[j] = theProblemModel.CRD.TMax - TravelTime(s, theDepot);
                    if (s.SiteType == SiteTypes.Customer)
                        maxValue_T[j] -= ServiceDuration(s);
                    else if (s.SiteType == SiteTypes.ExternalStation && theProblemModel.RechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                        maxValue_T[j] -= (BatteryCapacity(VehicleCategories.EV) / RechargingRate(s));
                }
                RHS_forNodeCoverage[i] = 1.0;
            }                
        }
        void SetBigMvalues()
        {
            BigDelta = new double[numDuplicatedNodes][];
            BigT = new double[numDuplicatedNodes][];

            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                BigDelta[i] = new double[numDuplicatedNodes];
                BigT[i] = new double[numDuplicatedNodes];

                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    BigDelta[i][j] = maxValue_Delta[j] - minValue_Delta[i] - minValue_Epsilon[i];
                    BigT[i][j] = maxValue_T[i] - minValue_T[j];
                }
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
            for (int i = 0; i < numDuplicatedNodes; i++)
                for (int j = 0; j < numDuplicatedNodes; j++)
            {
                Site s = preprocessedSites[j];
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(Prize(s, vehicleCategories[v]), X[i][j][v]);
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
            //First term: distance-based costs
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
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeTotalCost)
            {
                //Second term: vehicle fixed costs
                for (int j = 0; j < numDuplicatedNodes; j++)
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[v]), X[0][j][v]);

            }//Now adding the objective function to the model
            AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_NumberOfVisitsPerCustomerNode();//1
            AddConstraint_NumberOfEvVisitsPerESNode();//2
            AddConstraint_NoGDVVisitToESNodes();//3
            AddConstraint_IncomingXTotalEqualsOutgoingXTotal();//4
            AddConstraint_MaxNumberOfVehiclesPerCategory();//5
            AddConstraint_MaxEnergyGainAtNonDepotSite();//6

            if (rechargingDuration_status==RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
                AddConstraint_DepartureSOCFromNode_PartialRecharging();//7-8
            else
            {
                AddConstraint_DepartureSOCFromCustomerNode();//7
                AddConstraint_DepartureSOCFromESNode_FullRecharging();//8
            }

            AddConstraint_SOCRegulationFollowingNondepot();//9
            AddConstraint_SOCRegulationFollowingDepot();//10
            AddConstraint_TimeRegulationFollowingACustomerVisit();//11

            if(rechargingDuration_status==RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                AddConstraint_TimeRegulationFollowingAnESVisit_FixedRechargingDuration();//12
            else
                AddConstraint_TimeRegulationFollowingAnESVisit_VariableRechargingDuration();//12

            AddConstraint_ArrivalTimeLimits();
            AddConstraint_TotalTravelTime();
            AddConstrain_TimeFeasibilityOfTwoConsecutiveArcs();
            //AddConstrain_EnergyFeasibilityOfTwoConsecutiveArcs();
            
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }

        void AddConstraint_NumberOfVisitsPerCustomerNode()//1
        {
            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheCustomerNode = LinearNumExpr();
                for (int i = 0; i < numDuplicatedNodes; i++)
                    for (int v = 0; v < numVehCategories; v++)
                        NumberOfVehiclesVisitingTheCustomerNode.AddTerm(1.0, X[i][j][v]);

                string constraint_name;

                if (xCplexParam.TSP)//TODO: I don't think we should be checking for "TSP" separately, isn't it the same thing as "exactly once"?
                {
                    constraint_name = "The_customer_node_" + j.ToString()+"_must_be_visited";
                    allConstraints_list.Add(AddEq(NumberOfVehiclesVisitingTheCustomerNode, RHS_forNodeCoverage[j], constraint_name));
                }
                else
                {
                    if (theProblemModel.CoverConstraintType == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
                    {
                        constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
                        allConstraints_list.Add(AddEq(NumberOfVehiclesVisitingTheCustomerNode, 1.0, constraint_name));
                    }
                    else
                    {
                        constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheCustomerNode, 1.0, constraint_name));
                    }
                }
                NumberOfVehiclesVisitingTheCustomerNode.Clear();
            }
        }
        void AddConstraint_NumberOfEvVisitsPerESNode()//2
        {
            for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
            {
                ILinearNumExpr NumberOfEvVisitToTheESNode = LinearNumExpr();
                for (int i = 0; i < numDuplicatedNodes; i++)
                    NumberOfEvVisitToTheESNode.AddTerm(1.0, X[i][j][vIndex_EV]);

                string constraint_name = "At_most_one_EV_can_visit_the_ES_node_" + j.ToString();
                allConstraints_list.Add(AddLe(NumberOfEvVisitToTheESNode, 1.0, constraint_name));
                NumberOfEvVisitToTheESNode.Clear();
            }
        }
        void AddConstraint_NoGDVVisitToESNodes()//3
        {
            for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
            {
                ILinearNumExpr NumberOfGDVsVisitingTheNode = LinearNumExpr();
                for (int i = 0; i < numDuplicatedNodes; i++)
                    NumberOfGDVsVisitingTheNode.AddTerm(1.0, X[i][j][vIndex_GDV]);

                string constraint_name = "No_GDV_can_visit_the_ES_node_" + j.ToString();
                allConstraints_list.Add(AddEq(NumberOfGDVsVisitingTheNode, 0.0, constraint_name));
                NumberOfGDVsVisitingTheNode.Clear();
            }
        }
        void AddConstraint_IncomingXTotalEqualsOutgoingXTotal()//4
        {
            for (int j = 0; j < numDuplicatedNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusOutgoingXTotal = LinearNumExpr();
                    for (int i = 0; i < numDuplicatedNodes; i++)
                        IncomingXTotalMinusOutgoingXTotal.AddTerm(1.0, X[i][j][v]);
                    for (int k = 0; k < numDuplicatedNodes; k++)
                        IncomingXTotalMinusOutgoingXTotal.AddTerm(-1.0, X[j][k][v]);
                    string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_vehicles";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusOutgoingXTotal, 0.0, constraint_name));
                    IncomingXTotalMinusOutgoingXTotal.Clear();
                }
        }
        void AddConstraint_MaxNumberOfVehiclesPerCategory()//5
        {
            for (int v = 0; v < numVehCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                for (int j = 1; j < numDuplicatedNodes; j++)
                    NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, X[0][j][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_node_0_cannot_exceed_" + numVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, numVehicles[v], constraint_name));
                NumberOfVehiclesPerCategoryOutgoingFromTheDepot.Clear();
            }
        }
        void AddConstraint_MaxEnergyGainAtNonDepotSite()//6
        {
            for (int j = 1; j < numDuplicatedNodes; j++)
            {
                ILinearNumExpr EnergyGainAtNonDepotSite = LinearNumExpr();
                EnergyGainAtNonDepotSite.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numDuplicatedNodes; i++)
                    EnergyGainAtNonDepotSite.AddTerm(-1.0 * maxValue_Epsilon[j], X[i][j][vIndex_EV]);
                string constraint_name = "Max_Energy_Gain_At_NonDepot_Site_" + j.ToString();
                allConstraints_list.Add(AddLe(EnergyGainAtNonDepotSite, 0.0, constraint_name));
                EnergyGainAtNonDepotSite.Clear();
            }
        }
        void AddConstraint_DepartureSOCFromCustomerNode()//7
        {
            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numDuplicatedNodes; i++)
                    DepartureSOCFromCustomer.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV)-minValue_Delta[j]), X[i][j][vIndex_EV]);
                string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, minValue_Delta[j], constraint_name));
                DepartureSOCFromCustomer.Clear();
            }
        }
        void AddConstraint_DepartureSOCFromESNode_FullRecharging()//8
        {
            for (int j = firstESNodeIndex; j <= lastESNodeIndex; j++)
            {
                ILinearNumExpr DepartureSOCFromES = LinearNumExpr();
                DepartureSOCFromES.AddTerm(1.0, Delta[j]);
                DepartureSOCFromES.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numDuplicatedNodes; i++)
                    DepartureSOCFromES.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j][vIndex_EV]);
                string constraint_name = "Departure_SOC_From_ES_" + j.ToString();
                allConstraints_list.Add(AddEq(DepartureSOCFromES, minValue_Delta[j], constraint_name));
                DepartureSOCFromES.Clear();
            }
        }
        void AddConstraint_DepartureSOCFromNode_PartialRecharging()//8 Only in VP case
        {
            for (int j = 0; j < numDuplicatedNodes; j++)
            {
                ILinearNumExpr DepartureSOCFromNode = LinearNumExpr();
                DepartureSOCFromNode.AddTerm(1.0, Delta[j]);
                DepartureSOCFromNode.AddTerm(1.0, Epsilon[j]);
                for (int i = 0; i < numDuplicatedNodes; i++)
                    DepartureSOCFromNode.AddTerm(-1.0 * (BatteryCapacity(VehicleCategories.EV) - minValue_Delta[j]), X[i][j][vIndex_EV]);
                string constraint_name = "Departure_SOC_From_ES_" + j.ToString();
                allConstraints_list.Add(AddLe(DepartureSOCFromNode, minValue_Delta[j], constraint_name));
                DepartureSOCFromNode.Clear();
            }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()//9
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
                    SOCDifference.AddTerm(EnergyConsumption(sFrom, sTo, VehicleCategories.EV) + BigDelta[i][j], X[i][j][vIndex_EV]);
                    string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, BigDelta[i][j], constraint_name));
                    SOCDifference.Clear();
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()//10
        {
            for (int j = 0; j < numDuplicatedNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                SOCDifference.AddTerm(EnergyConsumption(theDepot, sTo,VehicleCategories.EV), X[0][j][vIndex_EV]);
                string constraint_name = "SOC_Regulation_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
                SOCDifference.Clear();
            }
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()//11
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
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j][v]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                    TimeDifference.Clear();
                }
        }
        void AddConstraint_TimeRegulationFollowingAnESVisit_FixedRechargingDuration()//12 
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
            for (int i = firstESNodeIndex; i <= lastESNodeIndex; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
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
            for (int j = 1; j < numDuplicatedNodes; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int i = 0; i < numDuplicatedNodes; i++)
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
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numDuplicatedNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j][v]);
                }
            }
            string constraint_name = "Total_Travel_Time";
            double rhs = (numVehicles[vIndex_EV]+numVehicles[vIndex_GDV]) * theProblemModel.CRD.TMax;
            rhs -= theProblemModel.SRD.GetTotalCustomerServiceTime();
            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));
            TotalTravelTime.Clear();
        }
        void AddConstrain_TimeFeasibilityOfTwoConsecutiveArcs()//15
        {
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site from = preprocessedSites[i];
                for (int k = 0; k < numDuplicatedNodes; k++)
                {
                    Site through = preprocessedSites[k];
                    for (int j = 0; j < numDuplicatedNodes; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (i != j && j != k && i != k)
                            if (from.SiteType == SiteTypes.Customer && through.SiteType == SiteTypes.Customer && to.SiteType == SiteTypes.Customer)
                                if (minValue_T[i] + ServiceDuration(from)+ TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
                                {
                                    ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
                                    TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
                                    string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                                    allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                    TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                                }
                    }
                }
            }
        }
        void AddConstrain_EnergyFeasibilityOfTwoConsecutiveArcs()//16
        {
            for (int i = 0; i < numDuplicatedNodes; i++)
            {
                Site from = preprocessedSites[i];
                for (int k = 0; k < numDuplicatedNodes; k++)
                {
                    Site through = preprocessedSites[k];
                    for (int j = 0; j < numDuplicatedNodes; j++)
                    {
                        Site to = preprocessedSites[j];
                        if (EnergyConsumption(from, through, VehicleCategories.EV) + EnergyConsumption(through, to, VehicleCategories.EV) > BatteryCapacity(VehicleCategories.EV))
                        {
                            ILinearNumExpr EnergyFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k][vIndex_EV]);
                            EnergyFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j][vIndex_EV]);
                            string constraint_name = "No_arc_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(EnergyFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                            EnergyFeasibilityOfTwoConsecutiveArcs.Clear();
                        }
                    }
                }
            }
        }

        
        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            List<string> activeX = new List<string>();
            List<string> allX = new List<string>();
            for (int i = 0; i < numDuplicatedNodes; i++)
                for (int j = 0; j < numDuplicatedNodes; j++)
                    for (int v = 0; v < numVehCategories; v++)
                    {
                        allX.Add(preprocessedSites[i].ID + "," + preprocessedSites[j].ID + "," + v.ToString() + "->" + GetValue(X[i][j][v]).ToString());
                        if (GetValue(X[i][j][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            activeX.Add(preprocessedSites[i].ID + "," + preprocessedSites[j].ID + "," + v.ToString()+"->"+ GetValue(X[i][j][v]).ToString());
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
            foreach (List<string> nonDepotSiteIDs in GetListsOfNonDepotSiteIDs(vehicleCategory))
                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs,eSStayDurations));
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
                if (preprocessedSites[currentSiteIndex].SiteType == SiteTypes.ExternalStation)
                    AddESStayDuration(currentSiteIndex);
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
        void AddESStayDuration(int i)
        {
            Site ES = preprocessedSites[i];
            if (ES.SiteType != SiteTypes.ExternalStation)
                throw new System.Exception("Given site is not an ES.");
            else
                eSStayDurations.Add(ES.ID, GetValue(Epsilon[i]) / RechargingRate(ES));
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
            return "Node Duplicating wo U";
        }

        public override void RefineDecisionVariables(CustomerSet cS)
        {
            RHS_forNodeCoverage = new double[numDuplicatedNodes];
            int VCIndex = (int)xCplexParam.VehCategory;
            for (int i = 0; i < numDuplicatedNodes; i++)
                for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
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

            for (int j = firstCustomerNodeIndex; j <= lastCustomerNodeIndex; j++)
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
