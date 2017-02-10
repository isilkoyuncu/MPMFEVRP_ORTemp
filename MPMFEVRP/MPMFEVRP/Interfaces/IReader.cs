using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Interfaces
{
    public interface IReader
    {
        //Any class implementing this will be responsible of knowing all info that authors reported (constant or calculation) in the paper but not in the instance files

        void ProcessRawDataFromFile(string rawData);
        string getRecommendedOutputFileFullName();       
        Site[] getSiteArray();
        Vehicle[] getVehicleArray();
        int getNumberOfCustomers();
        int getNumberOfES();
        double getTravelSpeed();
        double[,] getDistanceMatrix();
        bool isLongLat();
    }
}
