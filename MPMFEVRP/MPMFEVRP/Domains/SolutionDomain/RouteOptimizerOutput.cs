using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizerOutput
    {
        bool retrievedFromArchive;
        RouteOptimizationStatus status;
        bool[] feasible = new bool[2];
        double[] ofv = new double[2];
        AssignedRoute[] optimizedRoute = new AssignedRoute[2];

        public bool RetrievedFromArchive { get { return retrievedFromArchive; } }
        public RouteOptimizationStatus Status { get { return status; } }
        public bool[] Feasible { get{ return feasible; } }
        public double [] OFV { get{ return ofv; } }
        public AssignedRoute[] OptimizedRoute { get { return optimizedRoute; } }
        
        public RouteOptimizerOutput()
        {
            retrievedFromArchive = false;
            status = RouteOptimizationStatus.NotYetOptimized;
        }


        public RouteOptimizerOutput(bool retrievedFromArchive, RouteOptimizerOutput twinRouteOptimizerOutput)
        {
            if (retrievedFromArchive)
                this.retrievedFromArchive = true;
            else
                this.retrievedFromArchive = twinRouteOptimizerOutput.retrievedFromArchive;
            status = twinRouteOptimizerOutput.Status;
            feasible = (bool[])twinRouteOptimizerOutput.Feasible.Clone();
            ofv = (double[])twinRouteOptimizerOutput.OFV.Clone();
            optimizedRoute = (AssignedRoute[])twinRouteOptimizerOutput.OptimizedRoute.Clone();
        }


        public RouteOptimizerOutput(RouteOptimizationStatus status, double[] ofv = null, AssignedRoute[] optimizedRoute = null)
        {
            retrievedFromArchive = false;
            this.status = status;
            switch (status)
            {
                case RouteOptimizationStatus.NotYetOptimized://This is the default at the beginning, with us having no idea before solving any TSP
                    {
                        break;
                    }
                case RouteOptimizationStatus.InfeasibleForBothGDVandEV://This is when we know the GDV TSP is infeasible
                    {
                        feasible = new bool[] { false, false };
                        break;
                    }
                case RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV://This is not an intermediate stop, but a final result concluding that the CS is useful for only GDVs
                    {
                        feasible = new bool[] { false, true };//This is because we always use index 0 to denote EV, and 1 for GDV
                        if ((ofv == null) || (optimizedRoute == null))
                        {
                            //TODO After making sure this is never incurred, delete this altogether. Or, keep one and delete the other!
                            System.Windows.Forms.MessageBox.Show("RouteOptimizerOutput cannot be created without ofv and optimizedRoute when RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV");
                            throw new Exception("RouteOptimizerOutput cannot be created without ofv and optimizedRoute when RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV");
                        }
                        this.ofv = ofv;
                        this.optimizedRoute = optimizedRoute;
                        break;
                    }
                case RouteOptimizationStatus.OptimizedForBothGDVandEV://This is the ultimately desirable case, but may not be expected for each CS due to EV restrictions
                    {
                        feasible = new bool[] { true, true };//This is because we always use index 0 to denote EV, and 1 for GDV
                        if ((ofv == null) || (optimizedRoute == null))
                        {
                            //TODO After making sure this is never incurred, delete this altogether. Or, keep one and delete the other!
                            System.Windows.Forms.MessageBox.Show("RouteOptimizerOutput cannot be created without ofv and optimizedRoute when RouteOptimizationStatus.OptimizedForBothGDVandEV");
                            throw new Exception("RouteOptimizerOutput cannot be created without ofv and optimizedRoute when RouteOptimizationStatus.OptimizedForBothGDVandEV");
                        }
                        if(optimizedRoute[0] == null)//This is a proactive check mechanism inserted in case GDV optimization is complete but AFV is not
                        {
                            //TODO After making sure this is never incurred, delete this altogether. Or, keep one and delete the other!
                            System.Windows.Forms.MessageBox.Show("RouteOptimizerOutput cannot be created without an optimizedRoute for EV when RouteOptimizationStatus.OptimizedForBothGDVandEV");
                            throw new Exception("RouteOptimizerOutput cannot be created without an optimizedRoute for EV when RouteOptimizationStatus.OptimizedForBothGDVandEV");
                        }
                        this.ofv = ofv;
                        this.optimizedRoute = optimizedRoute;
                        break;
                    }
            }
            //Any final adjustments?
        }
    }

    public class VehicleSpecificRouteOptimizationOutcome
    {
        VehicleCategories vehicleCategory;
        public VehicleCategories VehicleCategory { get { return vehicleCategory; } }

        VehicleSpecificRouteOptimizationStatus status;
        public VehicleSpecificRouteOptimizationStatus Status { get { return status; } }

        double objectiveFunctionValue;//This is for convenience than anything else, this information is easily reproducible based on the details in optimizedRoute, given a method to calculate the objective function value
        public double ObjectiveFunctionValue { get { return objectiveFunctionValue; } }

        AssignedRoute optimizedRoute;
        public AssignedRoute OptimizedRoute { get { return optimizedRoute; } }

        public VehicleSpecificRouteOptimizationOutcome()
        {
            status = VehicleSpecificRouteOptimizationStatus.NotYetOptimized;
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleSpecificRouteOptimizationOutcome twinVSROO)
        {
            vehicleCategory = twinVSROO.vehicleCategory;
            status = twinVSROO.status;
            objectiveFunctionValue = twinVSROO.objectiveFunctionValue;
            optimizedRoute = new AssignedRoute(twinVSROO.optimizedRoute);//A new instance is created here because we may want to extend the route manually in heuristic algorithms 
        }
        public VehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory, VehicleSpecificRouteOptimizationStatus status, double objectiveFunctionValue = 0.0, AssignedRoute optimizedRoute = null)
        {
            this.vehicleCategory = vehicleCategory;
            this.status = status;

            switch (status)
            {
                case VehicleSpecificRouteOptimizationStatus.NotYetOptimized:
                    break;
                case VehicleSpecificRouteOptimizationStatus.Infeasible:
                    break;
                case VehicleSpecificRouteOptimizationStatus.Optimized:
                    this.objectiveFunctionValue = objectiveFunctionValue;
                    this.optimizedRoute = optimizedRoute;
                    break;
            }
        }
    }

    public class RouteOptimizationOutcome//TODO: This class will completely replace RouteOptimizerOutput
    {
        List<VehicleSpecificRouteOptimizationOutcome> theList;
        bool retrievedFromArchive; public bool RetrievedFromArchive { get { return retrievedFromArchive; } }
        RouteOptimizationStatus overallStatus;

        public RouteOptimizationOutcome()
        {
            theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            retrievedFromArchive = false;
            overallStatus = RouteOptimizationStatus.NotYetOptimized;
        }
        public RouteOptimizationOutcome(bool retrievedFromArchive, RouteOptimizationOutcome twinROO)
        {
            if (retrievedFromArchive)
                this.retrievedFromArchive = true;
            else
                this.retrievedFromArchive = twinROO.retrievedFromArchive;
            theList = new List<VehicleSpecificRouteOptimizationOutcome>();
            foreach (VehicleSpecificRouteOptimizationOutcome vsroo in twinROO.theList)
                theList.Add(new VehicleSpecificRouteOptimizationOutcome(vsroo));
            overallStatus = twinROO.overallStatus;
        }
        public RouteOptimizationOutcome(RouteOptimizationStatus status, List<VehicleSpecificRouteOptimizationOutcome> theList = null)
        {
            if((status== RouteOptimizationStatus.NotYetOptimized)||(status== RouteOptimizationStatus.InfeasibleForBothGDVandEV))
            {
                this.theList = new List<VehicleSpecificRouteOptimizationOutcome>();
                retrievedFromArchive = false;
                overallStatus = status;
            }
            else
            {
                this.theList = theList;
                retrievedFromArchive = false;
                overallStatus = status;
            }
        }
        public RouteOptimizationOutcome(List<VehicleSpecificRouteOptimizationOutcome> theList)
        {
            this.theList = theList;
            retrievedFromArchive = false;
            UpdateOverallStatus();
        }

        public VehicleSpecificRouteOptimizationOutcome GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories vehicleCategory)
        {
            return theList.Find(x => x.VehicleCategory == vehicleCategory);
        }

        public VehicleSpecificRouteOptimizationStatus GetRouteOptimizationStatus(VehicleCategories vehicleCategory)
        {
            return theList.Find(x => x.VehicleCategory == vehicleCategory).Status;//TODO Find out what this does when the vehicleCategory in question hasn't been added yet
        }
        public RouteOptimizationStatus GetRouteOptimizationStatus()
        {
            if(overallStatus== RouteOptimizationStatus.NotYetOptimized)
                if(theList!=null)
                    if (theList.Count > 0)
                    {
                        //Need to update overallStatus
                        UpdateOverallStatus();
                    }
            return overallStatus;
        }
        void UpdateOverallStatus()
        {
            VehicleSpecificRouteOptimizationStatus GDVStatus = GetRouteOptimizationStatus(VehicleCategories.GDV);
            VehicleSpecificRouteOptimizationStatus EVStatus = GetRouteOptimizationStatus(VehicleCategories.EV);
            if (GDVStatus == VehicleSpecificRouteOptimizationStatus.Infeasible)
            {
                overallStatus = RouteOptimizationStatus.InfeasibleForBothGDVandEV;
                return;
            }
            if(GDVStatus== VehicleSpecificRouteOptimizationStatus.Optimized)
            {
                if (EVStatus == VehicleSpecificRouteOptimizationStatus.NotYetOptimized)
                    throw new Exception("Why not yet optimized???");
                if (EVStatus == VehicleSpecificRouteOptimizationStatus.Infeasible)
                {
                    overallStatus = RouteOptimizationStatus.OptimizedForGDVButInfeasibleForEV;
                    return;
                }
                //if we're here, EV must be optimized
                overallStatus = RouteOptimizationStatus.OptimizedForBothGDVandEV;
                return;
            }
        }
    }
}
