using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels;

namespace MPMFEVRP.Implementations
{
    public abstract class ProblemBase : IProblem
    {
        protected ProblemFramework pf;
        public ProblemFramework PF { get { return pf; } }
        
        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        public ProblemBase()
        {
            pf = new ProblemFramework();
        }
        public abstract string GetName();

        public override string ToString()
        {
            throw new NotImplementedException(); //TODO multiple-multiple run ederken anlamli olacak aciklamayi return et
        }

        public string CreateRawData()
        {
            throw new NotImplementedException();
        }

        public EVvsGDV_MaxProfit_VRP_Model GetProblemModel()
        {
            throw new NotImplementedException();
            //return new EVvsGDV_MaxProfit_VRP_Model(pf); //I think at some point instead of getting everything from reader, reader needs to put info to problem framework
            //and we should continue with pf.
            // TODO I'm not sure whether this should be here or not
        }
    }
}
