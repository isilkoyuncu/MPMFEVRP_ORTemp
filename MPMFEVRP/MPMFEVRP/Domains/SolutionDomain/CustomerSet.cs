using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using MPMFEVRP.Models;


namespace MPMFEVRP.Domains.SolutionDomain
{
    /// <summary>
    /// CustomerSet is created from customers specified in SRD
    /// More specifically, the string identifying a customer is its ID in SRD
    /// </summary>
    public class CustomerSet
    {
        List<string> customers = new List<string>();
        public List<string> Customers { get { return customers; } }
        public int NumberOfCustomers { get { return (customers == null) ? 0 : customers.Count; } }
        List<string> possibleOtherCustomers = new List<string>();
        public List<string> PossibleOtherCustomers { get { return possibleOtherCustomers; } }
        Dictionary<string, double> minAdditionalDistanceForPossibleOtherCustomer = new Dictionary<string, double>();
        public Dictionary<string, double> MinAdditionalDistanceForPossibleOtherCustomer { get { return minAdditionalDistanceForPossibleOtherCustomer; } }
        Dictionary<string, double> minAdditionalTimeForPossibleOtherCustomer = new Dictionary<string, double>();
        public Dictionary<string, double> MinAdditionalTimeForPossibleOtherCustomer { get { return minAdditionalTimeForPossibleOtherCustomer; } }
        List<string> impossibleOtherCustomers = new List<string>();
        public List<string> ImpossibleOtherCustomers { get { return impossibleOtherCustomers; } }

        RouteOptimizationOutcome routeOptimizationOutcome;
        public RouteOptimizationOutcome RouteOptimizationOutcome { get { return routeOptimizationOutcome; } set { routeOptimizationOutcome = value; } }
        public VehicleSpecificRouteOptimizationStatus GetVehicleSpecificRouteOptimizationStatus(VehicleCategories vehCategory) { return routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vehCategory).Status; }

        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } }

        public ObjectiveFunctionInputDataPackage OFIDP { get { return routeOptimizationOutcome.OFIDP; } }

        public CustomerSet()
        {
            customers = new List<string>();
            possibleOtherCustomers = new List<string>();
            impossibleOtherCustomers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            retrievedFromArchive = false;
        }
        public CustomerSet(string customerID, List<string> allCustomers, bool retrievedFromArchive = false)
        {
            customers = new List<string> { customerID };
            possibleOtherCustomers = new List<string>();
            foreach (string otherCustomer in allCustomers)
                if (otherCustomer != customerID)
                    possibleOtherCustomers.Add(otherCustomer);
            impossibleOtherCustomers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            this.retrievedFromArchive = retrievedFromArchive;
        }

        public CustomerSet(bool isEmpty, List<string> allCustomers)
        {
            if (isEmpty != true)
                throw new ArgumentException();

            customers = new List<string>();
            possibleOtherCustomers = new List<string>();
            foreach (string otherCustomer in allCustomers)
                possibleOtherCustomers.Add(otherCustomer);
            impossibleOtherCustomers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            retrievedFromArchive = false;
        }

        public CustomerSet(CustomerSet twinCS, EVvsGDV_ProblemModel theProblemModel = null, bool copyROO = false)
        {
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
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
        }
        public CustomerSet(List<string> customers, double computationTime = 0.0, VehicleSpecificRouteOptimizationStatus vsros = VehicleSpecificRouteOptimizationStatus.NotYetOptimized, VehicleSpecificRoute vehicleSpecificRoute = null)
        {
            this.customers = customers;
            this.customers.Sort();//just in case
            possibleOtherCustomers = new List<string>();
            impossibleOtherCustomers = new List<string>();
            VehicleCategories vehicleCategory = (vehicleSpecificRoute == null ? VehicleCategories.GDV : vehicleSpecificRoute.VehicleCategory);
            VehicleSpecificRouteOptimizationOutcome vsroo = new VehicleSpecificRouteOptimizationOutcome(vehicleCategory, computationTime, vsros, vehicleSpecificRoute);
            routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo });
            retrievedFromArchive = false;
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

        public void Extend(string customer)
        {
            if (!customers.Contains(customer))
            {
                if (!impossibleOtherCustomers.Contains(customer))
                {
                    possibleOtherCustomers.Remove(customer);
                    customers.Add(customer);
                    customers.Sort();
                    routeOptimizationOutcome = new RouteOptimizationOutcome();
                }
                else
                    throw new Exception("Customer was placed in the impossible list before, know before asking to extend!");
            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");
        }

        public void Optimize(EVvsGDV_ProblemModel theProblemModel)
        {
            routeOptimizationOutcome = theProblemModel.RouteOptimize(this);
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
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

        public void NewExtend(string customer)
        {
            possibleOtherCustomers.Remove(customer);
            customers.Add(customer);
            customers.Sort();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
        }
        public void NewOptimize(EVvsGDV_ProblemModel theProblemModel)
        {
            routeOptimizationOutcome = theProblemModel.NewRouteOptimize(this);
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void OptimizeByExploitingGDVs(EVvsGDV_ProblemModel theProblemModel, bool preserveCustomerVisitSequence)
        {
            routeOptimizationOutcome = theProblemModel.RouteOptimizeByExploitingGDVs(this, preserveCustomerVisitSequence);
            UpdateMinAdditionalsForAllPossibleOtherCustomers(theProblemModel);
            IdentifyNewImpossibleOtherCustomers(theProblemModel);
        }

        public void OptimizeByPlainAFVSolver(EVvsGDV_ProblemModel theProblemModel)
        {
            routeOptimizationOutcome = theProblemModel.RouteOptimizeByPlainAFVSolver(this);
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
                customers.Remove(customer);
            else
                throw new Exception("Customer is not in the set, cannot remove!");
        }
        public void RemoveAt(int position)
        {
            if (customers.Count > position)
                customers.RemoveAt(position);
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
            CS2.NewExtend(c1);
            if (!exploredCustomerSetMasterList.Includes(CS2))
            {
                CS2.NewOptimize(theProblemModel);
                exploredCustomerSetMasterList.Add(CS2);
            }
            CS1.NewExtend(c2);
            if (!exploredCustomerSetMasterList.Includes(CS1))
            {
                CS1.NewOptimize(theProblemModel);
                exploredCustomerSetMasterList.Add(CS1);
            }
        }
        public void SwapAndESInsert(EVvsGDV_ProblemModel theProblemModel)
        {
            VehicleSpecificRouteOptimizationOutcome vsroo_AFV_swapAndES = theProblemModel.NewRouteOptimize(this);

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

        public double GetReducedCost()
        {
            return routeOptimizationOutcome.OFIDP.GetVMT(VehicleCategories.EV);
        }
        public double GetLongestArc()
        {
            return routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).VSOptimizedRoute.GetLongestArcLength();
        }

        public double GetShortestTwoArcsToCSfromCustomerID (string customerID, EVvsGDV_ProblemModel theProblemModel)
        {
            List<double> arcLengthsToCustomerSet = new List<double>() { Math.Min(theProblemModel.SRD.GetDistance(theProblemModel.SRD.GetSingleDepotID(), customerID), theProblemModel.SRD.GetDistance(customerID, theProblemModel.SRD.GetSingleDepotID())) };
            foreach (string customer in customers)
                arcLengthsToCustomerSet.Add(Math.Min(theProblemModel.SRD.GetDistance(customerID, customer), theProblemModel.SRD.GetDistance(customer, customerID)));
            arcLengthsToCustomerSet.Sort();
            return Math.Max(0, arcLengthsToCustomerSet[0] + arcLengthsToCustomerSet[1]);
        }
    }

}