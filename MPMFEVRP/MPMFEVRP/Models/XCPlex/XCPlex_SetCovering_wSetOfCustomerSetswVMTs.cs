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
    public class XCPlex_SetCovering_wSetOfCustomerSetswVMTs : XCPlexBase
    {
        int vIndex_EV = 0;//TODO: Use this in all places
        INumVar[][] z;//[customerSet][2: 0 of EV, 1 for GDV]
        ILinearNumExpr obj;//Because of the enumerative and space consuming nature of the problem with 2 vehicle categories, it's more practical to create the obj expression beforehand and only add it later

        RandomSubsetOfCustomerSetsWithVMTs setOfCustomerSets;
        int nCustomerSets;
        CustomerSetWithVMTs[] customerSetsWithVMTs_array;
        //CustomerSet[] customerSetArray;
        int[] overrideNumberOfVehicles;
        //public XCPlex_SetCovering_wCustomerSets(ProblemModelBase problemModel, XCPlexParameters xCplexParam): base(problemModel, xCplexParam){}
        public XCPlex_SetCovering_wSetOfCustomerSetswVMTs() { }
        public XCPlex_SetCovering_wSetOfCustomerSetswVMTs(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam, RandomSubsetOfCustomerSetsWithVMTs setOfCustomerSets, bool noGDVUnlimitedEV = false, bool unlimitedGDVAndEV = false)
        {
            //implementation:
            this.theProblemModel = theProblemModel;
            this.xCplexParam = xCplexParam;
            XCPlexRelaxation relaxation;
            relaxation = xCplexParam.Relaxation;
            if ((xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                //||(xCplexParam.Relaxation == XCPlexRelaxation.AssignmentProblem)
                )
                variable_type = NumVarType.Float;
            this.setOfCustomerSets = setOfCustomerSets ?? throw new System.Exception("XCPlex_SetCovering_wSetOfCustomerSetswVMTs invoked with a null setOfCustomerSets!");

            nCustomerSets = setOfCustomerSets.RandomlySelectedCustomerSets.Count;
            customerSetsWithVMTs_array = new CustomerSetWithVMTs[nCustomerSets];
            for (int i = 0; i < nCustomerSets; i++)
                customerSetsWithVMTs_array[i] = setOfCustomerSets.RandomlySelectedCustomerSets[i];

            overrideNumberOfVehicles = new int[2];
            if (noGDVUnlimitedEV)
            {
                overrideNumberOfVehicles[vIndex_EV] = theProblemModel.SRD.NumCustomers;
                overrideNumberOfVehicles[1 - vIndex_EV] = 0;
            }
            if (unlimitedGDVAndEV)
            {
                overrideNumberOfVehicles[vIndex_EV] = theProblemModel.SRD.NumCustomers;
                overrideNumberOfVehicles[1 - vIndex_EV] = theProblemModel.SRD.NumCustomers;
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
                    obj.AddTerm(theProblemModel.VRD.GetTheVehicleOfCategory(vc[v]).VariableCostPerMile * customerSetsWithVMTs_array[i].GetVMT(vc[v]), z[i][v]); 
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
                    if (customerSetsWithVMTs_array[i].CustomerSet.Customers.Contains(customerID))
                    {
                        switch (customerSetsWithVMTs_array[i].Status)
                        {
                            case RouteOptimizationStatus.OptimizedForBothGDVandEV:
                                numTimesCustomerServed.AddTerm(1.0, z[i][0]);
                                numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                                break;
                            case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV:
                            case RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV:
                                numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                                break;
                            default:
                                break;
                        }
                    }
                }//for i
                string constraint_name = "Customer_" + customerID + "_must_be_covered_";
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
                for (int i = 0; i < nCustomerSets; i++)
                {
                    numTimesVehicleTypeIsUsed.AddTerm(1.0, z[i][v]);
                }//for i
                int nv = overrideNumberOfVehicles == null ? theProblemModel.NumVehicles[v] : overrideNumberOfVehicles[v];
                string constraint_name = "Vehicle type " + v.ToString() + " can be used at most" + nv.ToString() + "times";
                allConstraints_list.Add(AddLe(numTimesVehicleTypeIsUsed, nv, constraint_name));
            }
        }
        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            if (SolutionType != typeof(CustomerSetBasedSolution))
                throw new System.Exception("XCPlex_SetCovering_wCustomerSets prompted to output the wrong Solution type, it only outputs a solution of the CustomerSetBasedSolution type");

            if ((solutionStatus == XCPlexSolutionStatus.Feasible) || (solutionStatus == XCPlexSolutionStatus.Optimal))
                return new CustomerSetBasedSolution(theProblemModel, GetZVariablesSetTo1(), ExtractCustomerSetArrayFromSameWithVMTs(customerSetsWithVMTs_array));
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


        public Dictionary<string, double> GetCustomerCoverageConstraintShadowPrices()
        {
            Dictionary<string, double> outcome = new Dictionary<string, double>();
            for (int c = 0; c < allConstraints_array.Length; c++)
            {
                string constraintName = allConstraints_array[c].Name;
                if (constraintName.Contains("Customer") && constraintName.Contains("must_be_covered"))
                {
                    outcome.Add(constraintName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[1], AllShadowPrices[c]);
                }
            }
            return outcome;
        }

        CustomerSet[] ExtractCustomerSetArrayFromSameWithVMTs(CustomerSetWithVMTs[] CSwVMTs)
        {
            CustomerSet[] output = new CustomerSet[CSwVMTs.Length];
            for (int i = 0; i < CSwVMTs.Length; i++)
                output[i] = CSwVMTs[i].CustomerSet;
            return output;
        }

        protected override void SpecializedInitialize()
        {
            throw new NotImplementedException();
        }
    }
}
