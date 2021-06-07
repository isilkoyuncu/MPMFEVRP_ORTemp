using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Other;

namespace Instance_Generation.FormSections
{
    public class TypeGammaPrize_RelatedData
    {
        int nEVPremPayCustomers;        public int NEVPremPayCustomers { get { return nEVPremPayCustomers; } }

        int nISS, nISS_L1, nISS_L2, nISS_L3;
        public int NISS { get { return nISS; } }
        public int NISS_L1 { get { return nISS_L1; } }
        public int NISS_L2 { get { return nISS_L2; } }
        public int NISS_L3 { get { return nISS_L3; } }

        int nESS, nESS_L1, nESS_L2, nESS_L3;
        public int NESS { get { return nESS; } }
        public int NESS_L1 { get { return nESS_L1; } }
        public int NESS_L2 { get { return nESS_L2; } }
        public int NESS_L3 { get { return nESS_L3; } }

        double l1kWhPerMinute, l2kWhPerMinute, l3kWhPerMinute;
        public double L1kWhPerMinute { get { return l1kWhPerMinute; } }
        public double L2kWhPerMinute { get { return l2kWhPerMinute; } }
        public double L3kWhPerMinute { get { return l3kWhPerMinute; } }

        ChargingLevels selectedDepotChargingLvl;    public ChargingLevels SelectedDepotChargingLvl { get { return selectedDepotChargingLvl; } }
        BasePricingPolicy basePricingPol;   public BasePricingPolicy BasePricingPol { get { return basePricingPol; } }
        double basePricingDollar;       public double BasePricingDollar { get { return basePricingDollar; } }
        TripChargePolicy tripChargePol; public TripChargePolicy TripChargePol { get { return tripChargePol; } }
        double tripChargeDollar;        public double TripChargeDollar { get { return tripChargeDollar; } }
        double evPrizeCoefficient;      public double EVPrizeCoefficient { get { return evPrizeCoefficient; } }
        int numInNetworkESs;            public int NumInNetworkESs { get { return numInNetworkESs; } }
        double refuelingCostAtDepotPerKWH; public double RefuelingCostAtDepotPerKWH { get { return refuelingCostAtDepotPerKWH; } }
        double inNetworkCostPerKWH;     public double InNetworkCostPerKWH { get { return inNetworkCostPerKWH; } }
        double outNetworkCostPerKWH;    public double OutNetworkCostPerKWH { get { return outNetworkCostPerKWH; } }
        double gasolineDollarPerGallon; public double GasolineDollarPerGallon { get { return gasolineDollarPerGallon; } }


        public TypeGammaPrize_RelatedData(
            int nEVPremPayCustomers,
            int nISS_L1,
            int nISS_L2,
            int nISS_L3,
            int nESS_L1,
            int nESS_L2,
            int nESS_L3,
            double l1kWhPerMinute,
            double l2kWhPerMinute,
            double l3kWhPerMinute,
            ChargingLevels selectedDepotChargingLvl,
            BasePricingPolicy basePricingPol,
            double basePricingDollar,
            TripChargePolicy tripChargePol,
            double tripChargeDollar,
            double evPrizeCoefficient,
            int numInNetworkESs,
            double refuelingCostAtDepotPerKWH,
            double inNetworkCostPerKWH,
            double outNetworkCostPerKWH,
            double gasolineDollarPerGallon
            )
        {
            this.nEVPremPayCustomers = nEVPremPayCustomers;
            this.nISS_L1 = nISS_L1;
            this.nISS_L2 = nISS_L2;
            this.nISS_L3 = nISS_L3;
            nISS = nISS_L1 + nISS_L2 + nISS_L3;
            this.nESS_L1 = nESS_L1;
            this.nESS_L2 = nESS_L2;
            this.nESS_L3 = nESS_L3;
            nESS = nESS_L1 + nESS_L2 + nESS_L3;
            this.l1kWhPerMinute = l1kWhPerMinute;
            this.l2kWhPerMinute = l2kWhPerMinute;
            this.l3kWhPerMinute = l3kWhPerMinute;
            this.selectedDepotChargingLvl = selectedDepotChargingLvl;
            this.basePricingPol = basePricingPol;
            this.basePricingDollar = basePricingDollar;
            this.tripChargePol = tripChargePol;
            this.tripChargeDollar = tripChargeDollar;
            this.evPrizeCoefficient = evPrizeCoefficient;
            this.numInNetworkESs = numInNetworkESs;
            this.refuelingCostAtDepotPerKWH = refuelingCostAtDepotPerKWH;
            this.inNetworkCostPerKWH = inNetworkCostPerKWH;
            this.outNetworkCostPerKWH = outNetworkCostPerKWH;
            this.gasolineDollarPerGallon = gasolineDollarPerGallon;
        }
    }
}
