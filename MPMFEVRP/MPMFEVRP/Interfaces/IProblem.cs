using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Interfaces
{
    public interface IProblem
    {
        ObjectiveFunctionTypes ObjectiveFunctionType { get; }

        ProblemDataPackage PDP { get; }
        InputOrOutputParameterSet ProblemCharacteristics { get; }

        string GetName();

        string CreateRawData();

        void LoadProblemCharacteristics();
        // TODO for the different types of problems we need to add and indicatior here (GVRP classification)
    }
}
