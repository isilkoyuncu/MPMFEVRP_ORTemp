﻿using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using MPMFEVRP.Models;
using MPMFEVRP.Domains.AlgorithmDomain;
using System.Linq;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Domains.SolutionDomain
{
    /// <summary>
    /// CustomerSet is created from customers specified in SRD
    /// More specifically, the string identifying a customer is its ID in SRD
    /// </summary>
    public class CustomerSet
    {
        string customerSetID;   public string CustomerSetID { get { return customerSetID; } }
        List<string> customers = new List<string>();    public List<string> Customers { get { return customers; } }
        public int NumberOfCustomers { get { return (customers == null) ? 0 : customers.Count; } }
        List<string> possibleOtherCustomers = new List<string>();   public List<string> PossibleOtherCustomers { get { return possibleOtherCustomers; } }
        Dictionary<string, double> minAdditionalDistanceForPossibleOtherCustomer = new Dictionary<string, double>();    public Dictionary<string, double> MinAdditionalDistanceForPossibleOtherCustomer { get { return minAdditionalDistanceForPossibleOtherCustomer; } }
        Dictionary<string, double> minAdditionalTimeForPossibleOtherCustomer = new Dictionary<string, double>();        public Dictionary<string, double> MinAdditionalTimeForPossibleOtherCustomer { get { return minAdditionalTimeForPossibleOtherCustomer; } }
        List<string> impossibleOtherCustomers = new List<string>();        public List<string> ImpossibleOtherCustomers { get { return impossibleOtherCustomers; } }
        double[] centerOfGravityCoordinates = new double[2];    public double[] CenterOfGravityCoordinates { get { return centerOfGravityCoordinates; } }


        RouteOptimizationOutcome routeOptimizationOutcome;        public RouteOptimizationOutcome RouteOptimizationOutcome { get { return routeOptimizationOutcome; } set { routeOptimizationOutcome = value; } }
        public VehicleSpecificRouteOptimizationStatus GetVehicleSpecificRouteOptimizationStatus(VehicleCategories vehCategory) { return routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vehCategory).Status; }
        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } }
        public ObjectiveFunctionInputDataPackage OFIDP { get { return routeOptimizationOutcome.OFIDP; } }
        
        double swapInsertCPUtime;

        public CustomerSet()
        {
            customers = new List<string>();
            customerSetID = "";
            possibleOtherCustomers = new List<string>();
            impossibleOtherCustomers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            retrievedFromArchive = false;
            centerOfGravityCoordinates = new double[2] { 999999, 999999 };
        }
        public CustomerSet(string customerID, List<string> allCustomers, bool retrievedFromArchive = false)
        {
            customers = new List<string> { customerID };
            customerSetID = String.Join(",", customers.OrderBy(x => x));
            possibleOtherCustomers = new List<string>();
            foreach (string otherCustomer in allCustomers)
                if (otherCustomer != customerID)
                    possibleOtherCustomers.Add(otherCustomer);
            impossibleOtherCustomers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            this.retrievedFromArchive = retrievedFromArchive;
            centerOfGravityCoordinates = new double[2] { 999999, 999999 };
        }

        public CustomerSet(bool isEmpty, List<string> allCustomers)
        {
            if (isEmpty != true)
                throw new ArgumentException();

            customers = new List<string>();
            customerSetID = "";
            possibleOtherCustomers = new List<string>();
            foreach (string otherCustomer in allCustomers)
                possibleOtherCustomers.Add(otherCustomer);
            impossibleOtherCustomers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            retrievedFromArchive = false;
            centerOfGravityCoordinates = new double[2] { 999999, 999999 };
        }

        public CustomerSet(CustomerSet twinCS, EVvsGDV_ProblemModel theProblemModel = null, bool copyROO = false)
        {
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
            customerSetID = String.Join(",", customers.OrderBy(x => x));

            possibleOtherCustomers = new List<string>();
            foreach (string c in twinCS.PossibleOtherCustomers)
            {
                possibleOtherCustomers.Add(c);
            }
            impossibleOtherCustomers = new List<string>();
            foreach (string c in twinCS.ImpossibleOtherCustomers)
            {
                impossibleOtherCustomers.Add(c);
            }

            retrievedFromArchive = false;
            if (copyROO)
                routeOptimizationOutcome = new RouteOptimizationOutcome(twinCS.RouteOptimizationOutcome);
            else
                routeOptimizationOutcome = new RouteOptimizationOutcome();
            if (theProblemModel != null)
            {
                UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
                IdentifyNewImpossibleOtherCustomers(theProblemModel);
            }
            centerOfGravityCoordinates = new double[2] { twinCS.centerOfGravityCoordinates[0], twinCS.centerOfGravityCoordinates[1] };
        }

        public CustomerSet(CustomerSet twinCS, EVvsGDV_ProblemModel theProblemModel, RouteOptimizationOutcome newROO)
        {
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
            customerSetID = String.Join(",", customers.OrderBy(x => x));

            possibleOtherCustomers = new List<string>();
            foreach (string c in twinCS.PossibleOtherCustomers)
            {
                possibleOtherCustomers.Add(c);
            }
            impossibleOtherCustomers = new List<string>();
            foreach (string c in twinCS.ImpossibleOtherCustomers)
            {
                impossibleOtherCustomers.Add(c);
            }
            routeOptimizationOutcome = new RouteOptimizationOutcome(newROO);

            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
            centerOfGravityCoordinates = new double[2] { twinCS.centerOfGravityCoordinates[0], twinCS.centerOfGravityCoordinates[1] };
        }

        public CustomerSet(List<string> customers, double computationTime = 0.0, VehicleSpecificRouteOptimizationStatus vsros = VehicleSpecificRouteOptimizationStatus.NotYetOptimized, VehicleSpecificRoute vehicleSpecificRoute = null)
        {
            this.customers = customers;
            this.customers.Sort();//just in case
            customerSetID = String.Join(",", customers.OrderBy(x => x));

            possibleOtherCustomers = new List<string>();
            impossibleOtherCustomers = new List<string>();
            VehicleCategories vehicleCategory = (vehicleSpecificRoute == null ? VehicleCategories.GDV : vehicleSpecificRoute.VehicleCategory);
            VehicleSpecificRouteOptimizationOutcome vsroo = new VehicleSpecificRouteOptimizationOutcome(vehicleCategory, computationTime, vsros, vehicleSpecificRoute);
            routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo });
            retrievedFromArchive = false;
            centerOfGravityCoordinates = new double[2] { 999999, 999999 };
        }

        public bool IsIdentical(CustomerSet otherCS)
        {
            if (customers.Count != otherCS.NumberOfCustomers)
                return false;
            for (int i = 0; i < customers.Count; i++)
                if (customers[i] != otherCS.Customers[i])
                    return false;//This works because the customers list is ordered
            //If no difference is found, we'll have to return true, concluding that the two sets are identical
            return true;
        }

        public void ExtendAndOptimize(string customer, EVvsGDV_ProblemModel theProblemModel)//Keep for now but consider deleting after seeing at least one method fully working with the new architecture
        {
            if (!customers.Contains(customer))
            {
                if (!impossibleOtherCustomers.Contains(customer))
                {
                    if (possibleOtherCustomers.Count == 0)
                    {
                        PopulatePossibleOtherCustomers(theProblemModel.SRD.GetCustomerIDs());
                    }
                    possibleOtherCustomers.Remove(customer);
                    customers.Add(customer);
                    customers.Sort();
                    customerSetID = String.Join(",", customers.OrderBy(x => x));

                    VehicleSpecificRouteOptimizationOutcome vsroo_GDV = theProblemModel.RouteOptimize(this, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV));
                    VehicleSpecificRouteOptimizationOutcome vsroo_EV = new VehicleSpecificRouteOptimizationOutcome(); //TODO test if this will work
                    if (vsroo_GDV.Status == VehicleSpecificRouteOptimizationStatus.Infeasible)
                    {
                        // Do not try to optimize for EV
                        vsroo_EV = new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Infeasible);
                    }
                    else
                    {
                        vsroo_EV = theProblemModel.RouteOptimize(this, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV), GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                    }
                    routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
                    UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
                    IdentifyNewImpossibleOtherCustomers(theProblemModel);
                }
                else
                    throw new Exception("Customer was placed in the impossible list before, know before asking to extend!");
            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");

        }
       
        public void Optimize(EVvsGDV_ProblemModel theProblemModel, Vehicle vehicle, VehicleSpecificRouteOptimizationOutcome vsroo_GDV = null, bool requireGDVSolutionBeforeEV = true)
        {
            //This method makes heavy use of the problem model
            //A closer look therein reveals that the intelligence of checking whether GDV-optimal route is EV-feasible and if the customer set is definitely EV-infeasible are invoked
            //Since those more-detailed methods use very deep info from problem model, it sounds reasonable, but we may also migrate them over to the customer set class in the future, upon assuring that all information tehy need are actually publicly given out by the problem model

            if (vehicle.Category == VehicleCategories.GDV)
            {
                if ((vsroo_GDV != null) && (vsroo_GDV.Status != VehicleSpecificRouteOptimizationStatus.NotYetOptimized))
                    throw new Exception("CustomerSet.Optimize seems to be invoked to optimize for a GDV, with a GDV-optimal route already at hand!");
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV_new = theProblemModel.RouteOptimize(this, vehicle);
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV_new });
            }
            else//It's an EV, and the customer set must have been optimized for a GDV beforehand
            {
                VehicleSpecificRouteOptimizationOutcome vsroo_EV;
                if (requireGDVSolutionBeforeEV)
                {
                    if ((vsroo_GDV == null) || (vsroo_GDV.Status != VehicleSpecificRouteOptimizationStatus.Optimized))
                        throw new Exception("CustomerSet.Optimize invoked to optimize for an EV, without a GDV-optimal route at hand!");
                    vsroo_EV = theProblemModel.RouteOptimize(this, vehicle, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                }
                else
                {
                    vsroo_EV = theProblemModel.RouteOptimize(this, vehicle);
                }
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
            }
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void Extend(string customer)
        {
            possibleOtherCustomers.Remove(customer);
            customers.Add(customer);
            customers.Sort();
            customerSetID = String.Join(",", customers.OrderBy(x => x));
            centerOfGravityCoordinates = new double[2] { 0.0, 0.0 };

            routeOptimizationOutcome = new RouteOptimizationOutcome();
        }
        public void Optimize(EVvsGDV_ProblemModel theProblemModel)
        {
            routeOptimizationOutcome = theProblemModel.NewRouteOptimize(this);
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void OptimizeByExploitingGDVs(EVvsGDV_ProblemModel theProblemModel, Exploiting_GDVs_Flowchart flowchart, bool preserveCustomerVisitSequence, bool feasibleAFVSolnIsEnough=false, bool performSwap=false)
        {
            routeOptimizationOutcome = theProblemModel.RouteOptimizeByExploitingGDVs(this, flowchart, preserveCustomerVisitSequence, feasibleAFVSolnIsEnough, performSwap);
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void OptimizeByPlainAFVSolver(EVvsGDV_ProblemModel theProblemModel, Exploiting_GDVs_Flowchart flowchart)
        {
            routeOptimizationOutcome = theProblemModel.RouteOptimizeByPlainAFVSolver(this, flowchart);
            UpdateMinAdditionalsForAllPossibleOtherCustomerswPlainAFV(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void NewOptimize(EVvsGDV_ProblemModel theProblemModel, Vehicle vehicle, VehicleSpecificRouteOptimizationOutcome vsroo_GDV = null, bool requireGDVSolutionBeforeEV = true)
        {
            //This method makes heavy use of the problem model
            //A closer look therein reveals that the intelligence of checking whether GDV-optimal route is EV-feasible and if the customer set is definitely EV-infeasible are invoked
            //Since those more-detailed methods use very deep info from problem model, it sounds reasonable, but we may also migrate them over to the customer set class in the future, upon assuring that all information tehy need are actually publicly given out by the problem model

            if (vehicle.Category == VehicleCategories.GDV)
            {
                if ((vsroo_GDV != null) && (vsroo_GDV.Status != VehicleSpecificRouteOptimizationStatus.NotYetOptimized))
                    throw new Exception("CustomerSet.Optimize seems to be invoked to optimize for a GDV, with a GDV-optimal route already at hand!");
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV_new = theProblemModel.NewRouteOptimize(this, vehicle);
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV_new });
            }
            else//It's an EV, and the customer set must have been optimized for a GDV beforehand
            {
                VehicleSpecificRouteOptimizationOutcome vsroo_EV;
                if (requireGDVSolutionBeforeEV)
                {
                    if ((vsroo_GDV == null) || (vsroo_GDV.Status != VehicleSpecificRouteOptimizationStatus.Optimized))
                        throw new Exception("CustomerSet.Optimize invoked to optimize for an EV, without a GDV-optimal route at hand!");
                    vsroo_EV = theProblemModel.NewEVRouteOptimize(this, vehicle, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                }
                else
                {
                    vsroo_EV = theProblemModel.NewRouteOptimize(this, vehicle);
                }
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
            }
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void Remove(string customer)
        {
            if (customers.Contains(customer))
            {
                customers.Remove(customer);
                customerSetID = String.Join(",", customers.OrderBy(x => x));

            }

            else
                throw new Exception("Customer is not in the set, cannot remove!");
        }
        public void RemoveAt(int position)
        {
            if (customers.Count > position)
            {
                customers.RemoveAt(position);
                customerSetID = String.Join(",", customers.OrderBy(x => x));

            }

            else
                throw new Exception("Cannot remove customer at a position that doesn't exist!");
        }

        public static void NewRandomSwap(CustomerSet CS1, CustomerSet CS2, Random rand, CustomerSetList exploredCustomerSetMasterList, EVvsGDV_ProblemModel theProblemModel)
        {
            string c1, c2;
            int i = rand.Next(CS1.customers.Count);
            int j = rand.Next(CS2.customers.Count);
            c1 = CS1.customers[i];
            c2 = CS2.customers[j];
            CS1.RemoveAt(i);
            CS2.RemoveAt(j);
            CS2.Extend(c1);
            if (!exploredCustomerSetMasterList.Includes(CS2))
            {
                CS2.Optimize(theProblemModel);
                exploredCustomerSetMasterList.Add(CS2);
            }
            CS1.Extend(c2);
            if (!exploredCustomerSetMasterList.Includes(CS1))
            {
                CS1.Optimize(theProblemModel);
                exploredCustomerSetMasterList.Add(CS1);
            }
        }

        public List<CustomerSet> SwapAndESInsert(int position, EVvsGDV_ProblemModel theProblemModel)
        {
            List<CustomerSet> output = new List<CustomerSet>();
            VehicleSpecificRouteOptimizationOutcome vsroo_GDV = routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV);
            VehicleSpecificRouteOptimizationOutcome vsroo_AFV_swapAndES = new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, 0.0, VehicleSpecificRouteOptimizationStatus.Infeasible); ;
            List<VehicleSpecificRoute> vsr_AFV_list = GetVehicleSpecificRouteBySwapAdjacentAndInsertES(position, theProblemModel);
            foreach (VehicleSpecificRoute vsr_AFV in vsr_AFV_list)
                if (vsr_AFV.Feasible == true)
                {
                    vsroo_AFV_swapAndES = new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV, swapInsertCPUtime, VehicleSpecificRouteOptimizationStatus.Optimized, vsOptimizedRoute: vsr_AFV);
                    output.Add(new CustomerSet(this, theProblemModel, new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_AFV_swapAndES })));
                }
            return output;
        }

        List<VehicleSpecificRoute> GetVehicleSpecificRouteBySwapAdjacentAndInsertES(int position, EVvsGDV_ProblemModel theProblemModel)
        {
            List<VehicleSpecificRoute> vsr_AFV = new List<VehicleSpecificRoute>();
            List<string> nonDepotSites = new List<string>();
            RefuelingPathList refuelingPaths = new RefuelingPathList();
            DateTime startTime = DateTime.Now;
            nonDepotSites = routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.ListOfVisitedCustomerSiteIDs;
            string cs = nonDepotSites[position];
            nonDepotSites.RemoveAt(position);
            nonDepotSites.Insert(position + 1, cs);
            string origin = cs;
            string destination;
            if (nonDepotSites.Count <= position + 2)
                destination = theProblemModel.SRD.GetSingleDepotID();
            else
                destination = nonDepotSites[position + 2];
            foreach(RefuelingPath rp in theProblemModel.NonDominatedRefuelingPaths)
            {
                if (rp.Origin.ID == origin && rp.Destination.ID == destination && rp.GetRefuelingStopIDs().Count>0)
                {
                    nonDepotSites.InsertRange(position + 2, rp.GetRefuelingStopIDs());
                    vsr_AFV.Add(new VehicleSpecificRoute(theProblemModel, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV), nonDepotSites));
                }
            }            
            DateTime endTime = DateTime.Now;
            swapInsertCPUtime = (endTime - startTime).TotalSeconds;
            return vsr_AFV;
        }

        public static void Swap(CustomerSet CS1, int position1, CustomerSet CS2, int position2, EVvsGDV_ProblemModel theProblemModel)
        {
            string c1, c2;
            c1 = CS1.customers[position1];
            c2 = CS2.customers[position2];
            CS1.RemoveAt(position1);
            CS2.RemoveAt(position2);
            CS2.ExtendAndOptimize(c1, theProblemModel);
            CS1.ExtendAndOptimize(c2, theProblemModel);
        }
        public static void Swap(CustomerSet CS1, string customer1, CustomerSet CS2, string customer2, EVvsGDV_ProblemModel theProblemModel)
        {
            CS1.Remove(customer1);
            CS2.Remove(customer2);

            CS2.ExtendAndOptimize(customer1, theProblemModel);
            CS1.ExtendAndOptimize(customer2, theProblemModel);
        }

        public bool Contains(string customerID)
        {
            for (int i = 0; i < Customers.Count; i++)
                if (Customers[i] == customerID)
                    return true;
            return false;
        }

        void PopulatePossibleOtherCustomers(List<string> allCustomers)
        {
            possibleOtherCustomers.Clear();
            foreach (string customer in allCustomers)
            {
                if (customers.Contains(customer))
                    continue;
                if (impossibleOtherCustomers.Contains(customer))
                    continue;
                possibleOtherCustomers.Add(customer);
            }
        }
        public void IdentifyNewImpossibleOtherCustomers(EVvsGDV_ProblemModel theProblemModel)
        {
            //This method assumes that the customer set has already been optimized
            //This method focuses only on GDV feasibility
            if (routeOptimizationOutcome.Status == RouteOptimizationStatus.NotYetOptimized)
                return;
            if (routeOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                return;
            double totalTime = routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetTotalTime();
            double totalDistance = routeOptimizationOutcome.OFIDP.GetVMT(VehicleCategories.GDV);
            for (int i = possibleOtherCustomers.Count - 1; i >= 0; i--)
            {
                string otherCustomer = possibleOtherCustomers[i];
                if (theProblemModel.SRD.GetSiteByID(otherCustomer).ServiceDuration > theProblemModel.CRD.TMax - totalTime)
                {
                    MakeCustomerImpossible(otherCustomer);
                    continue;
                }
                if (minAdditionalTimeForPossibleOtherCustomer[otherCustomer] > theProblemModel.CRD.TMax - totalTime)
                {
                    MakeCustomerImpossible(otherCustomer);
                    continue;
                }
            }
        }
        public void UpdateMinAdditionalsForAllPossibleOtherCustomers(EVvsGDV_ProblemModel theProblemModel)
        {
            minAdditionalDistanceForPossibleOtherCustomer.Clear();
            minAdditionalTimeForPossibleOtherCustomer.Clear();
            if (routeOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                return;
            double maxDistance = routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).VSOptimizedRoute.GetLongestArcLength();
            foreach (string otherCustomer in possibleOtherCustomers)
            {
                List<double> arcLengthsToCustomerSet = new List<double>() { Math.Min(theProblemModel.SRD.GetDistance(theProblemModel.SRD.GetSingleDepotID(), otherCustomer), theProblemModel.SRD.GetDistance(otherCustomer, theProblemModel.SRD.GetSingleDepotID())) };
                foreach (string customer in customers)
                    arcLengthsToCustomerSet.Add(Math.Min(theProblemModel.SRD.GetDistance(otherCustomer, customer), theProblemModel.SRD.GetDistance(customer, otherCustomer)));
                arcLengthsToCustomerSet.Sort();
                double minAddlDist = Math.Max(0, arcLengthsToCustomerSet[0] + arcLengthsToCustomerSet[1] - maxDistance);
                minAdditionalDistanceForPossibleOtherCustomer.Add(otherCustomer, minAddlDist);
                minAdditionalTimeForPossibleOtherCustomer.Add(otherCustomer, minAddlDist / theProblemModel.CRD.TravelSpeed + theProblemModel.SRD.GetSiteByID(otherCustomer).ServiceDuration);
            }
        }

        public void UpdateMinAdditionalsForAllPossibleOtherCustomerswPlainAFV(EVvsGDV_ProblemModel theProblemModel)
        {
            minAdditionalDistanceForPossibleOtherCustomer.Clear();
            minAdditionalTimeForPossibleOtherCustomer.Clear();
            if (routeOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                return;
            double maxDistance = routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetLongestArcLength();
            foreach (string otherCustomer in possibleOtherCustomers)
            {
                List<double> arcLengthsToCustomerSet = new List<double>() { Math.Min(theProblemModel.SRD.GetDistance(theProblemModel.SRD.GetSingleDepotID(), otherCustomer), theProblemModel.SRD.GetDistance(otherCustomer, theProblemModel.SRD.GetSingleDepotID())) };
                foreach (string customer in customers)
                    arcLengthsToCustomerSet.Add(Math.Min(theProblemModel.SRD.GetDistance(otherCustomer, customer), theProblemModel.SRD.GetDistance(customer, otherCustomer)));
                arcLengthsToCustomerSet.Sort();
                double minAddlDist = Math.Max(0, arcLengthsToCustomerSet[0] + arcLengthsToCustomerSet[1] - maxDistance);
                minAdditionalDistanceForPossibleOtherCustomer.Add(otherCustomer, minAddlDist);
                minAdditionalTimeForPossibleOtherCustomer.Add(otherCustomer, minAddlDist / theProblemModel.CRD.TravelSpeed + theProblemModel.SRD.GetSiteByID(otherCustomer).ServiceDuration);
            }
        }

        public void MakeCustomerImpossible(string otherCustomer)
        {
            if (possibleOtherCustomers.Contains(otherCustomer))
            {
                possibleOtherCustomers.Remove(otherCustomer);
                minAdditionalDistanceForPossibleOtherCustomer.Remove(otherCustomer);
                minAdditionalTimeForPossibleOtherCustomer.Remove(otherCustomer);
                impossibleOtherCustomers.Add(otherCustomer);
            }
            else throw new Exception("CustomerSet. called to make impossible a customer that wasn't possible!");
        }

        public double GetVMT(VehicleCategories vc)
        {
            return routeOptimizationOutcome.OFIDP.GetVMT(vc);
        }
        public double GetLongestArc(VehicleCategories vc = VehicleCategories.EV)
        {
            return routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vc).VSOptimizedRoute.GetLongestArcLength();
        }
        public double GetTwoLongestArcs(VehicleCategories vc = VehicleCategories.EV)
        {
            return routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vc).VSOptimizedRoute.GetLongestTwoArcsLength();
        }
        public double GetShortestTwoArcsToCSfromCustomerID (string customerID, EVvsGDV_ProblemModel theProblemModel)
        {
            List<double> arcLengthsToCustomerSet = new List<double>() { Math.Min(theProblemModel.SRD.GetDistance(theProblemModel.SRD.GetSingleDepotID(), customerID), theProblemModel.SRD.GetDistance(customerID, theProblemModel.SRD.GetSingleDepotID())) };
            foreach (string customer in customers)
                arcLengthsToCustomerSet.Add(Math.Min(theProblemModel.SRD.GetDistance(customerID, customer), theProblemModel.SRD.GetDistance(customer, customerID)));
            arcLengthsToCustomerSet.Sort();
            return Math.Max(0, arcLengthsToCustomerSet[0] + arcLengthsToCustomerSet[1]);
        }
        public double[] GetShortestThreeArcsToCSfromCustomerID(string customerID, EVvsGDV_ProblemModel theProblemModel)
        {
            double[] outcome = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            List<double> arcLengthsToCustomerSet = new List<double>() { Math.Min(theProblemModel.SRD.GetDistance(theProblemModel.SRD.GetSingleDepotID(), customerID), theProblemModel.SRD.GetDistance(customerID, theProblemModel.SRD.GetSingleDepotID())) };
            foreach (string customer in customers)
                arcLengthsToCustomerSet.Add(Math.Min(theProblemModel.SRD.GetDistance(customerID, customer), theProblemModel.SRD.GetDistance(customer, customerID)));
            arcLengthsToCustomerSet.Sort();
            for (int i = 0; i < Math.Min(3, arcLengthsToCustomerSet.Count); i++)
                outcome[i] = Math.Max(0, arcLengthsToCustomerSet[i]);
            return outcome;
        }
    
        public void CalculateCenterOfGravity(EVvsGDV_ProblemModel theProblemModel) //Run this method after extending a customer set.
        {
            centerOfGravityCoordinates = theProblemModel.CalculateTheCenterOfGravity(this);
        }
    }

}