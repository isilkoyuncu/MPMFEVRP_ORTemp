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
        public VehicleSpecificRouteOptimizationStatus GetVehicleSpecificRouteOptimizationStatus(VehicleCategories vehCategory) { return routeOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vehCategory).Status; }

        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } } //TODO: I just moved this from problem model, resolve errors due to this.

        public ObjectiveFunctionInputDataPackage OFIDP { get { return routeOptimizationOutcome.OFIDP; } } 

        public CustomerSet()
        {
            customers = new List<string>();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            retrievedFromArchive = false;
        }
        public CustomerSet(string customerID, bool retrievedFromArchive = false)
        {
            customers = new List<string> { customerID };
            routeOptimizationOutcome = new RouteOptimizationOutcome();
            this.retrievedFromArchive = retrievedFromArchive;
        }
        public CustomerSet(List<string> customerIDs, RouteOptimizationOutcome ROO = null, bool retrievedFromArchive = false)
        {
            customers = customerIDs;//Note that where we gather the list of customers, we must make sure that they are all customers! Since we don't want to pass problemModel here, we have no way of checking here!!!
            customers.Sort();//just in case
            if (ROO != null)
                routeOptimizationOutcome = ROO;
            else
                routeOptimizationOutcome = new RouteOptimizationOutcome();
            this.retrievedFromArchive = retrievedFromArchive;
        }
        public CustomerSet(CustomerSet twinCS, bool copyROO = false)
        {
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
            // TODO unit test to check if this works as intended
            retrievedFromArchive = false;
            if (copyROO)
                routeOptimizationOutcome = new RouteOptimizationOutcome(twinCS.RouteOptimizationOutcome);
            else
                routeOptimizationOutcome = new RouteOptimizationOutcome();
        }
        public CustomerSet(List<string> customers, VehicleSpecificRouteOptimizationStatus vsros = VehicleSpecificRouteOptimizationStatus.NotYetOptimized, VehicleSpecificRoute vehicleSpecificRoute = null)
        {
            this.customers = customers;
            this.customers.Sort();//just in case
            routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { new VehicleSpecificRouteOptimizationOutcome(vehicleSpecificRoute.VehicleCategory, vsros, vehicleSpecificRoute) });
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

        public void ExtendAndOptimize(string customer, EVvsGDV_ProblemModel theProblemModel)//Keep for now but consider deleting after seeing at least one method fully working iwth the new architecture
        {
            if (!customers.Contains(customer))
            {
                customers.Add(customer);
                customers.Sort();
                VehicleSpecificRouteOptimizationOutcome vsroo_GDV = theProblemModel.RouteOptimize(this, theProblemModel.VRD.VehicleArray[1]);
                VehicleSpecificRouteOptimizationOutcome vsroo_EV = new VehicleSpecificRouteOptimizationOutcome(); //TODO test if this will work
                if (vsroo_GDV.Status==VehicleSpecificRouteOptimizationStatus.Infeasible)
                {
                    // Do not try to optimize for EV
                }
                else
                {
                    vsroo_EV = theProblemModel.RouteOptimize(this, theProblemModel.VRD.VehicleArray[0], GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                }
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");
        }

        ////This method should not exist! TODO:Delete
        //public void ExtendAndOptimize(string customer, ProblemModelBase problemModelBase, Vehicle vehicle, VehicleSpecificRouteOptimizationOutcome vsroo_GDV = null)
        //{
        //    if (!customers.Contains(customer))
        //    {
        //        customers.Add(customer);
        //        customers.Sort();//TODO Write a unit test to see if this really works as intended
        //        if(vehicle.Category== VehicleCategories.GDV)
        //        {
        //            VehicleSpecificRouteOptimizationOutcome vsroo_GDV_new = problemModelBase.RouteOptimize(this, vehicle);
        //            routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV_new });
        //        }
        //        else//It's an EV, and the customer set must have been optimized for a GDV beforehand
        //        {
        //            VehicleSpecificRouteOptimizationOutcome vsroo_EV = problemModelBase.RouteOptimize(this, vehicle, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
        //            routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
        //        }

        //    }
        //    else
        //        throw new Exception("Customer is already in the set, know before asking to extend!");
        //}
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

        public void Optimize(EVvsGDV_ProblemModel theProblemModel)
        {
            routeOptimizationOutcome = theProblemModel.RouteOptimize(this);
        }
        public void Optimize(EVvsGDV_ProblemModel theProblemModel, Vehicle vehicle, VehicleSpecificRouteOptimizationOutcome vsroo_GDV = null)
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
                if ((vsroo_GDV == null) || (vsroo_GDV.Status != VehicleSpecificRouteOptimizationStatus.Optimized))
                    throw new Exception("CustomerSet.Optimize invoked to optimize for an EV, without a GDV-optimal route at hand!");
                VehicleSpecificRouteOptimizationOutcome vsroo_EV = theProblemModel.RouteOptimize(this, vehicle, GDVOptimalRoute: vsroo_GDV.VSOptimizedRoute);
                routeOptimizationOutcome = new RouteOptimizationOutcome(new List<VehicleSpecificRouteOptimizationOutcome>() { vsroo_GDV, vsroo_EV });
            }
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
    }
}
