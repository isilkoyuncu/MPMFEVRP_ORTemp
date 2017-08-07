﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public enum SiteTypes { Depot, Customer, ExternalStation };
    public enum VehicleCategories { EV, GDV };
    public enum ChargingLevels { L3, L2, L1 };
    public enum CustomerCoverageConstraint_EachCustomerMustBeCovered { ExactlyOnce, AtMostOnce, AtLeastOnce };
    public enum RechargingDurationAndAllowableDepartureStatusFromES { Fixed_Full, Variable_Full, Variable_Partial };
    
        

}
