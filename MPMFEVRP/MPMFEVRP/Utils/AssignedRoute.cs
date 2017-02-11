using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels;

namespace MPMFEVRP.Utils
{
    public class AssignedRoute
    {
        /* Each route starts at the depot (0) and ends at the depot (0)
         * */

        // Input related fields
        EVvsGDV_MaxProfit_VRP_Model fromProblem;
        int vehicleCategory;

        // Output related fields
        List<int> sitesVisited;
        double totalDistance;
        double totalCollectedPrize;
        double totalFixedCost;
        double totalVariableTravelCost;
        double totalProfit;
        List<double> arrivalTime;
        List<double> departureTime;
        List<double> arrivalSOC;
        List<double> departureSOC;
        List<bool> feasible;//[numVehicleCategories]

        // Intermediate steps and validity
        public int LastVisitedSite { get { return sitesVisited.Last(); } }
        public bool Complete { get { return sitesVisited.Last() == 0; } }

        public AssignedRoute(){} //An empty constructor

        public AssignedRoute(EVvsGDV_MaxProfit_VRP_Model fromProblem, int vehicleCategory)
        {
            this.fromProblem = fromProblem;
            this.vehicleCategory = vehicleCategory;
            sitesVisited = new List<int>(); sitesVisited.Add(0);//The depot is the only visited site, this is how we get the route going
            totalDistance = 0.0;
            totalCollectedPrize = 0.0;
            totalFixedCost = fromProblem.VehicleArray[vehicleCategory].FixedCost;
            totalVariableTravelCost = 0.0;
            totalProfit = - totalFixedCost;
            arrivalTime = new List<double>(); arrivalTime.Add(0.0);//Starting at the depot at time 0
            departureTime = new List<double>(); departureTime.Add(0.0);//Starting at the depot at time 0
            arrivalSOC = new List<double>(); arrivalSOC.Add(1.0);//Starting at the depot with full charge
            departureSOC = new List<double>(); departureSOC.Add(1.0);//Starting at the depot with full charge
            feasible = new List<bool>(); feasible.Add(true);//Starting at the depot with full charge
        }
        public void Extend(int nextSite)
        {
            int lastSite = sitesVisited.Last();
            sitesVisited.Add(nextSite);
            totalDistance += fromProblem.Distance[lastSite, nextSite];
            totalCollectedPrize += fromProblem.SiteArray[nextSite].Prize[vehicleCategory];
            totalVariableTravelCost += fromProblem.Distance[lastSite, nextSite] * fromProblem.VehicleArray[vehicleCategory].VariableCostPerMile;
            totalProfit += fromProblem.SiteArray[nextSite].Prize[vehicleCategory] - fromProblem.Distance[lastSite, nextSite] * fromProblem.VehicleArray[vehicleCategory].VariableCostPerMile;
            double nextArrivalTime;
            double nextDepartureTime;
            double nextArrivalSOC;
            double nextDepartureSOC;
            bool nextFeasible;

            nextArrivalTime = departureTime.Last() + fromProblem.TimeConsumption[lastSite, nextSite];
            nextArrivalSOC = departureSOC.Last() - fromProblem.EnergyConsumption[lastSite, nextSite, vehicleCategory];
            switch (fromProblem.SiteArray[nextSite].SiteType)
            {
                case SiteTypes.Depot:
                    nextDepartureTime = nextArrivalTime;
                    nextDepartureSOC = nextArrivalSOC;
                    break;
                case SiteTypes.Customer:
                    nextDepartureTime = nextArrivalTime + fromProblem.SiteArray[nextSite].ServiceDuration;
                    nextDepartureSOC = Math.Min(1.0, nextArrivalSOC + fromProblem.SiteArray[nextSite].RechargingRate * fromProblem.SiteArray[nextSite].ServiceDuration);
                    break;
                case SiteTypes.ExternalStation:
                    nextDepartureTime = nextArrivalTime + ((1.0 - nextArrivalSOC) / fromProblem.SiteArray[nextSite].RechargingRate);
                    nextDepartureSOC = 1.0;
                    break;
                default:
                    throw new Exception("Not all cases of SiteType accounted for in Route.Extend!");
            }//switch (fromProblem.SiteArray[nextSite].SiteType)
            nextFeasible = feasible.Last();
            if (nextFeasible)
            {
                if ((nextArrivalSOC < -1.0 * ProblemConstants.ERROR_TOLERANCE) || (nextDepartureTime > fromProblem.TMax + ProblemConstants.ERROR_TOLERANCE))
                    nextFeasible = false;
            }//if (nextFeasible[vc])

            arrivalTime.Add(nextArrivalTime);
            departureTime.Add(nextDepartureTime);
            arrivalSOC.Add(nextArrivalSOC);
            departureSOC.Add(nextDepartureSOC);
            feasible.Add(nextFeasible);
        }
    }
}
