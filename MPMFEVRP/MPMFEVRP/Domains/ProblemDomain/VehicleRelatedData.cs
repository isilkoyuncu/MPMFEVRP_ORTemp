using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class VehicleRelatedData
    {
        int numVehicleCategories;    //Vehicle categories, must equal numVehicles.Length!
        //int[] numVehicles;   //array length must equal numVehicleCategories!
        Vehicle[] vehicleArray;

        public int NumVehicleCategories { get { return numVehicleCategories; } set { numVehicleCategories = value; } }
        //public int[] NumVehicles { get { return numVehicles; } set { numVehicles = value; } }
        //public Vehicle[] VehicleArray { get { return vehicleArray; } set { vehicleArray = value; } }

        public VehicleRelatedData() { }
        public VehicleRelatedData(int numVehicleCategories, Vehicle[] vehicleArray)
        {
            this.numVehicleCategories = numVehicleCategories;
            //this.numVehicles = numVehicles;
            this.vehicleArray = vehicleArray;
        }

        public VehicleRelatedData(VehicleRelatedData twinVRD)
        {
            numVehicleCategories = twinVRD.NumVehicleCategories;
            //numVehicles = twinVRD.NumVehicles;
            vehicleArray = new Vehicle[numVehicleCategories];
            
        }
        public List<Vehicle> GetVehiclesOfCategory(VehicleCategories vehicleCategory)
        {
            List<Vehicle> outcome = new List<Vehicle>();
            foreach (Vehicle v in vehicleArray)
                if (v.Category == vehicleCategory)
                    outcome.Add(v);
            return outcome;
        }
        public Vehicle GetTheVehicleOfCategory(VehicleCategories vehicleCategory, bool returnFirstIfMultiple = true)
        {
            List<Vehicle> theList = GetVehiclesOfCategory(vehicleCategory);
            if (theList.Count == 0)
                return null;
            if (theList.Count == 1)
                return theList.First();
            //if we're here, the list contains multiple elements
            if (returnFirstIfMultiple)
                return theList.First();
            else
                throw new Exception("VehicleRelatedData.GetTheVehicleOfCategory invoked with returnFirstIfMultiple = false, but there are multiple vehicles of the desired category and the method can't choose which one to return!");
        }
    }
}
