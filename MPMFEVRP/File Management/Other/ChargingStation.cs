using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Other
{
    class ChargingStation
    {
        public static double RechargingRate(ChargingLevels cl)//KWH/minute
        {
            switch(cl)
            {
                case ChargingLevels.L1:
                    return 1.6/60.0;
                    //break;
                case ChargingLevels.L2:
                    return 6.5 / 60.0;
                    //break;
                case ChargingLevels.L3:
                    return 60.0 / 30.0;
                    //break;
                default:
                    return 0.0;
            }
        }

        public static string[] getHeaderRow()
        {
            return new string[] { "Recharging Level", "Rate" };
        }

        public static string[] getIndividualRow(ChargingLevels cl)
        {
            return new string[] { cl.ToString(), RechargingRate(cl).ToString() };
        }
    }
}
