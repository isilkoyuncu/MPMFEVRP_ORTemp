using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Domains.SolutionDomain
{
    /// <summary>
    /// This class is created as a temporary solution to quickly complte the TR-E paper revision. We're aware that it's not the best that we can do.
    /// </summary>
    public class CustomerSetWithVMTs
    {
        CustomerSet customerSet;
        public CustomerSet CustomerSet { get => customerSet; }

        RouteOptimizationStatus status;
        public RouteOptimizationStatus Status { get => status; }

        double vmt_GDV;
        public double VMT_GDV { get => vmt_GDV; }

        double vmt_EV;
        public double VMT_EV { get => vmt_EV; }

        public double GetVMT(VehicleCategories vehicleCategory)
        {
            switch (vehicleCategory)
            {
                case VehicleCategories.EV:
                    return vmt_EV;
                case VehicleCategories.GDV:
                    return vmt_GDV;
                default:
                    throw new Exception("CustomerSetWithVMTs.GetVMT doesn't account for all VehicleCategories!");
            }
        }

        RouteOptimizationStatus[] premature = new RouteOptimizationStatus[] { RouteOptimizationStatus.NotYetOptimized, RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV };
        RouteOptimizationStatus[] requiresPositiveVMT_GDV = new RouteOptimizationStatus[] { RouteOptimizationStatus.OptimizedForBothGDVandEV, RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV, RouteOptimizationStatus.OptimizedForGDVButNotYetOptimizedForEV };
        RouteOptimizationStatus[] requiresPositiveVMT_EV = new RouteOptimizationStatus[] { RouteOptimizationStatus.OptimizedForBothGDVandEV };
        RouteOptimizationStatus[] requiresInfiniteVMT_GDV = new RouteOptimizationStatus[] { RouteOptimizationStatus.InfeasibleForBothGDVandEV };
        RouteOptimizationStatus[] requiresInfiniteVMT_EV = new RouteOptimizationStatus[] { RouteOptimizationStatus.InfeasibleForBothGDVandEV, RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV };

        public CustomerSetWithVMTs(CustomerSet customerSet, RouteOptimizationStatus status, double vmt_GDV = double.MinValue, double vmt_EV = double.MinValue)
        {
            if (premature.Contains(status))
                throw new Exception("CustomerSetsWithVMTs should have not been invoked prematurely, before optimizing for both vehicle types!");
            if (requiresPositiveVMT_GDV.Contains(status) && (vmt_GDV <= 0.0))
                throw new Exception("CustomerSetsWithVMTs should have been invoked with a positive vmt_GDV!");
            if (requiresPositiveVMT_EV.Contains(status) && (vmt_EV <= 0.0))
                throw new Exception("CustomerSetsWithVMTs should have been invoked with a positive vmt_EV!");

            this.customerSet = customerSet;
            this.status = status;

            if (requiresInfiniteVMT_GDV.Contains(status))
                this.vmt_GDV = double.MaxValue;
            else
                this.vmt_GDV = vmt_GDV;
            if (requiresInfiniteVMT_EV.Contains(status))
                this.vmt_EV = double.MaxValue;
            else
                this.vmt_EV = vmt_EV;
        }
    }
}
