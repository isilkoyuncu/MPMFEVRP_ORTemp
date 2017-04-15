using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Interfaces
{
    public interface IAlgorithm
    {
        void Initialize(ProblemModelBase model);
        void Run();
        void Conclude();

        void Reset();

        AlgorithmParameters AlgorithmParameters { get; }
        ISolution Solution { get; }

        string GetName();
    }
}
