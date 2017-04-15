using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILOG.Concert;
using ILOG.CPLEX;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlex_ArcDuplicatingFormulation : XCPlexBase
    {
        List<int> customerSiteNodeIndices, depotPlusCustomerSiteNodeIndices, ESSiteNodeIndices;
        int numCustomers, numES;

        double[] minValue_T, maxValue_T, minValue_delta, maxValue_delta, minValue_epsilon, maxValue_epsilon;

        INumVar[][][] X;
        INumVar[][][] Y;
        INumVar[][] U;
        INumVar[] T;
        INumVar[] delta;
        INumVar[] epsilon;

        public XCPlex_ArcDuplicatingFormulation(ProblemModelBase problemModel, XCPlexParameters xCplexParam)
            : base(problemModel, xCplexParam)
        {
        }
        protected override void DefineDecisionVariables()
        {
            ArrangeNodesIntoLists();
            allVariables_list = new List<INumVar>();
            //X
            string[][][] X_name = new string[numCustomers+1][][];
            X = new INumVar[numCustomers + 1][][];
            for (int i = 0; i <= numCustomers; i++)
            {
                X_name[i] = new string[numCustomers + 1][];
                X[i] = new INumVar[numCustomers + 1][];
                for (int j = 0; j <= numCustomers; j++)
                {
                    X_name[i][j] = new string[problemModel.VRD.NumVehicleCategories];
                    X[i][j] = new INumVar[problemModel.VRD.NumVehicleCategories];
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    {
                        X_name[i][j][v] = "X_(" + i.ToString() + "," + j.ToString() + "," + v.ToString() + ")";
                        X[i][j][v] = NumVar(0, 1, variable_type, X_name[i][j][v]);
                        allVariables_list.Add(X[i][j][v]);
                    }//for v
                }//for j
            }//for i
            //Y
            string[][][] Y_name = new string[numCustomers + 1][][];
            Y = new INumVar[numCustomers + 1][][];
            for (int i = 0; i <= numCustomers; i++)
            {
                Y_name[i] = new string[numES][];
                Y[i] = new INumVar[numES][];
                for (int r = 0; r < numES; r++)
                {
                    Y_name[i][r] = new string[numCustomers + 1];
                    Y[i][r] = new INumVar[numCustomers + 1];
                    for (int j = 0; j <= numCustomers; j++)
                    {
                        Y_name[i][r][j] = "Y_(" + i.ToString() + "," + r.ToString() + "," + j.ToString() + ")";
                        Y[i][r][j] = NumVar(0, 1, variable_type, Y_name[i][r][j]);
                        allVariables_list.Add(Y[i][r][j]);
                    }//for j
                }//for r
            }//for i

            //U
            string[][] U_name = new string[numCustomers + 1][];
            U = new INumVar[numCustomers + 1][];
            for (int j = 0; j <= numCustomers; j++)
            {
                U_name[j] = new string[problemModel.VRD.NumVehicleCategories];
                U[j] = new INumVar[problemModel.VRD.NumVehicleCategories];
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    U_name[j][v] = "U_(" + j.ToString() + "," + v.ToString() + ")";
                    U[j][v] = NumVar(0, GetUpperBound(j,v), variable_type, U_name[j][v]);
                    allVariables_list.Add(U[j][v]);
                }//for v
            }//for j
            //auxiliaries (T, delta, epsilon)
            SetMinAndMaxValuesOfAuxiliaryVariables();
            T = new INumVar[numCustomers + 1];
            string[] T_name = new string[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
            {
                T_name[j] = "T_(" + j.ToString() + ")";
                T[j] = NumVar(minValue_T[j], maxValue_T[j], NumVarType.Float, T_name[j]);
                allVariables_list.Add(T[j]);
            }
            delta = new INumVar[numCustomers + 1];
            string[] delta_name = new string[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
            {
                delta_name[j] = "delta_(" + j.ToString() + ")";
                delta[j] = NumVar(minValue_delta[j], maxValue_delta[j], NumVarType.Float, delta_name[j]);
                allVariables_list.Add(delta[j]);
            }
            epsilon = new INumVar[numCustomers + 1];
            string[] epsilon_name = new string[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
            {
                epsilon_name[j] = "epsilon_(" + j.ToString() + ")";
                epsilon[j] = NumVar(minValue_epsilon[j], maxValue_epsilon[j], NumVarType.Float, epsilon_name[j]);
                allVariables_list.Add(epsilon[j]);
            }
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXYVariablesTo0();
        }
        void ArrangeNodesIntoLists()
        {
            numCustomers = problemModel.SRD.NumCustomers;
            numES = problemModel.SRD.NumES;

            customerSiteNodeIndices = new List<int>();
            depotPlusCustomerSiteNodeIndices = new List<int>();
            ESSiteNodeIndices = new List<int>();

            for (int orgSiteIndex = 0; orgSiteIndex < problemModel.SRD.SiteArray.Length; orgSiteIndex++)
            {
                switch (problemModel.SRD.SiteArray[orgSiteIndex].SiteType)
                {
                    case SiteTypes.Depot:
                        depotPlusCustomerSiteNodeIndices.Add(orgSiteIndex);
                        break;
                    case SiteTypes.Customer:
                        customerSiteNodeIndices.Add(orgSiteIndex);
                        depotPlusCustomerSiteNodeIndices.Add(orgSiteIndex);
                        break;
                    case SiteTypes.ExternalStation:
                        ESSiteNodeIndices.Add(orgSiteIndex);
                        break;
                    default:
                        throw new System.Exception("Site type incompatible!");
                }
            }
        }
        int GetUpperBound(int j, int v)
        {
            if (j == 0)
                return problemModel.VRD.NumVehicles[v];
            else
                return 1;
        }
        void SetUndesiredXYVariablesTo0()
        {
            //No arc from a node to itself
            for (int j = 0; j <= numCustomers; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    X[j][j][v].UB = 0.0;
            //No arc from a node to itself
            for (int j = 0; j <= numCustomers; j++)
                for (int r = 0; r < numES; r++)
                    Y[j][r][j].UB = 0.0;
            //No arc from depot to its duplicate
            for (int j = 0; j <= numCustomers; j++)
                for (int r = 0; r < numES; r++)
                    if (r == 0)//Depot duplicate
                    {
                        Y[0][r][j].UB = 0.0;
                        Y[j][r][0].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > 1
            for (int i = 0; i < numCustomers; i++)
                for (int j = 0; j < numCustomers; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                        {
                            if (problemModel.SRD.EnergyConsumption[customerSiteNodeIndices[i], customerSiteNodeIndices[j], v] > 1)
                                X[i][j][v].UB = 0.0;
                        }

        }
        void SetMinAndMaxValuesOfAuxiliaryVariables()
        {
            minValue_T = new double[numCustomers+1];
            maxValue_T = new double[numCustomers + 1];
            minValue_delta = new double[numCustomers + 1];
            maxValue_delta = new double[numCustomers + 1];
            minValue_epsilon = new double[numCustomers + 1];
            maxValue_epsilon = new double[numCustomers + 1];

            for (int j = 0; j <= numCustomers; j++)
            {
                minValue_T[j] = problemModel.SRD.TimeConsumption[0, depotPlusCustomerSiteNodeIndices[j]];
                maxValue_T[j] = problemModel.CRD.TMax - problemModel.SRD.TimeConsumption[depotPlusCustomerSiteNodeIndices[j], 0];
                if (problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[j]].SiteType == SiteTypes.Customer)
                    maxValue_T[j] -= problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[j]].ServiceDuration;

                //TODO Fine-tune the min and max values of delta
                if (j == 0)
                {
                    minValue_delta[j] = 0;
                    maxValue_delta[j] = 0;
                }
                else
                {
                    minValue_delta[j] = 0.0;
                    maxValue_delta[j] = 1.0;
                }
                if (j == 0)
                    minValue_epsilon[j] = 1;
                else
                    minValue_epsilon[j] = 0.0;
                if (problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[j]].SiteType == SiteTypes.Customer)
                    maxValue_epsilon[j] = Math.Min(1.0, problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[j]].ServiceDuration * Math.Min(problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[j]].RechargingRate, problemModel.VRD.VehicleArray[0].MaxChargingRate) / problemModel.VRD.VehicleArray[0].BatteryCapacity);
                else
                    maxValue_epsilon[j] = 1.0;
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
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: prize collection
            for (int j = 1; j <= numCustomers; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    objFunction.AddTerm(problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[j]].Prize[v], U[j][v]);
            //Second term Part I: distance-based costs from customer to customer directly
            for (int i = 0; i <= numCustomers; i++)
                for (int j = 0; j <= numCustomers; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        objFunction.AddTerm(-1.0 * problemModel.VRD.VehicleArray[v].VariableCostPerMile * problemModel.SRD.Distance[depotPlusCustomerSiteNodeIndices[i], depotPlusCustomerSiteNodeIndices[j]], X[i][j][v]);
            //Second term Part II: distance-based costs from customer to customer through an ES
            for (int i = 0; i <= numCustomers; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j <= numCustomers; j++)
                        for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                            if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                                objFunction.AddTerm(-1.0 * problemModel.VRD.VehicleArray[v].VariableCostPerMile * (problemModel.SRD.Distance[depotPlusCustomerSiteNodeIndices[i], ESSiteNodeIndices[r]] + problemModel.SRD.Distance[ESSiteNodeIndices[r], depotPlusCustomerSiteNodeIndices[j]]), Y[i][r][j]);
            //Third term: vehicle fixed costs
            for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                objFunction.AddTerm(-1.0 * problemModel.VRD.VehicleArray[v].FixedCost, U[0][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddConstraint_IncomingXandYTotalEqualsUforEV();       //1
            AddConstraint_IncomingXTotalEqualsUforGDV();          //2
            AddConstraint_OutgoingXandYTotalEqualsUforEV();       //3
            AddConstraint_OutgoingXTotalEqualsUforGDV();          //4
            AddConstraint_AtMostOneVisitPerNode();                //5
            AddConstraint_MaxNumberOfVehiclesPerCategory();       //6
            AddConstraint_TimeRegulationForDirectArcFromACustomer(); //7a
            AddConstraint_TimeRegulationForDirectArcFromDepot();//7b
            AddConstraint_TimeRegulationForCustomerAfterDepotThroughAnES(); //8a
            AddConstraint_TimeRegulationForCustomerThroughAnES();//8b
            AddConstraint_SOCRegulationFollowingNondepot();       //9a
            AddConstraint_SOCRegulationFollowingDepot();          //9b
            AddConstraint_SOCRegulationToAnES();                  //10
            AddConstraint_SOCRegulationFollowingAnES();           //11
            AddConstraint_MaxRechargeAtCustomerNode();            //13
            AddConstraint_MaxDepartureSOCFromCustomerNode();      //14
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddConstraint_IncomingXandYTotalEqualsUforEV() //1
        {
            for (int j = 0; j <= numCustomers; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    ILinearNumExpr IncomingXandYTotalMinusU = LinearNumExpr();
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                    {
                        for (int i = 0; i <= numCustomers; i++)
                        {
                            IncomingXandYTotalMinusU.AddTerm(1.0, X[i][j][v]);
                            for (int r = 0; r < numES; r++)
                                IncomingXandYTotalMinusU.AddTerm(1.0, Y[i][r][j]);
                        }
                        IncomingXandYTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(AddEq(IncomingXandYTotalMinusU, 0.0, constraint_name));
                    }
                    
                }
        }
        void AddConstraint_IncomingXTotalEqualsUforGDV() //2
        {
            for (int j = 0; j <= numCustomers; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.GDV)
                    {
                        for (int i = 0; i <= numCustomers; i++)
                            IncomingXTotalMinusU.AddTerm(1.0, X[i][j][v]);
                        IncomingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
                    }
                    
                }
        }
        void AddConstraint_OutgoingXandYTotalEqualsUforEV() //3
        {
            for (int j = 0; j <= numCustomers; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    ILinearNumExpr OutgoingXandYTotalMinusU = LinearNumExpr();
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                    {
                        for (int k = 0; k <= numCustomers; k++)
                        {
                            OutgoingXandYTotalMinusU.AddTerm(1.0, X[j][k][v]);
                            for (int r = 0; r < numES; r++)
                                OutgoingXandYTotalMinusU.AddTerm(1.0, Y[j][r][k]);
                        }
                        OutgoingXandYTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(AddEq(OutgoingXandYTotalMinusU, 0.0, constraint_name));
                    }
                }
        }
        void AddConstraint_OutgoingXTotalEqualsUforGDV() //4
        {
            for (int j = 0; j <= numCustomers; j++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                {
                    ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.GDV)
                    {
                        for (int k = 0; k <= numCustomers; k++)
                            OutgoingXTotalMinusU.AddTerm(1.0, X[j][k][v]);
                        OutgoingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
                    }
                }
        }
        void AddConstraint_AtMostOneVisitPerNode() //5
        {
            for (int j = 1; j <= numCustomers; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    NumberOfVehiclesVisitingTheNode.AddTerm(1.0, U[j][v]);
                string constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheNode, 1.0, constraint_name));
            }
        }
        void AddConstraint_MaxNumberOfVehiclesPerCategory()//6
        {
            for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, U[0][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_and_returning_to_node_0_cannot_exceed_" + problemModel.VRD.NumVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, problemModel.VRD.NumVehicles[v], constraint_name));
            }
        }
        void AddConstraint_TimeRegulationForDirectArcFromACustomer()//7
        {
            for (int i = 1; i <= numCustomers; i++)
                    for (int j = 0; j <= numCustomers; j++)
                    {
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                            TimeDifference.AddTerm(-1.0 * (problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[i]].ServiceDuration + problemModel.SRD.TimeConsumption[depotPlusCustomerSiteNodeIndices[i], depotPlusCustomerSiteNodeIndices[j]] + (maxValue_T[i] - minValue_T[j])), X[i][j][v]);
                        string constraint_name = "Time_Regulation_for_Direct_Arc_From_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationForDirectArcFromDepot()//7
        {
            int i = 0;
            for (int j = 0; j <= numCustomers; j++)
            {
                ILinearNumExpr TimeDifference = LinearNumExpr();
                TimeDifference.AddTerm(1.0, T[j]);
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    TimeDifference.AddTerm(-1.0 * (problemModel.SRD.TimeConsumption[depotPlusCustomerSiteNodeIndices[i], depotPlusCustomerSiteNodeIndices[j]]), X[i][j][v]);
                string constraint_name = "Time_Regulation_for_Direct_Arc_from_depot_to_node_" + j.ToString();
                allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
            }
        }
        void AddConstraint_TimeRegulationForCustomerAfterDepotThroughAnES()//8a
        {
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j <= numCustomers; j++)
                    {
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0 * (problemModel.SRD.TimeConsumption[depotPlusCustomerSiteNodeIndices[0], ESSiteNodeIndices[r]] + problemModel.SRD.TimeConsumption[ESSiteNodeIndices[r], depotPlusCustomerSiteNodeIndices[j]]), Y[0][r][j]);
                        string constraint_name = "Time_Regulation_from_Depot_to_Customer_node_" + j.ToString() + "_through_ES_node_" + r.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
                    }
        }
        void AddConstraint_TimeRegulationForCustomerThroughAnES()//8b
        {
            for (int i = 1; i <= numCustomers; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j <= numCustomers; j++)
                    {
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (problemModel.SRD.SiteArray[depotPlusCustomerSiteNodeIndices[i]].ServiceDuration + problemModel.SRD.TimeConsumption[depotPlusCustomerSiteNodeIndices[i], ESSiteNodeIndices[r]] + problemModel.SRD.TimeConsumption[ESSiteNodeIndices[r], depotPlusCustomerSiteNodeIndices[j]] + (maxValue_T[i] - minValue_T[j] + (1.0+ problemModel.SRD.EnergyConsumption[depotPlusCustomerSiteNodeIndices[i], ESSiteNodeIndices[r],0]) / problemModel.SRD.SiteArray[ESSiteNodeIndices[r]].RechargingRate)), Y[i][r][j]);//This assumes vehicle type 0 is the EV, and only it is the EV
                        TimeDifference.AddTerm(1.0 / problemModel.SRD.SiteArray[ESSiteNodeIndices[r]].RechargingRate, delta[i]);
                        TimeDifference.AddTerm(1.0 / problemModel.SRD.SiteArray[ESSiteNodeIndices[r]].RechargingRate, epsilon[i]);
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_Customer_node_" + j.ToString()+"_through_ES_node_" + r.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                    }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()//9a
        {
            for (int i = 1; i <= numCustomers; i++)
                for (int j = 0; j <= numCustomers; j++)
                {
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, delta[j]);
                    SOCDifference.AddTerm(-1.0, delta[i]);
                    SOCDifference.AddTerm(-1.0, epsilon[i]);
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                            SOCDifference.AddTerm(problemModel.SRD.EnergyConsumption[depotPlusCustomerSiteNodeIndices[i], depotPlusCustomerSiteNodeIndices[j], v] + maxValue_delta[j] - minValue_delta[i], X[i][j][v]);
                    string constraint_name = "SOC_Regulation_from_Customer_node_" + i.ToString() + "_to_Customer_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, maxValue_delta[j] - minValue_delta[i], constraint_name));
                }
        }
        void AddConstraint_SOCRegulationFollowingDepot()//9b
        {
            for (int j = 0; j <= numCustomers; j++)
            {
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, delta[j]);
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                    {
                        SOCDifference.AddTerm(problemModel.SRD.EnergyConsumption[depotPlusCustomerSiteNodeIndices[0], depotPlusCustomerSiteNodeIndices[j], v], X[0][j][v]);
                        string constraint_name = "SOC_Regulation_from_depot_to_customer_" + j.ToString();
                        allConstraints_list.Add(AddLe(SOCDifference, 1.0, constraint_name));
                    }
            }
        }
        void AddConstraint_SOCRegulationToAnES()//10
        {
            for (int j = 0; j <= numCustomers; j++)
                for (int r = 0; r < numES; r++)
                {
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, delta[j]);
                    SOCDifference.AddTerm(1.0, epsilon[j]);
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                        {
                            for (int k = 0; k <= numCustomers; k++)
                                SOCDifference.AddTerm(-1.0*problemModel.SRD.EnergyConsumption[depotPlusCustomerSiteNodeIndices[j], ESSiteNodeIndices[r], v], Y[j][r][k]);
                            string constraint_name = "SOC_Regulation_from_Customer_node_" + j.ToString() + "_to_ES_node_" + r.ToString();
                            allConstraints_list.Add(AddGe(SOCDifference, 0.0, constraint_name));
                        }
                }
        }
        void AddConstraint_SOCRegulationFollowingAnES()//11
        {
            for (int j = 0; j <= numCustomers; j++)
                for (int r = 0; r < numES; r++)
                {
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, delta[j]);
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                        {
                            for (int i = 0; i <= numCustomers; i++)
                                SOCDifference.AddTerm(problemModel.SRD.EnergyConsumption[ESSiteNodeIndices[r], depotPlusCustomerSiteNodeIndices[j], v], Y[i][r][j]);
                            string constraint_name = "SOC_Regulation_from_ES_node_" + r.ToString() + "_to_Customer_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(SOCDifference, 1.0, constraint_name));
                        }
                }
        }
        void AddConstraint_MaxRechargeAtCustomerNode()//13
        {
            for (int j = 1; j <= numCustomers; j++)
            {
                ILinearNumExpr RechargeAtCustomer = LinearNumExpr();
                RechargeAtCustomer.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                    {
                        RechargeAtCustomer.AddTerm(-1.0 * maxValue_epsilon[j], U[j][v]);
                        string constraint_name = "Max_Recharge_At_Customer_" + j.ToString();
                        allConstraints_list.Add(AddLe(RechargeAtCustomer, 0.0, constraint_name));
                    }
            }
        }
        void AddConstraint_MaxDepartureSOCFromCustomerNode()//14

        {
            for (int j = 1; j <= numCustomers; j++)
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, epsilon[j]);
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (problemModel.VRD.VehicleArray[v].Category == VehicleCategories.EV)
                    {
                        DepartureSOCFromCustomer.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                        allConstraints_list.Add(AddLe(DepartureSOCFromCustomer, 0.0, constraint_name));
                    }
            }
        }


        public List<Tuple<int, int, int>> GetXVariablesSetTo1()
        {
            if (solutionStatus != XCPlexSolutionStatus.Optimal)
                return null;
            List<Tuple<int, int, int>> outcome = new List<Tuple<int, int, int>>();
            for (int i = 0; i <= numCustomers; i++)
                for (int j = 0; j <= numCustomers; j++)
                    for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                        if (GetValue(X[i][j][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            outcome.Add(new Tuple<int, int, int>(depotPlusCustomerSiteNodeIndices[i], depotPlusCustomerSiteNodeIndices[j], v));
            return outcome;
        }
        public List<Tuple<int, int, int>> GetYVariablesSetTo1()
        {
            if (solutionStatus != XCPlexSolutionStatus.Optimal)
                return null;
            List<Tuple<int, int, int>> outcome = new List<Tuple<int, int, int>>();
            for (int i = 0; i <= numCustomers; i++)
                for (int r = 0; r < numES; r++)
                    for (int j = 0; j <= numCustomers; j++)
                        if (GetValue(Y[i][r][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            outcome.Add(new Tuple<int, int, int>(depotPlusCustomerSiteNodeIndices[i], ESSiteNodeIndices[r], depotPlusCustomerSiteNodeIndices[j]));
            return outcome;
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            List<Tuple<int, int, int>> XVariablesSetTo1 = GetXVariablesSetTo1();
            List<Tuple<int, int, int>> YVariablesSetTo1 = GetYVariablesSetTo1();
            foreach (Tuple<int, int, int> t in YVariablesSetTo1)
            {
                //Here we assume there are 2 categories of vehicles and category 0 is always the EV
                XVariablesSetTo1.Add(new Tuple<int, int, int>(t.Item1, t.Item2, 0));
                XVariablesSetTo1.Add(new Tuple<int, int, int>(t.Item2, t.Item3, 0));
            }

            return new NEW_RouteBasedSolution(problemModel, XVariablesSetTo1);
        }
    }
}
