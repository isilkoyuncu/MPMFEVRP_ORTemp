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
        string GetRecommendedOutputFileFullName();       
        Site[] GetSiteArray();
        Vehicle[] GetVehicleArray();
        int GetNumberOfCustomers();
        int GetNumberOfES();
        double GetTravelSpeed();
        double[,] GetDistanceMatrix();
        bool IsLongLat();
    }
}
