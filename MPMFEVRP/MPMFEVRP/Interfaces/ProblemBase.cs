using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels;

namespace MPMFEVRP.Interfaces
{
    public abstract class ProblemBase : IProblem
    {
        protected ProblemDataPackage pdp;
        public ProblemDataPackage PDP { get { return pdp; } set { pdp = value; } }
        
        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        public ProblemBase()
        {
            pdp = new ProblemDataPackage();
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
