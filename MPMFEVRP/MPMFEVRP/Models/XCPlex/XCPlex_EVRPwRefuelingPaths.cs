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
        int numNonESNodes; //The depot + customers
        int numCustomers, numES;

        //int firstCustomerVisitationConstraintIndex = -1;//This is followed by one constraint for each customer
        //int totalTravelTimeConstraintIndex = -1;
        //int totalNumberOfActiveArcsConstraintIndex = -1;//This is followed by one more-specific constraint for EV and one for GDV

        //double[] RHS_forNodeCoverage; //For different customer coverage constraints and solving TSP we need this preprocessed RHS values based on customer sets

        INumVar[][][][] X; double[][][][] X_LB, X_UB; //X_ijpv=1 if a vehicle v travels from i to j using refueling path p, for v=GDV p is always 0
        INumVar[][] U; double[][] U_LB, U_UB; //U_jv=1 if a vehicle visits a nonES node j
        INumVar[] Epsilon; double[] Epsilon_LB, Epsilon_UB; //Epsilon_j=energy gained at customer j if customer j is visited by an EV, Epsilon_LB otherwise
        INumVar[][][] Delta; double[][][] Delta_LB, Delta_UB; //Delta_ijp=departure SOC at customer j if an EV travels from i to j using refueling path p, Delta_LB otherwise 
        INumVar[][][] T; double[][][] T_LB, T_UB;//T_ijp=remaining duration of the workday when departing from customer j if an EV travels from i to j using refueling path p, Delta_LB otherwise

        public XCPlex_EVRPwRefuelingPaths() { }
        public XCPlex_EVRPwRefuelingPaths(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint)
            :base(theProblemModel, xCplexParam, customerCoverageConstraint)
        {
        }
        protected override void DefineDecisionVariables()
        {
            numCustomers = theProblemModel.SRD.NumCustomers;
            numES = theProblemModel.SRD.NumES;
            numNonESNodes = numCustomers + 1;
        }
        void SetMinAndMaxValuesOfModelSpecificVariables()
        {
            X_LB = new double[numNonESNodes][][][];//Xijpv: (i,j) in nonESNodes, p in refuelingPaths(i,j), v in vehicleCategories
            X_UB = new double[numNonESNodes][][][];
            Delta_LB = new double[numNonESNodes][][];//Delta_ijp: (i,j) in nonESNodes, p in refuelingPaths(i,j)
            Delta_UB = new double[numNonESNodes][][];
            T_LB = new double[numNonESNodes][][];//T_ijp: (i,j) in nonESNodes, p in refuelingPaths(i,j)
            T_UB = new double[numNonESNodes][][];
            U_LB = new double[numNonESNodes][];//Ujv: j in nonESNodes, v in vehicleCategories
            U_UB = new double[numNonESNodes][];
            Epsilon_LB = new double[numNonESNodes];//Epsilon_j: j in nonESNodes
            Epsilon_UB = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                X_LB[i] = new double[numNonESNodes][][];
                X_UB[i] = new double[numNonESNodes][][];
                Delta_LB[i] = new double[numNonESNodes][];
                Delta_UB[i] = new double[numNonESNodes][];
                T_LB[i] = new double[numNonESNodes][];
                T_UB[i] = new double[numNonESNodes][];
                U_LB[i] = new double[numVehCategories];
                U_UB[i] = new double[numVehCategories];
                Epsilon_LB[i] = 0.0;
                if(i==0)
                    Epsilon_UB[i] = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity * theProblemModel.NumVehicles[vIndex_EV];
                else
                    Epsilon_UB[i] = Math.Min(theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity,theProblemModel.SRD.GetSitesList(SiteTypes.Customer)[i-1].ServiceDuration);

                for (int j = 0; j < numNonESNodes; j++)
                {
                    //TODO: Write a refueling paths generator and its necessary methods
                    int numRefuelingPaths = 10; // getNumRefuelPaths(i, j);
                    X_LB[i][j] = new double[numRefuelingPaths][];
                    X_UB[i][j] = new double[numRefuelingPaths][];
                    Delta_LB[i][j] = new double[numRefuelingPaths];
                    Delta_UB[i][j] = new double[numRefuelingPaths];
                    T_LB[i][j] = new double[numRefuelingPaths];
                    T_UB[i][j] = new double[numRefuelingPaths];

                    for (int p = 0; p < numRefuelingPaths; p++)
                    {
                        X_LB[i][j][p] = new double[numVehCategories];
                        X_UB[i][j][p] = new double[numVehCategories];
                        Delta_LB[i][j][p] = 0.0; //this needs to be at least the energy to get to the first node in p
                        Delta_UB[i][j][p] = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity;
                        T_LB[i][j][p] = 0.0;//this needs to be at least the time remaining to do i-p-j-0 as fast as possible
                        T_UB[i][j][p] = theProblemModel.SRD.GetSingleDepotSite().DueDate; //this needs to be Tmax-t0i-si

                        for (int v = 0; v < numVehCategories; v++)
                        {
                            X_LB[i][j][p][v] = 0.0;
                            X_UB[i][j][p][v] = 1.0;
                        }
                    }
                }
                for (int v = 0; v < numVehCategories; v++)
                {
                    U_LB[i][v] = 0.0;
                    U_UB[i][v] = 1.0;
                }
            }
        }


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
