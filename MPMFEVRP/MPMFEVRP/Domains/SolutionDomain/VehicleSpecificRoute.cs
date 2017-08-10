using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class VehicleSpecificRoute
    {
        //Defining fields
        IProblemModel problemModel;

        Vehicle vehicle;
        public Vehicle Vehicle { get { return vehicle; } }
        public VehicleCategories VehicleCategory { get { return vehicle.Category; } }

        bool alwaysClosedLoop;//
        public bool AlwaysClosedLoop { get { return alwaysClosedLoop; } }
        public bool AtDepot { get { return (siteVisits.Last().SiteID == problemModel.SRD.GetSingleDepotID()); } }
        public string LastSiteID { get { return siteVisits.Last().SiteID; } }

        //Other fields
        List<SiteVisit> siteVisits;
        //public List<string> SitesVisited_ID { get { return sitesVisited_ID; } }//Keep unaccessible until needed, which hopefully won't be needed at all

    }

    public class SiteVisit
    {
        //site visited
        string siteID; public string SiteID { get { return siteID; } }

        //time and SOC at the site
        double arrivalTime;
        double arrivalSOC;
        double SOCGain;
        double departureTime;
        double departureSOC;

        //the cumulatives (for statistics)
        double cumulativeTravelDistance; //(by arrival at this site)
    }
}
