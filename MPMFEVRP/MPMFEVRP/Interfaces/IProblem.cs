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

        

        string GetName();

        string CreateRawData();
    }
}
