using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MPMFEVRP.Models.XCPlex
{
    public abstract class XCPlexVRPBase : XCPlexBase
    {
        //The originals
        List<SiteWithAuxiliaryVariables> allOriginalSWAVs = new List<SiteWithAuxiliaryVariables>();
        List<SiteWithAuxiliaryVariables> depots; protected List<SiteWithAuxiliaryVariables> Depots { get { return depots; } }
        List<SiteWithAuxiliaryVariables> customers; protected List<SiteWithAuxiliaryVariables> Customers { get { return customers; } }
        List<SiteWithAuxiliaryVariables> externalStations; protected List<SiteWithAuxiliaryVariables> ExternalStations { get { return externalStations; } }

        //How do we want to process the auxiliary variable bounds
        bool useTighterBounds = true;

        //The preprocessed (duplicated) ones
        protected SiteWithAuxiliaryVariables[] preprocessedSites;//Ready-to-use
        protected int NumPreprocessedSites { get { return preprocessedSites.Length; } }
        protected int FirstESNodeIndex = int.MaxValue, LastESNodeIndex = int.MinValue, FirstCustomerNodeIndex = int.MaxValue, LastCustomerNodeIndex = int.MinValue;
        protected SiteWithAuxiliaryVariables TheDepot { get { return depots.First(); } } //There is a single depot

        protected int vIndex_EV = -1, vIndex_GDV = -1;
        protected int[] numVehicles;

        protected double[] minValue_T, maxValue_T;
        protected double[] minValue_Delta, maxValue_Delta;
        protected double[] minValue_Epsilon, maxValue_Epsilon;
        protected double[][] BigDelta;
        protected double[][] BigT;

        protected RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        public XCPlexVRPBase() { }

        public XCPlexVRPBase(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam) : base(theProblemModel, xCplexParam) { }

        protected override void Initialize()
        {
            rechargingDuration_status = theProblemModel.RechargingDuration_status;
            useTighterBounds = xCplexParam.TighterAuxBounds;
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

        protected void PreprocessSites(int numCopiesOfEachES = 0)
        {
            foreach (Site s in theProblemModel.SRD.GetAllSitesList())
                allOriginalSWAVs.Add(new SiteWithAuxiliaryVariables(s));

            PopulateSubLists();
            CalculateBoundsForAllOriginalSWAVs();
            PopulatePreprocessedSWAVs(numCopiesOfEachES);
            SetFirstAndLastNodeIndices();
            PopulateAuxiliaryBoundArraysFromSWAVs();
            SetBigMvalues();
        }
        void PopulateSubLists()
        {
            depots = new List<SiteWithAuxiliaryVariables>();
            customers = new List<SiteWithAuxiliaryVariables>();
            externalStations = new List<SiteWithAuxiliaryVariables>();
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                switch (swav.SiteType)
                {
                    case SiteTypes.Depot:
                        depots.Add(swav);
                        break;
                    case SiteTypes.Customer:
                        customers.Add(swav);
                        break;
                    case SiteTypes.ExternalStation:
                        externalStations.Add(swav);
                        break;
                }
        }
        protected void CalculateBoundsForAllOriginalSWAVs()
        {
            CalculateEpsilonBounds();
            CalculateDeltaBounds();
            CalculateTBounds();
        }
        void CalculateEpsilonBounds()
        {
            double epsilonMax = double.MinValue;
            Vehicle theEV = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                switch (swav.SiteType)
                {
                    case SiteTypes.Customer:
                        epsilonMax = Calculators.MaxSOCGainAtSite(swav, theEV, maxStayDuration: swav.ServiceDuration);
                        break;
                    case SiteTypes.ExternalStation:
                        epsilonMax = Calculators.MaxSOCGainAtESSite(theProblemModel, swav, theEV);
                        break;
                    default:
                        epsilonMax = 0.0;
                        break;
                }
                swav.UpdateEpsilonBounds(epsilonMax);
            }
        }
        void CalculateDeltaBounds()
        {
            if (!useTighterBounds)
            {
                foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                    swav.UpdateDeltaBounds(theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity, 0.0);
            }
            else
            {
                CalculateDeltaMinsViaLabelSetting();
                CalculateDeltaMaxsViaLabelSetting_noShallows();
            }
        }
        void CalculateDeltaMinsViaLabelSetting()
        {
            List<SiteWithAuxiliaryVariables> tempSWAVs = new List<SiteWithAuxiliaryVariables>(allOriginalSWAVs);
            List<SiteWithAuxiliaryVariables> permSWAVs = new List<SiteWithAuxiliaryVariables>();

            foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                if (swav.SiteType == SiteTypes.Depot)
                    swav.UpdateDeltaMin(0.0);
                else
                    swav.UpdateDeltaMin(Math.Max(0, EnergyConsumption(swav, TheDepot, VehicleCategories.EV) - swav.EpsilonMax));

            while (tempSWAVs.Count != 0)
            {
                SiteWithAuxiliaryVariables swavToPerm = tempSWAVs.First();
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    if(swav.DeltaMin<swavToPerm.DeltaMin)
                    {
                        swavToPerm = swav.ShallowCopy();
                    }
                tempSWAVs.Remove(swavToPerm);
                permSWAVs.Add(swavToPerm);
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    swav.UpdateDeltaMin(Math.Min(swav.DeltaMin, Math.Max(0, swavToPerm.DeltaMin + theProblemModel.SRD.GetEVEnergyConsumption(swav.ID, swavToPerm.ID) - swav.EpsilonMax)));
            }
            if (allOriginalSWAVs.Count != permSWAVs.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMinViaLabelSetting could not produce proper delta bounds hence allOriginalSWAVs.Count!=permSWAVs.Count");
        }
        void CalculateDeltaMaxsViaLabelSetting()
        {
            List<SiteWithAuxiliaryVariables> tempSWAVs = new List<SiteWithAuxiliaryVariables>(allOriginalSWAVs);
            List<SiteWithAuxiliaryVariables> permSWAVs = new List<SiteWithAuxiliaryVariables>();

            foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                if (swav.SiteType == SiteTypes.Depot)
                {
                    swav.UpdateDeltaMax(0.0);
                    swav.UpdateDeltaPrimeMax(BatteryCapacity(VehicleCategories.EV));
                }
                else
                {
                    swav.UpdateDeltaMax(BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(TheDepot, swav, VehicleCategories.EV));
                    swav.UpdateDeltaPrimeMax(Math.Min(BatteryCapacity(VehicleCategories.EV), (swav.DeltaMax + swav.EpsilonMax)));
                }
            while (tempSWAVs.Count != 0)
            {
                SiteWithAuxiliaryVariables swavToPerm = tempSWAVs.First();
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    if (swav.DeltaPrimeMax > swavToPerm.DeltaPrimeMax)
                    {
                        swavToPerm = swav.ShallowCopy();
                    }
                tempSWAVs.Remove(swavToPerm);
                permSWAVs.Add(swavToPerm);
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                {
                    swav.UpdateDeltaMax(Math.Max(swav.DeltaMax, swavToPerm.DeltaPrimeMax - theProblemModel.SRD.GetEVEnergyConsumption(swav.ID, swavToPerm.ID)));
                    swav.UpdateDeltaPrimeMax(Math.Min(BatteryCapacity(VehicleCategories.EV), (swav.DeltaMax + swav.EpsilonMax)));
                }
            }
            if (allOriginalSWAVs.Count != permSWAVs.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMaxViaLabelSetting could not produce proper delta bounds hence allOriginalSWAVs.Count!=permSWAVs.Count");

            //Revisiting the depot
            double eMinToDepot = double.MaxValue;
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                if ((swav.X != TheDepot.X) || (swav.Y != TheDepot.Y))
                {
                    eMinToDepot = Math.Min(eMinToDepot, EnergyConsumption(swav, TheDepot, VehicleCategories.EV));
                }
            TheDepot.UpdateDeltaMax(BatteryCapacity(VehicleCategories.EV) - eMinToDepot);
        }
        void CalculateDeltaMaxsViaLabelSetting_noShallows()
        {
            List<SiteWithAuxiliaryVariables> tempSWAVs = new List<SiteWithAuxiliaryVariables>(allOriginalSWAVs);
            List<SiteWithAuxiliaryVariables> permSWAVs = new List<SiteWithAuxiliaryVariables>();

            foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                if (swav.SiteType == SiteTypes.Depot)
                {
                    swav.UpdateDeltaMax(0.0);
                    swav.UpdateDeltaPrimeMax(BatteryCapacity(VehicleCategories.EV));
                }
                else
                {
                    swav.UpdateDeltaMax(BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(TheDepot, swav, VehicleCategories.EV));
                    swav.UpdateDeltaPrimeMax(Math.Min(BatteryCapacity(VehicleCategories.EV), (swav.DeltaMax + swav.EpsilonMax)));
                }
            while (tempSWAVs.Count != 0)
            {
                SiteWithAuxiliaryVariables swavToPerm = tempSWAVs.First();
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    if (swav.DeltaPrimeMax > swavToPerm.DeltaPrimeMax)
                    {
                        swavToPerm = swav;
                    }
                tempSWAVs.Remove(swavToPerm);
                permSWAVs.Add(swavToPerm);
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                {
                    swav.UpdateDeltaMax(Math.Max(swav.DeltaMax, swavToPerm.DeltaPrimeMax - theProblemModel.SRD.GetEVEnergyConsumption(swav.ID, swavToPerm.ID)));
                    swav.UpdateDeltaPrimeMax(Math.Min(BatteryCapacity(VehicleCategories.EV), (swav.DeltaMax + swav.EpsilonMax)));
                }
            }
            if (allOriginalSWAVs.Count != permSWAVs.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMaxViaLabelSetting could not produce proper delta bounds hence allOriginalSWAVs.Count!=permSWAVs.Count");

            //Revisiting the depot
            double eMinToDepot = double.MaxValue;
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                if ((swav.X != TheDepot.X) || (swav.Y != TheDepot.Y))
                {
                    eMinToDepot = Math.Min(eMinToDepot, EnergyConsumption(swav, TheDepot, VehicleCategories.EV));
                }
            TheDepot.UpdateDeltaMax(BatteryCapacity(VehicleCategories.EV) - eMinToDepot);
        }
        void CalculateTBounds()
        {
                double tLS = double.MinValue;
                double tES = double.MaxValue;
                Site theDepot = theProblemModel.SRD.GetSingleDepotSite();
                foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                {

                    tLS = theProblemModel.CRD.TMax - TravelTime(swav, theDepot);
                    tES = TravelTime(theDepot, swav);
                    switch (swav.SiteType)
                    {
                        case SiteTypes.Customer:
                            tLS -= ServiceDuration(swav);
                            break;
                        case SiteTypes.ExternalStation:
                            if (theProblemModel.RechargingDuration_status == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                                tLS -= (BatteryCapacity(VehicleCategories.EV) / RechargingRate(swav));
                            break;
                        default:
                            break;
                    }
                    swav.UpdateTBounds(tLS, tES);
                }
        }

        protected void PopulatePreprocessedSWAVs(int numCopiesOfEachES = 0)
        {
            List<SiteWithAuxiliaryVariables> preprocessedSWAVs_list = new List<SiteWithAuxiliaryVariables>();
            foreach(SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                if (swav.SiteType == SiteTypes.ExternalStation)
                    preprocessedSWAVs_list.AddRange(SWAVDuplicates(swav, numCopiesOfEachES));
                else
                    preprocessedSWAVs_list.Add(swav);
            }
            preprocessedSites = preprocessedSWAVs_list.ToArray();
        }
        /// <summary>
        /// Returns a list of deep-copied SWAVs
        /// </summary>
        /// <param name="swav"></param> The original SWAV to be duplicated
        /// <param name="numCopies"></param>Total number of copies, including the original
        /// <returns></returns>
        List<SiteWithAuxiliaryVariables> SWAVDuplicates(SiteWithAuxiliaryVariables swav, int numCopies)
        {
            List<SiteWithAuxiliaryVariables> outcome = new List<SiteWithAuxiliaryVariables>();
            if (numCopies > 0)
            {
                outcome.Add(swav);
                while (outcome.Count < numCopies)
                    outcome.Add(swav);
            }
            return outcome;
        }
        void SetFirstAndLastNodeIndices()
        {
            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                if (preprocessedSites[i].SiteType == SiteTypes.Customer)
                {
                    if (FirstCustomerNodeIndex == int.MaxValue)
                        FirstCustomerNodeIndex = i;
                        LastCustomerNodeIndex = i;
                }
                if (preprocessedSites[i].SiteType == SiteTypes.ExternalStation)
                {
                    if (FirstESNodeIndex == int.MaxValue)
                        FirstESNodeIndex = i;
                        LastESNodeIndex = i;
                }
            }
        }

        void PopulateAuxiliaryBoundArraysFromSWAVs()
        {
            //Make sure you invoke this method when preprocessedSWAVs have been created!
            minValue_Epsilon = new double[NumPreprocessedSites];
            maxValue_Epsilon = new double[NumPreprocessedSites];
            minValue_Delta = new double[NumPreprocessedSites];
            maxValue_Delta = new double[NumPreprocessedSites];
            minValue_T = new double[NumPreprocessedSites];
            maxValue_T = new double[NumPreprocessedSites];
            for (int i=0;i<preprocessedSites.Length;i++)
            {
                minValue_Epsilon[i] = preprocessedSites[i].EpsilonMin;
                maxValue_Epsilon[i] = preprocessedSites[i].EpsilonMax;
                minValue_Delta[i] = preprocessedSites[i].DeltaMin;
                maxValue_Delta[i] = preprocessedSites[i].DeltaMax;
                minValue_T[i] = preprocessedSites[i].TES;
                maxValue_T[i] = preprocessedSites[i].TLS;
            }
        }

        protected void SetBigMvalues()
        {
            BigDelta = new double[NumPreprocessedSites][];
            BigT = new double[NumPreprocessedSites][];

            for (int i = 0; i < NumPreprocessedSites; i++)
            {
                BigDelta[i] = new double[NumPreprocessedSites];
                BigT[i] = new double[NumPreprocessedSites];

                for (int j = 0; j < NumPreprocessedSites; j++)
                {
                    BigDelta[i][j] = maxValue_Delta[j] - minValue_Delta[i] - minValue_Epsilon[i];
                    BigT[i][j] = maxValue_T[i] - minValue_T[j];
                }
            }
        }

        public abstract List<VehicleSpecificRoute> GetVehicleSpecificRoutes();
        public abstract void RefineDecisionVariables(CustomerSet cS);

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
