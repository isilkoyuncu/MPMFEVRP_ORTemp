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
        string InputFileName { get; }
        string NameOfProblemOfModel { get; }

        //Individual Data Fields
        SiteRelatedData SRD { get; }
        VehicleRelatedData VRD { get; }
        ContextRelatedData CRD { get; }        

        string GetName();
        string GetDescription();

        //Stuff exclusive to the "model"
        ISolution GetRandomSolution(int seed);
        bool CheckFeasibilityOfSolution(ISolution solution);
        double CalculateObjectiveFunctionValue(ISolution solution);
        bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
