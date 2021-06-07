using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class BestOfRandom : AlgorithmBase
    {
        SolutionList unexploredList;
        double lowerBound;
        public override void AddSpecializedParameters() { }


        public override string GetName()
        {
            return "Best of Random";
        }

        public override void SpecializedConclude()
        {

        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel model)
        {
            unexploredList = new SolutionList();

            // Step 0: Create root and add it to unexploredList

            ISolution root = new CustomerSetBasedSolution();//ISSUE (#5): Make solution type parametric, any solution type should be good as long as it can generate random instances
            unexploredList.Add(root);

            lowerBound = root.LowerBound;
        }

        public override void SpecializedReset()
        {

        }

        public override void SpecializedRun()
        {
            //ISSUE (#7):  Re-think the BestOfRandom algorithm: Why is it creating solutions via branching? Isn't there a method to get pseudo-random complete solutions already?
            while (unexploredList.Count > 0)
            {
                // Node selection step
                ISolution current = unexploredList.Pop(); // TODO get parameter from algo

                // Specify current
                current.TriggerSpecification();

                if (current.IsComplete)
                {
                    bestSolutionFound = current;
                }
                else // if (!current.IsComplete)
                {
                    List<ISolution> childrenOfCurrent = current.GetAllChildren();
                    childrenOfCurrent.Sort();//TODO Checkout the default comparer and replace if necessary
                    unexploredList.Add(childrenOfCurrent[0]);
                }
            } // while (unexploredList.Count > 0)
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }

        public override bool setListener(IListener listener)
        {
            throw new NotImplementedException();
        }
    }
}
