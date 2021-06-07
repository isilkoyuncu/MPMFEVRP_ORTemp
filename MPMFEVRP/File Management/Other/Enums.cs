using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Other
{

    public enum DepotLocations { Center, LowerLeftCorner, Random };

    public enum ServiceDurationDistributions {f30, f45, f60, r30_60, r30_45_60 };

    public enum Vehicles { YC_24KWH, YC_1_6L_4cyl_Automatic, EMH_60KWH, EMH_1_6L_4cyl_Automatic, Schneider_60KWH, Schneider_1_6L_4cyl_Automatic, Nissan_Leaf_2021_40KWH, Nissan_Leaf_2021_62KWH, Nissan_Versa_2021_1_6L_4cyl_Automatic };

    public enum VehicleCategories { EV, GDV };

    public enum ChargingLevels { L3, L2, L1 };

    public enum BasePricingPolicy { Identical, ProportionalToServiceDuration };
    public enum TripChargePolicy { None, TwoTier, Individualized };

    public enum DistanceMatrixSource { File, EuclideanCalculation, HaversianCalculation}

    public enum CustomerRemovalCriteria { DirectRouteExceedsWorkdayLength, CannotBeReachedWithAtMostOneESVisit }
}
