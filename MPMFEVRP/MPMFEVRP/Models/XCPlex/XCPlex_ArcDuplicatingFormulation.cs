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
    // TODO apply all the changes that are done in node based formulation!!!
    //TODO get all sites here and then use the mapping we coded
    public class XCPlex_ArcDuplicatingFormulation : XCPlexBase
    {
        int numNonESNodes;
        int numCustomers, numES;

        Site theDepot;
        
        INumVar[][][] X; double[][][] X_LB, X_UB;
        INumVar[][][] Y; double[][][] Y_LB, Y_UB;
        INumVar[][] U; double[][] U_LB, U_UB;
        INumVar[] T; double[] minValue_T, maxValue_T;
        INumVar[] Delta; double[] minValue_Delta, maxValue_Delta;
        INumVar[] Epsilon; double[] minValue_Epsilon, maxValue_Epsilon;

        public XCPlex_ArcDuplicatingFormulation(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
            : base(theProblemModel, xCplexParam){}

        protected override void DefineDecisionVariables()
        {
            OrganizeSites();
            SetMinAndMaxValuesOfAllVariables();
            allVariables_list = new List<INumVar>();

            //dvs: X_ijv, Y_irj and U_jv
            AddThreeDimensionalDecisionVariable("X", X_LB, X_UB, NumVarType.Int, numNonESNodes, numNonESNodes, numVehCategories, out X);
            AddThreeDimensionalDecisionVariable("Y", Y_LB, Y_UB, NumVarType.Int, numNonESNodes, numES, numNonESNodes, out Y);
            AddTwoDimensionalDecisionVariable("U", U_LB, U_UB, NumVarType.Int, numNonESNodes, numVehCategories, out U);

            //auxiliaries (T_j, Delta_j, Epsilon_j)
            AddOneDimensionalDecisionVariable("T", minValue_T, maxValue_T, NumVarType.Float, numNonESNodes, out T);
            AddOneDimensionalDecisionVariable("Delta", minValue_Delta, maxValue_Delta, NumVarType.Float, numNonESNodes, out Delta);
            AddOneDimensionalDecisionVariable("Epsilon", minValue_Epsilon, maxValue_Epsilon, NumVarType.Float, numNonESNodes, out Epsilon);

            //All variables defined
            allVariables_array = allVariables_list.ToArray();
            //Now we need to set some to the variables to 0
            SetUndesiredXYVariablesTo0();
        }
        void OrganizeSites()
        {
            Site[] originalSites = theProblemModel.SRD.GetAllSitesArray();
            numCustomers = theProblemModel.SRD.NumCustomers;
            numES = theProblemModel.SRD.NumES;
            numNonESNodes = numCustomers + 1;
            preprocessedSites = new Site[numNonESNodes];
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
                        externalStations.Add(s);
                        break;
                    case SiteTypes.Customer:
                        preprocessedSites[nodeCounter++] = s;
                        customers.Add(s);
                        break;
                    default:
                        throw new System.Exception("Site type incompatible!");
                }
            }
            theDepot = depots[0];
        }
        
        void SetUndesiredXYVariablesTo0()
        {
            //No arc from a node to itself
            for (int j = 0; j < numNonESNodes; j++) {
                for (int v = 0; v < numVehCategories; v++)
                    X[j][j][v].UB = 0.0;

                for (int r = 0; r < numES; r++)
                    Y[j][r][j].UB = 0.0;
            }
            //No arc from depot to its duplicate
            for (int j = 0; j < numNonESNodes; j++)
                for (int r = 0; r < numES; r++)
                    if ((externalStations[r].X == theDepot.X) && (externalStations[r].Y == theDepot.Y))//Comparing X and Y coordinates to those of the depot makes sures that the ES at hand corresponds to the one at the depot!
                    {
                        Y[0][r][j].UB = 0.0;
                        Y[j][r][0].UB = 0.0;
                    }
            //No arc from a node to another if energy consumption is > 1
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        if (EnergyConsumption(sFrom, sTo, base.vehicleCategories[v]) > 1)
                            X[i][j][v].UB = 0.0;
                }
            }

        }
        void SetMinAndMaxValuesOfAllVariables()
        {
            X_LB = new double[numNonESNodes][][];
            X_UB = new double[numNonESNodes][][];
            Y_LB = new double[numNonESNodes][][];
            Y_UB = new double[numNonESNodes][][];
            U_LB = new double[numNonESNodes][];
            U_UB = new double[numNonESNodes][];
            minValue_T = new double[numNonESNodes];
            maxValue_T = new double[numNonESNodes];
            minValue_Delta = new double[numNonESNodes];
            maxValue_Delta = new double[numNonESNodes];
            minValue_Epsilon = new double[numNonESNodes];
            maxValue_Epsilon = new double[numNonESNodes];

            for (int i = 0; i < numNonESNodes; i++)
            {
                Y_LB[i] = new double[numVehCategories][];
                Y_UB[i] = new double[numVehCategories][];
                for (int r = 0; r < numVehCategories; r++)
                {
                    Y_LB[i][r] = new double[numNonESNodes];
                    Y_UB[i][r] = new double[numNonESNodes];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Y_LB[i][r][j] = 0.0;
                        Y_UB[i][r][j] = 1.0;
                    }
                }
            }

            for (int i = 0; i < numNonESNodes; i++)
            {
                X_LB[i] = new double[numNonESNodes][];
                X_UB[i] = new double[numNonESNodes][];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site s = preprocessedSites[j];
                    X_LB[i][j] = new double[numVehCategories];
                    X_UB[i][j] = new double[numVehCategories];
                    U_LB[j] = new double[numVehCategories];
                    U_UB[j] = new double[numVehCategories];
                    for(int v = 0; v < numVehCategories; v++)
                    {
                        X_LB[i][j][v] = 0.0;
                        X_UB[i][j][v] = 1.0;
                        if (s.SiteType == SiteTypes.Depot)
                            U_UB[j][v] = theProblemModel.GetNumVehicles(base.vehicleCategories[v]);
                        else
                            U_UB[j][v] = 1.0;
                        U_LB[j][v] = 0.0;
                    }

                    minValue_T[j] = TravelTime(theDepot, s);
                    maxValue_T[j] = theProblemModel.CRD.TMax - TravelTime(s, theDepot);
                    if (s.SiteType == SiteTypes.Customer)
                        maxValue_T[j] -= ServiceDuration(s);

                    //TODO Fine-tune the min and max values of delta
                    minValue_Delta[j] = 0.0;
                    maxValue_Delta[j] = 1.0;

                    minValue_Epsilon[j] = 0.0;

                    if (s.SiteType == SiteTypes.Customer)//TODO: Unit test the following utility function. It should give us MaxSOCGainAtSite s with EV.
                        maxValue_Epsilon[j] = Utils.Calculators.MaxSOCGainAtSite(s, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV), maxStayDuration: s.ServiceDuration); //Math.Min(1.0, ServiceDuration(j) * Math.Min(RechargingRate(j), theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate) / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity);
                    else
                        maxValue_Epsilon[j] = 1.0;
                }
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
            if (theProblemModel.ObjectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                AddMaxTypeObjectiveFunction();
            else
                AddMinTypeObjectiveFunction();
        }
        void AddMaxTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //First term: prize collection
            for(int j=0; j<numNonESNodes;j++)
            {
                Site s = preprocessedSites[j];
                if(s.SiteType==SiteTypes.Customer)
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(Prize(s, base.vehicleCategories[v]), U[j][v]);
            }
            //Second term Part I: distance-based costs from customer to customer directly
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(base.vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Second term Part II: distance-based costs from customer to customer through an ES
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int r = 0; r < numES; r++)
                {
                    Site ES = externalStations[r];
                    for (int j = 0; j < numVehCategories; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        objFunction.AddTerm(-1.0 * GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES)+ Distance(ES, sTo)), Y[i][r][j]);
                    }
                }
            }
            //Third term: vehicle fixed costs
                for (int v = 0; v < numVehCategories; v++)
                    objFunction.AddTerm(GetVehicleFixedCost(base.vehicleCategories[v]), U[0][v]);
            //Now adding the objective function to the model
            AddMaximize(objFunction);
        }
        void AddMinTypeObjectiveFunction()
        {
            ILinearNumExpr objFunction = LinearNumExpr();
            //Second term Part I: distance-based costs from customer to customer directly
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    for (int v = 0; v < numVehCategories; v++)
                        objFunction.AddTerm(GetVarCostPerMile(vehicleCategories[v]) * Distance(sFrom, sTo), X[i][j][v]);
                }
            }
            //Second term Part II: distance-based costs from customer to customer through an ES
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int r = 0; r < numES; r++)
                {
                    Site ES = externalStations[r];
                    for (int j = 0; j < numVehCategories; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        objFunction.AddTerm(GetVarCostPerMile(VehicleCategories.EV) * (Distance(sFrom, ES) + Distance(ES, sTo)), Y[i][r][j]);
                    }
                }
            }
            //Third term: vehicle fixed costs
            for (int v = 0; v < numVehCategories; v++)
                objFunction.AddTerm(GetVehicleFixedCost(vehicleCategories[v]), U[0][v]);
            //Now adding the objective function to the model
            AddMinimize(objFunction);
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
            for (int j = 0; j < numNonESNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr IncomingXandYTotalMinusU = LinearNumExpr();
                    if (base.vehicleCategories[v] == VehicleCategories.EV)
                    {
                        for (int i = 0; i < numNonESNodes; i++)
                        {
                            IncomingXandYTotalMinusU.AddTerm(1.0, X[i][j][v]);
                            for (int r = 0; r < numES; r++)
                                IncomingXandYTotalMinusU.AddTerm(1.0, Y[i][r][j]);
                        }
                        IncomingXandYTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(base.AddEq(IncomingXandYTotalMinusU, 0.0, constraint_name));
                    }
                    
                }
        }
        void AddConstraint_IncomingXTotalEqualsUforGDV() //2
        {
            for (int j = 0; j < numNonESNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr IncomingXTotalMinusU = LinearNumExpr();
                    if (base.vehicleCategories[v] == VehicleCategories.GDV)
                    {
                        for (int i = 0; i < numNonESNodes; i++)
                            IncomingXTotalMinusU.AddTerm(1.0, X[i][j][v]);
                        IncomingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_incoming_to_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(base.AddEq(IncomingXTotalMinusU, 0.0, constraint_name));
                    }
                    
                }
        }
        void AddConstraint_OutgoingXandYTotalEqualsUforEV() //3
        {
            for (int j = 0; j < numNonESNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr OutgoingXandYTotalMinusU = LinearNumExpr();
                    if (base.vehicleCategories[v] == VehicleCategories.EV)
                    {
                        for (int k = 0; k < numNonESNodes; k++)
                        {
                            OutgoingXandYTotalMinusU.AddTerm(1.0, X[j][k][v]);
                            for (int r = 0; r < numES; r++)
                                OutgoingXandYTotalMinusU.AddTerm(1.0, Y[j][r][k]);
                        }
                        OutgoingXandYTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(base.AddEq(OutgoingXandYTotalMinusU, 0.0, constraint_name));
                    }
                }
        }
        void AddConstraint_OutgoingXTotalEqualsUforGDV() //4
        {
            for (int j = 0; j < numNonESNodes; j++)
                for (int v = 0; v < numVehCategories; v++)
                {
                    ILinearNumExpr OutgoingXTotalMinusU = LinearNumExpr();
                    if (base.vehicleCategories[v] == VehicleCategories.GDV)
                    {
                        for (int k = 0; k <= numCustomers; k++)
                            OutgoingXTotalMinusU.AddTerm(1.0, X[j][k][v]);
                        OutgoingXTotalMinusU.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Number_of_type_" + v.ToString() + "_vehicles_outgoing_from_node_" + j.ToString() + "_equals_the_U_variable";
                        allConstraints_list.Add(base.AddEq(OutgoingXTotalMinusU, 0.0, constraint_name));
                    }
                }
        }
        void AddConstraint_AtMostOneVisitPerNode() //5
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr NumberOfVehiclesVisitingTheNode = LinearNumExpr();
                for (int v = 0; v < numVehCategories; v++)
                    NumberOfVehiclesVisitingTheNode.AddTerm(1.0, U[j][v]);
                string constraint_name = "At_most_one_vehicle_can_visit_node_" + j.ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesVisitingTheNode, 1.0, constraint_name));
            }
        }
        void AddConstraint_MaxNumberOfVehiclesPerCategory()//6
        {
            for (int v = 0; v < numVehCategories; v++)
            {
                ILinearNumExpr NumberOfVehiclesPerCategoryOutgoingFromTheDepot = LinearNumExpr();
                NumberOfVehiclesPerCategoryOutgoingFromTheDepot.AddTerm(1.0, U[0][v]);
                string constraint_name = "Number_of_Vehicles_of_category_" + v.ToString() + "_outgoing_from_and_returning_to_node_0_cannot_exceed_" + theProblemModel.NumVehicles[v].ToString();
                allConstraints_list.Add(AddLe(NumberOfVehiclesPerCategoryOutgoingFromTheDepot, theProblemModel.NumVehicles[v], constraint_name));
            }
        }
        void AddConstraint_TimeRegulationForDirectArcFromACustomer()//7
        {
            for (int i = 1; i <= numCustomers; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j <= numCustomers; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0, T[i]);
                    for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom,sTo) + (maxValue_T[i] - minValue_T[j])), X[i][j][v]);
                    string constraint_name = "Time_Regulation_for_Direct_Arc_From_Customer_node_" + i.ToString() + "_to_node_" + j.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                }
            }
        }
        void AddConstraint_TimeRegulationForDirectArcFromDepot()//7
        {
            for (int i = 0; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                if (sFrom.SiteType == SiteTypes.Depot)
                    for (int j = 0; j <= numCustomers; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        for (int v = 0; v < numVehCategories; v++)
                            TimeDifference.AddTerm(-1.0 * TravelTime(sFrom,sTo), X[i][j][v]);
                        string constraint_name = "Time_Regulation_for_Direct_Arc_from_depot_to_node_" + j.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
                    }
            }
        }
        void AddConstraint_TimeRegulationForCustomerAfterDepotThroughAnES()//8a
        {
            for (int r = 0; r < numES; r++)
            {
                Site ES = externalStations[r];
                for (int j = 0; j < numNonESNodes; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr TimeDifference = LinearNumExpr();
                    TimeDifference.AddTerm(1.0, T[j]);
                    TimeDifference.AddTerm(-1.0 * (TravelTime(theDepot,ES) + TravelTime(ES,sTo)), Y[0][r][j]);
                    string constraint_name = "Time_Regulation_from_Depot_to_Customer_node_" + j.ToString() + "_through_ES_node_" + r.ToString();
                    allConstraints_list.Add(AddGe(TimeDifference, 0.0, constraint_name));
                }
            }
        }
        void AddConstraint_TimeRegulationForCustomerThroughAnES()//8b
        {
            for (int i = 1; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int r = 0; r < numES; r++)
                {
                    Site ES = externalStations[r];
                    for (int j = 0; j < numNonESNodes; j++)
                    {
                        Site sTo = preprocessedSites[j];
                        ILinearNumExpr TimeDifference = LinearNumExpr();
                        TimeDifference.AddTerm(1.0, T[j]);
                        TimeDifference.AddTerm(-1.0, T[i]);
                        TimeDifference.AddTerm(-1.0 * (ServiceDuration(sFrom) + TravelTime(sFrom,ES) + TravelTime(ES,sTo) + (maxValue_T[i] - minValue_T[j] + (1.0 + EnergyConsumption(sFrom,ES,VehicleCategories.EV)) / RechargingRate(ES))), Y[i][r][j]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Delta[i]);
                        TimeDifference.AddTerm(1.0 / RechargingRate(ES), Epsilon[i]);
                        string constraint_name = "Time_Regulation_from_Customer_node_" + i.ToString() + "_to_Customer_node_" + j.ToString() + "_through_ES_node_" + r.ToString();
                        allConstraints_list.Add(AddGe(TimeDifference, -1.0 * (maxValue_T[i] - minValue_T[j]), constraint_name));
                    }
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingNondepot()//9a
        {
            for (int i = 1; i < numNonESNodes; i++)
            {
                Site sFrom = preprocessedSites[i];
                for (int j = 0; j <= numCustomers; j++)
                {
                    Site sTo = preprocessedSites[j];
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, Delta[j]);
                    SOCDifference.AddTerm(-1.0, Delta[i]);
                    SOCDifference.AddTerm(-1.0, Epsilon[i]);
                    for (int v = 0; v < numVehCategories; v++)
                        if (vehicleCategories[v] == VehicleCategories.EV)
                            SOCDifference.AddTerm(EnergyConsumption(sFrom,sTo,vehicleCategories[v]) + maxValue_Delta[j] - minValue_Delta[i], X[i][j][v]);
                    string constraint_name = "SOC_Regulation_from_Customer_node_" + i.ToString() + "_to_Customer_node_" + j.ToString();
                    allConstraints_list.Add(AddLe(SOCDifference, maxValue_Delta[j] - minValue_Delta[i], constraint_name));
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingDepot()//9b
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                ILinearNumExpr SOCDifference = LinearNumExpr();
                SOCDifference.AddTerm(1.0, Delta[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (vehicleCategories[v] == VehicleCategories.EV)
                    {
                        SOCDifference.AddTerm(EnergyConsumption(theDepot,sTo,vehicleCategories[v]), X[0][j][v]);
                        string constraint_name = "SOC_Regulation_from_depot_to_customer_" + j.ToString();
                        allConstraints_list.Add(AddLe(SOCDifference, 1.0, constraint_name));
                    }
            }
        }
        void AddConstraint_SOCRegulationToAnES()//10
        {
            for (int j = 0; j <= numCustomers; j++)
            {
                Site sFrom = preprocessedSites[j];
                for (int r = 0; r < numES; r++)
                {
                    Site ES = externalStations[r];
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, Delta[j]);
                    SOCDifference.AddTerm(1.0, Epsilon[j]);
                    for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                        if (vehicleCategories[v] == VehicleCategories.EV)
                        {
                            for (int k = 0; k <= numCustomers; k++)
                                SOCDifference.AddTerm(-1.0 * EnergyConsumption(sFrom,ES,vehicleCategories[v]), Y[j][r][k]);
                            string constraint_name = "SOC_Regulation_from_Customer_node_" + j.ToString() + "_to_ES_node_" + r.ToString();
                            allConstraints_list.Add(AddGe(SOCDifference, 0.0, constraint_name));
                        }
                }
            }
        }
        void AddConstraint_SOCRegulationFollowingAnES()//11
        {
            for (int j = 0; j < numNonESNodes; j++)
            {
                Site sTo = preprocessedSites[j];
                for (int r = 0; r < numES; r++)
                {
                    Site ES = externalStations[r];
                    ILinearNumExpr SOCDifference = LinearNumExpr();
                    SOCDifference.AddTerm(1.0, Delta[j]);
                    for (int v = 0; v < numVehCategories; v++)
                        for (int i = 0; i < numNonESNodes; i++)
                        {
                            SOCDifference.AddTerm(EnergyConsumption(ES, sTo, base.vehicleCategories[v]), Y[i][r][j]);
                            string constraint_name = "SOC_Regulation_from_ES_node_" + r.ToString() + "_to_Customer_node_" + j.ToString();
                            allConstraints_list.Add(AddLe(SOCDifference, 1.0, constraint_name));
                        }
                }
            }
        }
        void AddConstraint_MaxRechargeAtCustomerNode()//13
        {
            for (int j = 1; j < numNonESNodes; j++)
            {
                ILinearNumExpr RechargeAtCustomer = LinearNumExpr();
                RechargeAtCustomer.AddTerm(1.0, Epsilon[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (base.vehicleCategories[v] == VehicleCategories.EV)
                    {
                        RechargeAtCustomer.AddTerm(-1.0 * maxValue_Epsilon[j], U[j][v]);
                        string constraint_name = "Max_Recharge_At_Customer_" + j.ToString();
                        allConstraints_list.Add(base.AddLe(RechargeAtCustomer, 0.0, constraint_name));
                    }
            }
        }
        void AddConstraint_MaxDepartureSOCFromCustomerNode()//14

        {
            for (int j = 1; j <= numCustomers; j++)
            {
                ILinearNumExpr DepartureSOCFromCustomer = LinearNumExpr();
                DepartureSOCFromCustomer.AddTerm(1.0, Delta[j]);
                DepartureSOCFromCustomer.AddTerm(1.0, Epsilon[j]);
                for (int v = 0; v < numVehCategories; v++)
                    if (base.vehicleCategories[v] == VehicleCategories.EV)
                    {
                        DepartureSOCFromCustomer.AddTerm(-1.0, U[j][v]);
                        string constraint_name = "Departure_SOC_From_Customer_" + j.ToString();
                        allConstraints_list.Add(base.AddLe(DepartureSOCFromCustomer, 0.0, constraint_name));
                    }
            }
        }

        public List<VehicleSpecificRoute> GetVehicleSpecificRoutes()
        {
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
                outcome.Add(new VehicleSpecificRoute(theProblemModel, theVehicle, nonDepotSiteIDs));
            return outcome;
        }
        List<List<string>> GetListsOfNonDepotSiteIDs(VehicleCategories vehicleCategory)
        {
            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;

            //We first determine the route start points
            List<List<int>> listOfFirstSiteIndices = new List<List<int>>();
            for (int j = 0; j < numNonESNodes; j++)
                if (GetValue(X[0][j][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    listOfFirstSiteIndices.Add(new List<int>() { j });
                }
            for(int r=0;r<numES;r++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (GetValue(Y[0][r][j]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
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

            if(firstSiteIndices.Count==1)
                if (GetValue(X[0][firstSiteIndices.Last()][vc_int]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to an X-flow from the depot!");
                }
            if (firstSiteIndices.Count == 2)
                if (GetValue(Y[0][firstSiteIndices.First()][firstSiteIndices.Last()]) < 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    throw new System.Exception("XCPlex_ArcDuplicatingFormulation.GetNonDepotSiteIDs called for a (firstSiteIndices,vehicleCategory) pair that doesn't correspond to a Y-flow from the depot!");
                }

            List<int> currentSiteIndices = firstSiteIndices;
            List<int> nextSiteIndices;

            do
            {
                if(currentSiteIndices.Count==2)
                    outcome.Add(externalStations[currentSiteIndices.First()].ID);
                outcome.Add(preprocessedSites[currentSiteIndices.Last()].ID);

                nextSiteIndices = GetNextSiteIndices(currentSiteIndices.Last(), vehicleCategory);
                if(preprocessedSites[nextSiteIndices.Last()].ID == theDepot.ID)
                {
                    if(nextSiteIndices.Count==2)
                        outcome.Add(externalStations[nextSiteIndices.First()].ID);
                    return outcome;
                }
                currentSiteIndices = nextSiteIndices;
            }
            while (preprocessedSites[currentSiteIndices.Last()].ID != theDepot.ID);

            return outcome;
        }
        List<int> GetNextSiteIndices(int currentSiteIndex, VehicleCategories vehicleCategory)
        {
            int vc_int = (vehicleCategory == VehicleCategories.EV) ? 0 : 1;
            for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
                if (GetValue(X[currentSiteIndex][nextCustomerIndex][vc_int]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                {
                    return new List<int>() { nextCustomerIndex };
                }
            if (vehicleCategory == VehicleCategories.EV)
                for (int nextESIndex = 0; nextESIndex < numES; nextESIndex++)
                    for (int nextCustomerIndex = 0; nextCustomerIndex < numNonESNodes; nextCustomerIndex++)
                        if (GetValue(Y[currentSiteIndex][nextESIndex][nextCustomerIndex]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                            return new List<int>() { nextESIndex, nextCustomerIndex };
            throw new System.Exception("Flow ended before returning to the depot!");
        }

        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            return new RouteBasedSolution(GetVehicleSpecificRoutes());
        }
    }
}
