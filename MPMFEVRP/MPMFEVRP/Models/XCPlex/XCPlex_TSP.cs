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
    public class XCPlex_TSP : XCPlexVRPBase
    {
        //Problem specific parameters
        int numNonESNodes;
        double planningHorizonLength; //mins
        double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets
        int totalTravelTimeConstraintIndex = -1;

        //Decision Variables
        INumVar[][] X; double[][] X_LB, X_UB;    //X_ij
        INumVar[] ArrivalTime; double[] ArrivalTime_LB, ArrivalTime_UB; //Tau_j

        //Extracted decision variables. They are public for only testing purposes. Never ever use these or manipulate these outside of this class.
        //The output of this solver should never be the values of these decision variables but rather the vehicle specific routes.
        public double[] TauValues;
        public double[,] x_ij_Values;

        public XCPlex_TSP() { }
        public XCPlex_TSP(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            : base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
            //Do not uncomment these. They are here to show which methods are being implemented in the base

            //Initialize()
            //SpecializedInitialize()
            //DefineDecisionVariables()
            //AddTheObjectiveFunction()
            //AddAllConstraints()
            //SetCplexParameters()
            //InitializeOutputVariables()
        }
        /// <summary>
        /// SpecializedInitialize method is responsible from pulling all necessary parameters from theProblemModel, creating the model specific parameters, nondominated refueling arcs, and everything else before the decision variables are defined.
        /// Based on the model parameter selection, rechargingDuration_status, useTighterBounds, numVehicles are set in the Initialize() method.
        /// </summary>
        protected override void SpecializedInitialize()
        {
            numNonESNodes = numCustomers + 1; //because there's only a single depot
            planningHorizonLength = theProblemModel.CRD.TMax;
            SetAllOriginalSWAVs(); //from base
            PopulateSubLists(); //from base
            CalculateMinimumAndMaximumArrivalTimeBounds();
            PopulatePreprocessedSWAVs(); //from base. No ES should be copied to the preprocessedSites
            CalculateMinNumVehicles();
        }
        
        // Minimum and Maximum Arrival Time Limits
        void CalculateMinimumAndMaximumArrivalTimeBounds()
        {
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                double tauMin = TravelTime(TheDepot, swav);
                double tauMax = planningHorizonLength - TravelTime(swav, TheDepot) - ServiceDuration(swav);
                swav.UpdateArrivalTimeBounds(tauMax, tauMin);
            }
        }

        //Populate the general domain of the decision variables
        void PopulateUpperAndLowerBoundsOfDecisionVariables()
        {
            //Make sure you invoke this method when preprocessedSWAVs have been created!
            ArrivalTime_LB = new double[numNonESNodes];
            ArrivalTime_UB = new double[numNonESNodes];

            X_LB = new double[numNonESNodes][]; //numNonESNodes, numNonESNodes
            X_UB = new double[numNonESNodes][]; //numNonESNodes, numNonESNodes

            RHS_forNodeCoverage = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {              
                ArrivalTime_LB[i] = preprocessedSites[i].TauMin;
                ArrivalTime_UB[i] = preprocessedSites[i].TauMax;

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

        protected override void DefineDecisionVariables()
        {
            PopulateUpperAndLowerBoundsOfDecisionVariables();
            allVariables_list = new List<INumVar>();

            //dvs: X_ij
            AddTwoDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, out X);
        
            //auxiliaries (T_j, Delta_j, Epsilon_ij)
            AddOneDimensionalDecisionVariable("ArrivalTime", ArrivalTime_LB, ArrivalTime_UB, NumVarType.Float, numNonESNodes, out ArrivalTime);
            //All variables defined
            //allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            DecisionVariableFixing();
        }
        void DecisionVariableFixing()
        {
            ArrivalTime[0].LB = theProblemModel.CRD.TMax;
            ArrivalTime[0].UB = theProblemModel.CRD.TMax;
        }

        public override string GetDescription_AllVariables_Array()
        {
            throw new NotImplementedException();
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
            if (theProblemModel.ObjectiveFunction == ObjectiveFunctions.MinimizeVMT) //For a mixed fleet vrp this is not a meaningful objective function
            {
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                            objFunction.AddTerm(Distance(preprocessedSites[i], preprocessedSites[j]), X[i][j]);                    
                    }
            }
            else
            {                
                double refuelCostOfGasPerGallon = theProblemModel.CRD.RefuelCostofGas;
                double averageGasCostPerMile = refuelCostOfGasPerGallon * theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV).ConsumptionRate;
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        objFunction.AddTerm(averageGasCostPerMile * Distance(preprocessedSites[i], preprocessedSites[j]), X[i][j]);                        
                    }
            }
            //Now adding the objective function to the model
            objective = AddMinimize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();

            //Now adding the constraints one (family) at a time
            AddConstraint_NumberOfVisitsPerCustomerNodeIs1();
            AddConstraint_TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0();

            AddConstraint_ArrivingNumberOfGDVsAtDepotMustBeOne();

            AddConstraint_ArrivalTimeRegulationFollowingACustomerDirectly();
            AddConstraint_ArrivalTimeRegulationFollowingTheDepotDirectly();

            AddConstraint_TotalTravelTime();

            //AddKnownSolutionForTesting_Route();

            //All constraints and cuts added
            allConstraints_array = allConstraints_list.ToArray();
        }


        void AddConstraint_NumberOfVisitsPerCustomerNodeIs1()
        {
            for (int j = 1; j < numNonESNodes; j++)//Index 0 is the depot
            {
                ILinearNumExpr IncomingXandYToCustomerNodes = CreateCoreOf_Constraint_NumberOfVisitsPerCustomerNodeIs1(j);
                string constraint_name = "Exactly_1_vehicle_must_visit_the_customer_node_" + j.ToString();
                allConstraints_list.Add(AddEq(IncomingXandYToCustomerNodes, 1.0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_NumberOfVisitsPerCustomerNodeIs1(int j)
        {
            ILinearNumExpr IncomingXandYToCustomerNodes = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
            {
                IncomingXandYToCustomerNodes.AddTerm(1.0, X[i][j]);
            }
            return IncomingXandYToCustomerNodes;
        }
       
        void AddConstraint_TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0()
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                ILinearNumExpr TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0 = CreateCoreOf_Constraint_TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0(j);
                string constraint_name = "Number_of_GDVs_incoming_to_node_" + j.ToString() + "_equals_to_outgoing_GDVs";
                allConstraints_list.Add(AddEq(TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0, 0.0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0(int j)
        {
            ILinearNumExpr TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0 = LinearNumExpr();
            for (int i = 0; i < numNonESNodes; i++)
            {
                TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0.AddTerm(1.0, X[i][j]);
            }
            for (int k = 0; k < numNonESNodes; k++)
            {
                TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0.AddTerm(-1.0, X[j][k]);
            }
            return TotalIncomingGDVArcsMinusTotalOutgoingGDVArcsIs0;
        }

        void AddConstraint_ArrivingNumberOfGDVsAtDepotMustBeOne()
        {
            ILinearNumExpr NumberOfGDVsIncomingToTheDepotIsLimited = CreateCoreOf_Constraint_ArrivingNumberOfGDVsAtDepotMustBeOne();
            string constraint_name = "Number_of_GDVs_incoming_to_node_0_must_be_1";
            allConstraints_list.Add(AddLe(NumberOfGDVsIncomingToTheDepotIsLimited, 1, constraint_name));
        }
        private ILinearNumExpr CreateCoreOf_Constraint_ArrivingNumberOfGDVsAtDepotMustBeOne()
        {
            ILinearNumExpr NumberOfGDVsIncomingToTheDepotIsLimited = LinearNumExpr();
            for (int i = 1; i < numNonESNodes; i++)
            {
                NumberOfGDVsIncomingToTheDepotIsLimited.AddTerm(1.0, X[i][0]);
            }
            return NumberOfGDVsIncomingToTheDepotIsLimited;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingACustomerDirectly()
        {
            for (int i = 1; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {
                    double M = (preprocessedSites[i].TauMax - preprocessedSites[j].TauMin);
                    ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingACustomerDirectly(i, j, M);
                    string constraint_name = "Time_Regulation_Following_Customer_" + i.ToString() + "_to_node_" + j.ToString() + "_directly";
                    allConstraints_list.Add(AddGe(TimeDifference, (-1.0 * M), constraint_name));
                }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingACustomerDirectly(int i, int j, double M)
        {
            double totalArcTravelTime = preprocessedSites[i].ServiceDuration + TravelTime(preprocessedSites[i], preprocessedSites[j]); //service time and travel time
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            TimeDifference.AddTerm(-1.0, ArrivalTime[i]);
            TimeDifference.AddTerm(-1.0 * totalArcTravelTime, X[i][j]);
            TimeDifference.AddTerm(-1.0 * M, X[i][j]);
            return TimeDifference;
        }

        void AddConstraint_ArrivalTimeRegulationFollowingTheDepotDirectly()
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr TimeDifference = CreateCoreOf_Constraint_TimeRegulationFollowingTheDepotDirectly(j);
                string constraint_name = "Time_Regulation_Following_TheDepot_to_node_" + j.ToString() + "_directly";
                allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
            }
        }
        private ILinearNumExpr CreateCoreOf_Constraint_TimeRegulationFollowingTheDepotDirectly(int j)
        {
            double totalArcTravelTime = TravelTime(TheDepot, preprocessedSites[j]); //travel time only (from the depot)
            ILinearNumExpr TimeDifference = LinearNumExpr();
            TimeDifference.AddTerm(1.0, ArrivalTime[j]);
            TimeDifference.AddTerm(-1.0 * totalArcTravelTime, X[0][j]);
            return TimeDifference;
        }

        void AddConstraint_TotalTravelTime()
        {
            totalTravelTimeConstraintIndex = allConstraints_list.Count;

            if (RHS_forNodeCoverage != null)
            {
                ILinearNumExpr TotalTravelTime = LinearNumExpr();
                for (int i = 0; i < numNonESNodes; i++)
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sFrom = preprocessedSites[i];
                        Site sTo = preprocessedSites[j];
                        TotalTravelTime.AddTerm(TravelTime(sFrom, sTo), X[i][j]);                        
                    }
                string constraint_name = "Total_Travel_Time";
                double totalServiceTime = 0;
                for (int i = 0; i < numNonESNodes; i++)
                    totalServiceTime += RHS_forNodeCoverage[i] * preprocessedSites[i].ServiceDuration;
                double rhs = theProblemModel.CRD.TMax - totalServiceTime;
                allConstraints_list.Add(AddLe(TotalTravelTime, rhs, constraint_name));
            }
        }

        void AddKnownSolutionForTesting_Route()
        {
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                {               
                    X[i][j].UB = 0.0;
                    X[i][j].LB = 0.0;
                }
            //First tour
            X[0][1].UB = 1.0;
            X[0][1].LB = 1.0;

            X[1][2].UB = 1.0;
            X[1][2].LB = 1.0;

            X[2][3].UB = 1.0;
            X[2][3].LB = 1.0;

            X[3][4].UB = 1.0;
            X[3][4].LB = 1.0;

            X[4][5].UB = 1.0;
            X[4][5].LB = 1.0;

            X[5][6].UB = 1.0;
            X[5][6].LB = 1.0;

            X[6][7].UB = 1.0;
            X[6][7].LB = 1.0;

            X[7][8].UB = 1.0;
            X[7][8].LB = 1.0;

            X[8][0].UB = 1.0;
            X[8][0].LB = 1.0;
        }




        public override List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();

            outcome.AddRange(GetVehicleSpecificRoutes(VehicleCategories.GDV));

            return outcome;
        }
        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory)
        {
            GetDecisionVariableValues();

            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            List<List<string>> listOfNonDepotSiteIDs;            
            listOfNonDepotSiteIDs = GetListOfNonDepotSiteIDsGDV();
            int count = 0;
            foreach (List<string> NDSIDs in listOfNonDepotSiteIDs)
            {
                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, NDSIDs));
                count++;
            }
            return outcome;
        }

        public void GetDecisionVariableValues()
        {            
            x_ij_Values = new double[numNonESNodes, numNonESNodes];          
            TauValues = new double[numNonESNodes];
            for (int i = 0; i < numNonESNodes; i++)
            {
                TauValues[i] = GetValue(ArrivalTime[i]);
                for (int j = 0; j < numNonESNodes; j++)
                {
                    x_ij_Values[i, j] = GetValue(X[i][j]);
                }
            }
            WriteDecisionVariables();

        }
        void WriteDecisionVariables()
        {
            var pathX = @"C:\Users\tha810\Documents\Xvariables.txt";
            var pathT = @"C:\Users\tha810\Documents\Tvariables.txt";
            var sw = new System.IO.StreamWriter(pathX);

            for (int i = 0; i < numNonESNodes; i++)
            {
                for (int j = 0; j < numNonESNodes; j++)
                {
                        sw.Write(preprocessedSites[i].ID + "\t" + preprocessedSites[j].ID + "\t");
                        sw.WriteLine(x_ij_Values[i, j].ToString());
                }
            }
            sw.Close();
            sw = new System.IO.StreamWriter(pathT);
            for (int i = 0; i < numNonESNodes; i++)
            {
                sw.WriteLine(preprocessedSites[i].ID + "\t" + TauValues[i].ToString());
            }
            sw.Close();
            Console.WriteLine("data written to file");
        }
        List<List<string>> GetListOfNonDepotSiteIDsGDV()
        {
            List<List<string>> outcome = new List<List<string>>();
            List<string> singleRoute = new List<string>();
            List<List<string>> allActiveArcsGDV = new List<List<string>>();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (x_ij_Values[i, j] >= 0.5)
                    {
                        List<string> activeArc = new List<string>();
                        activeArc.Add(preprocessedSites[i].ID);
                        activeArc.Add(preprocessedSites[j].ID);
                        allActiveArcsGDV.Add(activeArc);
                    }

            List<List<string>> tempActiveArcs = new List<List<string>>(allActiveArcsGDV);
            string lastID = theProblemModel.SRD.GetSingleDepotID();
            while (tempActiveArcs.Count > 0)
            {
                do
                {
                    for (int i = 0; i < tempActiveArcs.Count; i++)
                    {
                        if (tempActiveArcs[i][0] == lastID)
                        {
                            singleRoute.Add(tempActiveArcs[i][1]);
                            lastID = singleRoute.Last();
                            tempActiveArcs.RemoveAt(i);
                            break;
                        }
                    }
                } while (lastID != theProblemModel.SRD.GetSingleDepotID());
                singleRoute.RemoveAt(singleRoute.Count - 1);
                outcome.Add(singleRoute);
                lastID = theProblemModel.SRD.GetSingleDepotID();
            }
            return outcome;

        }

        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }
        public override string GetModelName()
        {
            return "GVRP Mixed Fleet Multiple ES Visits";
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
            RefineTotalTravelTimeConstraints();
        }
        void RefineRightHandSidesOfCustomerVisitationConstraints()
        {
            int c = 0;

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
        void RefineTotalTravelTimeConstraints()
        {
            double rhs = 0.0;
            for (int j = 1; j < numNonESNodes; j++)
                if (RHS_forNodeCoverage[j] == 1)
                    rhs += preprocessedSites[j].ServiceDuration;

            allConstraints_array[totalTravelTimeConstraintIndex].LB = rhs;
            allConstraints_array[totalTravelTimeConstraintIndex].UB = rhs;
        }
        public override void RefineObjectiveFunctionCoefficients(Dictionary<string, double> customerCoverageConstraintShadowPrices)
        {
            throw new NotImplementedException();
        }
    }
}

