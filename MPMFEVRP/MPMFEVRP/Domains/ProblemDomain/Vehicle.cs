using System;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class Vehicle
    {
        string id;                      public string ID { get { return id; } }
        VehicleCategories               category; public VehicleCategories Category { get { return category; } }
        int loadCapacity;               public int LoadCapacity { get { return loadCapacity; } }
        double batteryCapacity;         public double BatteryCapacity { get { return batteryCapacity; } }
        double consumptionRate;         public double ConsumptionRate { get { return consumptionRate; } }
        double fixedCost;               public double FixedCost { get { return fixedCost; } }
        double variableCostPerMile;     public double VariableCostPerMile { get { return variableCostPerMile; } }
        double maxChargingRate;         public double MaxChargingRate { get { return maxChargingRate; } }
        double fixedRefuelingTime;      public double FixedRefuelingTime { get { return fixedRefuelingTime; } }


        public Vehicle(string id, VehicleCategories category, int loadCapacity, double batteryCapacity, double consumptionRate, double fixedCost, double variableCostPerMile, double maxChargingRate, double fixedRefuelingTime)
        {
            this.id = id;
            this.category = category;
            this.loadCapacity = loadCapacity;
            this.batteryCapacity = batteryCapacity;
            this.consumptionRate = consumptionRate;
            this.fixedCost = fixedCost;
            this.variableCostPerMile = variableCostPerMile;
            this.maxChargingRate = maxChargingRate;
            this.fixedRefuelingTime = fixedRefuelingTime;
        }       
        public Vehicle(Vehicle twinVehicle)
        {
            id = twinVehicle.id;
            category = twinVehicle.category;
            loadCapacity = twinVehicle.loadCapacity;
            batteryCapacity = twinVehicle.batteryCapacity;
            consumptionRate = twinVehicle.consumptionRate;
            fixedCost = twinVehicle.fixedCost;
            variableCostPerMile = twinVehicle.variableCostPerMile;
            maxChargingRate = twinVehicle.maxChargingRate;
            fixedRefuelingTime = twinVehicle.fixedRefuelingTime;
        }
    }
}
