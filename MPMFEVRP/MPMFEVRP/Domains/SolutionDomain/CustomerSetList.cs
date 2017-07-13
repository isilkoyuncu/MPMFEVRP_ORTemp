using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class CustomerSetList : List<CustomerSet>
    {
        public enum CustomerListPopStrategy {First, MinOFVforMostAdvancedVehicle, MaxOFVforMostAdvancedVehicle, MinOFVforAnyVehicle, MaxOFVforAnyVehicle, Random };

        public CustomerSetList() { }

        public bool Includes(CustomerSet candidate)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i].IsIdentical(candidate))
                    return true;
            return false;
        }

        public CustomerSet Pop(CustomerListPopStrategy strategy)
        {
            CustomerSet outcome = null;
            if (Count >= 0)
            {
                var resultIndex = 0; // default is the first one
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
                }
                outcome = this[resultIndex];
                RemoveAt(resultIndex);
            }
            return outcome;
        }
        int GetIndexOfBestOFVforMostAdvancedVehicleCustomerSet(bool maximizationTypeOF = false)
        {
            int outcome = -1;

            double objSignAdjustor = (maximizationTypeOF) ? -1.0 : 1.0;
            double[] minOFV = new double[] { double.MaxValue, double.MaxValue };
            int indexOfKeyOFV = 1;

            for (int i = 0; i < Count; i++)
            {
                if (this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.NotYetOptimized ||
                    this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.WontOptimize_Duplicate ||
                    this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                    continue;
                if (this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV && (indexOfKeyOFV==0))
                    continue;

                if (this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.OptimizedForBothGDVandEV && (indexOfKeyOFV == 1))
                    indexOfKeyOFV = 0;

                if (minOFV[indexOfKeyOFV] > objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[indexOfKeyOFV])
                {
                    outcome = i;
                    minOFV[indexOfKeyOFV] = objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[indexOfKeyOFV];
                }
            }

            return outcome;
        }
        int GetIndexOfBestOFVforAnyVehicleCustomerSet(bool maximizationTypeOF = false)
        {
            int outcome = -1;

            double objSignAdjustor = (maximizationTypeOF) ? -1.0 : 1.0;
            double minOFV = double.MaxValue;

            for (int i = 0; i < Count; i++)
            {
                if (this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.NotYetOptimized ||
                    this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.WontOptimize_Duplicate ||
                    this[i].RouteOptimizerOutcome.Status == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                    continue;

                for (int v = 0; v < 2; v++)
                    if (this[i].RouteOptimizerOutcome.Feasible[v])
                        if (minOFV > objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[v])
                        {
                            outcome = i;
                            minOFV = objSignAdjustor * this[i].RouteOptimizerOutcome.OFV[v];
                        }
            }

            return outcome;
        }
    }
}
