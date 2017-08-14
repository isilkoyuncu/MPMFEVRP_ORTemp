using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Utils;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class BranchAndBound : AlgorithmBase
    {
        SolutionList unexploredList;
        double lowerBound, upperBound;
        public override void AddSpecializedParameters() { }

        public override string GetName()
        {
            return "Branch and Bound";
        }

        public override void SpecializedConclude()
        {

        }

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            unexploredList = new SolutionList();

            // Step 0: Create root and add it to unexploredList (solution type is fetched as a parameters to the algorithm)
            ISolution root = SolutionUtil.CreateSolutionByName(algorithmParameters.GetParameter(ParameterID.ALG_SOLUTION_TYPES).GetStringValue(), model);

            unexploredList.Add(root);

            upperBound = Double.MaxValue;
            lowerBound = root.LowerBound;
        }

        public override void SpecializedReset()
        {

        }

        public override void SpecializedRun()
        {
            while (unexploredList.Count > 0)
            {
                // Node selection step
                ISolution current = unexploredList.Pop(strategy: PopStrategy.LowestLowerBound); // TODO get parameter from algo

                // Specify current
                current.TriggerSpecification();

                if (current.IsComplete)
                {
                    if (current.ObjectiveFunctionValue < upperBound)
                    {
                        bestSolutionFound = current;
                        UpdateUpperBound();
                    }
                    else
                    {
                        // Eliminate the current solution because it is sub-optimal
                        continue;
                    }
                }
                else // if (!current.IsComplete)
                {
                    if (current.LowerBound >= upperBound)
                    {
                        // Eliminate the current solution because it sub-optimal
                        continue;
                    }
                    else
                    {
                        List<ISolution> childrenOfCurrent = current.GetAllChildren();
                        foreach (var child in childrenOfCurrent)
                        {
                            // TODO: if some specification needed before adding to unexploredList
                            unexploredList.Add(child);
                        }
                    }
                }
            } // while (unexploredList.Count > 0)
        }

        /// <summary>
        /// This only works for the bestSolutionFound
        /// </summary>
        void UpdateUpperBound()
        {
            upperBound = bestSolutionFound.ObjectiveFunctionValue;
            unexploredList.Cut(upperBound);
            // TODO stats update
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }
    }
}
