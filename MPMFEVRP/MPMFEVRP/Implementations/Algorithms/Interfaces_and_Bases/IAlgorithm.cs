using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models;
using System.ComponentModel;
using MPMFEVRP.SupplementaryInterfaces.Listeners;

namespace MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases
{
    public interface IAlgorithm
    {
        void Initialize(EVvsGDV_ProblemModel theProblemModel);
        void setBackgroundWorker(BackgroundWorker bg);
        void Run();
        void Conclude();
        void Reset();
        InputOrOutputParameterSet AlgorithmParameters { get; }
        //AlgorithmSolutionStatus Status { get; }
        //AlgorithmStatistics Stats { get; }

        ISolution Solution { get; }
        string GetName();
        string[] GetOutputSummary();

        bool setListener(IListener listener);

    }
}
