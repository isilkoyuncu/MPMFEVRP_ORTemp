using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class RouteOptimizerOutput
    {
        bool feasible;
        double ofv;
        List<int> route = new List<int>();
        int vehicleIDnumber;

        public RouteOptimizerOutput() { }
        public RouteOptimizerOutput(bool feasible, double ofv, List<int> route, int vehicleIDnumber)
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

        public bool GetFeasible(int vehicleIDnumber)
        {
            return feasible;
        }
        public double GetOFV(int vehicleIDnumber)
        {
            return ofv;
        }
        public List<int> GetRoute(int vehicleIDnumber)
        {
            return route;
        }
    }
}
