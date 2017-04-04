using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizerOutput
    {
        RouteOptimizationStatus[] status = new RouteOptimizationStatus[2];
        bool[] feasible = new bool[2];
        double[] ofv = new double[2];
        Route[] optimizedRoute = new Route[2];
        public RouteOptimizationStatus[] Status { get { return status; } }
        public bool[] Feasible { get{ return feasible; } }
        public double [] OFV { get{ return ofv; } }
        public Route[] OptimizedRoute { get { return optimizedRoute; } }
        
        public RouteOptimizerOutput()
        {
            status[0] = RouteOptimizationStatus.NotYetOptimized;
            status[1] = RouteOptimizationStatus.NotYetOptimized;
        }
        public RouteOptimizerOutput(RouteOptimizationStatus[] status, bool[] feasible, double[] ofv)
        {
            this.status = status;
            this.feasible = feasible;
            this.ofv = ofv;
        }
        public void SetFeasible(bool[] feasible)
        {
            if (feasible != null)
                this.feasible = feasible;
        }
        public void SetOFV(double[] ofv)
        {
            if (ofv != null)
                this.ofv = ofv;
        }
        public void SetOptimizedRoute(NewCompleteSolution[] solutions)
        {
            throw new NotImplementedException(); // TODO get routes from NewSolution
        }
    }
}
