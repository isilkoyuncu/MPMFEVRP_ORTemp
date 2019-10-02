using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using ILOG.Concert;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Solutions;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCplex_RefuelingPathReplacement : XCPlexBase
    {
        RefuelingPathGenerator rpg;
        List<string> GDV_optRouteIDs;
        List<SiteWithAuxiliaryVariables> GDV_optRoute;
        int numNodes; //= 1 + numCustomers + 1, since the route is given as: theDepot-customers-theDepot
        int numArcs;
        int[] numRefuelingPaths; //[numArcs], since there will be numNodes-1 active arcs
        RefuelingPathList[] refuelingPathsCS;

        INumVar[][] X; double[][] X_LB, X_UB; //[i=1..numArcs][numRefuelingPaths(i)], Defined for each possible arc (Binary)
        INumVar[][] W; double[][] W_LB, W_UB; //[i=1..numArcs][numRefuelingPaths(i)], Defined for each possible arc (Continuous)
        INumVar[][] U; double[][] U_LB, U_UB; //[i=1..numArcs][numRefuelingPaths(i)], Defined for each possible arc (Continuous)

        INumVar[] EnergySlack; double[] EnergySlack_LB, EnergySlack_UB;//Defined for each node
        INumVar[] TimeSlack; double[] TimeSlack_LB, TimeSlack_UB;//Defined for each node

        public List<string> NonDepotSiteIDs { get { return GetNonDepotSiteIDs(); } }

        public XCplex_RefuelingPathReplacement() { }
        public XCplex_RefuelingPathReplacement(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, VehicleSpecificRouteOptimizationOutcome vsroo_GDV)
        {
            //Verification
            if (vsroo_GDV.VehicleCategory != VehicleCategories.GDV)
                throw new System.Exception("XCplex_RefuelingPathReplacement is called with a VehicleSpecificRouteOptimizationOutcome other than GDV!");

            //Implementation
            this.theProblemModel = theProblemModel;
            rpg = new RefuelingPathGenerator(theProblemModel);
            this.xCplexParam = xCplexParam;
            GDV_optRouteIDs = vsroo_GDV.VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs;
            GDV_optRoute = GetGDVoptRouteSWAVs();
            numNodes = GDV_optRoute.Count;
            numArcs = numNodes - 1;
            refuelingPathsCS = GetNonDominatedRefuelingPathsForGDV_OptRoute(vsroo_GDV);
            numRefuelingPaths = new int[numArcs];
            for (int i = 0; i < numArcs; i++)
                numRefuelingPaths[i] = refuelingPathsCS[i].Count;
            SetUBLBofDVs();
        }

        protected override void DefineDecisionVariables()
        {
            for (int i = 0; i < numNodes - 1; i++)
            {
                AddTwoDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numArcs, (refuelingPathsCS[i].Count), out X);
                AddTwoDimensionalDecisionVariable("W", W_LB, W_UB, NumVarType.Float, numArcs, (refuelingPathsCS[i].Count), out W);
                AddTwoDimensionalDecisionVariable("U", U_LB, U_UB, NumVarType.Float, numArcs, (refuelingPathsCS[i].Count), out U);
                AddOneDimensionalDecisionVariable("EnergySlack", EnergySlack_LB, EnergySlack_UB, NumVarType.Float, numNodes, out EnergySlack);
                AddOneDimensionalDecisionVariable("TimeSlack", TimeSlack_LB, TimeSlack_UB, NumVarType.Float, numNodes, out TimeSlack);
            }
        }
        protected override void AddTheObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();        
            objFunction.AddTerm(1.0,TimeSlack[numNodes-1]);
            //Now adding the objective function to the model
            objective = AddMaximize(objFunction);
        }
        void SetUBLBofDVs()
        {
            X_LB = new double[numArcs][];
            X_UB = new double[numArcs][];

            W_LB = new double[numArcs][];
            W_UB = new double[numArcs][];

            U_LB = new double[numArcs][];
            U_UB = new double[numArcs][];
            for (int i = 0; i < numArcs; i++)
            {
                RefuelingPathList rps = refuelingPathsCS[i];
                int numRPs = rps.Count;
                for (int j = 0; j < numRPs; j++)
                {
                    X_LB[i] = new double[numRPs];
                    X_UB[i] = new double[numRPs];
                    X_LB[i][j] = 0.0;
                    X_UB[i][j] = 1.0;
                    W_LB[i] = new double[numRPs];
                    W_UB[i] = new double[numRPs];
                    W_LB[i][j] = -1.0*(theProblemModel.SRD.GetEVEnergyConsumption(GDV_optRouteIDs[i],GDV_optRouteIDs[i+1]));
                    W_UB[i][j] = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity- rps[j].LastArcEnergyConsumption-rps[j].FirstArcEnergyConsumption;
                    U_LB[i] = new double[numRPs];
                    U_UB[i] = new double[numRPs];
                    U_LB[i][j] = theProblemModel.SRD.GetTravelTime(GDV_optRouteIDs[i], GDV_optRouteIDs[i + 1]);
                    U_UB[i][j] = rps[j].TotalTime;
                }
            }
            EnergySlack_LB = new double[numNodes];
            EnergySlack_UB = new double[numNodes];
            TimeSlack_LB = new double[numNodes];
            TimeSlack_UB = new double[numNodes];
            for (int k = 0; k < numNodes; k++)
            {
                EnergySlack_LB[k] = GDV_optRoute[k].DeltaMin;
                EnergySlack_UB[k] = GDV_optRoute[k].DeltaMax;
                TimeSlack_LB[k] = theProblemModel.CRD.TMax - GDV_optRoute[k].TLS;
                TimeSlack_UB[k] = theProblemModel.CRD.TMax - GDV_optRoute[k].TES;
            }
        }


        protected override void AddAllConstraints()
        {
            AddConstraint_TimeConservation();
            AddConstraint_ActiveRPSelection();
        }
        void AddConstraint_ActiveRPSelection()
        {
            for (int i = 0; i < numArcs; i++)
            {
                ILinearNumExpr ActiveRP = LinearNumExpr();
                for (int r = 0; r < numRefuelingPaths[i]; r++)
                {
                    ActiveRP.AddTerm(1.0, X[i][r]);
                    string constraint_name = "ActiveRP_" + r.ToString() + "_through_position_" + i.ToString();
                    allConstraints_list.Add(AddEq(ActiveRP, 1.0, constraint_name));
                }
            }
        }
        void AddConstraint_TimeConservation()
        {
            for (int j = 1; j < numNodes; j++)
                for (int r = 0; r < numRefuelingPaths[j - 1]; r++)
                {
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, TimeSlack[j - 1]);
                    TimeDifference.AddTerm(-1.0, TimeSlack[j]);
                    TimeDifference.AddTerm(-1.0, U[j - 1][r]);
                    string constraint_name = "Time_Regulation_through_RS_" + r.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddEq(TimeDifference, 0.0, constraint_name));
                }
        }
        void AddConstraint_TravelTime()
        {
            for (int j = 0; j < numArcs; j++)
                for (int r = 0; r < numRefuelingPaths[j - 1]; r++)
                {
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, TimeSlack[j - 1]);
                    TimeDifference.AddTerm(-1.0, TimeSlack[j]);
                    TimeDifference.AddTerm(-1.0, U[j - 1][r]);
                    string constraint_name = "Time_Regulation_through_RS_" + r.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddEq(TimeDifference, 0.0, constraint_name));
                }
        }
        List<SiteWithAuxiliaryVariables> GetGDVoptRouteSWAVs()
        {
            List<SiteWithAuxiliaryVariables> output = new List<SiteWithAuxiliaryVariables>();
            foreach (string id in GDV_optRouteIDs)
                output.Add(theProblemModel.SRD.GetSWAVByID(id));
            return output;
        }
        RefuelingPathList[] GetNonDominatedRefuelingPathsForGDV_OptRoute(VehicleSpecificRouteOptimizationOutcome vsroo_GDV)
        {
            List<RefuelingPathList> outcome = new List<RefuelingPathList>();
            List<string> listOfVisitedSiteIncludingDepotIDs = vsroo_GDV.VSOptimizedRoute.ListOfVisitedSiteIncludingDepotIDs;
            SiteWithAuxiliaryVariables from = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[0]);
            for (int i=1; i< listOfVisitedSiteIncludingDepotIDs.Count; i++)
            {
                SiteWithAuxiliaryVariables to = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
                RefuelingPathList tempRPList = theProblemModel.SRD.PopulateRefuelingPathsBetween(rpg, from, to);
                outcome.Add(tempRPList);
                from = theProblemModel.SRD.GetSWAVByID(listOfVisitedSiteIncludingDepotIDs[i]);
            }
            return outcome.ToArray();
        }
        protected override void Initialize()
        {
            throw new NotImplementedException();
        }
        public List<VehicleSpecificRoute> GetEVSpecificRoute()
        {
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
                outcome.AddRange(GetVehicleSpecificRoutes(VehicleCategories.EV));
            return outcome;
        }
        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes(VehicleCategories vehicleCategory)
        {
            Vehicle theVehicle = theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory);//Pulling the vehicle infor from the Problem Model. Not exactly flexible, but works as long as we have only two categories of vehicles and no more than one of each
            List<VehicleSpecificRoute> outcome = new List<VehicleSpecificRoute>();
            outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, NonDepotSiteIDs));
            return outcome;
        }
        public void RefineDecisionVariables(VehicleSpecificRoute GDV_OptRoute)
        {

        }
        public VehicleSpecificRoute GetEVRecoveredRoute()
        {
            throw new NotImplementedException();
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetEVSpecificRoute());
        }
        List<string> GetNonDepotSiteIDs()
        {
            List<string> EVRoute = new List<string>();
            int[][] activeDVs = new int[numNodes - 1][];
            for (int i = 0; i < numNodes - 1; i++)
            {
                activeDVs[i] = new int[refuelingPathsCS[i].Count];
                for(int j=0; j<refuelingPathsCS[i].Count; j++)
                    if(GetValue(X[i][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                    {
                        activeDVs[i][j] = 1;
                        EVRoute.Add(GDV_optRouteIDs[i]);
                        EVRoute.AddRange(refuelingPathsCS[i][j].GetRefuelingStopIDs());
                    }

            }
            EVRoute.Add(GDV_optRouteIDs[numNodes-1]);
            return EVRoute;
        }
        public override string GetDescription_AllVariables_Array()
        {
            throw new NotImplementedException();
        }
        public override string GetModelName()
        {
            return "Refueling path replacement model";
        }
    }
}
