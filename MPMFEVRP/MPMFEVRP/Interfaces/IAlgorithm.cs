using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Interfaces
{
    public interface IAlgorithm
    {
        void Initialize(ProblemModelBase model);
        void Run();
        void Conclude();
        void Reset();
        AlgorithmParameters AlgorithmParameters { get; }
        //AlgorithmSolutionStatus Status { get; }
        //AlgorithmStatistics Stats { get; }

        ISolution Solution { get; }
        string GetName();
        string[] GetOutputSummary();
    }
}
