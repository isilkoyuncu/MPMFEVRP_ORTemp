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

        public int NumVehicleCategories { get { return numVehicleCategories; } set {; } }
        public int[] NumVehicles { get { return numVehicles; } set {; } }
        public Vehicle[] VehicleArray { get { return vehicleArray; } set {; } }

        public VehicleRelatedData() { }
        public VehicleRelatedData(int numVehicleCategories, int[] numVehicles, Vehicle[] vehicleArray)
        {

        }
    }
}
