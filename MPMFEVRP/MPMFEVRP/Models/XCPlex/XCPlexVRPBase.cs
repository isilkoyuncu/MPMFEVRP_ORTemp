using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;


namespace MPMFEVRP.Models.XCPlex
{
    public abstract class XCPlexVRPBase : XCPlexBase
    {
        protected Site[] preprocessedSites;//Ready-to-use
        protected List<Site> depots;
        protected List<Site> customers;
        protected List<Site> externalStations;//Preprocessed, Ready-to-use
        protected VehicleCategories[] vehicleCategories = new VehicleCategories[] { VehicleCategories.EV, VehicleCategories.GDV };
        protected int numVehCategories;
        protected RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        public XCPlexVRPBase(){ }

        public XCPlexVRPBase(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam):base(theProblemModel, xCplexParam)
        {
                        rechargingDuration_status = theProblemModel.RechargingDuration_status;
        }

        public abstract List<VehicleSpecificRoute> GetVehicleSpecificRoutes();
    }
}
