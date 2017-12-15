using MPMFEVRP.Domains.AlgorithmDomain;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Implementations.Algorithms
{
    class RandomizedCustomerSetExplorerViaTreeSearch : AlgorithmBase
    {
        //        XCPlexParameters xCplexParam;
        Vehicle theGDV;
        Vehicle theEV;

        CustomerSetList.CustomerListPopStrategy popStrategy;

        PartitionedCustomerSetList unexploredCustomerSets;
        PartitionedCustomerSetList exploredFeasibleCustomerSets;
        PartitionedCustomerSetList exploredInfeasibleCustomerSets;


        public override void AddSpecializedParameters()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Randomized Customer Set Explorer via Tree Search";
        }

        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }

        public override bool setListener(IListener listener)
        {
            //TODO: Add later
            System.Windows.Forms.MessageBox.Show("RandomizedCustomerSetExplorerViaTreeSearch.setListener called for some reason; this is not ready yet!");
            throw new NotImplementedException();
        }

        public override void SpecializedConclude()
        {
            System.Windows.Forms.MessageBox.Show("RandomizedCustomerSetExplorerViaTreeSearch algorithm run successfully, now it's time to conclude by reporting some statistics!");
            throw new NotImplementedException();
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //            xCplexParam = new XCPlexParameters();
            theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV);
            theEV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);

            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_MIN_NUM_INFEASIBLE_CUSTOMER_SETS, "Minimum # of infeasible customer sets", 50));
            //algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_BEAM_WIDTH, "Beam width", 1));
            popStrategy = CustomerSetList.CustomerListPopStrategy.Random;

            exploredFeasibleCustomerSets = new PartitionedCustomerSetList();
            exploredInfeasibleCustomerSets = new PartitionedCustomerSetList();

            unexploredCustomerSets = new PartitionedCustomerSetList(popStrategy);
            PopulateAndPlaceInitialUnexploredCustomerSets();
        }
        void PopulateAndPlaceInitialUnexploredCustomerSets()
        {
            List<string> allCustomers = theProblemModel.SRD.GetCustomerIDs();
            int nCustomers = allCustomers.Count;
            foreach (string customerID in allCustomers)
            {
                CustomerSet candidate = new CustomerSet(customerID, allCustomers);//The customer set is not TSP-optimized

                if ((exploredFeasibleCustomerSets.ContainsAnIdenticalCustomerSet(candidate)) || (exploredInfeasibleCustomerSets.ContainsAnIdenticalCustomerSet(candidate)))
                    continue;

                OptimizeAndEvaluateForLists(candidate);
            }
        }
        void OptimizeAndEvaluateForLists(CustomerSet cs)
        {
            if ((exploredFeasibleCustomerSets.ContainsAnIdenticalCustomerSet(cs)) || (exploredInfeasibleCustomerSets.ContainsAnIdenticalCustomerSet(cs)))
                    return;

            cs.Optimize(theProblemModel, theGDV);//TODO: This is necessary only because the current infrastructure assumes GDV-optimization must precede EV-Optimization
            cs.Optimize(theProblemModel, theEV, vsroo_GDV:cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV), requireGDVSolutionBeforeEV: false);
            bool feasible = (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).Status == VehicleSpecificRouteOptimizationStatus.Optimized);
            bool infeasible = (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).Status == VehicleSpecificRouteOptimizationStatus.Infeasible);
            bool largeEnough = (cs.NumberOfCustomers >= 3);
            if (largeEnough)
            {
                if (feasible)
                {
                    exploredFeasibleCustomerSets.Add(cs);
                }
                else if (infeasible)
                {
                    exploredInfeasibleCustomerSets.Add(cs);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("We should have shown the customer set feasible or infeasible!");
                }
            }
            if (feasible)
                unexploredCustomerSets.Add(cs);
        }


        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedRun()
        {
            int minNumInfeasibles = algorithmParameters.GetParameter(ParameterID.ALG_MIN_NUM_INFEASIBLE_CUSTOMER_SETS).GetIntValue();
            int beamWidth = 1;// algorithmParameters.GetParameter(ParameterID.ALG_BEAM_WIDTH).GetIntValue();
            int currentLevel;
            while (exploredInfeasibleCustomerSets.TotalCount< minNumInfeasibles)
            {
                currentLevel = unexploredCustomerSets.GetHighestNonemptyLevel();
                CustomerSet theParent = unexploredCustomerSets.Pop(currentLevel, beamWidth)[0];

                foreach (string customerID in theParent.PossibleOtherCustomers)
                {
                    CustomerSet candidate = new CustomerSet(theParent);
                    candidate.Extend(customerID);
                    theParent.MakeCustomerImpossible(customerID);
                    OptimizeAndEvaluateForLists(candidate);
                }//foreach (string customerID in remainingCustomers)

            }//while(exploredInfeasibleCustomerSets.TotalCount< minNumInfeasibles)
        }
    }
}
