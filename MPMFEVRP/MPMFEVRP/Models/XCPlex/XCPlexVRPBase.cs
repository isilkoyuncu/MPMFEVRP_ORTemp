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
        protected int vIndex_EV = -1, vIndex_GDV=-1;
        protected int[] numVehicles;

        protected RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        public XCPlexVRPBase(){ }

        public XCPlexVRPBase(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam) : base(theProblemModel, xCplexParam)
        {
            rechargingDuration_status = theProblemModel.RechargingDuration_status;
            for (int v = 0; v < numVehCategories; v++) {
                if (vehicleCategories[v] == VehicleCategories.EV)
                    vIndex_EV = v;
                else if (vehicleCategories[v] == VehicleCategories.GDV)
                    vIndex_GDV = v;
                else
                    throw new System.Exception("EV category could not found!");
            }
            if (xCplexParam.TSP)
            {
                switch (xCplexParam.VehCategory)
                {
                    case VehicleCategories.EV:
                        numVehicles = new int[] { 1, 0 };
                        break;
                    case VehicleCategories.GDV:
                        numVehicles = new int[] { 0, 1 };
                        break;
                    default:
                        throw new System.Exception("Vehicle Category unrecognized!!!");
                }
            }
            else//not TSP
            {
                numVehicles = new int[] { theProblemModel.GetNumVehicles(VehicleCategories.EV), theProblemModel.GetNumVehicles(VehicleCategories.GDV) };
            }
        }

        public abstract List<VehicleSpecificRoute> GetVehicleSpecificRoutes();
        public abstract void RefineDecisionVariables(CustomerSet cS);
    }
}
