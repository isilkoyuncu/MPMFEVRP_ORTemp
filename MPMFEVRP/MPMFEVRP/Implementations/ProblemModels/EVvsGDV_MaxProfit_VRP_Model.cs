using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.Solutions;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EVvsGDV_MaxProfit_VRP_Model: ProblemModelBase
    {
        public EVvsGDV_MaxProfit_VRP_Model()//empty constructor
        {

        }
        public EVvsGDV_MaxProfit_VRP_Model(int numCustomers, int numES, int numNodes, Site[] siteArray, int numVehicleCategories, int[] numVehicles, Vehicle[] vehicleArray, double travelSpeed, double tMax, int lambda, double[,] distance, double[,,] energyConsumption, double[,] timeConsumption)
        {
            this.numCustomers = numCustomers;
            this.numES = numES;
            this.numNodes = numNodes;
            this.siteArray = new Site[siteArray.Length];
            for (int s = 0; s < numNodes; s++)
            {
                this.siteArray[s] = new Site(siteArray[s]);
            }
            this.numVehicleCategories = numVehicleCategories;
            this.numVehicles = numVehicles;
            this.vehicleArray = new Vehicle[vehicleArray.Length];
            for (int v = 0; v < numVehicleCategories; v++)
            {
                this.vehicleArray[v] = new Vehicle(vehicleArray[v]);
            }
            this.travelSpeed = travelSpeed;
            this.tMax=tMax;
            this.lambda=lambda;
            this.distance=distance;  
            this.energyConsumption=energyConsumption;
            this.timeConsumption=timeConsumption;
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "EV vs GDV Profit Maximization Problem";
        }

        public override ISolution GetRandomSolution(int seed)
        {
            throw new NotImplementedException();
        }

        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            throw new NotImplementedException();
        }

        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            throw new NotImplementedException();

        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }
    }
}
