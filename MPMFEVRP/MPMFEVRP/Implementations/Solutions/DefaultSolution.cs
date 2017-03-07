using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.ProblemModels;
using BestRandom;
using MPMFEVRP.Forms;

namespace MPMFEVRP.Implementations.Solutions
{



    public class DefaultSolution : SolutionBase
    {

        //TODO new solutions must be created!!!
        //    protected IProblemModel problemData;
        //    protected Random random;

        //    // TODO refactor the solution structre. Probably, we will need problemData in all solutions for calculations.


        //    // lowerbound, upperbound and objectivefunctionvalue will be calculated in each of these constructors
        //    // This is computed in a method and just called. (ComputeVariables)
        //    // But to the external, they are just properties and always has a value.
        //    public DefaultSolution()
        //    {
        //        //ComputeVariables();
        //    }

        //    public DefaultSolution(ISolution solution, string extensionJobId, IProblemModel problemData)
        //    {
        //        this.ids.AddRange(solution.IDs);
        //        this.ids.Add(extensionJobId);
        //        this.problemData = problemData;
        //        ComputeVariables(false);
        //    }

        //    public DefaultSolution(IProblemModel problemData)
        //    {
        //        this.random = new Random(DateTime.Now.Ticks.GetHashCode());
        //        this.problemData = problemData;
        //        ComputeVariables(false);
        //    }

        //    public DefaultSolution(IProblemModel problemData, Random rnd)
        //    {
        //        this.random = rnd;
        //        this.problemData = problemData;
        //        ComputeVariables(false);
        //    }

        //    protected void ComputeVariables(bool backwardLoading = true)
        //    {
        //        int nSequencedJobs = ids.Count;
        //        List<string> remainingIds = problemData.IDs.Where(x => !ids.Contains(x)).ToList();
        //        int nRemainingJobs = remainingIds.Count;
        //        int totalRemainingProcessingTime = 0;
        //        foreach (var j in remainingIds)
        //            totalRemainingProcessingTime += problemData.ProcessingTimes[Array.IndexOf(problemData.IDs, j)];

        //        isComplete = (ids.Count == problemData.TotalJobs);
        //        objectiveFunctionValue = int.MinValue;

        //        int[] completionTime = new int[problemData.TotalJobs];
        //        int[] dueDate = new int[problemData.TotalJobs];
        //        int[] lateness = new int[problemData.TotalJobs];

        //        int position = (backwardLoading ? nRemainingJobs : 0);//preparing to forward-count positions starting with the first non-blank 
        //        int previousCompletionTime = backwardLoading ? totalRemainingProcessingTime : 0;
        //        if (position > 0)
        //            completionTime[position - 1] = totalRemainingProcessingTime;//the completion time of the last remaining job, if backwardLoading
        //        //Now iterating over the sequenced jobs
        //        foreach (var j in ids)
        //        {
        //            int indexOfJobIdInProblemData = Array.IndexOf(problemData.IDs, j);
        //            completionTime[position] = previousCompletionTime + problemData.ProcessingTimes[indexOfJobIdInProblemData];//TODO Was there an easire way meant for this?
        //            dueDate[position] = problemData.DueDates[indexOfJobIdInProblemData];
        //            lateness[position] = completionTime[position] - dueDate[position];
        //            if (objectiveFunctionValue < lateness[position])
        //                objectiveFunctionValue = lateness[position];
        //            previousCompletionTime = completionTime[position++];
        //        }
        //        if (!backwardLoading)
        //            completionTime[problemData.TotalJobs - 1] = previousCompletionTime + totalRemainingProcessingTime;//the completion time of the last remaining job, if forwardLoading

        //        lowerBound = objectiveFunctionValue;
        //        if (nRemainingJobs > 0)
        //        {
        //            int lastBlankPosition = backwardLoading ? nRemainingJobs - 1 : problemData.TotalJobs - 1;
        //            int maxRemainingDueDate = int.MinValue;
        //            foreach (var j in remainingIds)
        //            {
        //                int remainingJobDueDate = problemData.DueDates[Array.IndexOf(problemData.IDs, j)];
        //                if (maxRemainingDueDate < remainingJobDueDate)
        //                    maxRemainingDueDate = remainingJobDueDate;
        //            }
        //            lowerBound = (int)Math.Max(objectiveFunctionValue, completionTime[lastBlankPosition] - maxRemainingDueDate);
        //        }
        //    }

        //    public override ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2)
        //    {
        //        int signOfOFVDifference = Math.Sign(problemData.CalculateObjectiveFunctionValue(solution1) - problemData.CalculateObjectiveFunctionValue(solution2));
        //        if (signOfOFVDifference < 0)
        //            return ComparisonResult.FirstIsBetter;
        //        else if (signOfOFVDifference > 0)
        //            return ComparisonResult.SecondIsBetter;
        //        return ComparisonResult.Equal;
        //    }

        //    public override ISolution GenerateRandom()
        //    {
        //        ISolution someSolution = new DefaultSolution(problemData);
        //        someSolution.IDs.AddRange(problemData.IDs.ToList().OrderBy(x => random.Next()));
        //        ComputeVariables();
        //        return someSolution;
        //    }

        //    public override List<ISolution> GetAllChildren()
        //    {
        //        List<ISolution> children = new List<ISolution>();

        //        // Get the job IDs that doesnot exist in solution's sequence.
        //        List<string> remainingIds = problemData.IDs.Where(x => !ids.Contains(x)).ToList();

        //        foreach (var id in remainingIds)
        //        {
        //            children.Add(new DefaultSolution(this, id, problemData));
        //        }
        //        return children;
        //    }

        //    public override string GetName()
        //    {
        //        return "Default Solution";
        //    }

        //    public override void View(IProblem problem)
        //    {
        //        new DefaultSolutionViewer(problem, this).Show();
        //    }

        //    public override void TriggerSpecification()
        //    {
        //        // TODO specification
        //        ComputeVariables(false);
        //    }
        public override ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        public override ISolution GenerateRandom()
        {
            throw new NotImplementedException();
        }

        public override List<ISolution> GetAllChildren()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            throw new NotImplementedException();
        }

        public override void TriggerSpecification()
        {
            throw new NotImplementedException();
        }

        public override void View(IProblem problem)
        {
            throw new NotImplementedException();
        }
    }
}
