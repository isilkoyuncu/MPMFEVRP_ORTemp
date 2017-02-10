using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    class ChargingStation
    {
        public static double RechargingRate(ChargingLevels cl)//KWH/minute
        {
            switch (cl)
            {
                case ChargingLevels.L1:
                    return 30.0 / 480.0;
                //break;
                case ChargingLevels.L2:
                    return 30.0 / 240.0;
                //break;
                case ChargingLevels.L3:
                    return 30.0 / 30.0;
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
