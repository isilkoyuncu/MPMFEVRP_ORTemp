using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels;


namespace MPMFEVRP.Implementations.Problems
{
    public class EVvsGDV_MaxProfit_VRP:ProblemBase
    {
        /* This problem is a heteregoneus VRP which allows any number of vehicle types, but they belong to two categories: EV and GDV
         * The objective is to maximize profit = revenue - cost (fixed per day & variable per mile)
         * Uncapacitated problem: customer demands are all 0, vehicle capacities are undefined
         * No time windows
         * */

        public EVvsGDV_MaxProfit_VRP() { }

        public EVvsGDV_MaxProfit_VRP(ProblemDataPackage PDP)
        {
            base.PDP = new ProblemDataPackage(PDP);
            //The following are the problem characteristics. Each problem will have these fixed here, and they have nothing to do with data!
            objectiveFunctionType = Models.ObjectiveFunctionTypes.Maximize;

            //This code is extremely strict, for sake of simplicity!
            //First, we must be given exactly 2 vehicles
            if (PDP.VRD.VehicleArray.Length != 2)
                throw new ArgumentException("Reader had the wrong number of vehicle categories!");
            //Then, the first (0) must be an EV, and the other (1) must be a GDV!
            if ((PDP.VRD.VehicleArray[0].Category != VehicleCategories.EV) ||
                (PDP.VRD.VehicleArray[1].Category != VehicleCategories.GDV))
                throw new ArgumentException("Reader had the wrong composition or ordering of vehicle categories!");

            PDP.VRD.NumVehicles[0] = 3; //These do not belong here! They should be a part of computational experiment
            PDP.VRD.NumVehicles[1] = 3;
        }

        public override string GetName()
        {
            return "EV vs GDV Maximum Profit VRP";
        }

        //public new EVvsGDV_MaxProfit_VRP_Model GetProblemModel()
        //{
        //    return new EVvsGDV_MaxProfit_VRP_Model( numCustomers,  numES,  numNodes,  siteArray,  numVehicleCategories,  numVehicles, vehicleArray,  travelSpeed,  tMax,  lambda,  distance,  energyConsumption, timeConsumption);
        //}

        public override string ToString()
        {
            return PDP.InputFileName;
        }
    }
}
