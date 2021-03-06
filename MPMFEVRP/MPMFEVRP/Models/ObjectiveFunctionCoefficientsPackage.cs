﻿using MPMFEVRP.Domains.ProblemDomain;
using System.Collections.Generic;

namespace MPMFEVRP.Models
{
    public class ObjectiveFunctionCoefficientsPackage
    {
        //This class is written to provide the coefficients (which may change as part of an experimentation)
        //The class will have several Set methods
        //Everything here corresponds 1-on-1 to OFIDP

        double fixedPrizePerCustomerServedByEV;
        double fixedPrizePerCustomerServedByGDV;
        public double GetFixedPrizePerCustomerServed(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? fixedPrizePerCustomerServedByEV : fixedPrizePerCustomerServedByGDV); }
        public void SetFixedPrizePerCustomerServed(VehicleCategories vehicleCategory, double fixedPrize)
        {
            if (vehicleCategory == VehicleCategories.EV)
                fixedPrizePerCustomerServedByEV = fixedPrize;
            else
                fixedPrizePerCustomerServedByGDV = fixedPrize;
        }
        public void SetFixedPrizePerCustomerServed(double fixedPrize)
        {
            //This setter doesn't differentiate between vehicle categories, but sets a blanket prize per customer served
            fixedPrizePerCustomerServedByEV = fixedPrize;
            fixedPrizePerCustomerServedByGDV = fixedPrize;
        }

        double coefficient_TotalPrizeCollectedByEVs;
        double coefficient_TotalPrizeCollectedByGDVs;
        public double GetEVTotalPrizeCoefficient(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? coefficient_TotalPrizeCollectedByEVs : coefficient_TotalPrizeCollectedByGDVs); }
        public void SetCoefficient_TotalPrizeCollected(VehicleCategories vehicleCategory, double coefficient)
        {
            if (vehicleCategory == VehicleCategories.EV)
                coefficient_TotalPrizeCollectedByEVs = coefficient;
            else
                coefficient_TotalPrizeCollectedByGDVs = coefficient;
        }
        public void SetCoefficient_TotalPrizeCollected(double coefficient)
        {
            //This setter doesn't differentiate between vehicle categories, but sets a blanket prize per customer served
            coefficient_TotalPrizeCollectedByEVs = coefficient;
            coefficient_TotalPrizeCollectedByGDVs = coefficient;
        }

        double fixedCostPerEV;
        double fixedCostPerGDV;
        public double GetFixedCostPerVehicle(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? fixedCostPerEV : fixedCostPerGDV); }
        public void SetFixedCostPerVehicle(VehicleCategories vehicleCategory, double fixedCost)
        {
            if (vehicleCategory == VehicleCategories.EV)
                fixedCostPerEV = fixedCost;
            else
                fixedCostPerGDV = fixedCost;
        }
        public void SetFixedCostPerVehicle(double fixedCost)
        {
            //This setter doesn't differentiate between vehicle categories, but sets a blanket prize per customer served
            fixedCostPerEV = fixedCost;
            fixedCostPerGDV = fixedCost;
        }

        double costPerMileOfTravel_EV;
        double costPerMileOfTravel_GDV;
        public double GetCostPerMileOfTravel(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? costPerMileOfTravel_EV : costPerMileOfTravel_GDV); }
        public void SetCostPerMileOfTravel(VehicleCategories vehicleCategory, double cost)
        {
            if (vehicleCategory == VehicleCategories.EV)
                costPerMileOfTravel_EV = cost;
            else
                costPerMileOfTravel_GDV = cost;
        }
        public void SetCostPerMileOfTravel(double cost)
        {
                costPerMileOfTravel_EV = cost;
                costPerMileOfTravel_GDV = cost;
        }

        public ObjectiveFunctionCoefficientsPackage() { }//Do-nothing: all numbers are 0 by default
        public ObjectiveFunctionCoefficientsPackage(double fixedPrizePerCustomerServedByEV, double fixedPrizePerCustomerServedByGDV, 
            double coefficient_TotalPrizeCollectedByEVs, double coefficient_TotalPrizeCollectedByGDVs,
            double fixedCostPerEV, double fixedCostPerGDV,
            double costPerMileOfTravel_EV, double costPerMileOfTravel_GDV)
        {
            //This is the full constructor
            this.fixedPrizePerCustomerServedByEV = fixedPrizePerCustomerServedByEV;
            this.fixedPrizePerCustomerServedByGDV= fixedPrizePerCustomerServedByGDV;
            this.coefficient_TotalPrizeCollectedByEVs = coefficient_TotalPrizeCollectedByEVs;
            this.coefficient_TotalPrizeCollectedByGDVs = coefficient_TotalPrizeCollectedByGDVs;
            this.fixedCostPerEV = fixedCostPerEV;
            this.fixedCostPerGDV = fixedCostPerGDV;
            this.costPerMileOfTravel_EV = costPerMileOfTravel_EV;
            this.costPerMileOfTravel_GDV = costPerMileOfTravel_GDV;
        }
        public ObjectiveFunctionCoefficientsPackage(double costPerMileOfTravel_EV, double costPerMileOfTravel_GDV)
        {
            //This is a partial constructor designed with the Minimization of Total Variable Cost in mind
            this.costPerMileOfTravel_EV = costPerMileOfTravel_EV;
            this.costPerMileOfTravel_GDV = costPerMileOfTravel_GDV;
        }

    }
}
