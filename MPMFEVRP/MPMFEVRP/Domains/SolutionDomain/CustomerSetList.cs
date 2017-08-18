using System;
using System.Collections.Generic;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class CustomerSetList : List<CustomerSet>
    {
        public enum CustomerListPopStrategy { First, MinOFVforMostAdvancedVehicle, MaxOFVforMostAdvancedVehicle, MinOFVforAnyVehicle, MaxOFVforAnyVehicle, Random };

        bool popStrategyDefined;
        CustomerListPopStrategy popStrategy;

        public CustomerSetList()
        {
            popStrategyDefined = false;
        }

        public CustomerSetList(CustomerListPopStrategy popStrategy)
        {
            popStrategyDefined = true;
            this.popStrategy = popStrategy;
        }

        public CustomerSetList(CustomerSetList twinCSList, bool deepCopy)
        {
            popStrategyDefined = twinCSList.popStrategyDefined;
            if (popStrategyDefined)
                popStrategy = twinCSList.popStrategy;
            foreach (CustomerSet twinCS in twinCSList)
            {
                if (deepCopy)
                    Add(new CustomerSet(twinCS));
                else
                    Add(twinCS);
            }
        }

        public bool Includes(CustomerSet candidate)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return true;
            return false;
        }

        public RouteOptimizationOutcome Retrieve(CustomerSet candidate)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return this[i].RouteOptimizationOutcome;
            return null;
        }

        public CustomerSetList Pop(int numberToPop)
        {
            if (popStrategyDefined)
                return Pop(numberToPop, popStrategy);
            else
                return Pop(numberToPop, CustomerListPopStrategy.First);//Using the default strategy
        }
        public CustomerSetList Pop(int numberToPop, CustomerListPopStrategy strategy)
        {
            CustomerSetList outcome;
            if (numberToPop >= this.Count)
            {
                outcome = new CustomerSetList(this, false);//TODO: Verify whether shallow copy works or deep copy is needed
                Clear();
            }
            else
            {
                outcome = new CustomerSetList();
                for (int i = 0; i < numberToPop; i++)
                    outcome.Add(Pop(strategy));
            }
            return outcome;
        }
        public CustomerSet Pop()
        {
            if (popStrategyDefined)
                return Pop(popStrategy);
            else
                return Pop(CustomerListPopStrategy.First);//Using the default strategy
        }
        public CustomerSet Pop(CustomerListPopStrategy strategy)
        {
            CustomerSet outcome = null;
            if (Count >= 0)
            {
                int resultIndex;
                switch (strategy)
                {
                    case CustomerListPopStrategy.Random:
                        resultIndex = new Random(DateTime.Now.Ticks.GetHashCode()).Next(Count);
                        break;
                    case CustomerListPopStrategy.MinOFVforMostAdvancedVehicle:
                        resultIndex = GetIndexOfBestOFVforMostAdvancedVehicleCustomerSet(false);
                        break;
                    case CustomerListPopStrategy.MaxOFVforMostAdvancedVehicle:
                        resultIndex = GetIndexOfBestOFVforMostAdvancedVehicleCustomerSet(true);
                        break;
                    case CustomerListPopStrategy.MinOFVforAnyVehicle:
                        resultIndex = GetIndexOfBestOFVforAnyVehicleCustomerSet(false);
                        break;
                    case CustomerListPopStrategy.MaxOFVforAnyVehicle:
                        resultIndex = GetIndexOfBestOFVforAnyVehicleCustomerSet(true);
                        break;
                    default:
                        resultIndex = 0;
                        break;
                }
                outcome = this[resultIndex];
                RemoveAt(resultIndex);
            }
            return outcome;
        }
        int GetIndexOfBestOFVforMostAdvancedVehicleCustomerSet(bool maximizationTypeOF = false)
        {
            int outcome = -1;
            //TODO: Reengineer this method entirely based on the simplified structure of the OF and the data package needed for it
            //double objSignAdjustor = (maximizationTypeOF) ? -1.0 : 1.0;
            //double[] minOFV = new double[] { double.MaxValue, double.MaxValue };
            //int indexOfKeyOFV = 1;

            //for (int i = 0; i < Count; i++)
            //{
            //    if (this[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.NotYetOptimized ||
            //        this[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
            //        continue;
            //    if (this[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV && (indexOfKeyOFV == 0))
            //        continue;

            //    if (this[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV && (indexOfKeyOFV == 1))
            //        indexOfKeyOFV = 0;

            //    if (minOFV[indexOfKeyOFV] > objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[indexOfKeyOFV])
            //    {
            //        outcome = i;
            //        minOFV[indexOfKeyOFV] = objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[indexOfKeyOFV];
            //    }
            //}

            return outcome;
        }
        int GetIndexOfBestOFVforAnyVehicleCustomerSet(bool maximizationTypeOF = false)
        {
            int outcome = -1;

            //TODO: Reengineer this method entirely based on the simplified structure of the OF and the data package needed for it
            //double objSignAdjustor = (maximizationTypeOF) ? -1.0 : 1.0;
            //double minOFV = double.MaxValue;

            //for (int i = 0; i < Count; i++)
            //{
            //    if (this[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.NotYetOptimized ||
            //        this[i].RouteOptimizationOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
            //        continue;

            //    for (int v = 0; v < 2; v++)
            //        if (this[i].RouteOptimizerOutcome.Feasible[v])
            //            if (minOFV > objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[v])
            //            {
            //                outcome = i;
            //                minOFV = objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[v];
            //            }
            //}

            return outcome;
        }

        //TODO: Either re-fit to the new architecture or eliminate
        //public List<double> GetDeltaProfit()
        //{
        //    List<double> outcome = new List<double>();
        //    foreach (CustomerSet cs in this)
        //        outcome.Add(cs.RouteOptimizerOutcome.OFV[0] - Math.Max(cs.RouteOptimizerOutcome.OFV[1], 0));
        //    return outcome;
        //}

        public bool ContainsAnIdenticalCustomerSet(CustomerSet candidate)
        {
            foreach (CustomerSet cs in this)
                if (cs.IsIdentical(candidate))
                    return true;
            return false;
        }
    }
}
