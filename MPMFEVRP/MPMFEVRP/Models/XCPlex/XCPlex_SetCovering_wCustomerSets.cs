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
        INumVar[][] z;//[customerSet][2: 0 of EV, 1 for GDV]
        ILinearNumExpr obj;//Because of the enumerative and space consuming nature of the problem with 2 vehicle categories, it's more practical to create the obj expression beforehand and only add it later
        int nCustomerSets;
        CustomerSet[] customerSetArray;
        //public XCPlex_SetCovering_wCustomerSets(ProblemModelBase problemModel, XCPlexParameters xCplexParam): base(problemModel, xCplexParam){}
        public XCPlex_SetCovering_wCustomerSets() { }
        public XCPlex_SetCovering_wCustomerSets(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, CustomerSetList cs_List = null)
        {
            this.theProblemModel = theProblemModel;
            this.xCplexParam = xCplexParam;
            XCPlexRelaxation relaxation;
            relaxation = xCplexParam.Relaxation;
            if ((xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                //||(xCplexParam.Relaxation == XCPlexRelaxation.AssignmentProblem)
                )
                variable_type = NumVarType.Float;
            if (cs_List != null)
            {
                nCustomerSets = cs_List.Count;
                customerSetArray = new CustomerSet[nCustomerSets];
                for (int i = 0; i < nCustomerSets; i++)
                {
                    customerSetArray[i] = cs_List[i];
                }
            }
            else
            {
                nCustomerSets = theProblemModel.CustomerSetArchive.Count;
                customerSetArray = new CustomerSet[nCustomerSets];
                for (int i = 0; i < nCustomerSets; i++)
                {
                    customerSetArray[i] = theProblemModel.CustomerSetArchive[i];
                }
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

            string[][] z_name = new string[nCustomerSets][];
            z = new INumVar[nCustomerSets][];
            for (int i = 0; i < nCustomerSets; i++)
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
                for (int i = 0; i < nCustomerSets; i++)
                {
                    if (customerSetArray[i].Customers.Contains(customerID))
                    {
                        if (customerSetArray[i].RouteOptimizationOutcome.IsFeasible(VehicleCategories.EV))
                        {
                            numTimesCustomerServed.AddTerm(1.0, z[i][0]);
                            numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                        }
                        else if (customerSetArray[i].RouteOptimizationOutcome.IsFeasible(VehicleCategories.GDV))
                        {
                            numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                        }
                    }
                }//for i
                string constraint_name = "Customer " + customerID;
                switch (coverConstraintType)
                {
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
                        constraint_name += " at most once";
                        allConstraints_list.Add(AddLe(numTimesCustomerServed, 1.0, constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
                        constraint_name += " exactly once";
                        allConstraints_list.Add(AddEq(numTimesCustomerServed, 1.0, constraint_name));
                        break;
                    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
                        constraint_name += " at least once";
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
                for (int i = 0; i < nCustomerSets; i++)
                {
                    numTimesVehicleTypeIsUsed.AddTerm(1.0, z[i][v]);
                }//for i
                string constraint_name = "Vehicle type "+v.ToString()+" can be used at most" + theProblemModel.NumVehicles[v] + "times";
                allConstraints_list.Add(AddLe(numTimesVehicleTypeIsUsed, theProblemModel.NumVehicles[v], constraint_name));
            }
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            if (SolutionType != typeof(CustomerSetBasedSolution))
                throw new System.Exception("XCPlex_SetCovering_wCustomerSets prompted to output the wrong Solution type, it only outputs a solution of the CustomerSetBasedSolution type");

            if ((solutionStatus == XCPlexSolutionStatus.Feasible) || (solutionStatus == XCPlexSolutionStatus.Optimal))
                return new CustomerSetBasedSolution(theProblemModel, GetZVariablesSetTo1(), customerSetArray);
            else
                return null;
        }
        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i < nCustomerSets; i++)\nfor (int v = 0; v < 2; v++)\ny[i][v]";
        }
        public int[,] GetZVariablesSetTo1()
        {
            int[,] outcome = new int[nCustomerSets, theProblemModel.VRD.NumVehicleCategories];
            for (int cs = 0; cs < nCustomerSets; cs++)
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
                    if (GetValue(z[cs][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                        outcome[cs, v] = 1;

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
    }
}