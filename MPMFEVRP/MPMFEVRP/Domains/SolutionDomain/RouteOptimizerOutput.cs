﻿using System;
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
            if (twinRouteOptimizerOutput == null)
                return;
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


}
