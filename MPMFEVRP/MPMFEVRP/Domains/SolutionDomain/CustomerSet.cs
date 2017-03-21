﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class CustomerSet
    {
        List<string> customers = new List<string>();
        public List<string> Customers { get { return customers; } }
        public int NumberOfCustomers { get { return customers.Count; } }

        RouteOptimizerOutput routeOptimizerOutcome;
        public RouteOptimizerOutput RouteOptimizerOutcome { get { return routeOptimizerOutcome;} }

        public CustomerSet() { }
        public CustomerSet(CustomerSet twinCS)
        {
            customers = new List<string>();
            foreach(string c in twinCS.Customers)
            {
                customers.Add(c);
            }
        }

        public void Extend(string customer)
        {
            if (!customers.Contains(customer))
                customers.Add(customer);
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

        public static void Swap(CustomerSet CS1, int position1, CustomerSet CS2, int position2)
        {
            string c1, c2;
            c1 = CS1.customers[position1];
            c2 = CS2.customers[position2];
            CS1.RemoveAt(position1);
            CS2.RemoveAt(position2);
            CS2.Extend(c1);
            CS1.Extend(c2);
        }
        public static void Swap(CustomerSet CS1, string customer1, CustomerSet CS2, string customer2)
        {
            CS1.Remove(customer1);
            CS2.Remove(customer2);

            CS2.Extend(customer1);
            CS1.Extend(customer2);
        }

    }
}