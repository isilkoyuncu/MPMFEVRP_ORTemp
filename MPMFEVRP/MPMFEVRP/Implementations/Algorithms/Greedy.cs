using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class Greedy : AlgorithmBase
    {
        SolutionList unexploredList;
        double lowerBound;

        public override string GetName()
        {
            return "Randomized Greedy";
        }

        public override void SpecializedConclude()
        {

        }

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            //TODO uncomment this afer writing new default solution


            //unexploredList = new SolutionList();

            //// Step 0: Create root and add it to unexploredList

            //ISolution root = new DefaultSolution(model);
            //unexploredList.Add(root);

            //lowerBound = root.LowerBound;
        }

        public override void SpecializedReset()
        {

        }

        public override void SpecializedRun()
        {
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
    }
}
