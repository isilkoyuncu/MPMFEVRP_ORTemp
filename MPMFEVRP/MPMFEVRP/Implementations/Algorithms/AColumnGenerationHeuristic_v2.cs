using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class AColumnGenerationHeuristic_v2 : AlgorithmBase
    {
        ObjectiveFunctionTypes ofvType;

        //These are the important characteristics that will have to be tied to the form
        int beamWidth;
        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList allCustomerSets;
        PartitionedCustomerSetList unexploredCustomerSets;
        CustomerSetList parents, children;

        DateTime startTime;

        XCPlexBase CPlexExtender = null;
        XCPlexParameters XcplexParam = new XCPlexParameters(); //TODO do we need to add additional parameters for the assigment problem?

        Vehicle theGDV;

        public override string GetName()
        {
            return "Customer Set-based Column Generation Heuristic Version 2: Keeps the customer set archive within (not in problem model)";
        }
        public override void AddSpecializedParameters() { }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel model)
        {
            startTime = DateTime.Now;

            ofvType = Utils.ProblemUtil.CreateProblemByName(model.GetNameOfProblemOfModel()).ObjectiveFunctionType;//TODO Eliminate creating problem by name just to get the objective function type of an existing problem, this should be much simpler!

            theGDV = model.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);//Technically this returns the first GDV, but there shouldn't be more than one anyways

            //These are the important characteristics that will have to be tied to the form
            beamWidth = 20;
            popStrategy = CustomerSetList.CustomerListPopStrategy.MinOFVforAnyVehicle;//This is too tightly coupled! Will cause issues in generalizing to tree search

            allCustomerSets = new PartitionedCustomerSetList();
            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);

            children = new CustomerSetList();//this was necessary to avoid a null reference
        }
        public override void SpecializedRun()
        {
            //The following are miscellaneous variables needed in the algorithm
            string GDVEvaluatedCustomerSetArchiveFileName = theProblemModel.InputFileName.Insert(theProblemModel.InputFileName.Length - 4, "-GDVEvaluatedCSArchive");
            string GDVFeasibleCustomerSetUnexploredFileName = theProblemModel.InputFileName.Insert(theProblemModel.InputFileName.Length - 4, "-UnexploredGDVFeasibleCSList");
            bool ArchiveFileExists = System.IO.File.Exists(GDVEvaluatedCustomerSetArchiveFileName);
            bool UnexploredFileExists = System.IO.File.Exists(GDVFeasibleCustomerSetUnexploredFileName);
            bool UnexploredFileEmpty = (UnexploredFileExists ? (new System.IO.FileInfo(GDVFeasibleCustomerSetUnexploredFileName).Length == 0) : true);
            if (ArchiveFileExists)
            {
                //If the archive file exists, it must be read
                //the archive file doesn't exist, we'll simply ignore the unexplored file even if it exists
                allCustomerSets = SetCoverFileUtilities.CustomerSetArchive.RecreateFromFile(GDVEvaluatedCustomerSetArchiveFileName, theProblemModel);
                if (UnexploredFileExists)
                    unexploredCustomerSets = SetCoverFileUtilities.CustomerSetArchive.RecreateFromFile(GDVFeasibleCustomerSetUnexploredFileName, theProblemModel);
            }
            bool phase1Completed = (ArchiveFileExists && UnexploredFileEmpty);



            int currentLevel;//, highestNonemptyLevel, deepestNonemptyLevel;
            int deepestPossibleLevel = theProblemModel.SRD.NumCustomers - 1;//This is for the unexplored, when explored its children will be at the next level which is the number of customers; thus, a CS visiting all customers will be created, TSP-optimized and hence added to the repository for the set cover model but it won't ever be added to the unexplored list
            double runTimeLimitInSeconds = algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            int iter = 0;

            if (!phase1Completed)
            {
                //Pre-process

                if (!ArchiveFileExists)
                {
                    int nCustomers = theProblemModel.SRD.NumCustomers;
                    foreach (string customerID in theProblemModel.SRD.GetCustomerIDs())
                    {
                        CustomerSet candidate = new CustomerSet(customerID);//The customer set is not TSP-optimized
                        GDVOptimizeCustomerSetAndEvaluateForLists(candidate, theGDV);//Now it is optimized and added to proper lists
                    }
                }

                //Iterations
                iter = 0;
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

                        //populate children from parents
                        PopulateChildren();//TODO: Make sure this uses the right extend method and route optimization

                        //add the children to the unexplored list (level:currentLevel+1)
                        PlaceChildren();

                        //end of the level, moving on to the next level
                        currentLevel++;
                    }

                    //After the iteration:
                    //run the set cover model and update the best found solution
                    //RunSetCover();
                } while ((unexploredCustomerSets.TotalCount > 0) && ((DateTime.Now - startTime).TotalSeconds < runTimeLimitInSeconds));

                //Post-process
                //Write to the archive file (don't append, just wipe old stuff out and replace because we can't keep track of which is old which is new without separating lists and separation would just overly complicate the code)
                if (unexploredCustomerSets.TotalCount == 0)
                {
                    SetCoverFileUtilities.CustomerSetArchive.SaveToFile(allCustomerSets, GDVEvaluatedCustomerSetArchiveFileName, theProblemModel);
                    phase1Completed = true;
                }
                else
                {
                    SetCoverFileUtilities.CustomerSetArchive.SaveToFile(unexploredCustomerSets, GDVFeasibleCustomerSetUnexploredFileName, theProblemModel);
                    SetCoverFileUtilities.CustomerSetArchive.SaveToFile(allCustomerSets, GDVEvaluatedCustomerSetArchiveFileName, theProblemModel);
                }
            }

            //TODO: readers, writers, decision on whether run phase 2 with incomplete phase 1, etc.
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

                    //populate children from parents
                    PopulateChildren();

                    //add the children to the unexplored list (level:currentLevel+1)
                    PlaceChildren();

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
            CPlexExtender.ClearModel();
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
                    if (!allCustomerSets.ContainsAnIdenticalCustomerSet(candidate))
                    {
                        candidate.Optimize(theProblemModel, theGDV);
                        allCustomerSets.Add(candidate);
                        //if (!candidate.RouteOptimizationOutcome.RetrievedFromArchive)//Disabled because it is irrelevant
                            if (candidate.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).Status == VehicleSpecificRouteOptimizationStatus.Optimized)
                            {
                                children.Add(candidate);
                            }
                    }//if candidate is not already in the archive
                }//foreach (string customerID in remainingCustomers)
            }//foreach (CustomerSet cs in parents)
            parents.Clear();
        }
        void PlaceChildren()
        {
            unexploredCustomerSets.ConsiderForAddition(children);
            children.Clear();
        }

        void RunSetCover()
        {
            CPlexExtender = new XCPlex_SetCovering_wCustomerSets(theProblemModel, XcplexParam);
            CPlexExtender.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
        }
        void GDVOptimizeCustomerSetAndEvaluateForLists(CustomerSet customerSet, Vehicle vehicle)//TODO: Re-think this method's name. If it's for GDV only, why does it take vehicle as an input and not verify it's a GDV?
        {
            //Evaluate is not to be mistaken with explore
            //Evaluate is for a newly created node
            //At the end of the evaluation, a node can be eliminated or kept (by adding to the unexplored list)

            if (allCustomerSets.ContainsAnIdenticalCustomerSet(customerSet))
                return;
            VehicleSpecificRouteOptimizationOutcome vsroo = theProblemModel.RouteOptimize(customerSet, vehicle);
            customerSet.RouteOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo });
            allCustomerSets.Add(customerSet);
            if (vsroo.Status == VehicleSpecificRouteOptimizationStatus.Optimized)
            {
                unexploredCustomerSets.Add(customerSet);
            }
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
