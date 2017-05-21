using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class AssignedRoute
    {
        /* Each route starts at the depot (0) and ends at the depot (0)
         * */

        // Input related fields
        IProblemModel problemModel;
        int vehicleCategory;
        public int VehicleCategory { get { return vehicleCategory; } }
        // Output related fields
        List<int> sitesVisited;
        public List<int> SitesVisited { get { return sitesVisited; } }

        double totalDistance;
        public double TotalDistance { get { return totalDistance; } }

        double totalCollectedPrize;
        public double TotalCollectedPrize { get { return totalCollectedPrize; } }

        double totalFixedCost;
        public double TotalFixedCost { get { return totalFixedCost; } }

        double totalVariableTravelCost;
        public double TotalVariableTravelCost { get { return totalVariableTravelCost; } }

        double totalProfit;
        public double TotalProfit { get { return totalProfit; } }

        List<double> arrivalTime;
        public List<double> ArrivalTime { get { return arrivalTime; } }

        List<double> departureTime;
        public List<double> DepartureTime { get { return departureTime; } }

        List<double> arrivalSOC;
        public List<double> ArrivalSOC { get { return arrivalSOC; } }


        List<double> departureSOC;
        public List<double> DepartureSOC { get { return departureSOC; } }

        List<bool> feasible;//[numVehicleCategories]
        public List<bool> Feasible { get { return feasible; } }

        // Intermediate steps and validity
        public int LastVisitedSite { get { return sitesVisited.Last(); } }
        public bool Complete { get { return sitesVisited.Last() == 0; } }

        public AssignedRoute(){} //An empty constructor

        public AssignedRoute(IProblemModel problemModel, int vehicleCategory)
        {
            this.problemModel = problemModel;
            this.vehicleCategory = vehicleCategory;
            sitesVisited = new List<int>(); sitesVisited.Add(0);//The depot is the only visited site, this is how we get the route going
            totalDistance = 0.0;
            totalCollectedPrize = 0.0;
            totalFixedCost = problemModel.VRD.VehicleArray[vehicleCategory].FixedCost;
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
            totalDistance += problemModel.SRD.Distance[lastSite, nextSite];
            totalCollectedPrize += problemModel.SRD.SiteArray[nextSite].Prize[vehicleCategory];
            totalVariableTravelCost += problemModel.SRD.Distance[lastSite, nextSite] * problemModel.VRD.VehicleArray[vehicleCategory].VariableCostPerMile;
            totalProfit += problemModel.SRD.SiteArray[nextSite].Prize[vehicleCategory] - problemModel.SRD.Distance[lastSite, nextSite] * problemModel.VRD.VehicleArray[vehicleCategory].VariableCostPerMile;
            double nextArrivalTime;
            double nextDepartureTime;
            double nextArrivalSOC;
            double nextDepartureSOC;
            bool nextFeasible;

            nextArrivalTime = departureTime.Last() + problemModel.SRD.TimeConsumption[lastSite, nextSite];
            nextArrivalSOC = departureSOC.Last() - problemModel.SRD.EnergyConsumption[lastSite, nextSite, vehicleCategory];
            switch (problemModel.SRD.SiteArray[nextSite].SiteType)
            {
                case SiteTypes.Depot:
                    nextDepartureTime = nextArrivalTime;
                    nextDepartureSOC = nextArrivalSOC;
                    break;
                case SiteTypes.Customer:
                    nextDepartureTime = nextArrivalTime + problemModel.SRD.SiteArray[nextSite].ServiceDuration;
                    nextDepartureSOC = Math.Min(1.0, nextArrivalSOC + problemModel.SRD.SiteArray[nextSite].RechargingRate * problemModel.SRD.SiteArray[nextSite].ServiceDuration);
                    break;
                case SiteTypes.ExternalStation:
                    nextDepartureTime = nextArrivalTime + ((1.0 - nextArrivalSOC) / problemModel.SRD.SiteArray[nextSite].RechargingRate);
                    nextDepartureSOC = 1.0;
                    break;
                default:
                    throw new Exception("Not all cases of SiteType accounted for in Route.Extend!");
            }//switch (fromProblem.SiteArray[nextSite].SiteType)
            nextFeasible = feasible.Last();
            if (nextFeasible)
            {
                if ((nextArrivalSOC < -1.0 * ProblemConstants.ERROR_TOLERANCE) || (nextDepartureTime > problemModel.CRD.TMax + ProblemConstants.ERROR_TOLERANCE))
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
