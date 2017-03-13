using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Other
{

    public enum DepotLocations { Center, LowerLeftCorner, Random };

    public enum ServiceDurationDistributions {f30, f45, f60, r30_60, r30_45_60 };

    public enum Vehicles { EMH_60KWH, EMH_1_6L_4cyl_Automatic, Ford_Focus_Electric_2016_23KWH, Ford_Focus_2016_2_0L_4cyl_AutoAM_S6, Nissan_Leaf_2016_24KWH, Nissan_Leaf_2016_30KWH, Nissan_Versa_2016_1_6L_4cyl_Automatic };

    public enum VehicleCategories { EV, GDV };

    public enum ChargingLevels { L3, L2, L1 };

    public enum BasePricingPolicy { Identical, ProportionalToServiceDuration };
    public enum TripChargePolicy { None, TwoTier, Individualized };

    public enum DistanceMatrixSource { File, EuclideanCalculation, HaversianCalculation}
}
