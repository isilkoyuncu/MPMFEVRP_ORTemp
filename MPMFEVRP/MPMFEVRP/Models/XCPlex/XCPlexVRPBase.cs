using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;


namespace MPMFEVRP.Models.XCPlex
{
    public abstract class XCPlexVRPBase : XCPlexBase
    {
        protected Site[] preprocessedSites;//Ready-to-use
        protected int NumPreprocessedSites { get { return preprocessedSites.Length; } }
        protected List<Site> depots;
        protected List<Site> customers;
        protected List<Site> externalStations;//Preprocessed, Ready-to-use
        protected int vIndex_EV = -1, vIndex_GDV=-1;
        protected int[] numVehicles;

        protected double[] minValue_T, maxValue_T;
        protected double[] minValue_Delta, maxValue_Delta;
        protected double[] minValue_Epsilon, maxValue_Epsilon;

        protected RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        public XCPlexVRPBase(){ }

        public XCPlexVRPBase(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam) : base(theProblemModel, xCplexParam)
        {

        }


        public abstract List<VehicleSpecificRoute> GetVehicleSpecificRoutes();
        public abstract void RefineDecisionVariables(CustomerSet cS);

        protected override void Initialize()
        {
            rechargingDuration_status = theProblemModel.RechargingDuration_status;
            for (int v = 0; v < numVehCategories; v++)
            {
                if (vehicleCategories[v] == VehicleCategories.EV)
                    vIndex_EV = v;
                else if (vehicleCategories[v] == VehicleCategories.GDV)
                    vIndex_GDV = v;
                else
                    throw new System.Exception("EV category could not found!");
            }
            //If you change v index above, change the following num vehicles for a TSP as well
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
        protected void SetMinAndMaxValuesOfCommonVariables()
        {
            minValue_T = new double[NumPreprocessedSites];
            maxValue_T = new double[NumPreprocessedSites];
            minValue_Delta = new double[NumPreprocessedSites];
            maxValue_Delta = new double[NumPreprocessedSites];
            minValue_Epsilon = new double[NumPreprocessedSites];
            maxValue_Epsilon = new double[NumPreprocessedSites];

            for (int j = 0; j < NumPreprocessedSites; j++)
            {
                Site s = preprocessedSites[j];
                Site theDepot = depots[0];

                //UB - LB on Epsilon
                minValue_Epsilon[j] = 0.0;

                if (s.SiteType == SiteTypes.Customer)
                    maxValue_Epsilon[j] = Math.Min(BatteryCapacity(VehicleCategories.EV), s.ServiceDuration * RechargingRate(s)); //Utils.Calculators.MaxSOCGainAtSite(s, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV), maxStayDuration: s.ServiceDuration);
                else //TODO: Unit test the above utility function. It should give us MaxSOCGainAtSite s with EV.
                    maxValue_Epsilon[j] = BatteryCapacity(VehicleCategories.EV); //TODO fine tune this. It needs to be the same as the paper.

                //TODO Fine-tune the min and max values of delta using label setting algorithm
                minValue_Delta[j] = 0.0;
                maxValue_Delta[j] = BatteryCapacity(VehicleCategories.EV);

                minValue_T[j] = TravelTime(theDepot, s);
                maxValue_T[j] = theProblemModel.CRD.TMax - TravelTime(s, theDepot);
                if (s.SiteType == SiteTypes.Customer)
                    maxValue_T[j] -= ServiceDuration(s);
                else if (s.SiteType == SiteTypes.ExternalStation && theProblemModel.RechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                    maxValue_T[j] -= (BatteryCapacity(VehicleCategories.EV) / RechargingRate(s));
            }
        }
        protected double[] SetDeltaMinViaLabelSetting(double[] epsilon_Max)
        {
            double[] delta_Min = new double[preprocessedSites.Length]; //final outcome

            List<Tuple<string, int, double, double>> listOfTempSites = new List<Tuple<string, int, double, double>>(); //site ID, index, delta min, epsilon max
            List<Tuple<string, int, double, double>> listOfPermSites = new List<Tuple<string, int, double, double>>();

            Site theDepot = preprocessedSites[0];
            listOfPermSites.Add(new Tuple<string, int, double, double>(theDepot.ID, 0, 0.0, epsilon_Max[0]));

            for (int j= 1;j<preprocessedSites.Length;j++)
            {
                Site s = preprocessedSites[j];
                double epsilonMax = epsilon_Max[j];
                double tempDelta = EnergyConsumption(theDepot, s, VehicleCategories.EV) - epsilonMax;
                if (tempDelta<0.0)
                    tempDelta = 0.0;
                listOfTempSites.Add(new Tuple<string, int, double, double>(s.ID, j, tempDelta, epsilonMax));
            }          

            while(listOfTempSites.Count!=0)
            {
                Tuple<string, int, double, double> siteToPerm = listOfTempSites[0];
                foreach (Tuple<string, int, double, double> tempSite in listOfTempSites)
                {
                    if (tempSite.Item3 < siteToPerm.Item3)
                    {
                        siteToPerm = tempSite;
                    }
                }
                listOfTempSites.Remove(siteToPerm);
                listOfPermSites.Add(siteToPerm);
                foreach(Tuple<string, int, double, double> tempSite in listOfTempSites)
                {

                }
            }


            return delta_Min;
        }

        protected List<double> SetDeltaMaxViaLabelSetting()
        {
            List<double> delta_Max = new List<double>();

            return delta_Max;
        }

        protected double Distance(Site from, Site to)
        {
            return theProblemModel.SRD.GetDistance(from.ID, to.ID);
        }
        protected double EnergyConsumption(Site from, Site to, VehicleCategories vehicleCategory)
        {
            if (vehicleCategory == VehicleCategories.GDV)
                return 0;
            return theProblemModel.SRD.GetEVEnergyConsumption(from.ID, to.ID);
        }
        protected double BatteryCapacity(VehicleCategories vehicleCategory)
        {
            return theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).BatteryCapacity;
        }
        protected double TravelTime(Site from, Site to)
        {
            return theProblemModel.SRD.GetTravelTime(from.ID, to.ID);
        }
        protected double ServiceDuration(Site site)
        {
            return site.ServiceDuration;
        }
        protected double RechargingRate(Site site)
        {
            return site.RechargingRate;
        }
        protected double Prize(Site site, VehicleCategories vehicleCategory)
        {
            return site.GetPrize(vehicleCategory);
        }

        //TODO: Complete the GetCompleteSolution method at XCPlexVRPBase level
        //public override SolutionBase GetCompleteSolution(Type SolutionType)
        //{
        //    SolutionBase output;

        //    if(SolutionType == typeof(CustomerSetBasedSolution))
        //    {
        //        output = new CustomerSetBasedSolution()
        //    }

        //    switch (SolutionType)
        //    {
        //        case (CustomerSetBasedSolution):
        //            break;
        //    }

        //    return output;
        //}
    }
}
