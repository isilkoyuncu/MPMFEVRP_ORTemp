using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Utils;


namespace MPMFEVRP.Implementations.Problems
{
    class EVvsGDV_MaxProfit_VRP:AbstractProblem
    {
        /* This problem is a heteregoneus VRP which allows any number of vehicle types, but they belong to two categories: EV and GDV
         * The objective is to maximize profit = revenue - cost (fixed per day & variable per mile)
         * Uncapacitated problem: customer demands are all 0, vehicle capacities are undefined
         * No time windows
         * */

        public EVvsGDV_MaxProfit_VRP(IReader reader)
        {
            //This code is extremely strict, for sake of simplicity!
            //First, we must be given exactly 2 vehicles
            if (reader.getVehicleArray().Length != 2)
                throw new ArgumentException("Reader had the wrong number of vehicle categories!");
            //Then, the first (0) must be an EV, and the other (1) must be a GDV!
            if ((reader.getVehicleArray()[0].Category != VehicleCategories.EV) ||
                (reader.getVehicleArray()[1].Category != VehicleCategories.GDV))
                throw new ArgumentException("Reader had the wrong composition or ordering of vehicle categories!");

            constructAbstractProblem(reader);
            numVehicles[0] = 6;
            numVehicles[1] = numCustomers;
        }
        public new ProblemToAlgorithm ToAlgorithm()
        {
            return new ProblemToAlgorithm( numCustomers,  numES,  numNodes,  siteArray,  numVehicleCategories,  numVehicles, vehicleArray,  travelSpeed,  tMax,  lambda,  distance,  energyConsumption, timeConsumption);
        }
        public override string ToString()
        {
            return InputFileName;
        }
    }
}
