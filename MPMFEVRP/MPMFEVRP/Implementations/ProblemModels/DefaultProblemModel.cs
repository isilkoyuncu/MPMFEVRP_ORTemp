using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class DefaultProblemModel : ProblemModelBase
    {
        string problemName;
        public DefaultProblemModel()
        {
            Implementations.Problems.DefaultProblem problem = new Problems.DefaultProblem();
            problemName = problem.GetName();
        }

        public DefaultProblemModel(IProblem problem)
        {
            //this.totalJobs = problem.Jobs.Count;
            //this.processingTimes = problem.Jobs.Select(x => x.ProcessingTime).ToArray();
            //this.dueDates = problem.Jobs.Select(x => x.DueDate).ToArray();
            //this.ids = problem.Jobs.Select(x => x.ID).ToArray();
            problemName = problem.GetName();
        }

        
        public override ISolution GetRandomSolution(int seed)
        {
            throw new NotImplementedException();
            //TODO uncomment this afer writing new default solution
            //return new DefaultSolution(this, new Random(seed));
        }

        public override string GetName()
        {
            return "Default Problem Model";
        }

        public override string GetDescription()
        {
            return "Problem model with id, processingTime, dueDate arrays and totalJob count";
        }

        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            //TODO: Compatibility of Solution to Problem must be checked here
            //TODO: A series of conditions to return false must be added here
            return true;
        }

        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            //TODO: Compatibility of Solution to Problem must be checked here
            //TODO: The code here assumes the solution is always the sequence-based solution, this must be generalized
            double OFV = int.MinValue;//maxLateness
            //if (solution.GetType() == typeof(DefaultSolution))
            //{
            //    int completionTime = 0;
            //    int lateness = int.MinValue;
            //    for (int i = 0; i < totalJobs; i++)
            //    {
            //        int position = Array.IndexOf(ids, solution.IDs[i]);
            //        completionTime += processingTimes[position];
            //        lateness = completionTime - dueDates[position];
            //        if (OFV < lateness)
            //            OFV = lateness;
            //    }
            //}
            return OFV;
        }

        public override bool CompareTwoSolutions(ISolution incumbent, ISolution challenger)
        {
            // TODO return as bool may be improved to an enum later on
            // TODO if problem model can be generalized, this can go to ProblemModelBase
            return (CalculateObjectiveFunctionValue(incumbent) > CalculateObjectiveFunctionValue(challenger));
        }

        public override string GetNameOfProblemOfModel()
        {
            return problemName;
        }
    }
}
