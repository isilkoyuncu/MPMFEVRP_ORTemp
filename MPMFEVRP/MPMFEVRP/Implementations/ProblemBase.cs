using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Utils;
using MPMFEVRP.Implementations.ProblemModels;

namespace MPMFEVRP.Implementations
{
    public abstract class ProblemBase : IProblem
    {

        protected string InputFileName;   //For record only

        protected int numCustomers;    //Number of Customers, not all of which must be served
        protected int numES;  // Number of ES may or may not include a replica of the depot
        protected int numNodes;   //This is numCustomers+nES+1 = siteArray.Length
        protected Site[] siteArray;

        protected int numVehicleCategories;    //Vehicle categories, must equal numVehicles.Length!
        protected int[] numVehicles;   //array length must equal numVehicleCategories!
        protected Vehicle[] vehicleArray;

        protected double travelSpeed;   //This is miles per minute

        //Calculated
        protected double tMax;    //Leghts of the workday, minutes
        protected int lambda; //Max number of recharges per EV in a workday
        protected double[,] distance;   //[numNodes,numNodes]  
        protected double[,,] energyConsumption;    //[numNodes,numNodes,numVehicleTypes]
        protected double[,] timeConsumption;  //[numNodes,numNodes]

        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        public ProblemBase() { }

        public ProblemBase(IReader reader)
        {

            InputFileName = reader.getRecommendedOutputFileFullName();
            numCustomers = reader.getNumberOfCustomers();
            numES = reader.getNumberOfES();
            numNodes = numCustomers + numES + 1;
            numVehicleCategories = reader.getVehicleArray().Length;
            numVehicles = new int[numVehicleCategories];
            for (int v = 0; v < numVehicleCategories; v++)
                numVehicles[v] = numCustomers;//TODO We entered numCustomers as the available number of vehicles in a category to make it unrestrictive. Limiting the numbers of vehicles is something we'd love to experiment on, and thus, this point will have to be clarified later on.

            siteArray = new Site[numNodes];
            for (int s = 0; s < numNodes; s++)
            {
                siteArray[s] = new Site(reader.getSiteArray()[s]);
            }

            vehicleArray = new Vehicle[numVehicleCategories];
            for (int v = 0; v < numVehicleCategories; v++)
            {
                vehicleArray[v] = new Vehicle(reader.getVehicleArray()[v]);
            }

            //Assign travel speed
            travelSpeed = reader.getTravelSpeed();

            //Assign Distance matrix if any
            distance = new double[numNodes, numNodes];
            if (reader.getDistanceMatrix() != null)//This is the case when distances are given in the data file (asymmetric, or whatever)
            {
                distance = (double[,])reader.getDistanceMatrix().Clone();
            }
            else//The distances are not given in the data file, but they have to be calculated
            {
                if (reader.isLongLat())//Haversine calculations
                {
                    for (int i = 0; i < numNodes; i++)
                    {
                        for (int j = 0; j < numNodes; j++)
                        {
                            distance[i, j] = Calculators.HaversineDistance(siteArray[i].X, siteArray[i].Y, siteArray[j].X, siteArray[j].Y);
                        }
                    }
                }
                else//Euclidean calculations
                {
                    for (int i = 0; i < numNodes; i++)
                    {
                        for (int j = 0; j < numNodes; j++)
                        {
                            distance[i, j] = Calculators.EuclideanDistance(siteArray[i].X, siteArray[j].X, siteArray[i].Y, siteArray[j].Y);
                        }
                    }
                }
            }

            tMax = -1;
            for (int s = 0; s < numNodes; s++)
            {
                if (tMax < siteArray[s].DueDate)
                    tMax = siteArray[s].DueDate;
            }

            energyConsumption = new double[numNodes, numNodes, numVehicleCategories];
            timeConsumption = new double[numNodes, numNodes];

            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    for (int vc = 0; vc < numVehicleCategories; vc++)
                    {
                        if (vehicleArray[vc].Category == VehicleCategories.EV)
                            energyConsumption[i, j, vc] = distance[i, j] / (vehicleArray[vc].BatteryCapacity / vehicleArray[vc].ConsumptionRate);
                        else
                            energyConsumption[i, j, vc] = 0.0;
                    }
                    timeConsumption[i, j] = distance[i, j] / travelSpeed;
                }

            lambda = 2;//TODO We entered 2 for now; we'd love to experiment on it.

        }
        

        public abstract string GetName();

        public override string ToString()
        {
            throw new NotImplementedException(); //TODO multiple-multiple run ederken anlamli olacak aciklamayi return et
        }

        public string CreateRawData()
        {
            throw new NotImplementedException();
        }

        public EVvsGDV_MaxProfit_VRP_Model GetProblemModel()
        {
            return new EVvsGDV_MaxProfit_VRP_Model(numCustomers, numES, numNodes, siteArray, numVehicleCategories, numVehicles, vehicleArray, travelSpeed, tMax, lambda, distance, energyConsumption, timeConsumption);// TODO This is not going to stay as blank constructor!
        }
    }
}
