﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Interfaces;
using ILOG.Concert;
using ILOG.CPLEX;
using MPMFEVRP.Implementations.Solutions;

namespace MPMFEVRP.Models.XCPlex
{
    class XCPlex_Assignment_RecoveryForRandGreedy : XCPlexBase
    {
        INumVar[][] z;//[customerSet][2: 0 of EV, 1 for GDV]
        ILinearNumExpr obj;//Because of the enumerative and space consuming nature of the problem with 2 vehicle categories, it's more practical to create the obj expression beforehand and only add it later

        CustomerSetBasedSolution trialSolution;

        public XCPlex_Assignment_RecoveryForRandGreedy(ProblemModelBase problemModel, XCPlexParameters xCplexParam) : base(problemModel, xCplexParam)
        {
        }
        public void GetCSList2bRecovered(CustomerSetBasedSolution trialSolution)
        {
            this.trialSolution = new CustomerSetBasedSolution(trialSolution);
        }

        public override SolutionBase GetCompleteSolution(Type SolutionType)
        {
            if (SolutionType != typeof(MPMFEVRP.Implementations.Solutions.CustomerSetBasedSolution))
                throw new System.Exception("XCPlex_Assignment_RecoveryForRandGreedy prompted to output the wrong Solution type, it only outputs a solution of the CustomerSetBasedSolution type");

            return new CustomerSetBasedSolution(problemModel, GetZVariablesSetTo1(), trialSolution);
        }

        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i < nCustomerSets; i++)\nfor (int v = 0; v < 2; v++)\nz[i][v]";
        }

        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            AddCustomerCoverageConstraints();
            AddMaxNumberOfEVsConstraint();
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void AddCustomerCoverageConstraints()
        {
            List<CustomerSet> CS_List = trialSolution.CS_List;
            //CustomerCoverageConstraint_EachCustomerMustBeCovered assignmentConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce;//TODO Make this parametric and pass it through since wherever it may originate

            for (int i= 0; i < CS_List.Count; i++)
            {
                ILinearNumExpr numTimesCustomerServed = LinearNumExpr();
                numTimesCustomerServed.AddTerm(1.0, z[i][0]);
                numTimesCustomerServed.AddTerm(1.0, z[i][1]);
                string constraint_name = "Customer Set " + i;
                //switch (assignmentConstraintType)
                //{
                //    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce:
                //        constraint_name += " at most once";
                        allConstraints_list.Add(AddLe(numTimesCustomerServed, 1.0, constraint_name));
                //        break;
                //    case CustomerCoverageConstraint_EachCustomerMustBeCovered.ExactlyOnce:
                //        constraint_name += " exactly once";
                //        allConstraints_list.Add(AddEq(numTimesCustomerServed, 1.0, constraint_name));
                //        break;
                //    case CustomerCoverageConstraint_EachCustomerMustBeCovered.AtLeastOnce:
                //        constraint_name += " at least once";
                //        allConstraints_list.Add(AddGe(numTimesCustomerServed, 1.0, constraint_name));
                //        break;
                //}
            }//foreach CS
        }
        void AddMaxNumberOfEVsConstraint()
        {
            List<CustomerSet> CS_List = trialSolution.CS_List;
            ILinearNumExpr numTimesEVsUsed = LinearNumExpr();
            for (int i = 0; i < CS_List.Count; i++)
            {
                numTimesEVsUsed.AddTerm(1.0, z[i][0]);
            }
            string constraint_name = "EVs Used";
            allConstraints_list.Add(AddLe(numTimesEVsUsed, problemModel.VRD.NumVehicles[0], constraint_name));
        }
        protected override void AddTheObjectiveFunction()
        {
            //created already in DefineDecisionVariables()
            //add (Maximize or Minimize depends on the problem)
            ObjectiveFunctionTypes objectiveFunctionType = Utils.ProblemUtil.CreateProblemByName(problemModel.GetNameOfProblemOfModel()).ObjectiveFunctionType;
            if (objectiveFunctionType == ObjectiveFunctionTypes.Maximize)
                AddMaximize(obj);
            else
                AddMinimize(obj);
        }

        protected override void DefineDecisionVariables()
        {
            allVariables_list = new List<INumVar>();
            obj = LinearNumExpr();

            int nCustomerSets = trialSolution.CS_List.Count;
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
                    obj.AddTerm(trialSolution.CS_List[i].RouteOptimizerOutcome.OFV[v], z[i][v]);
                    allVariables_list.Add(z[i][v]);
                }
            }
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
        }

        public int[,] GetZVariablesSetTo1()
        {
            int[,] outcome = new int[trialSolution.CS_List.Count, problemModel.VRD.NumVehicleCategories];
            for (int cs = 0; cs < trialSolution.CS_List.Count; cs++)
                for (int v = 0; v < problemModel.VRD.NumVehicleCategories; v++)
                    if (GetValue(z[cs][v]) >= 1.0 - ProblemConstants.ERROR_TOLERANCE)
                        outcome[cs,v] = 1;

            return outcome;
        }

    }
}