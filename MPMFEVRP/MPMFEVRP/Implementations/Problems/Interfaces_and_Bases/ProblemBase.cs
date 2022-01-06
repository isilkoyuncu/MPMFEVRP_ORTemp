using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models;
using System;

namespace MPMFEVRP.Implementations.Problems.Interfaces_and_Bases
{
    public abstract class ProblemBase : IProblem
    {
        protected ProblemDataPackage pdp;
        public ProblemDataPackage PDP { get { return pdp; } set { pdp = value; } }
        
        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; }set { objectiveFunctionType = value; } }

        protected OldObjectiveFunctions objectiveFunction;
        public OldObjectiveFunctions ObjectiveFunction { get { return objectiveFunction; } }

        protected ObjectiveFunctionCoefficientsPackage objectiveFunctionCoefficientsPackage;
        public ObjectiveFunctionCoefficientsPackage ObjectiveFunctionCoefficientsPackage { get { return objectiveFunctionCoefficientsPackage; } }

        protected InputOrOutputParameterSet problemCharacteristics;
        public InputOrOutputParameterSet ProblemCharacteristics { get { return problemCharacteristics; } }

        public ProblemBase()
        {
            pdp = new ProblemDataPackage();
            problemCharacteristics = new InputOrOutputParameterSet();
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
