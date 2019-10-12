using ILOG.Concert;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;


namespace MPMFEVRP.Models.XCPlex
{
    class XCPlex_SetCovering_wCustomerSets : XCPlexBase
    {
        int vIndex_EV = 0;//TODO: Use this in all places
        INumVar[][] z;//[customerSet][2: 0 of EV, 1 for GDV]
        ILinearNumExpr obj;//Because of the enumerative and space consuming nature of the problem with 2 vehicle categories, it's more practical to create the obj expression beforehand and only add it later
        int nFeasibleCustomerSets = 0;
        CustomerSet[] customerSetArray;
        int[] overrideNumberOfVehicles;
        //public XCPlex_SetCovering_wCustomerSets(ProblemModelBase problemModel, XCPlexParameters xCplexParam): base(problemModel, xCplexParam){}
        public XCPlex_SetCovering_wCustomerSets() { }
        public XCPlex_SetCovering_wCustomerSets(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerSetList cs_List = null, bool noGDVUnlimitedEV = false)
        {
            this.theProblemModel = theProblemModel;
            this.xCplexParam = xCplexParam;
            //XCPlexRelaxation relaxation;
            //relaxation = xCplexParam.Relaxation;
            if ((xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                //||(xCplexParam.Relaxation == XCPlexRelaxation.AssignmentProblem)
                )
                variable_type = NumVarType.Float;
            if (cs_List != null)
            {
                for (int j = 0; j < cs_List.Count; j++)
                    if (cs_List[j].RouteOptimizationOutcome.IsFeasible(VehicleCategories.GDV))
                        nFeasibleCustomerSets = nFeasibleCustomerSets + 1;

                customerSetArray = new CustomerSet[nFeasibleCustomerSets];
                int count = 0;
                for (int j = 0; j < cs_List.Count; j++)
                    if (cs_List[j].RouteOptimizationOutcome.IsFeasible(VehicleCategories.GDV))
                        customerSetArray[count++] = cs_List[j];
            }
            else
            {
                nFeasibleCustomerSets = theProblemModel.CustomerSetArchive.Count;
                customerSetArray = new CustomerSet[nFeasibleCustomerSets];
                for (int i = 0; i < nFeasibleCustomerSets; i++)
                {
                    customerSetArray[i] = theProblemModel.CustomerSetArchive[i];
                }
            }
            overrideNumberOfVehicles = new int[2] { theProblemModel.NumVehicles[vIndex_EV], theProblemModel.NumVehicles[1- vIndex_EV] };
            if (noGDVUnlimitedEV)
            {
                overrideNumberOfVehicles[vIndex_EV] = theProblemModel.SRD.NumCustomers;
                overrideNumberOfVehicles[1 - vIndex_EV] = 0;
            }

            //now we are ready to put the model together and then solve it
            //Define the variables
            DefineDecisionVariables();
            //Objective function
            AddTheObjectiveFunction();
            //Constraints
            AddAllConstraints();
            //Cplex parameters
            SetCplexParameters();
            //output variables
            InitializeOutputVariables();
        }

        protected override void DefineDecisionVariables()
        {
            allVariables_list = new List<INumVar>();
            obj = LinearNumExpr();
            VehicleCategories[] vc = new VehicleCategories[] { VehicleCategories.EV, VehicleCategories.GDV };

            string[][] z_name = new string[nFeasibleCustomerSets][];
            z = new INumVar[nFeasibleCustomerSets][];
            for (int i = 0; i < nFeasibleCustomerSets; i++)
            {
                z_name[i] = new string[2];
                z[i] = new INumVar[2];
                for (int v = 0; v < 2; v++)
                {
                        z_name[i][v] = "z_(" + i.ToString() + "," + v.ToString() + ")";
                        z[i][v] = NumVar(0, 1, variable_type, z_name[i][v]);
                        obj.AddTerm(theProblemModel.CalculateObjectiveFunctionValue(customerSetArray[i].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vc[v]).GetObjectiveFunctionInputDataPackage()), z[i][v]);
                        allVariables_list.Add(z[i][v]);
                }
            }
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
        }
        protected override void AddTheObjectiveFunction()
        {
            //created already in DefineDecisionVariables()
            ObjectiveFunctionTypes objectiveFunctionType = theProblemModel.ObjectiveFunctionType;
            if (objectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                AddMaximize(obj);
            else
                AddMinimize(obj);
        }
        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddCustomerCoverageConstraints();
            AddUseLimitedNumberOfVehiclesConstraints();
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddCustomerCoverageConstraints()
        {
            List<string> customerIDs = theProblemModel.SRD.GetCustomerIDs();
            int nCustomers = customerIDs.Count;
            CustomerCoverageConstraint_EachCustomerMustBeCovered coverConstraintType = theProblemModel.CoverConstraintType;

            foreach (string customerID in customerIDs)
            {
                ILinearNumExpr numTimesCustomerServed = LinearNumExpr();
                for (int i = 0; i < nFeasibleCustomerSets; i++)
                {
                    if (customerSetArray[i].RouteOptimizationOutcome.Status==RouteOptimizationStatus.OptimizedForBothGDVandEV)
                        if (customerSetArray[i].RouteOptimizationOutcome.TheListofVSROOs[1].VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs.Contains(customerID))
                        {
                            numTimesCustomerServed.AddTerm(1.0, z[i][0]);
                            numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                        }
                        else { }
                    else if (customerSetArray[i].RouteOptimizationOutcome.Status==RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV || customerSetArray[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV)
                    {
                        if (customerSetArray[i].RouteOptimizationOutcome.TheListofVSROOs[0].VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs.Contains(customerID))
                            numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                    }

                }//for i
                string constraint_name = "Customer_" + customerID +"_must_be_covered_";
                switch (coverConstraintType)
                {
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
                        constraint_name += "at_most_once";
                        allConstraints_list.Add(AddLe(numTimesCustomerServed, 1.0, constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
                        constraint_name += "exactly_once";
                        allConstraints_list.Add(AddEq(numTimesCustomerServed, 1.0, constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
                        constraint_name += "at_least_once";
                        allConstraints_list.Add(AddGe(numTimesCustomerServed, 1.0, constraint_name));
                        break;
                }
            }//foreach customerID
        }
        void AddUseLimitedNumberOfVehiclesConstraints()
        {
            //TODO:This constraint was defined for vehicle type 0 only, assuming unlimited GDVs in a mixed-fleet, we better constrain both numbers of vehicles to what's specified in the problem. The code was revised to make set partition work for EMH for now but will have to be revisited
            for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
            {
                ILinearNumExpr numTimesVehicleTypeIsUsed = LinearNumExpr();
                for (int i = 0; i < nFeasibleCustomerSets; i++)
                {
                    numTimesVehicleTypeIsUsed.AddTerm(1.0, z[i][v]);
                }//for i
                //int nv = overrideNumberOfVehicles == null ? theProblemModel.NumVehicles[v] : overrideNumberOfVehicles[v];
                string constraint_name = "Vehicle type "+v.ToString()+" can be used at most" + theProblemModel.NumVehicles[v].ToString() + "times";
                allConstraints_list.Add(AddLe(numTimesVehicleTypeIsUsed, overrideNumberOfVehicles[v], constraint_name));
            }
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        { 
            if (SolutionType != typeof(CustomerSetBasedSolution))
                throw new System.Exception("XCPlex_SetCovering_wCustomerSets prompted to output the wrong Solution type, it only outputs a solution of the CustomerSetBasedSolution type");

            if ((solutionStatus == XCPlexSolutionStatus.Feasible) || (solutionStatus == XCPlexSolutionStatus.Optimal))
                return new CustomerSetBasedSolution(theProblemModel, GetZVariablesSetTo1(), customerSetArray);
            else
            {
                CustomerSetBasedSolution csbs = new CustomerSetBasedSolution(theProblemModel);
                csbs.UpdateUpperLowerBoundsAndStatusForInfeasible();
                return csbs;
            }
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i < nCustomerSets; i++)\nfor (int v = 0; v < 2; v++)\ny[i][v]";
        }
        public int[,] GetZVariablesSetTo1()
        {
            int[,] outcome = new int[nFeasibleCustomerSets, theProblemModel.VRD.NumVehicleCategories];
            for (int cs = 0; cs < nFeasibleCustomerSets; cs++)
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                    if (GetValue(z[cs][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                        outcome[cs, v] = 1;

            return outcome;
        }
        public double [,] GetObjValuesForZSetTo1()
        {
            VehicleCategories[] vc = new VehicleCategories[] { VehicleCategories.EV, VehicleCategories.GDV };
            double[,] outcome = new double[nFeasibleCustomerSets, theProblemModel.VRD.NumVehicleCategories];
            for (int cs = 0; cs < nFeasibleCustomerSets; cs++)
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                    if (GetValue(z[cs][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                        outcome[cs, v] = theProblemModel.CalculateObjectiveFunctionValue(customerSetArray[cs].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vc[v]).GetObjectiveFunctionInputDataPackage());
            return outcome;
        }
        public override string GetModelName()
        {
            return "Set Covering";
        }

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }


        public Dictionary<string, double> GetCustomerCoverageConstraintShadowPrices()
        {
            Dictionary<string, double> outcome = new Dictionary<string, double>();
            for(int c=0;c< allConstraints_array.Length;c++)
            {
                string constraintName = allConstraints_array[c].Name;
                if (constraintName.Contains("Customer") && constraintName.Contains("must_be_covered"))
                {
                    outcome.Add(constraintName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[1], AllShadowPrices[c]);
                }
            }
            return outcome;
        }

    }
}