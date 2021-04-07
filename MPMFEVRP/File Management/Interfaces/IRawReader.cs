using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Other;

namespace Instance_Generation.Interfaces
{
    public interface IRawReader
    {
        //Any class implementing this will be responsible of knowing all info that authors reported (constant or calculation) in the paper but not in the instance files

        void Read();
        string getRecommendedOutputFileFullName();

        string[] getIDColumn();
        string[] getTypeColumn();
        bool usesGeographicPositions();
        bool needToShuffleCustomers();
        double[] getXorLongitudeColumn();
        double[] getYorLatitudeColumn();
        double[] getDemandColumn();
        double[] getReadyTimeColumn();
        double[] getDueDateColumn();
        double[] getServiceDurationColumn();
        double[] getRechargingRates();
        double getESRechargingRate();
        double[,] getPrizeMatrix();

        double[,] getDistanceMatrix();

        Vehicle[] getVehicleRows();

        double getTravelSpeed();
        int getNumCustomers();

        string getInputFileType();

        int getNumESS();
    }
}
