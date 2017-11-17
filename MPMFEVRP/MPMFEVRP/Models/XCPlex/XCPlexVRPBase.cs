using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Utils;
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

            List<FlexibleStringMultiDoubleTuple> tempListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>(); //List of SiteID, epsilon_Max, delta_Min
            List<FlexibleStringMultiDoubleTuple> permListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>(); //List of SiteID, epsilon_Max, delta_Min

            Site theDepot = preprocessedSites[0];
            tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(theDepot.ID, epsilon_Max[0],0.0));

            for (int j= 1;j<preprocessedSites.Length;j++)
            {
                Site s = preprocessedSites[j];
                double epsilonMax = epsilon_Max[j];
                double tempDeltaMin = Math.Max(0,EnergyConsumption(s, theDepot, VehicleCategories.EV) - epsilonMax);
                tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(s.ID, epsilonMax, tempDeltaMin));
            }          

            while(tempListOfSiteAuxiliaryBounds.Count!=0)
            {
                FlexibleStringMultiDoubleTuple siteToPerm = new FlexibleStringMultiDoubleTuple(tempListOfSiteAuxiliaryBounds.First().ID);
                int indexToRemove = 0;
                for (int i=0; i<tempListOfSiteAuxiliaryBounds.Count; i++)
                {
                    if (tempListOfSiteAuxiliaryBounds[i].Item2 < siteToPerm.Item2)
                    {
                        siteToPerm = new FlexibleStringMultiDoubleTuple(tempListOfSiteAuxiliaryBounds[i]);
                        indexToRemove = i;
                    }
                }
                tempListOfSiteAuxiliaryBounds.RemoveAt(indexToRemove);
                permListOfSiteAuxiliaryBounds.Add(siteToPerm);
                for(int i=0; i<tempListOfSiteAuxiliaryBounds.Count; i++)
                {
                    FlexibleStringMultiDoubleTuple theOtherNodesTempData = tempListOfSiteAuxiliaryBounds[i];
                    double minEnergyConsumption = theProblemModel.SRD.GetEVEnergyConsumption(theOtherNodesTempData.ID, siteToPerm.ID);
                    double tempDeltaMin = Math.Min(theOtherNodesTempData.Item2, Math.Max(0,siteToPerm.Item2+minEnergyConsumption- theOtherNodesTempData.Item1));
                    if ((tempDeltaMin <= 0.001) && (theOtherNodesTempData.Item1 <= 0.001))
                        Console.WriteLine("Error with delta min for node " + theOtherNodesTempData.ID+"!");
                    tempListOfSiteAuxiliaryBounds[i].SetItem2(tempDeltaMin);
                }
            }
            if (preprocessedSites.Length != permListOfSiteAuxiliaryBounds.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMinViaLabelSetting could not produce proper delta bounds hence preprocessedSites.Length!=permListOfSiteAuxiliaryBounds.Count");
            for(int i=0; i<preprocessedSites.Length; i++)
            {
                for (int j = 0; j < permListOfSiteAuxiliaryBounds.Count; j++)
                    if (preprocessedSites[i].ID == permListOfSiteAuxiliaryBounds[j].ID)
                    {
                        delta_Min[i] = permListOfSiteAuxiliaryBounds[j].Item2;
                        break;
                    }
            }
            return delta_Min;
        }

        protected double[] SetDeltaMaxViaLabelSetting(double[] epsilon_Max)
        {
            double[] delta_Max = new double[preprocessedSites.Length]; //final outcome

            List<FlexibleStringMultiDoubleTuple> tempListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>(); // List of SiteID, epsilon_Max, delta_Max, deltaPrime_Max
            List<FlexibleStringMultiDoubleTuple> permListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>();

            Site theDepot = preprocessedSites[0];
            tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(theDepot.ID, epsilon_Max[0], 0.0, BatteryCapacity(VehicleCategories.EV)));

            for (int j = 1; j < preprocessedSites.Length; j++)
            {
                Site s = preprocessedSites[j];
                double epsilonMax = epsilon_Max[j];
                double tempDeltaMax = BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(theDepot, s, VehicleCategories.EV);
                double tempDeltaPrimeMax = Math.Min(BatteryCapacity(VehicleCategories.EV),(tempDeltaMax+epsilonMax));
                tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(s.ID, epsilonMax, tempDeltaMax,tempDeltaPrimeMax));
            }

            while (tempListOfSiteAuxiliaryBounds.Count != 0)
            {
                FlexibleStringMultiDoubleTuple siteToPerm = new FlexibleStringMultiDoubleTuple(tempListOfSiteAuxiliaryBounds.First().ID);
                int indexToRemove = 0;
                for (int i = 0; i < tempListOfSiteAuxiliaryBounds.Count; i++)
                {
                    if (tempListOfSiteAuxiliaryBounds[i].Item3 > siteToPerm.Item3)
                    {
                        siteToPerm = new FlexibleStringMultiDoubleTuple(tempListOfSiteAuxiliaryBounds[i]);
                        indexToRemove = i;
                    }
                }
                tempListOfSiteAuxiliaryBounds.RemoveAt(indexToRemove);
                permListOfSiteAuxiliaryBounds.Add(siteToPerm);
                for (int i = 0; i < tempListOfSiteAuxiliaryBounds.Count; i++)
                {
                    double minEnergyConsumption = theProblemModel.SRD.GetEVEnergyConsumption(tempListOfSiteAuxiliaryBounds[i].ID, siteToPerm.ID);
                    double tempDeltaMax = Math.Max(tempListOfSiteAuxiliaryBounds[i].Item2,siteToPerm.Item3-minEnergyConsumption);
                    double tempDeltaPrimeMax = Math.Min(BatteryCapacity(VehicleCategories.EV), (tempDeltaMax + tempListOfSiteAuxiliaryBounds[i].Item1));
                    tempListOfSiteAuxiliaryBounds[i].SetItem2(tempDeltaMax);
                    tempListOfSiteAuxiliaryBounds[i].SetItem3(tempDeltaPrimeMax);
                }
            }
            if (preprocessedSites.Length != permListOfSiteAuxiliaryBounds.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMaxViaLabelSetting could not produce proper delta bounds hence preprocessedSites.Length!=permListOfSiteAuxiliaryBounds.Count");
            for (int i = 0; i < preprocessedSites.Length; i++)
            {
                for (int j = 0; j < permListOfSiteAuxiliaryBounds.Count; j++)
                    if (preprocessedSites[i].ID == permListOfSiteAuxiliaryBounds[j].ID)
                    {
                        delta_Max[i] = permListOfSiteAuxiliaryBounds[j].Item2;
                        break;
                    }
            }
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
