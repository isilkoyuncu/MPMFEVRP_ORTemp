using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class VehicleRelatedData
    {
        int numVehicleCategories;    //Vehicle categories, must equal numVehicles.Length!
        int[] numVehicles;   //array length must equal numVehicleCategories!
        Vehicle[] vehicleArray;

        public int NumVehicleCategories { get { return numVehicleCategories; } set { numVehicleCategories = value; } }
        public int[] NumVehicles { get { return numVehicles; } set { numVehicles = value; } }
        public Vehicle[] VehicleArray { get { return vehicleArray; } set { vehicleArray = value; } }

        public VehicleRelatedData() { }
        public VehicleRelatedData(int numVehicleCategories, int[] numVehicles, Vehicle[] vehicleArray)
        {
            this.numVehicleCategories = numVehicleCategories;
            this.numVehicles = numVehicles;
            this.vehicleArray = vehicleArray;
        }

        public VehicleRelatedData(VehicleRelatedData twinVRD)
        {
            numVehicleCategories = twinVRD.NumVehicleCategories;
            numVehicles = twinVRD.NumVehicles;
            vehicleArray = twinVRD.VehicleArray;
        }
    }
}
