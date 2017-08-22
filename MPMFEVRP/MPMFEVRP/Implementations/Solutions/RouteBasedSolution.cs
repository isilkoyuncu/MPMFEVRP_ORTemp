using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Implementations.Solutions
{
    public class RouteBasedSolution : SolutionBase
    {
        List<VehicleSpecificRoute> routes;
        public List<VehicleSpecificRoute> Routes { get { return routes; } }
        public RouteBasedSolution()
        {
        }
public RouteBasedSolution(List<VehicleSpecificRoute> VSRs)
        {
            routes = VSRs;
        }

        public RouteBasedSolution(EVvsGDV_ProblemModel theProblemModel)
        {
            
        }

        public override string GetName()
        {
            return "Route Based Solution";
        }

        public override ISolution GenerateRandom()
        {
            throw new NotImplementedException();
        }

        public override ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        public override void View(IProblem problem)
        {
            throw new NotImplementedException();
        }

        public override List<ISolution> GetAllChildren()
        {
            throw new NotImplementedException();
        }

        public override void TriggerSpecification()
        {
            throw new NotImplementedException();
        }

        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>
            {
                //"Solution Status: " + status,
                //"UB: " + upperBound,
                //"LB: " + lowerBound,
            };
            if (status == AlgorithmSolutionStatus.Feasible || status == AlgorithmSolutionStatus.Optimal)
            {
                list.Add("Optimality Gap: " + (Math.Abs(upperBound - lowerBound) / (upperBound + 0.0000000000001)));
            }

            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
        }

        public override string[] GetWritableSolution()
        {
            List<string> list = new List<string>
            {
                "Route\tVehicle\tPrizeCollected\tCostIncurred\tProfit\tTime2ReturnDepot",
            };
            if (status == AlgorithmSolutionStatus.Feasible || status == AlgorithmSolutionStatus.Optimal)
            {
                for (int r = 0; r < Routes.Count; r++)
                {
                    //TODO this is not necessary to fix, we need to code a converter that will convert all the solutions into meaningful text files.
                    //list.Add(String.Join("-", Routes[r].SitesVisited) + "\t" + Routes[r].VehicleCategoryIndex.ToString() + "\t" + Routes[r].TotalCollectedPrize + "\t" + (Routes[r].FixedCost + Routes[r].TotalVariableTravelCost).ToString() + "\t" + Routes[r].TotalProfit + "\t" + Routes[r].DepartureTime.Last().ToString());
                }
            }
            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
        }
    }
}
