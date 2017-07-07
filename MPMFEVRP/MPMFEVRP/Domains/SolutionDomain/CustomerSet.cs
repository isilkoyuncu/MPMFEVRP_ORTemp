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
        int assignedVehicle = -1; public int AssignedVehicle {get{return assignedVehicle;}set { assignedVehicle = value; } } // TODO check if this belongs to here: -1:not assigned, 0:EV, 1:GDV

        List<string> customers = new List<string>();//TODO Should this really be a string? Ordering is an issue (1, 10, 2, .., 9)
        public List<string> Customers { get { return customers; } }
        public int NumberOfCustomers { get { return (customers == null) ? 0 : customers.Count; } }

        RouteOptimizerOutput routeOptimizerOutcome;
        public RouteOptimizerOutput RouteOptimizerOutcome { get { return routeOptimizerOutcome; } set { routeOptimizerOutcome = value; } }

        public CustomerSet() { customers = new List<string>(); }
        public CustomerSet(string customerID, ProblemModelBase problemModelBase) { customers = new List<string>(); customers.Add(customerID); routeOptimizerOutcome = problemModelBase.OptimizeForSingleVehicle(this); }
        public CustomerSet(CustomerSet twinCS)
        {
            assignedVehicle = twinCS.AssignedVehicle;
            customers = new List<string>();
            foreach (string c in twinCS.Customers)
            {
                customers.Add(c);
            }
            // TODO check if this works as intended
            routeOptimizerOutcome = new RouteOptimizerOutput(twinCS.RouteOptimizerOutcome);
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

    }
}
