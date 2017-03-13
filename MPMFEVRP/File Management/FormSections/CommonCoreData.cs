using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Other;

namespace Instance_Generation.FormSections
{
    public class CommonCoreData
    {
        DepotLocations depotLocation;
        public DepotLocations DepotLocation { get { return depotLocation; } }

        int nCustomers;
        public int NCustomers { get { return nCustomers; } }

        string customerDistribution;
        public string CustomerDistribution { get { return customerDistribution; } }

        ServiceDurationDistributions serviceDurationDistribution;
        public ServiceDurationDistributions ServiceDurationDistribution { get { return serviceDurationDistribution; } }

        double xMax, yMax;
        public double XMax { get { return xMax; } }
        public double YMax { get { return yMax; } }

        double tMax;
        public double TMax { get { return tMax; } }
        double travelSpeed;
        public double TravelSpeed { get { return travelSpeed; } }

        public CommonCoreData(
            DepotLocations depotLocation,
            int nCustomers,
            string customerDistribution,
            ServiceDurationDistributions serviceDurationDistribution,
            double xMax, double yMax,
            double tMax,
            double travelSpeed
            )
        {
            this.depotLocation = depotLocation;
            this.nCustomers = nCustomers;
            this.customerDistribution = customerDistribution;
            this.serviceDurationDistribution = serviceDurationDistribution;
            this.xMax = xMax;
            this.yMax = yMax;
            this.tMax = TMax;
            this.travelSpeed = travelSpeed;
        }
    }
}
