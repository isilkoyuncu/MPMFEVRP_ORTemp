using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class AColumnGenerationHeuristic : AlgorithmBase
    {
        ObjectiveFunctionTypes ofvType;

        //These are the important characteristics that will have to be tied to the form
        int beamWidth;
        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents, children;

        DateTime startTime;

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam = new XCPlexParameters(); //TODO do we need to add additional parameters for the assigment problem?

        public override string GetName()
        {
            return "Customer Set-based Column Generation Heuristic";
        }
        public override void AddSpecializedParameters(){ }
        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            startTime = DateTime.Now;

            ofvType = Utils.ProblemUtil.CreateProblemByName(theProblemModel.GetNameOfProblemOfModel()).ObjectiveFunctionType;

            //These are the important characteristics that will have to be tied to the form
            beamWidth = 1;
            popStrategy = CustomerSetList.CustomerListPopStrategy.MaxOFVforAnyVehicle;

            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);
            int nCustomers = theProblemModel.SRD.NumCustomers;
            foreach (string customerID in theProblemModel.SRD.GetCustomerIDs())
            {
                CustomerSet candidate = new CustomerSet(customerID);
                candidate.Optimize(theProblemModel);
                unexploredCustomerSets.ConsiderForAddition(candidate);
            }

            children = new CustomerSetList();//this was necessary to avoid a null reference
        }
        public override void SpecializedRun()
        {
            //The following are miscellaneous variables needed in the algorithm
            int currentLevel;//, highestNonemptyLevel, deepestNonemptyLevel;
            int deepestPossibleLevel = theProblemModel.SRD.NumCustomers - 1;//This is for the unexplored, when explored its children will be at the next level which is the number of customers; thus, a CS visiting all customers will be created, TSP-optimized and hence added to the repository for the set cover model but it won't ever be added to the unexplored list
            double runTimeLimitInSeconds = algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            int iter = 0;
            do
            {
                iter++;
                //We have a non-empty list 

                //The body of an iteration:
                currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();
                while (currentLevel <= deepestPossibleLevel)
                {
                    //Take the parents from the current level
                    if (currentLevel > unexploredCustomerSets.GetDeepestNonemptyLevel())
                        break;
                    parents = unexploredCustomerSets.Pop(currentLevel, beamWidth);

                    if (iter == 2)
                        if (currentLevel == 6)
                            Console.WriteLine("Stop and go into populate Children!");
                    //populate children from parents
                    PopulateChildren();

                    //add the children to the unexplored list (level:currentLevel+1)
                    unexploredCustomerSets.PopAndConsiderForAddition(children);//TODO: Verify that this clears the children set

                    //end of the level, moving on to the next level
                    currentLevel++;
                }

                //After the iteration:
                //run the set cover model and update the best found solution
                RunSetCover();
            } while ((unexploredCustomerSets.TotalCount > 0) && ((DateTime.Now - startTime).TotalSeconds < runTimeLimitInSeconds));
            //model.CustomerSetArchive.ExportAllCustomerSets("sample1.txt", false);
        }
        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }
        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        void PopulateChildren()
        {
            if (children.Count > 0)
                throw new Exception("children is not empty, can't populate new children!");
            foreach (CustomerSet cs in parents)
            {
                List<string> remainingCustomers = theProblemModel.GetAllCustomerIDs();
                foreach (string customerID in cs.Customers)
                    remainingCustomers.Remove(customerID);
                foreach (string customerID in remainingCustomers)
                {
                    CustomerSet candidate = new CustomerSet(cs);
                    candidate.Extend(customerID);
                    //Need to have an archive here so we can compare
                    if (candidate.RetrievedFromArchive)//retrieved//TODO Replace this by archive.Contains()
                        continue;
                    candidate.Optimize(theProblemModel);

                    if ((candidate.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV) || (candidate.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV))
                    {
                        children.Add(candidate);
                    }
                }//foreach (string customerID in remainingCustomers)
            }//foreach (CustomerSet cs in parents)
            parents.Clear();
        }
        void RunSetCover()
        {
            CPlexExtender = new XCPlex_SetCovering_wCustomerSets(theProblemModel, XcplexParam);
            CPlexExtender.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }
    }
}
