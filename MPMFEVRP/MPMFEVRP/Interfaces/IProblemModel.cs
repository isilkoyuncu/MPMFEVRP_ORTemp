using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Interfaces
{
    public interface IProblemModel
    {
        int NumCustomers { get; }
        int NumES { get; }
        int NumNodes { get; }
        Site[] SiteArray { get; }

        int NumVehicleCategories { get; }
        int[] NumVehicles { get; }
        Vehicle[] VehicleArray { get; }

        double TravelSpeed { get; }
        double TMax { get; }
        double[,] Distance { get; }
        double[,,] EnergyConsumption { get; }
        double[,] TimeConsumption { get; }

        string GetName();
        string GetDescription();

        ISolution GetRandomSolution(int seed);
        bool CheckFeasibilityOfSolution(ISolution solution);
        double CalculateObjectiveFunctionValue(ISolution solution);
        bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
