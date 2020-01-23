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
        int randomSeed; Random random;
        double runTimeLimitInSeconds = 0.0;
        int beamWidth = 7;
        int maxTreeLevel = 20;
        int randExtension = 3;
        bool resume = false;

        XCPlexParameters XcplexParam;
        CustomerSetList exploredCustomerSetMasterListsAtStart;
        CustomerSetList[] exploredCustomerSetMasterListsAtEachLevel;
        CustomerSetList columnsToSetCover; //Feasible for at least one vehicle
        CustomerSetList exploredSingleCustomerSetList;
        string[] writtenStatistics;

        //Local statistics
        DateTime globalStartTime;
        List<string> allCustomerIDs;
 
        public RouteDatabaseGenerator()
        {
            AddSpecializedParameters();
        }
        public override void AddSpecializedParameters()
        {
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE, "Preserve Customer Visit Sequence", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            AlgorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RANDOM_SEED, "Random Seed", "50"));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RESUME, "Resume", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
        }
        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;
            allCustomerIDs = theProblemModel.GetAllCustomerIDs();

            //Algorithm param
            runTimeLimitInSeconds = AlgorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
            preserveCustomerVisitSequence = AlgorithmParameters.GetParameter(ParameterID.ALG_PRESERVE_CUST_SEQUENCE).GetBoolValue();
            resume = AlgorithmParameters.GetParameter(ParameterID.ALG_RESUME).GetBoolValue();
            randomSeed = AlgorithmParameters.GetParameter(ParameterID.ALG_RANDOM_SEED).GetIntValue();
            random = new Random(randomSeed);
            XcplexParam = new XCPlexParameters();

            exploredCustomerSetMasterListsAtEachLevel = new CustomerSetList[maxTreeLevel];
            exploredSingleCustomerSetList = new CustomerSetList();
            columnsToSetCover = new CustomerSetList();
            if (resume)
            {
                exploredCustomerSetMasterListsAtStart = new CustomerSetList();
                ReadCSsFromData();
            }
        }
        public override void SpecializedRun()
        {
            globalStartTime = DateTime.Now;
            if (!resume)
            {
                InitializeTreeLevel_0();
                exploredCustomerSetMasterListsAtEachLevel[0] = new CustomerSetList(exploredSingleCustomerSetList, false);
            }
            else
            {
                exploredCustomerSetMasterListsAtEachLevel[0] = new CustomerSetList(exploredCustomerSetMasterListsAtStart, false);
            }
            for (int i = 0; i < maxTreeLevel - 1; i++)
            {
                if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds)
                    break;
                ExploreNextLevelBreadthFirst(i);
            }
        }
        
        public override void SpecializedConclude()
        {
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
        public void ReadCSsFromData()
        {
            randExtension = 1;
            beamWidth = 2;
            System.IO.StreamReader sr = new System.IO.StreamReader("C:/Users/ikoyuncu/Desktop/TestReader/EMH_Test_0_resume.txt");
            string wholeFile = sr.ReadToEnd();
            sr.Close();
            string[] allRows = wholeFile.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            for(int i=0; i<allRows.Count(); i++)
            {
                string[] customerSet = allRows[i].Split('\t')[0].Split('-');
                CustomerSet cs = new CustomerSet(customerSet[0], allCustomerIDs);
                for (int c = 1; c < customerSet.Count(); c++)
                    cs.NewExtend(customerSet[c]);
                exploredCustomerSetMasterListsAtStart.Add(cs);
            }
        }

        void InitializeTreeLevel_0()
        {
            for (int i = 0; i < theProblemModel.SRD.NumCustomers; i++)
            {
                CustomerSet singleCustomerCS = new CustomerSet(theProblemModel.GetAllCustomerIDs()[i], theProblemModel.GetAllCustomerIDs());
                singleCustomerCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                exploredSingleCustomerSetList.Add(singleCustomerCS);
            }
        }
        void ExploreNextLevelBreadthFirst(int i)
        {
            exploredCustomerSetMasterListsAtEachLevel[i + 1] = new CustomerSetList();
            for (int j=0; j<exploredCustomerSetMasterListsAtEachLevel[i].Count; j++)
            {
                List<string> customersToBeAdded = new List<string>();
                if (resume == false)
                {
                    if (exploredCustomerSetMasterListsAtEachLevel[i][j].RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                        continue;
                    if (exploredCustomerSetMasterListsAtEachLevel[i][j].GetVehicleSpecificRouteOptimizationStatus(VehicleCategories.GDV) == VehicleSpecificRouteOptimizationStatus.Optimized)
                        customersToBeAdded = SelectCustomersToExtend(exploredCustomerSetMasterListsAtEachLevel[i][j]);
                }
                else
                {
                    customersToBeAdded = SelectCustomersToExtend(exploredCustomerSetMasterListsAtEachLevel[i][j]);
                }
                for(int k=0; k<customersToBeAdded.Count; k++)
                {
                    CustomerSet tempCS = new CustomerSet(exploredCustomerSetMasterListsAtEachLevel[i][j]);
                    tempCS.NewExtend(customersToBeAdded[k]);
                    if (!exploredCustomerSetMasterListsAtEachLevel[i + 1].ContainsAnIdenticalCustomerSet(tempCS))
                    {
                        tempCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                        exploredCustomerSetMasterListsAtEachLevel[i + 1].Add(tempCS);
                    }
                    if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds)
                        break;
                }
                if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds)
                    break;
            }
        }
        void ExploreNextLevelsDepthFirst(int i, int j)
        {
            CustomerSet currentCS = new CustomerSet(exploredCustomerSetMasterListsAtEachLevel[i][j]);

            while (i < maxTreeLevel)
            {
                List<string> customersToBeAdded = SelectCustomersToExtend(currentCS);

                if (exploredCustomerSetMasterListsAtEachLevel[i + 1] == null)
                    exploredCustomerSetMasterListsAtEachLevel[i + 1] = new CustomerSetList();
                if (customersToBeAdded != null && customersToBeAdded.Count != 0)
                {
                    CustomerSet tempCS = new CustomerSet(currentCS);
                    tempCS.NewExtend(customersToBeAdded.First());
                    if (!exploredCustomerSetMasterListsAtEachLevel[i + 1].ContainsAnIdenticalCustomerSet(tempCS))
                    {
                        tempCS.OptimizeByExploitingGDVs(theProblemModel, preserveCustomerVisitSequence);
                        exploredCustomerSetMasterListsAtEachLevel[i + 1].Add(tempCS);
                    }
                    currentCS = new CustomerSet(tempCS);
                    if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds)
                        break;
                }
                else
                    break;
                i++;
                if ((DateTime.Now - globalStartTime).TotalSeconds > runTimeLimitInSeconds)
                    break;
            }
        }
        List<string> SelectCustomersToExtend(CustomerSet currentCS)
        {
            List<string> possibleCustIDs = currentCS.PossibleOtherCustomers;
            List<string> theBestTopXPercent = PopulateTheBestTopXPercentCustomersList(currentCS, possibleCustIDs, 20.0);
            List<string> toReturn = new List<string>();
            for (int k = 0; k < Math.Min(beamWidth, theBestTopXPercent.Count); k++)
                toReturn.Add(theBestTopXPercent[k]);
            if (possibleCustIDs.Count != 0)
                for (int r = 0; r < randExtension; r++)
                {
                    string candidate = possibleCustIDs[random.Next(possibleCustIDs.Count)];
                    if (!toReturn.Contains(candidate))
                        toReturn.Add(candidate);
                }
            return toReturn;
        }
        List<string> PopulateTheBestTopXPercentCustomersList(CustomerSet CS, List<string> visitableCustomers, double closestPercentSelect)
        {
            string[] customers = visitableCustomers.ToArray();
            int nCustomers = customers.Length;
            double[] shortestDistancesToCS = new double[nCustomers];
            for (int i = 0; i < nCustomers; i++)
            {
                shortestDistancesToCS[i] = ShortestDistanceOfCandidateToCurrentCustomerSet(CS, customers[i]);
            }
            Array.Sort(shortestDistancesToCS, customers);
            List<string> outcome = new List<string>();
            int numberToReturn = (int)Math.Ceiling(closestPercentSelect * nCustomers / 100.0);
            for (int i = 0; i < numberToReturn; i++)
            {
                outcome.Add(customers[i]);
            }
            return outcome;
        }
        double ShortestDistanceOfCandidateToCurrentCustomerSet(CustomerSet CS, string candidate)
        {
            if (CS.NumberOfCustomers == 0)
                return theProblemModel.SRD.GetDistance(candidate, theProblemModel.SRD.GetSingleDepotID());
            else
            {
                double outcome = double.MaxValue;
                foreach (string customer in CS.Customers)
                {
                    double distance = theProblemModel.SRD.GetDistance(candidate, customer);
                    if (outcome > distance)
                    {
                        outcome = distance;
                    }
                }
                return outcome;
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
                "Algorithm Name: " + GetName(),
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
            //output.Add("ID#\tObjValue\tNumAFVUsed\tNumGDVUsed\tSolnTime\tIterationNo");
            //for (int i = 0; i < allSolutions.Count; i++)
            //{
            //    CustomerSetBasedSolution solution = allSolutions[i];
            //    output.Add(i.ToString() + "\t" + solution.UpperBound.ToString() + "\t" + solution.NumCS_assigned2EV.ToString() + "\t" + solution.NumCS_assigned2GDV.ToString() + "\t" + incumbentTime[i].ToString());
            //}
            return output.ToArray();
        }
        string[] WriteSolutionStatistics()
        {
            List<string> output = new List<string>();
            output.Add("CustomerSet\tGDVRoute\tGDVVMT\tEVRoute\tEVVMT\tNumCustomers\tNumESVisits\tStatus\tGDVSolnTime\tEVSolnTime");
            for (int i = 0; i < maxTreeLevel; i++)
                if (exploredCustomerSetMasterListsAtEachLevel[i] != null)
                    foreach (CustomerSet cs in exploredCustomerSetMasterListsAtEachLevel[i])
                    {
                        string CS = "";
                        string GDVRoute = "";
                        double GDVVMT = 0.0;
                        string EVRoute = "";
                        double EVVMT = 0.0;
                        int numCustomers = cs.Customers.Count;
                        int numESVisits = 0;
                        RouteOptimizationStatus SolnStatus = cs.RouteOptimizationOutcome.Status;
                        double GDVSolnTime = 0.0;
                        double EVSolnTime = 0.0;

                        
                        CS = String.Join("-", cs.Customers);
                        if (SolnStatus != RouteOptimizationStatus.InfeasibleForBothGDVandEV && SolnStatus != RouteOptimizationStatus.NotYetOptimized)
                        {
                            if (SolnStatus == RouteOptimizationStatus.OptimizedForBothGDVandEV || SolnStatus == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV || SolnStatus == RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV)
                            {
                                VehicleSpecificRoute vsr_GDV = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute;
                                for (int c = 0; c < vsr_GDV.ListOfVisitedNonDepotSiteIDs.Count-1; c++)
                                    GDVRoute = GDVRoute + vsr_GDV.ListOfVisitedNonDepotSiteIDs[c] + "-";
                                GDVRoute = GDVRoute + vsr_GDV.ListOfVisitedNonDepotSiteIDs.Last();
                                GDVVMT = vsr_GDV.GetVehicleMilesTraveled();
                                GDVSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).ComputationTime;
                                if (SolnStatus == RouteOptimizationStatus.OptimizedForBothGDVandEV)
                                {
                                    VehicleSpecificRoute vsr_EV = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute;
                                    for (int c = 0; c < vsr_EV.ListOfVisitedNonDepotSiteIDs.Count-1; c++)
                                        EVRoute = EVRoute + vsr_EV.ListOfVisitedNonDepotSiteIDs[c] + "-";
                                    EVRoute = EVRoute + vsr_EV.ListOfVisitedNonDepotSiteIDs.Last();
                                    EVRoute.Trim('-');
                                    EVVMT = vsr_EV.GetVehicleMilesTraveled();
                                    numESVisits = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs.Count - cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.NumberOfCustomersVisited;
                                    EVSolnTime = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).ComputationTime;
                                }
                            }
                        }
                        output.Add(CS + "\t" + GDVRoute.ToString() + "\t" + GDVVMT.ToString() + "\t" + EVRoute.ToString() + "\t" + EVVMT.ToString() + "\t"+ numCustomers.ToString() + "\t" + numESVisits.ToString() + "\t" + SolnStatus.ToString() + "\t" + GDVSolnTime.ToString() + "\t" + EVSolnTime.ToString());
                    }
            return output.ToArray();
        }
    }
}
