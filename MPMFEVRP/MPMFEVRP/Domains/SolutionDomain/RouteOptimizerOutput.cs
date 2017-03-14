using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizerOutput
    {
        RouteOptimizationStatus status;
        bool[] feasible = new bool[2];
        double[] ofv = new double[2];
        Route[] optimizedRoute = new Route[2];
        public RouteOptimizationStatus Status { get { return status; } }
        public bool[] Feasible { get{ return feasible; } }
        public double [] OFV { get{ return ofv; } }
        public Route[] OptimizedRoute { get { return optimizedRoute; } }

        public RouteOptimizerOutput()
        {
            status = RouteOptimizationStatus.NotYetOptimized;
        }
        public RouteOptimizerOutput(bool[] feasible, double[] ofv)
        {
            this.feasible = feasible;
            this.ofv = ofv;
        }
    }
}
