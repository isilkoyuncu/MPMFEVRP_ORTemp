using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models;

namespace MPMFEVRP.Interfaces
{
    public interface IProblemModel
    {
        string InputFileName { get; }

        ObjectiveFunctionTypes ObjectiveFunctionType { get; }
        ObjectiveFunctions ObjectiveFunction { get; }
        InputOrOutputParameterSet ProblemCharacteristics { get; }

        CustomerCoverageConstraint_EachCustomerMustBeCovered CoverConstraintType { get; }

        //Individual Data Fields
        SiteRelatedData SRD { get; }
        VehicleRelatedData VRD { get; }
        ContextRelatedData CRD { get; }        

        string GetName();
        string GetDescription();
        string GetNameOfProblemOfModel();

        List<Type> GetCompatibleSolutions();

        //Stuff exclusive to the "model"
        ISolution GetRandomSolution(int seed, Type solutionType);
        bool CheckFeasibilityOfSolution(ISolution solution);
        double CalculateObjectiveFunctionValue(ISolution solution);
        bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
