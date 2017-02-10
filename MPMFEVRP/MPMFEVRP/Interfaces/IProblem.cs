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

        List<Job> Jobs { get; }//TODO This is project-dependent, can we make the interfaces general?

        string GetName();

        string CreateRawData();
    }
}
