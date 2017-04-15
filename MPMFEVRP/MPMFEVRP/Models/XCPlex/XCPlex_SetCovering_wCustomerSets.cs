using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Interfaces;
using ILOG.Concert;
using ILOG.CPLEX;

namespace MPMFEVRP.Models.XCPlex
{
    class XCPlex_SetCovering_wCustomerSets : XCPlexBase
    {
        INumVar[][] y;//[customerSet][2: 0 of EV, 1 for GDV]
        ILinearNumExpr obj;//Because of the enumetative and space consuming nature of the problem with 2 vehicle categories, it's more practical to create the obj expression beforehand and only add it later

        public XCPlex_SetCovering_wCustomerSets(ProblemModelBase problemModel, XCPlexParameters xCplexParam) : base(problemModel, xCplexParam)
        {
        }


        public override NewCompleteSolution GetCompleteSolution()
        {
            throw new NotImplementedException();
        }

        public override string GetDescription_AllVariables_Array()
        {
            return
                "for (int i = 0; i < nCustomerSets; i++)\nfor (int v = 0; v < 2; v++)\ny[i][v]";
        }

        protected override void AddAllConstraints()
        {
            allConstraints_list = new List<IRange>();
            //Now adding the constraints one (family) at a time
            addCustomerCoverageConstraints();
            //All constraints added
            allConstraints_array = allConstraints_list.ToArray();
        }
        void addCustomerCoverageConstraints()
        {
            int nCustomerSets = problemModel.CustomerSetArchive.Count;
            List<string> customerIDs = problemModel.SRD.GetCustomerIDs();
            int nCustomers = customerIDs.Count;
            CustomerCoverageConstraint_EachCustomerMustBeCovered coverConstraintType = CustomerCoverageConstraint_EachCustomerMustBeCovered.AtMostOnce;//TODO Make this parametric and pass it through since wherever it may originate

            foreach (string customerID in customerIDs)
            {
                ILinearNumExpr numTimesCustomerServed = LinearNumExpr();
                for (int i = 0; i < nCustomerSets; i++)
                {
                    if (problemModel.CustomerSetArchive[i].Customers.Contains(customerID))
                    {
                        numTimesCustomerServed.AddTerm(1.0, y[i][0]);
                        numTimesCustomerServed.AddTerm(1.0, y[i][1]);
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

            int nCustomerSets = problemModel.CustomerSetArchive.Count;
            string[][] y_name = new string[nCustomerSets][];
            y = new INumVar[nCustomerSets][];
            for (int i = 0; i < nCustomerSets; i++)
            {
                y_name[i] = new string[2];
                y[i] = new INumVar[2];
                for (int v = 0; v < 2; v++)
                {
                    y_name[i][v] = "y_(" + i.ToString() + "," + v.ToString() + ")";
                    y[i][v] = NumVar(0, 1, variable_type, y_name[i][v]);
                    obj.AddTerm(problemModel.CustomerSetArchive[i].RouteOptimizerOutcome.OFV[v], y[i][v]);
                    allVariables_list.Add(y[i][v]);
                }
            }
            //All variables defined
            allVariables_array = allVariables_list.ToArray();
        }
    }
}
