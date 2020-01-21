using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;
using MPMFEVRP.Domains.ProblemDomain;
using System.Linq;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class RouteDatabaseGenerator : AlgorithmBase
    {
        //Algorithm parameters
        bool preserveCustomerVisitSequence = false;
        int randomSeed;
        Random random;
        double runTimeLimitInSeconds = 0.0;
        int numberOfRoutesGeneratedAndRecorded = 0;

        TSPSolverType tspSolverType;
        XCPlexParameters XcplexParam;

        CustomerSetList exploredCustomerSetMasterList; //All columns including infeasible ones
        CustomerSetList columnsToSetCover; //Feasible for at least one vehicle
        CustomerSetList exploredSingleCustomerSetList;

        CustomerSetBasedSolution solution = new CustomerSetBasedSolution();
        List<CustomerSetBasedSolution> allSolutions;

        List<double> incumbentTime;
        List<int> iterationNo;
        string[] writtenOutput;
        string[] writtenStatistics;

        //Local statistics
        double totalRunTimeSec = 0.0;
        DateTime globalStartTime;
        DateTime globalFinishTime;

        bool terminate;

        public RouteDatabaseGenerator()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE, "Preserve Customer Visit Sequence", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE, "TSP Type", new List<object>() { TSPSolverType.GDVExploiter, TSPSolverType.PlainAFVSolver, TSPSolverType.OldiesADF }, TSPSolverType.GDVExploiter, UserInputObjectType.ComboBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_NUM_ROUTES_EACH_ITER, "# routes at each iter", "1000"));
        }
        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;

            //Algorithm param
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            preserveCustomerVisitSequence = AlgorithmParameters.GetParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE).GetBoolValue();
            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            tspSolverType = (TSPSolverType)AlgorithmParameters.GetParameter(ParameterID.ALG_TSP_OPTIMIZATION_MODEL_TYPE).Value;
            numberOfRoutesGeneratedAndRecorded = AlgorithmParameters.GetParameter(ParameterID.ALG_NUM_ROUTES_EACH_ITER).GetIntValue();
            XcplexParam = new XCPlexParameters();

            exploredCustomerSetMasterList = new CustomerSetList();
            exploredSingleCustomerSetList = new CustomerSetList();

            //Solution stat
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            allSolutions = new List<CustomerSetBasedSolution>();
            incumbentTime = new List<double>();
            iterationNo = new List<int>();
        }
        public override void SpecializedRun()
        {
            globalStartTime = DateTime.Now;
            
            exploredCustomerSetMasterList.AddRange(exploredSingleCustomerSetList);
            terminate = false;
            int currentTreeLevel = 0;
            CustomerSetList[] unexploredCustomerSetListsAtEachLevel = new CustomerSetList[theProblemModel.SRD.NumCustomers];
            CustomerSetList currentLevel = new CustomerSetList(exploredSingleCustomerSetList, false);
            unexploredCustomerSetListsAtEachLevel[0] = currentLevel;
            CustomerSet currentCS = new CustomerSet();
            while (!terminate)
            {
                currentLevel = new CustomerSetList();
                if (unexploredCustomerSetListsAtEachLevel[currentTreeLevel].Count == 0)
                {
                    currentTreeLevel = 0;
                }
                for (int k = 0; k < 5; k++)
                {
                    if (unexploredCustomerSetListsAtEachLevel[currentTreeLevel].Count > 0)
                    {
                        CustomerSet cs = unexploredCustomerSetListsAtEachLevel[currentTreeLevel][random.Next(unexploredCustomerSetListsAtEachLevel[currentTreeLevel].Count)];

                        string customer2add = "C1";// = negativeReducedCostCustomers.ElementAt(random.Next(negativeReducedCostCustomers.Count)).Key;//random.Next(negativeReducedCostCustomers.Count)).Key;
                            CustomerSet tempExtendedCS = new CustomerSet(cs, theProblemModel, true);
                            tempExtendedCS.NewExtend(customer2add);
                            if (exploredCustomerSetMasterList.Includes(tempExtendedCS))
                                tempExtendedCS = exploredCustomerSetMasterList.Retrieve_CS(tempExtendedCS); //retrieve the info needed from exploredCustomerSetList                       
                            else
                            {
                                tempExtendedCS.OptimizeByExploitingGDVs(theProblemModel, true);
                                exploredCustomerSetMasterList.Add(tempExtendedCS);
                            }
                            
                                if (tempExtendedCS.PossibleOtherCustomers.Count != 0)
                                {
                                    cs = new CustomerSet(tempExtendedCS, theProblemModel, true);
                                    currentLevel.Add(cs);
                                }
                            
                        
                    }
                    else
                        break;
                }
                currentTreeLevel++;
                if (unexploredCustomerSetListsAtEachLevel[currentTreeLevel] == null)
                {
                    unexploredCustomerSetListsAtEachLevel[currentTreeLevel] = new CustomerSetList();
                }
                if (currentLevel.Count > 0)
                {
                    unexploredCustomerSetListsAtEachLevel[currentTreeLevel].AddRange(currentLevel);
                }

                
            }
            globalFinishTime = DateTime.Now;
            totalRunTimeSec = (globalFinishTime - globalStartTime).TotalSeconds;
        }
        public override void SpecializedConclude()
        {
            writtenOutput = WriteIncumbentSolutions();
            string fileName = theProblemModel.InputFileName;
            fileName = fileName.Replace(".txt", "output.txt");
            System.IO.File.WriteAllLines(@fileName, writtenOutput);

            writtenStatistics = WriteSolutionStatistics();
            string fileName2 = theProblemModel.InputFileName;
            fileName2 = fileName2.Replace(".txt", "statistics.txt");
            System.IO.File.WriteAllLines(fileName2, writtenStatistics);

            
            GetOutputSummary();
        }
        public override void SpecializedReset()
        {
           
            GC.Collect();
        }
        

        void InitializeTreeLevel_1()
        {
            for (int i = 0; i < theProblemModel.SRD.NumCustomers; i++)
            {
                CustomerSet singleCustomerCS = new CustomerSet(theProblemModel.GetAllCustomerIDs()[i], theProblemModel.GetAllCustomerIDs());
                singleCustomerCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                exploredSingleCustomerSetList.Add(singleCustomerCS);
            }
        }
        public override string GetName()
        {
            return "Route Database Creator";

        }
        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>{
                 //Algorithm Name has to be the first entry for output file name purposes
                "Algorithm Name: " + GetName()+ "-" +algorithmParameters.GetParameter(ParameterID.ALG_RANDOM_POOL_SIZE).Value.ToString(),
                //Run time limit has to be the second entry for output file name purposes
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
                
                //Optional
                "Parameter: ",
                //algorithmParameters.GetAllParameters();
                //var asString = string.Join(";", algorithmParameters.GetAllParameters());
                //list.Add(asString);
                
                //Necessary statistics
                "CPU Run Time(sec): " + stats.RunTimeMilliSeconds.ToString(),
                "Solution Status: " + status.ToString()
            };
            switch (status)
            {
                case AlgorithmSolutionStatus.NotYetSolved:
                    {
                        break;
                    }
                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
                    {
                        break;
                    }
                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
                    {
                        break;
                    }
                default:
                    {
                        list.Add("UB(Best Int): " + stats.UpperBound.ToString());
                        list.Add("LB(Relaxed): " + stats.LowerBound.ToString());
                        break;
                    }
            }
            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
        }
        public override bool setListener(IListener listener)
        {
            throw new NotImplementedException();
        }
        string[] WriteIncumbentSolutions()
        {
            List<string> output = new List<string>();
            output.Add("ID#\tObjValue\tNumAFVUsed\tNumGDVUsed\tSolnTime\tIterationNo");
            for (int i = 0; i < allSolutions.Count; i++)
            {
                CustomerSetBasedSolution solution = allSolutions[i];
                output.Add(i.ToString() + "\t" + solution.UpperBound.ToString() + "\t" + solution.NumCS_assigned2EV.ToString() + "\t" + solution.NumCS_assigned2GDV.ToString() + "\t" + incumbentTime[i].ToString());
            }
            return output.ToArray();
        }
        string[] WriteSolutionStatistics()
        {
            List<string> output = new List<string>();
            output.Add("ID#\tNumCustomers\tStatus\tObjFncDifference\tAFVSolnTime\tGDVSolnTime\tNumESVisits\tGDVExploited\tAFVSolved\tAFVInfeasibileProved");
            int IDindex = 0;

            foreach (CustomerSet cs in exploredCustomerSetMasterList)
            {
                double objDiff = 9999999;
                double afvSolnTime = 0;
                double gdvSolnTime = 0;
                int numESVisits = 0;
                int numCustomers = cs.Customers.Count;
                bool gdvExploited = false;
                bool afvSolved = true;
                bool afvInfeasibleProved = false;

                if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV)
                {
                    objDiff = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetVehicleMilesTraveled() - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetVehicleMilesTraveled();
                    numESVisits = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs.Count - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.NumberOfCustomersVisited;
                    afvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
                    gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                    if (afvSolnTime == 0)
                    {
                        gdvExploited = true;
                        afvSolved = false;
                    }
                }
                else if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                {
                    if (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV) != null)
                    {
                        gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                    }
                    afvInfeasibleProved = true;
                    gdvExploited = true;
                    afvSolved = false;
                }
                else if (cs.RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV)
                {
                    afvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
                    gdvSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                    if (afvSolnTime == 0)
                    {
                        gdvExploited = true;
                        afvSolved = false;
                        afvInfeasibleProved = true;
                    }
                }
                output.Add(IDindex.ToString() + "\t" + numCustomers.ToString() + "\t" + cs.RouteOptimizationOutcome.Status.ToString() + "\t" + objDiff.ToString() + "\t" + afvSolnTime.ToString() + "\t" + gdvSolnTime.ToString() + "\t" + numESVisits.ToString() + "\t" + gdvExploited.ToString() + "\t" + afvSolved.ToString() + "\t" + afvInfeasibleProved.ToString());
                IDindex++;
            }
            return output.ToArray();
        }
    }
}
