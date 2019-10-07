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

        bool addTotalNumberOfActiveArcsCut = false;

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        INumVar[][][] X; double[][][] X_LB, X_UB; //[i=1,..,numNonESNodes][j=1,..,numNonESNodes][r=1,..,numNondominatedRPs(i,j)], defined for each possible arc in between nonESNodes i and j, (Binary)
        INumVar[][][] W; double[][][] W_LB, W_UB; //[i=1,..,numNonESNodes][j=1,..,numNonESNodes][r=1,..,numNondominatedRPs(i,j)], defined for each possible arc in between nonESNodes i and j, (Continuous)
        INumVar[][][] U; double[][][] U_LB, U_UB; //[i=1,..,numNonESNodes][j=1,..,numNonESNodes][r=1,..,numNondominatedRPs(i,j)], defined for each possible arc in between nonESNodes i and j, (Continuous)

        INumVar[] Epsilon;
        INumVar[] Delta;
        INumVar[] T;

        IndividualRouteESVisits singleRouteESvisits;
        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();

        List<List<string>> EV_optRouteIDs = new List<List<string>>();

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
                    allNondominatedRPs[i, j] = rpg.GenerateNonDominatedBetweenODPairIK(preprocessedSites[i], preprocessedSites[j], theProblemModel.SRD);
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
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT)
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
            AddConstraint_NumberOfVisitsPerNode();
            AddConstraint_IncomingXTotalEqualsOutgoingXTotal();
            //AddConstraint_SOCRegulationFromDepot();
            //AddConstraint_SOCRegulationThroughNondepotandDirect();
            //AddConstraint_OriginDepartureSOCThroughRP();
            //AddConstraint_DestinationDepartureSOCThroughRP();

            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_NumberOfVisitsPerNode()
        {
            firstCustomerVisitationConstraintIndex = allConstraints_list.Count;

            for (int j = 0; j <numNonESNodes; j++)//Index 0 is the depot
            {
                ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                {
                    for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j][r]);
                }
                string constraint_name = "Each_node_" + j.ToString() + "_must_be_visited_exactly_once.";
                allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
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
                    }
                }
                for (int k = 0; k < numNonESNodes; k++)
                {
                    for (int r = 0; r < allNondominatedRPs[j,k].Count; r++)
                    {
                        IncomingXYTotalMinusOutgoingXYTotal.AddTerm(1.0, X[j][k][r]);
                    }
                }
                string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
                allConstraints_list.Add(AddEq(IncomingXYTotalMinusOutgoingXYTotal, 0.0, constraint_name));
            }
        }
        void AddConstraint_SOCRegulationFromDepot()
        {
            for (int j = 1; j < numNonESNodes; j++)
                if (allNondominatedRPs[0, j].Count > 0)
                    for (int r = 0; r < allNondominatedRPs[0, j].Count; r++)
                    {
                        ILinearNumExpr SOCDifference = LinearNumExpr();
                        SOCDifference.AddTerm(1.0, Delta[j]);
                        double Xcoeff = 0.0;
                        Xcoeff += allNondominatedRPs[0, j][r].TotalEnergyConsumption;
                        Xcoeff += preprocessedSites[j].DeltaMax;
                        Xcoeff -= BatteryCapacity(VehicleCategories.EV);
                        SOCDifference.AddTerm(Xcoeff, X[0][j][r]);
                        string constraint_name = "SOC_Regulation_from_node_" + 0.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(SOCDifference, preprocessedSites[j].DeltaMax, constraint_name));
                    }
        }
        void AddConstraint_SOCRegulationThroughNondepotandDirect()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (allNondominatedRPs[i, j].Count > 0)
                        for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        {
                            ILinearNumExpr SOCDifference = LinearNumExpr();
                            SOCDifference.AddTerm(1.0, Delta[j]);
                            SOCDifference.AddTerm(-1.0, Delta[i]);
                            double Xcoeff = 0.0;
                            Xcoeff += allNondominatedRPs[i, j][r].TotalEnergyConsumption;
                            Xcoeff += preprocessedSites[j].DeltaMax;
                            Xcoeff -= preprocessedSites[i].DeltaMin;
                            SOCDifference.AddTerm(Xcoeff, X[i][j][r]);
                            string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(SOCDifference, (preprocessedSites[j].DeltaMax- preprocessedSites[i].DeltaMin), constraint_name));
                        }
        }
        void AddConstraint_OriginDepartureSOCThroughRP()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    {
                        ILinearNumExpr SOCDifference = LinearNumExpr();
                        SOCDifference.AddTerm(1.0, Delta[i]);
                        SOCDifference.AddTerm(-1.0*allNondominatedRPs[i, j][r].FirstArcEnergyConsumption, X[i][j][r]);
                        string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(SOCDifference, 0.0, constraint_name));
                    }
        }
        void AddConstraint_DestinationDepartureSOCThroughRP()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    {
                        ILinearNumExpr SOCDifference = LinearNumExpr();
                        SOCDifference.AddTerm(1.0, Delta[j]);
                        SOCDifference.AddTerm(allNondominatedRPs[i, j][r].LastArcEnergyConsumption, X[i][j][r]);
                        string constraint_name = "SOC_Regulation_from_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(SOCDifference, BatteryCapacity(VehicleCategories.EV), constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationThroughRP()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                    {
                        SiteWithAuxiliaryVariables sFrom = preprocessedSites[i];
                        SiteWithAuxiliaryVariables sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + allNondominatedRPs[i, j][r].TotalTime +sTo.TES-sFrom.TLS), X[i][j][r]);
                        string constraint_name = "Time_Regulation_from_customer_" + i.ToString() + "_through_ES_" + r.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (sTo.TES - sFrom.TLS), constraint_name));
                    }
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
            List<List<string>> listOfActiveArcs = new List<List<string>>();
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for (int r = 0; r < allNondominatedRPs[i, j].Count; r++)
                        if (GetValue(X[i][j][r]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                        {
                            List<string> activeArc = new List<string> { preprocessedSites[i].ID };
                            for (int k = 0; k < allNondominatedRPs[i, j][r].RefuelingStops.Count; k++)
                            {
                                activeArc.Add(allNondominatedRPs[i, j][r].RefuelingStops[k].ID);
                            }
                            activeArc.Add(preprocessedSites[j].ID);
                            listOfActiveArcs.Add(activeArc);
                        }


            List<List<string>> finalRoutes = new List<List<string>>();
            bool isNewRoute = true;
            while (listOfActiveArcs.Count > 0)
            {
                if (isNewRoute == true)
                {
                    foreach (List<string> r in listOfActiveArcs)
                    {
                        if (r[0] == theProblemModel.SRD.GetSingleDepotID())
                        {
                            EV_optRouteIDs.Add(r);
                            finalRoutes.Add(r);
                            finalRoutes.Last().RemoveAt(0); //To avoid adding the depot to the nonDepotSiteIDs
                            finalRoutes.Last().RemoveAt(finalRoutes.Last().Count - 1); //To avoid adding the same node at the end of the first arc and at the beginning of the next arc.
                            EV_optRouteIDs.Add(r);
                            EV_optRouteIDs.Last().RemoveAt(EV_optRouteIDs.Last().Count - 1);
                            listOfActiveArcs.Remove(r);
                            isNewRoute = false;
                            break;
                        }
                    }
                }
                while (isNewRoute == false)
                {
                    List<string> lastArc = finalRoutes.Last();
                    foreach (List<string> r in listOfActiveArcs)
                    {
                        if (r[0] == lastArc[lastArc.Count - 1])
                        {
                            finalRoutes.Last().AddRange(r);
                            listOfActiveArcs.Remove(r);
                            EV_optRouteIDs.Add(r);
                            EV_optRouteIDs.Last().RemoveAt(EV_optRouteIDs.Last().Count - 1);
                            if (lastArc[lastArc.Count - 1] != theProblemModel.SRD.GetSingleDepotID())
                            {
                                finalRoutes.Last().RemoveAt(finalRoutes.Last().Count - 1); //To avoid adding the same node at the end of the first arc and at the beginning of the next arc.
                                break;
                            }
                            else
                            {
                                isNewRoute = true;
                                break;
                            }                           
                        }
                    }
                }
            }
            return finalRoutes;
        }
        public void GetDecisionVariableValues()
        {
            double[,] xValues = new double[numNonESNodes, numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    for(int r=0; r<allNondominatedRPs[i,j].Count; r++)
                    xValues[i, j] = GetValue(X[i][j][r]);

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
         IndividualESVisitDataPackage GetIndividualESVisit(int i, int r, int j)
        {
            throw new NotImplementedException();
            //Site from = preprocessedSites[i];
            //Site to = preprocessedSites[j];

            //double timeSpentInES = GetValue(T[j]) - TravelTime(from, ES) - ServiceDuration(from) - GetValue(T[i]) - TravelTime(ES, to);
            //return new IndividualESVisitDataPackage(ES.ID, timeSpentInES / RechargingRate(ES), preprocessedESSiteIndex: r);
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        public void RefineDecisionVariables(CustomerSet cS, bool preserveCustomerVisitSequence)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            //GDV_optRouteIDs = cS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs;
            //GDV_optRoute = GetGDVoptRouteSWAVs();

            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        {
                            X[i][j][r].UB = 1.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 1.0;
                        }
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        for (int r = 0; r < allNondominatedRPs[i,j].Count; r++)
                        {
                            X[i][j][r].UB = 0.0;
                        }
                        for (int r = 0; r < allNondominatedRPs[j, i].Count; r++)
                        {
                            X[j][i][r].UB = 0.0;
                        }
                    }
                }
            RefineRightHandSidesOfCustomerVisitationConstraints();
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
                outcome.AddRange(theProblemModel.SRD.PopulateRefuelingPathsBetween(rpg, from, to));
                from = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
            }
            return outcome;
        }
        RefuelingPathList GetAllNonDominatedRefuelingPaths()
        {
            RefuelingPathList outcome = new RefuelingPathList();
            SiteWithAuxiliaryVariables from = theProblemModel.SRD.AllOriginalSWAVs.First();
            List<string> listOfVisitedSiteIncludingDepotIDs = new List<string>();
            for(int routeNo=0; routeNo< GetListsOfNonDepotSiteIDs(VehicleCategories.EV).Count; routeNo++)
            for (int i = 1; i < listOfVisitedSiteIncludingDepotIDs.Count; i++)
            {
                SiteWithAuxiliaryVariables to = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
                outcome.AddRange(theProblemModel.SRD.PopulateRefuelingPathsBetween(rpg, from, to));
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

