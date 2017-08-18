using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Problems.Interfaces_and_Bases
{
    public interface IProblem
    {
        ObjectiveFunctionTypes ObjectiveFunctionType { get; }
        ObjectiveFunctions ObjectiveFunction { get; }

        ProblemDataPackage PDP { get; }
        InputOrOutputParameterSet ProblemCharacteristics { get; }

        string GetName();

        string CreateRawData();

        // TODO for the different types of problems we need to add and indicatior here (GVRP classification)
    }
}
