using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class NewCompleteSolution
    {
        List<AssignedRoute> routes;
        public List<AssignedRoute> Routes { get { return routes; } }

        public NewCompleteSolution()
        {

        }
        public NewCompleteSolution(IProblemModel problemModel, List<Tuple<int,int,int>> XSetTo1)
        {
            routes = new List<AssignedRoute>();
            //first determining the number of routes
            List<Tuple<int, int, int>> tobeRemoved = new List<Tuple<int, int, int>>();
            foreach (Tuple<int,int,int> x in XSetTo1)
                if (x.Item1 == 0)
                {
                    routes.Add(new AssignedRoute(problemModel, x.Item3));
                    routes.Last().Extend(x.Item2);
                    tobeRemoved.Add(x);
                }
            foreach (Tuple<int, int, int> x in tobeRemoved)
            {
                XSetTo1.Remove(x);
            }
            tobeRemoved.Clear();
            //Next, completeing the routes one-at-a-time
            int lastSite = -1;
            bool extensionDetected = false;
            foreach (AssignedRoute r in routes)
            {
                while ((!r.Complete) && (XSetTo1.Count > 0))
                {
                    lastSite = r.LastVisitedSite;
                    extensionDetected = false;
                    foreach(Tuple<int, int, int> x in XSetTo1)
                    {
                        if (x.Item1 == lastSite)
                        {
                            r.Extend(x.Item2);
                            XSetTo1.Remove(x);
                            extensionDetected = true;
                            break;
                        }
                    }
                    if (!extensionDetected)
                        throw new Exception("Infeasible complete solution due to an incomplete route!");
                }
            }
            if (XSetTo1.Count > 0)
                throw new Exception("Infeasible complete solution due to subtours or routes that don't start/end at the depot");

        }
    }
}
