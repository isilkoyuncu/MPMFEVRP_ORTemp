using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.MFGVRPCGH
{
    public class ExperimentationParameters
    {
        int numberOfEVs;                        public int NumberOfEVs { get { return numberOfEVs; } }
        int numberOfGDVs;                       public int NumberOfGDVs { get { return numberOfGDVs; } }
        bool exploitGDVs;                       public bool ExploitGDVs { get { return exploitGDVs; } }
        ObjectiveFunctions objectiveFunction;   public ObjectiveFunctions ObjectiveFunction { get { return objectiveFunction; } }

        CustomerCoverageConstraint_EachCustomerMustBeCovered customerCoverageConstraint; public CustomerCoverageConstraint_EachCustomerMustBeCovered CustomerCoverageConstraint { get { return customerCoverageConstraint; } }
        public ExperimentationParameters(int numberOfEVs, int numberOfGDVs, ObjectiveFunctions objectiveFunction, bool exploitGDVs = true)
        {
            this.numberOfEVs = numberOfEVs;
            this.numberOfGDVs = numberOfGDVs;
            this.exploitGDVs = exploitGDVs;
            this.objectiveFunction = objectiveFunction;
        }
    }
}
