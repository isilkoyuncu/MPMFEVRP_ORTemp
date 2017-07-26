﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Implementations.Solutions;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class AColumnGenerationHeuristic : AlgorithmBase
    {
        ObjectiveFunctionTypes ofvType;
        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents, children;

        DateTime startTime;

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam = new XCPlexParameters(); //TODO do we need to add additional parameters for the assigment problem?

        public override string GetName()
        {
            return "Customer Set-based Column Generation Heuristic";
        }

        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            startTime = DateTime.Now;

            ofvType = Utils.ProblemUtil.CreateProblemByName(model.GetNameOfProblemOfModel()).ObjectiveFunctionType;

            unexploredCustomerSets = new PartitionedCustomerSetList();
            int nCustomers = model.SRD.NumCustomers;
            foreach(Site s in model.SRD.SiteArray)
            {
                if(s.SiteType == SiteTypes.Customer)
                {
                    CustomerSet candidate = new CustomerSet(s.ID, model);
                    unexploredCustomerSets.ConsiderForAddition(candidate);
                }
            }

            children = new CustomerSetList();//this was necessary to avoid a null reference
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedRun()
        {
            //These are the important characteristics that will have to be tied to the form
            int beamWidth = 3;

            //The following are miscellaneous variables needed in the algorithm
            int currentLevel;//, highestNonemptyLevel, deepestNonemptyLevel;
            int deepestPossibleLevel = model.SRD.NumCustomers - 1;//This is for the unexplored, when explored its children will be at the next level which is the number of customers; thus, a CS visiting all customers will be created, TSP-optimized and hence added to the repository for the set cover model but it won't ever be added to the unexplored list
            double runTimeLimitInSeconds = algorithmParameters.GetParameter(Domains.AlgorithmDomain.ParameterID.RUNTIME_SECONDS).GetDoubleValue();

            do
            {
                //We have a non-empty list 

                //The body of an iteration:
                currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();
                while (currentLevel <= deepestPossibleLevel)
                {
                    //Take the parents from the current level
                    if (currentLevel > unexploredCustomerSets.GetDeepestNonemptyLevel())
                        break;
                    parents = unexploredCustomerSets.Pop(currentLevel, beamWidth);

                    //populate children from parents
                    populateChildren();

                    //add the children to the unexplored list (level:currentLevel+1)
                    unexploredCustomerSets.ConsiderForAddition(children);//TODO: Verify that this clears the children set

                    //end of the level, moving on to the next level
                    currentLevel++;
                }

                //After the iteration:
                //run the set cover model and update the best found solution
                RunSetCover();
            } while ((unexploredCustomerSets.TotalCount > 0) && ((DateTime.Now - startTime).TotalSeconds < runTimeLimitInSeconds));


            throw new NotImplementedException();
        }
        void populateChildren()
        {
            if (children.Count > 0)
                throw new Exception("children is not empty, can't populate new children!");
            foreach (CustomerSet cs in parents)
            {
                List<string> remainingCustomers = model.GetAllCustomerIDs();
                foreach (string customerID in cs.Customers)
                    remainingCustomers.Remove(customerID);
                foreach (string customerID in remainingCustomers)
                {
                    CustomerSet candidate = new CustomerSet(cs);
                    candidate.Extend(customerID, model);
                    if(!candidate.RouteOptimizerOutcome.RetrievedFromArchive)//not retrieved
                        if((candidate.RouteOptimizerOutcome.Status== RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)||(candidate.RouteOptimizerOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV))
                        {
                            children.Add(candidate);
                        }
                }//foreach (string customerID in remainingCustomers)
            }//foreach (CustomerSet cs in parents)
            parents.Clear();
        }
        void RunSetCover()
        {
            CPlexExtender = new XCPlex_SetCovering_wCustomerSets(model, XcplexParam);
            //CPlexExtender.ExportModel(((XCPlex_Formulation)algorithmParameters.GetParameter(ParameterID.XCPLEX_FORMULATION).Value).ToString()+"model.lp");
            CPlexExtender.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }
    }
}
