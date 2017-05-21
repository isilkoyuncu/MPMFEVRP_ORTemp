using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class AColumnGenerationHeuristic : AlgorithmBase
    {
        ObjectiveFunctionTypes ofvType;
        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents, children;


        public override string GetName()
        {
            return "Customer Set-based Column Generation Heuristic";
        }

        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            ofvType = Utils.ProblemUtil.CreateProblemByName(model.GetNameOfProblemOfModel()).ObjectiveFunctionType;

            unexploredCustomerSets = new PartitionedCustomerSetList();
            int nCustomers = model.SRD.NumCustomers;
            foreach(Site s in model.SRD.SiteArray)
            {
                if(s.SiteType == SiteTypes.Customer)
                {
                    CustomerSet candidate = new CustomerSet(s.ID, model);
                    unexploredCustomerSets.ConsiderForAddition(candidate);
                }
            }
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedRun()
        {
            throw new NotImplementedException();
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }
    }
}
