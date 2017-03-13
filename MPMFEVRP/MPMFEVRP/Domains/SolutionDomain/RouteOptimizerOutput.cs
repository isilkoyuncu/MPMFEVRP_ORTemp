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
        public RouteOptimizationStatus Status { get { return status; } }

        bool[] feasible =new bool[2];
        double[] ofv = new double[2];
        Route[] optimizedRoute = new Route[2]; //TODO public

        List<int> route = new List<int>();
        int vehicleIDnumber;

        public RouteOptimizerOutput()
        {
            status = RouteOptimizationStatus.NotYetOptimized;
        }
        public RouteOptimizerOutput(bool[] feasible, double[] ofv, List<int> route, int vehicleIDnumber)
        {
            this.feasible = feasible;
            this.ofv = ofv;
            foreach(var x in route)
            {
                this.route.Add(x);
            }
            this.vehicleIDnumber= vehicleIDnumber;
        }
        public RouteOptimizerOutput(List<int> route)
        {
            foreach (var x in route)
            {
                this.route.Add(x);
            }
        }

        public bool[] GetFeasible(int vehicleIDnumber)
        {
            return feasible;
        }
        public double[] GetOFV(int vehicleIDnumber)
        {
            return ofv;
        }
        public List<int> GetRoute(int vehicleIDnumber)
        {
            return route;
        }
    }
}
