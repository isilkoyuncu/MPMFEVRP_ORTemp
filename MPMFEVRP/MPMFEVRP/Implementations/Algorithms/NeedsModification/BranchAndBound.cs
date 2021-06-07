//using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
//using MPMFEVRP.Models;
//using MPMFEVRP.SupplementaryInterfaces.Listeners;
//using MPMFEVRP.Utils;
//using System;
//using System.Collections.Generic;

//namespace MPMFEVRP.Implementations.Algorithms
//{
//    public class BranchAndBound : AlgorithmBase
//    {
//        SolutionList unexploredList;
//        double lowerBound, upperBound;
//        public override void AddSpecializedParameters() { }

//        public override string GetName()
//        {
//            return "Branch and Bound";
//        }

//        public override void SpecializedConclude()
//        {

//        }

//        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
//        {
//            base.theProblemModel = theProblemModel;
//            unexploredList = new SolutionList();

//            // Step 0: Create root and add it to unexploredList (solution type is fetched as a parameters to the algorithm)
//            ISolution root = SolutionUtil.CreateSolutionByName(algorithmParameters.GetParameter(ParameterID.ALG_SOLUTION_TYPES).GetStringValue(), theProblemModel);

//            unexploredList.Add(root);

//            upperBound = Double.MaxValue;
//            lowerBound = root.LowerBound;
//        }

//        public override void SpecializedReset()
//        {

//        }

//        public override void SpecializedRun()
//        {
//            while (unexploredList.Count > 0)
//            {
//                // Node selection step
//                ISolution current = unexploredList.Pop(strategy: PopStrategy.LowestLowerBound); // ISSUE (#5) get parameter from algo

//                // Specify current
//                current.TriggerSpecification();

//                if (current.IsComplete)
//                {
//                    if (theProblemModel.CalculateObjectiveFunctionValue(current) < upperBound)
//                    {
//                        bestSolutionFound = current;
//                        UpdateUpperBound();
//                    }
//                    else
//                    {
//                        // Eliminate the current solution because it is sub-optimal
//                        continue;
//                    }
//                }
//                else // if (!current.IsComplete)
//                {
//                    if (current.LowerBound >= upperBound)
//                    {
//                        // Eliminate the current solution because it sub-optimal
//                        continue;
//                    }
//                    else
//                    {
//                        List<ISolution> childrenOfCurrent = current.GetAllChildren();
//                        foreach (var child in childrenOfCurrent)
//                        {
//                            // TODO: if some specification needed before adding to unexploredList
//                            unexploredList.Add(child);
//                        }
//                    }
//                }
//            } // while (unexploredList.Count > 0)
//        }

//        /// <summary>
//        /// This only works for the bestSolutionFound
//        /// </summary>
//        void UpdateUpperBound()
//        {
//            upperBound = theProblemModel.CalculateObjectiveFunctionValue( bestSolutionFound);
//            unexploredList.Cut(upperBound);
//            // TODO stats update
//        }

//        public override string[] GetOutputSummary()
//        {
//            throw new NotImplementedException();
//        }

//        public override bool setListener(IListener listener)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
