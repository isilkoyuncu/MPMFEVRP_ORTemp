using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Implementations
{
    public abstract class ProblemModelBase : IProblemModel
    {
        //Site-related information (sites: depot, customer, ES)
        protected int numCustomers;    //Number of Customers, not all of which must be served
        protected int numES;  //# of ES, which includes the depot. If this is 0, intra-day charging is not allowed even at the depot. 
        protected int numNodes;   //This is numCustomers+nES+1 (1 is for the depot)
        protected Site[] siteArray;

        //Vehicle-related information
        protected int numVehicleCategories;    //Vehicle categories, must equal numVehicles.Length!B
        protected int[] numVehicles;   //array length must equal numVehicleCategories!
        protected Vehicle[] vehicleArray;

        //Overall (not site- or vehicle-related information)
        protected double travelSpeed;   //This is miles per minute
        protected double tMax;    //Leghts of the workday, minutes
        protected int lambda; //Max number of recharges per EV in a workday
        protected double[,] distance;   //[numNodes,numNodes]  
        protected double[,,] energyConsumption;    //[numNodes,numNodes,numVehicleTypes]
        protected double[,] timeConsumption;  //[numNodes,numNodes]

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

        public abstract string GetDescription();
        public abstract string GetName();
        public abstract ISolution GetRandomSolution(int seed);
        public abstract bool CheckFeasibilityOfSolution(ISolution solution);
        public abstract double CalculateObjectiveFunctionValue(ISolution solution);
        public abstract bool CompareTwoSolutions(ISolution solution1, ISolution solution2);
    }
}
