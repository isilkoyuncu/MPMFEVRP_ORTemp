using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
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
        double maxChargingRate;
        public string ID { get { return id; } }
        public VehicleCategories Category { get { return category; } }
        public int LoadCapacity { get { return loadCapacity; } }
        public double BatteryCapacity { get { return batteryCapacity; } }
        public double ConsumptionRate { get { return consumptionRate; } }
        public double FixedCost { get { return fixedCost; } }
        public double VariableCostPerMile { get { return variableCostPerMile; } }
        public double MaxChargingRate { get { return maxChargingRate; } }

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
        public Vehicle(string[] allInput)
        {
            if (allInput.Length != 7)
                throw new Exception("Expecting exactly 7 inputs into the 'Vehicle' class, because that's how many fields it has!");
            //TODO Parse all input to what they need to be and then set the field values appropriately; An example is below, test that it actually works.
            id = allInput[0];
            //category = allInput[1];
            loadCapacity = int.Parse(allInput[2]);
            batteryCapacity = double.Parse(allInput[3]);
            consumptionRate = double.Parse(allInput[4]);
            fixedCost = double.Parse(allInput[5]);
            variableCostPerMile = double.Parse(allInput[6]);
            maxChargingRate = double.Parse(allInput[7]);

            if (!int.TryParse(allInput[1], out loadCapacity))
                throw new Exception("load capacity was not successfully parsed from the input!");
            //TODO When this Vehicle constructor does all parsing correctly, you still need to verify that all fields were actually filled so we can use them later
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
        }
    }
}
