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
        protected ProblemDataPackage pf;
        public ProblemDataPackage PF { get { return pf; } }
        
        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        public ProblemBase()
        {
            pf = new ProblemDataPackage();
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
    }
}
