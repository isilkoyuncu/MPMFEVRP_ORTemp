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

        public override void SpecializedInitialize(ProblemModelBase model)
        {
            startTime = DateTime.Now;

            ofvType = Utils.ProblemUtil.CreateProblemByName(model.GetNameOfProblemOfModel()).ObjectiveFunctionType;

            //These are the important characteristics that will have to be tied to the form
            beamWidth = 3;
            popStrategy = CustomerSetList.CustomerListPopStrategy.MaxOFVforAnyVehicle;

            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);
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
        public override void SpecializedRun()
        {
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


            throw new NotImplementedException();
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
                    if (unexploredCustomerSets.TotalCount == 221)
                        if (cs.NumberOfCustomers == 6)
                            if (cs.Customers[0] == "C11")
                                if (cs.Customers[1] == "C14")
                                    if (cs.Customers[2] == "C18")
                                        if (cs.Customers[3] == "C20")
                                            if (cs.Customers[4] == "C3")
                                                if (cs.Customers[5] == "C9")
                                                    if (customerID == "C17")
                                                        Console.WriteLine("This is the suspicious case!");
                    CustomerSet candidate = new CustomerSet(cs);

                    //TODO delete the following after debugging
                    if (cs.NumberOfCustomers == 2)
                        if(cs.Customers.Contains("C8")&&cs.Customers.Contains("C5")|| cs.Customers.Contains("C8") && cs.Customers.Contains("C10")|| cs.Customers.Contains("C10") && cs.Customers.Contains("C5"))
                            if(customerID=="C10"|| customerID == "C5" || customerID == "C8")
                            Console.WriteLine("Potentially suspicious case!");

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
