using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;

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

        RouteOptimizationOutcome routeOptimizationOutcome;
        public RouteOptimizationOutcome RouteOptimizationOutcome { get { return routeOptimizationOutcome; } set { routeOptimizationOutcome = value; } }
        public bool IsGDVFeasible { get { return routeOptimizationOutcome.IsFeasible(VehicleCategories.GDV); } } // TODO change this to Get feasibility status(veh category)

        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } } //TODO: I just moved this from problem model, resolve errors due to this.

        public ObjectiveFunctionInputDataPackage OFDP { get { return routeOptimizationOutcome.OFDP; } } //TODO this error will be resolved when we add ofdp field in route optimization outcome.

        public CustomerSet()
        {
            customers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            //ofdp = new ObjectiveFunctionInputDataPackage();   //TODO should we create an instance with empty constructor here
        }
        public CustomerSet(string customerID, ProblemModelBase problemModelBase)
        {
            customers = new List<string>();
            if (problemModelBase.GetAllCustomerIDs().Contains(customerID))
            {
                customers.Add(customerID);
                routeOptimizationOutcome = problemModelBase.OptimizeForSingleVehicle(this);
                ofdp = new ObjectiveFunctionInputDataPackage(NumberOfCustomers, NumberOfCustomers, routeOptimizationOutcome.GetPrizeCollected(VehicleCategories.EV), routeOptimizationOutcome.GetPrizeCollected(VehicleCategories.GDV), 1, 1, routeOptimizationOutcome.GetMilesTraveled(VehicleCategories.EV), routeOptimizationOutcome.GetMilesTraveled(VehicleCategories.GDV));
            }
            else
            {
                routeOptimizationOutcome = new RouteOptimizationOutcome();
                //ofdp = new ObjectiveFunctionInputDataPackage();
            }
        }
        public CustomerSet(string customerID)
        {
            customers = new List<string> { customerID };
            routeOptimizationOutcome = new RouteOptimizationOutcome();
        }
        public CustomerSet(CustomerSet twinCS)
        {
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
            // TODO unit test to check if this works as intended
            retrievedFromArchive = false;
            routeOptimizationOutcome = new RouteOptimizationOutcome(twinCS.RouteOptimizationOutcome);
            FillOFDP();
        }
        public CustomerSet(ProblemModelBase problemModel, List<string> customers, VehicleSpecificRouteOptimizationStatus vsros = VehicleSpecificRouteOptimizationStatus.NotYetOptimized, Domains.ProblemDomain.Vehicle vehicle = null)
        {
            if (vehicle == null)
                vehicle = problemModel.VRD.VehicleArray[1];//Setting the vehicle to the GDV in the problem model unless otherwise specified
            if (vehicle.Category == ProblemDomain.VehicleCategories.EV)
                throw new Exception("The capability to recreate an EV-optimized customer set from file is not yet implemented!");
            this.customers = customers;
            switch (vsros)
            {
                case VehicleSpecificRouteOptimizationStatus.Optimized:
                    VehicleSpecificRoute vsr = new VehicleSpecificRoute(problemModel,vehicle,customers);
                    if (!vsr.Feasible)
                        throw new Exception("Reconstructed AssignedRoute is infeasible!");
                    VehicleSpecificRouteOptimizationOutcome vsroo = new VehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV, vsros, vsOptimizedRoute: vsr);//TODO: Streamline objective function value calculation in VehicleSpecificRouteOptimizationOutcome
                    routeOptimizationOutcome = new RouteOptimizationOutcome(RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV, new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo });
                    break;
                case VehicleSpecificRouteOptimizationStatus.NotYetOptimized:
                    routeOptimizationOutcome = new RouteOptimizationOutcome(RouteOptimizationStatus.NotYetOptimized);
                    break;
                case VehicleSpecificRouteOptimizationStatus.Infeasible:
                    routeOptimizationOutcome = new RouteOptimizationOutcome(RouteOptimizationStatus.InfeasibleForBothGDVandEV);
                    break;
            }
            this.customers.Sort();
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

        public void ExtendAndOptimize(string customer, ProblemModelBase problemModelBase)
        {
            if (!customers.Contains(customer))
            {
                customers.Add(customer);
                customers.Sort();//TODO Write a unit test to see if this really works as intended
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV = problemModelBase.OptimizeRoute(this, problemModelBase.VRD.VehicleArray[1]);
                VehicleSpecificRouteOptimizationOutcome vsroo_EV = new VehicleSpecificRouteOptimizationOutcome(); //TODO test if this will work
                if (vsroo_GDV.Status==VehicleSpecificRouteOptimizationStatus.Infeasible)
                {
                    // Do not try to optimize for EV
                }
                else
                {
                    vsroo_EV = problemModelBase.OptimizeRoute(this, problemModelBase.VRD.VehicleArray[0], GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                }
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
                FillOFDP();
            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");
        }
        public void ExtendAndOptimize(string customer, ProblemModelBase problemModelBase, Vehicle vehicle, VehicleSpecificRouteOptimizationOutcome vsroo_GDV = null)
        {
            if (!customers.Contains(customer))
            {
                customers.Add(customer);
                customers.Sort();//TODO Write a unit test to see if this really works as intended
                if(vehicle.Category== VehicleCategories.GDV)
                {
                    VehicleSpecificRouteOptimizationOutcome vsroo_GDV_new = problemModelBase.OptimizeRoute(this, vehicle);
                    routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV_new });
                    FillOFDP(vehicle.Category);
                }
                else//It's an EV, and the customer set must have been optimized for a GDV beforehand
                {
                    VehicleSpecificRouteOptimizationOutcome vsroo_EV = problemModelBase.OptimizeRoute(this, vehicle, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                    routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
                    FillOFDP();
                }

            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");
        }
        public void Extend(string customer)
        {
            if (!customers.Contains(customer))
            {
                customers.Add(customer);
                customers.Sort();
                routeOptimizationOutcome = new RouteOptimizationOutcome();
            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");
        }
        public void Optimize(ProblemModelBase problemModelBase, Vehicle vehicle, VehicleSpecificRouteOptimizationOutcome vsroo_GDV = null)
        {
            if (vehicle.Category == VehicleCategories.GDV)
            {
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV_new = problemModelBase.OptimizeRoute(this, vehicle);
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV_new });
                FillOFDP(vehicle.Category);
            }
            else//It's an EV, and the customer set must have been optimized for a GDV beforehand
            {
                VehicleSpecificRouteOptimizationOutcome vsroo_EV = problemModelBase.OptimizeRoute(this, vehicle, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
                FillOFDP();
            }
        }
        void FillOFDP()
        {
            ofdp = new ObjectiveFunctionInputDataPackage(NumberOfCustomers,
                                                         NumberOfCustomers,
                                                         routeOptimizationOutcome.GetPrizeCollected(VehicleCategories.EV),
                                                         routeOptimizationOutcome.GetPrizeCollected(VehicleCategories.GDV),
                                                         1, 1,
                                                         routeOptimizationOutcome.GetMilesTraveled(VehicleCategories.EV),
                                                         routeOptimizationOutcome.GetMilesTraveled(VehicleCategories.GDV));
        }

        void FillOFDP(VehicleCategories vehCategory)
        {
            if(vehCategory==VehicleCategories.GDV)
                ofdp = new ObjectiveFunctionInputDataPackage(VehicleCategories.GDV, 
                                                             NumberOfCustomers, 
                                                             routeOptimizationOutcome.GetPrizeCollected(VehicleCategories.GDV), 
                                                             1, 
                                                             routeOptimizationOutcome.GetMilesTraveled(VehicleCategories.GDV));
            else
                ofdp = new ObjectiveFunctionInputDataPackage(VehicleCategories.EV, 
                                                             NumberOfCustomers, 
                                                             routeOptimizationOutcome.GetPrizeCollected(VehicleCategories.EV), 
                                                             1, 
                                                             routeOptimizationOutcome.GetMilesTraveled(VehicleCategories.EV));
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

        public static void Swap(CustomerSet CS1, int position1, CustomerSet CS2, int position2, ProblemModelBase problemModelBase)
        {
            string c1, c2;
            c1 = CS1.customers[position1];
            c2 = CS2.customers[position2];
            CS1.RemoveAt(position1);
            CS2.RemoveAt(position2);
            CS2.ExtendAndOptimize(c1, problemModelBase);
            CS1.ExtendAndOptimize(c2, problemModelBase);
        }
        public static void Swap(CustomerSet CS1, string customer1, CustomerSet CS2, string customer2, ProblemModelBase problemModelBase)
        {
            CS1.Remove(customer1);
            CS2.Remove(customer2);

            CS2.ExtendAndOptimize(customer1, problemModelBase);
            CS1.ExtendAndOptimize(customer2, problemModelBase);
        }
        
        public bool Contains(string customerID)
        {
            for (int i = 0; i < Customers.Count; i++)
                if (Customers[i] == customerID)
                    return true;
            return false;
        }

        public string Encode()
        {
            string outcome = "";
            if (customers != null)
                if (customers.Count > 0)
                    outcome = customers[0];
            for (int i = 1; i < customers.Count; i++)
            {
                outcome += " " + customers[i];
            }
            return outcome;
        }
    }
}
