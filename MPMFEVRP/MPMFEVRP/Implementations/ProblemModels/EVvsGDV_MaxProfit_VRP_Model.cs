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

        //Site-related information (sites: depot, customer, ES)
        int numCustomers;    //Number of Customers, not all of which must be served
        int numES;  //# of ES, which includes the depot. If this is 0, intra-day charging is not allowed even at the depot. 
        int numNodes;   //This is numCustomers+nES+1 (1 is for the depot)
        Site[] siteArray;

        //Vehicle-related information
        int numVehicleCategories;    //Vehicle categories, must equal numVehicles.Length!B
        int[] numVehicles;   //array length must equal numVehicleCategories!
        Vehicle[] vehicleArray;

        //Overall (not site- or vehicle-related information)
        double travelSpeed;   //This is miles per minute
        double tMax;    //Leghts of the workday, minutes
        int lambda; //Max number of recharges per EV in a workday
        double[,] distance;   //[numNodes,numNodes]  
        double[, ,] energyConsumption;    //[numNodes,numNodes,numVehicleTypes]
        double[,] timeConsumption;  //[numNodes,numNodes]

        //Externals
        public int NumCustomers { get { return numCustomers; } }
        public int NumES { get { return numES; } }
        public int NumNodes { get { return numNodes; } }
        public Site[] SiteArray { get { return siteArray; } }

        public int NumVehicleCategories { get { return numVehicleCategories; } }
        public int[] NumVehicles { get { return numVehicles; } }
        public Vehicle[] VehicleArray { get { return vehicleArray; } }

        public double TravelSpeed { get { return travelSpeed; } }
        public double TMax { get { return tMax; } }
        public int Lambda { get { return lambda; } }
        public double[,] Distance { get { return distance; } }
        public double[,,] EnergyConsumption { get { return energyConsumption; } }
        public double[,] TimeConsumption { get { return timeConsumption; } }

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
