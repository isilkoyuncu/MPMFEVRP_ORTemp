using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Other
{
    public class Vehicle
    {
        string id; public string ID { get { return id; } }
        VehicleCategories category; public VehicleCategories Category { get { return category; } }
        int loadCapacity; public int LoadCapacity { get { return loadCapacity; } }
        double batteryTankCapacity; public double BatteryCapacity { get { return batteryTankCapacity; } }
        double consumptionRate; public double ConsumptionRate { get { return consumptionRate; } }
        double fixedCost; public double FixedCost { get { return fixedCost; } }
        double variableCostPerMile; public double VariableCostPerMile { get { return variableCostPerMile; } }
        double maxChargingRate; public double MaxChargingRate { get { return maxChargingRate; } }
        double fixedRefuelingTimeMins; public double FixedRefuelingTimeMins { get { return fixedRefuelingTimeMins; } }
        double co2emissionGramsPerMile; public double CO2emissionGramsPerMile { get { return co2emissionGramsPerMile; } }

        public Vehicle(Vehicles v)
        {
            switch (v)
            {
                case Vehicles.YC_24KWH:
                    id = "YC AVF 24kWh 0.2kWh/mile";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryTankCapacity = 24.0;
                    consumptionRate = 0.20;
                    fixedCost = 0.0;//40.0
                    variableCostPerMile = 0.5;
                    maxChargingRate = 24.0 / 30.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.YC_1_6L_4cyl_Automatic:
                    id = "YC 1.6L 4cyl Automatic";
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryTankCapacity = 0;
                    consumptionRate = 0.029; // gallons per mile
                    fixedCost = 0.0;//30.0
                    variableCostPerMile = 1;
                    maxChargingRate = 0.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.EMH_60KWH:
                    id = "EMH AVF 60kWh 0.2kWh/mile";//300 miles range
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryTankCapacity = 60.0;
                    consumptionRate = 0.20;
                    fixedCost = 60.0;
                    variableCostPerMile = 0.05;
                    maxChargingRate = 60.0/15.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.EMH_1_6L_4cyl_Automatic:
                    id = "EMH 1.6L 4cyl Automatic";
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryTankCapacity = 0;
                    consumptionRate = 0.029; // gallons per mile
                    fixedCost = 40.0;
                    variableCostPerMile = 0.1;
                    maxChargingRate = 0.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.Schneider_60KWH:
                    id = "Schneider AVF 60kWh 1kWh/mile";//300 miles range
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryTankCapacity = 79.69;
                    consumptionRate = 1.0;
                    fixedCost = 60.0;
                    variableCostPerMile = 0.05;
                    maxChargingRate = 3.39;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.Schneider_1_6L_4cyl_Automatic:
                    id = "Schneider 1.6L 4cyl Automatic";
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryTankCapacity = 0;
                    consumptionRate = 0.029; // gallons per mile
                    fixedCost = 40.0;
                    variableCostPerMile = 0.1;
                    maxChargingRate = 0.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.Nissan_Leaf_2021_40KWH:
                    id = "Nissan Leaf 2021 w/ 40KWH Battery";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryTankCapacity = 40.0; //Goes 149 miles with a single charge
                    consumptionRate = 0.268;
                    fixedCost = 32000;
                    variableCostPerMile = 0.0388;
                    maxChargingRate = 3.3 / 60.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.Nissan_Leaf_2021_62KWH:
                    id = "Nissan Leaf 2021 w/ 62KWH Battery";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryTankCapacity = 62.0; //Goes 226 miles with a single charge
                    consumptionRate = 0.274;
                    fixedCost = 38000;
                    variableCostPerMile = 0.0400;
                    maxChargingRate = 3.3/60.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 0.0;
                    break;
                case Vehicles.Nissan_Versa_2021_1_6L_4cyl_Automatic:
                    id = "Nissan Versa 2021 1.6L 4cyl Automatic";
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryTankCapacity = 10.8;
                    consumptionRate = 0.029; //gallons per mile or 1/mpg =(1/35=0.029) 32/40 mpg
                    fixedCost = 17000;
                    variableCostPerMile = 0.0656;
                    maxChargingRate = 0.0;
                    fixedRefuelingTimeMins = 0.0;
                    co2emissionGramsPerMile = 254.0;
                    break;               
            }
        }

        public static string[] GetHeaderRow()
        {
            return new string[] { "ID", "Category", "Load Capacity", "Battery Capacity", "Consumption Rate", "Fixed Cost", "Variable Cost Per Mile" , "Maximum Charging Rate", "Fixed Refueling Duration", "CO2 Emission (gr/mile)"};
        }
        public string[] GetIndividualRow()
        {
            return new string[] { id.ToString(), category.ToString(), loadCapacity.ToString(), batteryTankCapacity.ToString(), consumptionRate.ToString(), fixedCost.ToString(), variableCostPerMile.ToString(), maxChargingRate.ToString(), fixedRefuelingTimeMins.ToString(), co2emissionGramsPerMile.ToString()};
        }

        public Vehicle(string id, VehicleCategories category, int loadCapacity, double batteryTankCapacity, double consumptionRate, double fixedCost, double variableCostPerMile, double maxChargingRate, double fixedRefuelingTimeMins, double co2emissionGramsPerMile)
        {
            this.id = id;
            this.category = category;
            this.loadCapacity = loadCapacity;
            this.batteryTankCapacity = batteryTankCapacity;
            this.consumptionRate = consumptionRate;
            this.fixedCost = fixedCost;
            this.variableCostPerMile = variableCostPerMile;
            this.maxChargingRate = maxChargingRate;
            this.fixedRefuelingTimeMins = fixedRefuelingTimeMins;
            this.co2emissionGramsPerMile = co2emissionGramsPerMile;
        }
    }
}
