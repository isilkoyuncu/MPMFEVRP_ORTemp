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
    public class XCPlex_Model_GDV_SingleCustomerSet : XCPlexVRPBase
    {
        int numNonESNodes;
        int numCustomers;

        int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
        int totalTravelTimeConstraintIndex = -1;
        int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

        bool addTotalNumberOfActiveArcsCut = false;

        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        INumVar[][] X; double[][] X_LB, X_UB;
        INumVar[] T;

        IndividualRouteESVisits singleRouteESvisits;
        List<IndividualRouteESVisits> allRoutesESVisits = new List<IndividualRouteESVisits>();
        public XCPlex_Model_GDV_SingleCustomerSet() { }
        
        public XCPlex_Model_GDV_SingleCustomerSet(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
        }

        protected override void DefineDecisionVariables()
        {
            PreprocessSites();
            numCustomers = theProblemModel.SRD.NumCustomers;
            numNonESNodes = numCustomers + 1;
            SetMinAndMaxValuesOfModelSpecificVariables();

            allVariables_list = new List<INumVar>();

            //dvs: X_ijv and Y_irj
            AddTwoDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, out X);

            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out T);

            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXandTVariablesToLimits();
        }

        void SetUndesiredXandTVariablesToLimits()
        {
            T[0].LB = theProblemModel.CRD.TMax;
            T[0].UB = theProblemModel.CRD.TMax;

            //No arc from a node to itself
            for (int j = 0; j < numNonESNodes; j++)
            {
                X[j][j].UB = 0.0;
            }
        }
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {
            X_LB = new double[numNonESNodes][];
            X_UB = new double[numNonESNodes][];

            RHS_forNodeCoverage = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                X_LB[i] = new double[numNonESNodes];
                X_UB[i] = new double[numNonESNodes];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    X_LB[i][j] = 0.0;
                    X_UB[i][j] = 1.0;
                }
                RHS_forNodeCoverage[i] = 1.0;
            }
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i <= numCustomers; i++)\nfor (int j = 0; j <= numCustomers; j++)\nfor (int v = 0; v < fromProblem.NumVehicleCategories; v++)\nX[i][j][v]"
                + "then\n"
                + "for (int j = 0; j <= numCustomers; j++)\nT[j]\n";
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
                    {
                        objFunction.AddTerm(Prize(s, vehicleCategories[vIndex_EV]), X[i][j]);
                    }
            }
            //Second term Part I: distance-based costs from customer to customer directly
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    objFunction.AddTerm(-1.0 * GetVarCostPerMile(vehicleCategories[vIndex_EV]) * Distance(sFrom, sTo), X[i][j]);
                }
            }
            //Third term: vehicle fixed costs
            for (int j = 0; j < numNonESNodes; j++)
            {
                objFunction.AddTerm(-1.0 * GetVehicleFixedCost(vehicleCategories[vIndex_GDV]), X[0][j]);
            }
            //Now adding the objective function to the model
            objective = AddMaximize(objFunction);
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
                        objFunction.AddTerm(Distance(sFrom, sTo), X[i][j]);
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
                        objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[vIndex_EV]) * Distance(sFrom, sTo), X[i][j]);
                    }
                }
                //Third term: vehicle fixed costs
                for (int j = 0; j < numNonESNodes; j++)
                    objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[vIndex_GDV]), X[0][j]);
            }
            //Now adding the objective function to the model
            objective = AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_NumberOfVisitsPerCustomerNode();//1
            AddConstraint_IncomingXTotalEqualsOutgoingXTotalforGDV();//2
            AddConstraint_DepartingNumberOfGDVs();//3
            AddConstraint_TimeRegulationFollowingACustomerVisit();//4
            AddConstraint_ArrivalTimeLimits();//5
            AddConstraint_TotalTravelTime();//6

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
                ILinearNumExpr IncomingXToCustomerNodes = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                {
                    IncomingXToCustomerNodes.AddTerm(1.0, X[i][j]);
                }
                string constraint_name;

                switch (customerCoverageConstraint)
                {
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
                        constraint_name = "Exactly_one_vehicle_must_visit_the_customer_node_" + j.ToString();
                        allConstraints_list.Add(AddEq(IncomingXToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
                        constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                        allConstraints_list.Add(AddLe(IncomingXToCustomerNodes, RHS_forNodeCoverage[j], constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode invoked for CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce, which must not happen for a VRP!");
                    default:
                        throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
                }

            }
        }
        void AddConstraint_IncomingXTotalEqualsOutgoingXTotalforGDV()//2
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                
                    ILinearNumExpr IncomingXTotalMinusOutgoingXTotal = LinearNumExpr();

                    for (int i = 0; i < numNonESNodes; i++)
                    {
                        
                            IncomingXTotalMinusOutgoingXTotal.AddTerm(1.0, X[i][j]);
                            IncomingXTotalMinusOutgoingXTotal.AddTerm(-1.0, X[j][i]);
                        
                    }
                    string constraint_name = "Number_of_EVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_EVs";
                    allConstraints_list.Add(AddEq(IncomingXTotalMinusOutgoingXTotal, 0.0, constraint_name));
                
            }
        }
        void AddConstraint_DepartingNumberOfGDVs()//3
        {
            ILinearNumExpr NumberOfGDVsOutgoingFromTheDepot = LinearNumExpr();
            for (int j = 1; j < numNonESNodes; j++)
            {
                
                    NumberOfGDVsOutgoingFromTheDepot.AddTerm(1.0, X[0][j]);
                
            }
            if (customerCoverageConstraint == CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce)
            {
                string constraint_name = "Number_of_GDVs_outgoing_from_node_0_cannot_exceed_1";
                allConstraints_list.Add(AddLe(NumberOfGDVsOutgoingFromTheDepot, 1.0, constraint_name));
            }
            else if (customerCoverageConstraint == CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce)
            {
                string constraint_name = "Number_of_GDVs_outgoing_from_node_0_cannot_exceed_1";
                allConstraints_list.Add(AddEq(NumberOfGDVsOutgoingFromTheDepot, 1.0, constraint_name));
            }
            else
                throw new System.Exception("AddConstraint_NumberOfVisitsPerCustomerNode doesn't account for all CustomerCoverageConstraint_EachCustomerMustBeCovered!");
        }
        void AddConstraint_TimeRegulationFollowingACustomerVisit()//4
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom, sTo) + BigT[i][j]), X[i][j]);
                    string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * BigT[i][j], constraint_name));
                }
        }
        void AddConstraint_ArrivalTimeLimits()//5
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int i = 0; i < numNonESNodes; i++)
                {
                    TimeDifference.AddTerm((maxValue_T[j] - minValue_T[j]), X[i][j]);
                }
                string constraint_name = "Arrival_Time_Limit_at_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, maxValue_T[j], constraint_name));
            }
        }
        void AddConstraint_TotalTravelTime()//6
        {
            totalTravelTimeConstraintIndex = allConstraints_list.Count;

            ILinearNumExpr TotalTravelTime = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sFrom = preprocessedSites[i];
                    Site sTo = preprocessedSites[j];
                    TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j]);
                }

            string constraint_name = "Total_Travel_Time";
            double rhs = theProblemModel.CRD.TMax;           
            allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));

        }

        void AddAllCuts()
        {
            AddCut_TimeFeasibilityOfTwoConsecutiveArcs();
            AddCut_TotalNumberOfActiveArcs();
        }
        void AddCut_TimeFeasibilityOfTwoConsecutiveArcs()//16
        {
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site from = preprocessedSites[i];
                for (int k = 1; k < numNonESNodes; k++)
                {
                    Site through = preprocessedSites[k];
                    if (X[i][k].UB == 0.0)//The direct arc (from,through) has already been marked infeasible
                        continue;
                    for (int j = 0; j < numNonESNodes; j++)//This was starting at 1
                    {
                        Site to = preprocessedSites[j];
                        if (X[k][j].UB == 0.0)//The direct arc (through,to) has already been marked infeasible
                            continue;
                        if (i != j && j != k && i != k)
                        {
                            ILinearNumExpr TimeFeasibilityOfTwoConsecutiveArcs = LinearNumExpr();
                            if (minValue_T[i] + ServiceDuration(from) + TravelTime(from, through) + ServiceDuration(through) + TravelTime(through, to) > maxValue_T[j])
                            {

                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[i][k]);
                                TimeFeasibilityOfTwoConsecutiveArcs.AddTerm(1.0, X[k][j]);
                                string constraint_name = "No_arc_pair_from_node_" + i.ToString() + "_through_node_" + k.ToString() + "to_node_" + j.ToString();
                                allConstraints_list.Add(AddLe(TimeFeasibilityOfTwoConsecutiveArcs, 1.0, constraint_name));
                                TimeFeasibilityOfTwoConsecutiveArcs.Clear();
                            }
                        }//if (i != j && j != k && i != k)
                    }
                }
            }
        }
        void AddCut_TotalNumberOfActiveArcs()
        {
            totalNumberOfActiveArcsConstraintIndex = allConstraints_list.Count;

            int nActiveArcs_GDV = 1;
            ILinearNumExpr totalArcFlow_GDV = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    totalArcFlow_GDV.AddTerm(1.0, X[i][j]);
                }
            string constraintName_EV = "Number_of_active_GDV_arcs_cannot_exceed_" + nActiveArcs_GDV.ToString();
            allConstraints_list.Add(AddLe(totalArcFlow_GDV, nActiveArcs_GDV, constraintName_EV));
        }

        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            outcome.AddRange(GetVehicleSpecificRoutes(VehicleCategories.GDV));
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
            //We first determine the route start points
            List<List<int>> listOfFirstSiteIndices = new List<List<int>>();
            for (int j = 0; j < numNonESNodes; j++)
                if (GetValue(X[0][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    listOfFirstSiteIndices.Add(new List<int>() { j });
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
                    xValues[i, j] = GetValue(X[i][j]);//CONSULT (w/ Isil): Why only 0 when xValues is defined over all numVehCategories? IK: This was just debugging purposes, since EMH does not have any GDVs, I only wrote [0].
            
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
                if (GetValue(X[0][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to an X-flow from the depot!");
                }
            List<int> currentSiteIndices = firstSiteIndices;
            List<int> nextSiteIndices;
            singleRouteESvisits = new IndividualRouteESVisits();
            int i = 0, j = 0;
            do
            {
                j = currentSiteIndices.Last();
                if (currentSiteIndices.Count == 2)
                {
                    outcome.Add(ExternalStations[currentSiteIndices.First()].ID);
                }
                outcome.Add(preprocessedSites[currentSiteIndices.Last()].ID);

                nextSiteIndices = GetNextSiteIndices(currentSiteIndices.Last(), vehicleCategory);
                i = currentSiteIndices.Last();
                if (preprocessedSites[nextSiteIndices.Last()].ID == TheDepot.ID)
                {
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
                if (GetValue(X[currentSiteIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    return new List<int>() { nextCustomerIndex };
                }
            throw new System.Exception("Flow ended before returning to the depot!");
        }
        
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }

        public override string GetModelName()
        {
            return "GDV Optimize Single Customer Set";
        }
        public override void RefineDecisionVariables(CustomerSet cS)
        {
            RHS_forNodeCoverage = new double[numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 1; j < numNonESNodes; j++)
                {
                    if (cS.Customers.Contains(preprocessedSites[j].ID))
                    {
                        RHS_forNodeCoverage[j] = 1.0;
                        X[i][j].UB = 1.0;
                        X[j][i].UB = 1.0;
                    }
                    else
                    {
                        RHS_forNodeCoverage[j] = 0.0;
                        X[i][j].UB = 0.0;
                        X[j][i].UB = 0.0;
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
                allConstraints_array[c].UB = theProblemModel.CRD.TMax - 30.0 * RHS_forNodeCoverage.Sum();//theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
                //allConstraints_array[c].LB = theProblemModel.CRD.TMax - theProblemModel.SRD.GetTotalCustomerServiceTime(cS.Customers);
            }
            if (addTotalNumberOfActiveArcsCut)
            {
                c = totalNumberOfActiveArcsConstraintIndex;
                allConstraints_array[c].UB = theProblemModel.CRD.TMax + cS.NumberOfCustomers;
                //allConstraints_array[c].LB = allConstraints_array[c].LB + cS.NumberOfCustomers;
            }
        }

        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            throw new NotImplementedException();
        }
    }
}

