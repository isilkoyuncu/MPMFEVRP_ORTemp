using System;
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

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            startTime = DateTime.Now;

            ofvType = Utils.ProblemUtil.CreateProblemByName(model.GetNameOfProblemOfModel()).ObjectiveFunctionType;//TODO Eliminate creating problem by name just to get the objective function type of an existing problem, this should be much simpler!

            theGDV = model.VRD.VehicleArray.Where(x => x.Category == VehicleCategories.GDV).ToArray()[0];//Technically this returns the first GDV, but there shouldn't be more than one anyways

            //These are the important characteristics that will have to be tied to the form
            beamWidth = 1;
            popStrategy = CustomerSetList.CustomerListPopStrategy.MaxOFVforAnyVehicle;//This is too tightly coupled! Will cause issues in generalizing to tree search

            allCustomerSets = new PartitionedCustomerSetList();
            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);

            children = new CustomerSetList();//this was necessary to avoid a null reference
        }
        public override void SpecializedRun()
        {
            //The following are miscellaneous variables needed in the algorithm
            string GDVFeasibleCustomerSetArchiveFileName = model.InputFileName.Insert(model.InputFileName.Length - 4, "-GDVFeasibleCSArchive");
            string GDVFeasibleCustomerSetUnexploredFileName = model.InputFileName.Insert(model.InputFileName.Length - 4, "-UnexploredGDVFeasibleCSList");
            bool ArchiveFileExists = System.IO.File.Exists(GDVFeasibleCustomerSetArchiveFileName);
            bool UnexploredFileExists = System.IO.File.Exists(GDVFeasibleCustomerSetUnexploredFileName);
            bool UnexploredFileEmpty = (UnexploredFileExists ? (new System.IO.FileInfo(GDVFeasibleCustomerSetUnexploredFileName).Length == 0) : true);
            bool phase1Completed = (ArchiveFileExists && UnexploredFileEmpty);

            int currentLevel;//, highestNonemptyLevel, deepestNonemptyLevel;
            int deepestPossibleLevel = model.SRD.NumCustomers - 1;//This is for the unexplored, when explored its children will be at the next level which is the number of customers; thus, a CS visiting all customers will be created, TSP-optimized and hence added to the repository for the set cover model but it won't ever be added to the unexplored list
            double runTimeLimitInSeconds = algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            int iter = 0;

            if (!phase1Completed)
            {
                //Pre-process
                //If the archive file exists, it must be read
                if (ArchiveFileExists)//This means the unexplored file is not empty
                {
                    allCustomerSets = SetCoverFileUtilities.CustomerSetArchive.RecreateFromFile(GDVFeasibleCustomerSetArchiveFileName, model);
                    unexploredCustomerSets = SetCoverFileUtilities.CustomerSetArchive.RecreateFromFile(GDVFeasibleCustomerSetUnexploredFileName, model);
                }
                else//the archive file doesn't exist, we'll simply ignore the unexplored file even if it exists
                {
                    int nCustomers = model.SRD.NumCustomers;
                    foreach (Site s in model.SRD.SiteArray)
                    {
                        if (s.SiteType == SiteTypes.Customer)
                        {
                            CustomerSet candidate = new CustomerSet(s.ID);//The customer set is not TSP-optimized
                            EvaluateForGDV(candidate, theGDV);//Now it is optimized and added to proper lists
                        }
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
                        unexploredCustomerSets.ConsiderForAddition(children);//TODO: Verify that this clears the children set

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
                    SetCoverFileUtilities.CustomerSetArchive.SaveToFile(allCustomerSets, GDVFeasibleCustomerSetArchiveFileName, model);
                    phase1Completed = true;
                }
                else
                {
                    SetCoverFileUtilities.CustomerSetArchive.SaveToFile(unexploredCustomerSets, GDVFeasibleCustomerSetUnexploredFileName, model);
                    SetCoverFileUtilities.CustomerSetArchive.SaveToFile(allCustomerSets, GDVFeasibleCustomerSetArchiveFileName, model);
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
                    unexploredCustomerSets.ConsiderForAddition(children);//TODO: Verify that this clears the children set

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
                List<string> remainingCustomers = model.GetAllCustomerIDs();
                foreach (string customerID in cs.Customers)
                    remainingCustomers.Remove(customerID);
                foreach (string customerID in remainingCustomers)
                {
                    CustomerSet candidate = new CustomerSet(cs);
                    candidate.Extend(customerID, model);
                    if (!candidate.RouteOptimizerOutcome.RetrievedFromArchive)//not retrieved
                        if ((candidate.RouteOptimizerOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV) || (candidate.RouteOptimizerOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV))
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
            CPlexExtender.Solve_and_PostProcess();
            bestSolutionFound = (CustomerSetBasedSolution)CPlexExtender.GetCompleteSolution(typeof(CustomerSetBasedSolution));
        }
        void EvaluateForGDV(CustomerSet customerSet, Vehicle vehicle)
        {
            //Evaluate is not to be mistaken with explore
            //Evaluate is for a newly created node
            //At the end of the evaluation, a node can be eliminated or kept (by adding to the unexplored list)

            if (allCustomerSets.Contains(customerSet))
                return;
            VehicleSpecificRouteOptimizationOutcome vsroo = model.OptimizeRoute(customerSet, vehicle);
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
    }
}
