using System;
using System.Collections.Generic;
using MPMFEVRP.Domains.ProblemDomain;

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

        public bool ContainsCS(string CSID)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].CustomerSetID == CSID)
                    return true;
            return false;
        }
        CustomerSet GetCustomerSet (string customerSetID)
        {
            for(int i=0; i<this.Count; i++)
                if (this[i].CustomerSetID == customerSetID)
                    return this[i];
            return null;
        }

        public RouteOptimizationOutcome Retrieve(CustomerSet candidate)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return this[i].RouteOptimizationOutcome;
            return null;
        }

        public VehicleSpecificRouteOptimizationOutcome Retrieve_vsroo(CustomerSet candidate, Vehicle vehicle)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return this[i].RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(vehicle.Category);
            return null;
        }

        public CustomerSet Retrieve_CS(CustomerSet candidate)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return this[i];
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
        public CustomerSetList Pop(int numberToPop, VehicleCategories vehicleCategory, Dictionary<string,double> shadowPrices)
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
                    outcome.Add(Pop(vehicleCategory, shadowPrices));
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
        public CustomerSet Pop(VehicleCategories vehicleCategory, Dictionary<string, double> shadowPrices)
        {
            CustomerSet outcome = null;
            if (Count >= 0)
            {
                int resultIndex = GetIndexOfBestNewRow0Estimate(vehicleCategory, shadowPrices);
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


            //Hardcoded to see for now
            double bestVMT = double.MaxValue;
            int besti = -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].OFIDP.GetVMT(ProblemDomain.VehicleCategories.GDV) < bestVMT)
                {
                    bestVMT = this[i].OFIDP.GetVMT(ProblemDomain.VehicleCategories.GDV);
                    besti = i;
                }
            }
            outcome = besti;

            return outcome;
        }
        int GetIndexOfBestNewRow0Estimate(VehicleCategories vehicleCategory, Dictionary<string,double> ShadowPrices)
        {
            double bestValue = double.MaxValue;
            int bestIndex = -1;
            double tempValue = 0.0;
            for (int i = 0; i < Count; i++)
            {
                tempValue = this[i].OFIDP.GetVMT(vehicleCategory) - SumOfCustomerShadowPricesInTheSet(this[i], ShadowPrices);
                if (tempValue < bestValue)
                {
                    bestValue = this[i].OFIDP.GetVMT(ProblemDomain.VehicleCategories.GDV)-SumOfCustomerShadowPricesInTheSet(this[i],ShadowPrices);
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
        double SumOfCustomerShadowPricesInTheSet(CustomerSet theSet, Dictionary<string, double> ShadowPrices)
        {
            double outcome = 0.0;
            foreach (string customer in theSet.Customers)
                outcome += ShadowPrices[customer];
            return outcome;
        }

        public bool ContainsAnIdenticalCustomerSet(CustomerSet candidate)
        {
            foreach (CustomerSet cs in this)
                if (cs.IsIdentical(candidate))
                    return true;
            return false;
        }

    }
}
