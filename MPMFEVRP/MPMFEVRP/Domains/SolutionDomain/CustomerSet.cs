using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;


namespace MPMFEVRP.Domains.SolutionDomain
{
    /// <summary>
    /// CustomerSet is created from customers specified in SRD
    /// More specifically, the string identifying a customer is its ID in SRD
    /// </summary>
    public class CustomerSet
    {
        //int assignedVehicle = -1; public int AssignedVehicle {get{return assignedVehicle;}set { assignedVehicle = value; } } // TODO check if this belongs to here: -1:not assigned, 0:EV, 1:GDV

        List<string> customers = new List<string>();//TODO Should this really be a string? Ordering is an issue (1, 10, 2, .., 9)
        public List<string> Customers { get { return customers; } }
        public int NumberOfCustomers { get { return (customers == null) ? 0 : customers.Count; } }

        RouteOptimizerOutput routeOptimizerOutcome;
        public RouteOptimizerOutput RouteOptimizerOutcome { get { return routeOptimizerOutcome; } set { routeOptimizerOutcome = value; } }

        RouteOptimizationOutcome routeOptimizationOutcome;//TODO Delete RouteOptimizerOutput and use this instead
        public RouteOptimizationOutcome RouteOptimizationOutcome { get { return routeOptimizationOutcome; } }
        public bool IsGDVFeasible { get { return routeOptimizerOutcome.Feasible[1]; } }
        public double GDVMilesTraveled { get { if (routeOptimizerOutcome.Feasible[1]) return routeOptimizerOutcome.OptimizedRoute[1].TotalDistance; else return 0.0; } }
        public bool IsAFVFeasible { get { return routeOptimizerOutcome.Feasible[0]; } }
        public double AFVMilesTraveled { get { if (routeOptimizerOutcome.Feasible[0]) return routeOptimizerOutcome.OptimizedRoute[0].TotalDistance; else return 0.0; } }

        public CustomerSet()
        {
            customers = new List<string>();
            routeOptimizerOutcome = new RouteOptimizerOutput();
            routeOptimizationOutcome = new RouteOptimizationOutcome();
        }
        public CustomerSet(string customerID, ProblemModelBase problemModelBase)
        {
            customers = new List<string>();
            if (problemModelBase.GetAllCustomerIDs().Contains(customerID))
            {
                customers.Add(customerID);
                routeOptimizerOutcome = problemModelBase.OptimizeForSingleVehicle(this);
                //routeOptimizationOutcome = problemModelBase.OptimizeForSingleVehicle(this);TODO Implement this instead of the line right before this
            }
            else
            {
                routeOptimizerOutcome = new RouteOptimizerOutput();
                routeOptimizationOutcome = new RouteOptimizationOutcome();
            }
        }
        public CustomerSet(string customerID)
        {
            customers = new List<string>();
                customers.Add(customerID);
                routeOptimizerOutcome = new RouteOptimizerOutput();
                routeOptimizationOutcome = new RouteOptimizationOutcome();
        }
        public CustomerSet(CustomerSet twinCS)
        {
            //assignedVehicle = twinCS.AssignedVehicle;
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
            // TODO check if this works as intended
            routeOptimizerOutcome = new RouteOptimizerOutput(false, twinCS.RouteOptimizerOutcome);
            routeOptimizationOutcome = new RouteOptimizationOutcome(false, twinCS.RouteOptimizationOutcome);
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

        public void Extend(string customer, ProblemModelBase problemModelBase)
        {
            if (!customers.Contains(customer))
            {
                customers.Add(customer);
                customers.Sort();//TODO Write a unit test to see if this really works as intended
                routeOptimizerOutcome = problemModelBase.OptimizeForSingleVehicle(this);
            }
            else
                throw new Exception("Customer is already in the set, know before asking to extend!");
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
            CS2.Extend(c1, problemModelBase);
            CS1.Extend(c2, problemModelBase);
        }
        public static void Swap(CustomerSet CS1, string customer1, CustomerSet CS2, string customer2, ProblemModelBase problemModelBase)
        {
            CS1.Remove(customer1);
            CS2.Remove(customer2);

            CS2.Extend(customer1, problemModelBase);
            CS1.Extend(customer2, problemModelBase);
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
