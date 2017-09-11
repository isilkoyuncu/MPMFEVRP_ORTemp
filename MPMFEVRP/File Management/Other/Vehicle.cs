using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Other
{
    public class Vehicle
    {
        string id;
        VehicleCategories category;
        int loadCapacity;
        double batteryCapacity;
        double consumptionRate;//Only for electric vehicles
        double fixedCost;
        double variableCostPerMile;//per mile
        double maxChargingRate; //kWh/minute
        public string ID { get { return id; } }
        public VehicleCategories Category { get { return category; } }
        public int LoadCapacity { get { return loadCapacity; } }
        public double BatteryCapacity { get { return batteryCapacity; } }
        public double ConsumptionRate { get { return consumptionRate; } }
        public double FixedCost { get { return fixedCost; } }
        public double VariableCostPerMile { get { return variableCostPerMile; } }
        public double MaxChargingRate { get { return maxChargingRate; } }

        public Vehicle(Vehicles v)
        {
            switch (v)
            {
                case Vehicles.EMH_60KWH:
                    id = "EMH AVF 50kWh 0.2kWh/mile";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryCapacity = 60.0;
                    consumptionRate = 0.20;
                    fixedCost = 0.0;//40
                    variableCostPerMile = 0.0388;
                    maxChargingRate = 60.0/60.0;
                    break;
                case Vehicles.EMH_1_6L_4cyl_Automatic:
                    id = "EMH 1.6L 4cyl Automatic";
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryCapacity = 0;
                    consumptionRate = 0;
                    fixedCost = 0.0;//30
                    variableCostPerMile = 0.0656;
                    maxChargingRate = 0.0;
                    break;
                case Vehicles.Ford_Focus_Electric_2016_23KWH:
                    id = "Ford Focus Electric 2016 w/ 23KWH Battery";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryCapacity = 23.0;
                    consumptionRate = 0.30;
                    fixedCost = 50.0; // TODO Focus Electric might be expensive than Leaf check the price
                    variableCostPerMile = 0.0400;
                    maxChargingRate = 6.6 / 60.0; // TODO Focus Electric might be different
                    break;
                case Vehicles.Ford_Focus_2016_2_0L_4cyl_AutoAM_S6:
                    id = "Ford Focus 2016 2.0L 4cyl AutoAM S6";
                    // TODO Update the values below:
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryCapacity = 0;
                    consumptionRate = 0;
                    fixedCost = 30.0;
                    variableCostPerMile = 0.0656;
                    maxChargingRate = 0.0; 
                    break;
                case Vehicles.Nissan_Leaf_2016_24KWH:
                    id = "Nissan Leaf 2016 w/ 24KWH Battery";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryCapacity = 24.0;
                    consumptionRate = 0.30;
                    fixedCost = 40.0;
                    variableCostPerMile = 0.0388;
                    maxChargingRate = 3.3 / 60.0;
                    break;
                case Vehicles.Nissan_Leaf_2016_30KWH:
                    id = "Nissan Leaf 2016 w/ 30KWH Battery";
                    category = VehicleCategories.EV;
                    loadCapacity = 200;
                    batteryCapacity = 30.0;
                    consumptionRate = 0.30;
                    fixedCost = 50.0;
                    variableCostPerMile = 0.0400;
                    maxChargingRate = 3.3/60.0;
                    break;
                case Vehicles.Nissan_Versa_2016_1_6L_4cyl_Automatic:
                    id = "Nissan Versa 2016 1.6L 4cyl Automatic";
                    category = VehicleCategories.GDV;
                    loadCapacity = 200;
                    batteryCapacity = 0;
                    consumptionRate = 0;
                    fixedCost = 30.0;
                    variableCostPerMile = 0.0656;
                    maxChargingRate = 0.0;
                    break;

            }
        }

        public static string[] getHeaderRow()
        {
            return new string[] { "ID", "Category", "Load Capacity", "Battery Capacity", "Consumption Rate", "Fixed Cost", "Variable Cost Per Mile" , "Maximum Charging Rate"};
        }
        public string[] getIndividualRow()
        {
            return new string[] { id.ToString(), category.ToString(), loadCapacity.ToString(), batteryCapacity.ToString(), consumptionRate.ToString(), fixedCost.ToString(), variableCostPerMile.ToString(), maxChargingRate.ToString()};
        }

        public Vehicle(string id, VehicleCategories category, int loadCapacity, double batteryCapacity, double consumptionRate, double fixedCost, double variableCostPerMile, double maxChargingRate)
        {
            this.id = id;
            this.category = category;
            this.loadCapacity = loadCapacity;
            this.batteryCapacity = batteryCapacity;
            this.consumptionRate = consumptionRate;
            this.fixedCost = fixedCost;
            this.variableCostPerMile = variableCostPerMile;
            this.maxChargingRate = maxChargingRate;
        }
        //public Vehicle(string id, VehicleCategories category, int loadCapacity, double batteryCapacity, double consumptionRate, double variableCostPerMile)
        //{
        //    this.id = id;
        //    this.category = category;
        //    this.loadCapacity = loadCapacity;
        //    this.batteryCapacity = batteryCapacity;
        //    this.consumptionRate = consumptionRate;
        //    this.variableCostPerMile = variableCostPerMile;
        //}
    }
}
