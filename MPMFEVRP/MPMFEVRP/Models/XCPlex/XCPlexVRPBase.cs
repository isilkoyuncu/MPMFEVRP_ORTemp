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

        //The preprocessed (duplicated) ones
        protected SiteWithAuxiliaryVariables[] preprocessedSWAVs;//Ready-to-use
        protected int NumPreprocessedSWAVs { get { return preprocessedSWAVs.Length; } }
        protected int FirstESNodeIndex = int.MaxValue, LastESNodeIndex = int.MinValue, FirstCustomerNodeIndex = int.MaxValue, LastCustomerNodeIndex = int.MinValue;
        protected Site TheDepot { get { return depots.First(); } } //There is a single depot


        //protected Site[] preprocessedSites;//Ready-to-use
        //protected int NumPreprocessedSites { get { return preprocessedSites.Length; } }
        //protected List<Site> depots;
        //protected List<Site> customers;
        //protected List<Site> externalStations;//Preprocessed, Ready-to-use
        protected int vIndex_EV = -1, vIndex_GDV = -1;
        protected int[] numVehicles;

        protected double[] minValue_T, maxValue_T;
        protected double[] minValue_Delta, maxValue_Delta;
        protected double[] minValue_Epsilon, maxValue_Epsilon;
        protected double[][] BigDelta;
        protected double[][] BigT;

        protected RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;

        public XCPlexVRPBase() { }

        public XCPlexVRPBase(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam) : base(theProblemModel, xCplexParam)
        {
        }

        protected void PreprocessSites(int numCopiesOfEachES = 0)
        {
            PopulateAllOriginalSWAVs();
            PopulateSubLists();
            PopulatePreprocessedSWAVs(numCopiesOfEachES);
            SetFirstAndLastNodeIndices();
            PopulateAuxiliaryBoundArraysFromSWAVs();
            SetBigMvalues();
        }
        protected void PopulateAllOriginalSWAVs()
        {
            foreach (Site s in theProblemModel.SRD.GetAllSitesList())
                allOriginalSWAVs.Add(new SiteWithAuxiliaryVariables(s));
            //ISSUE (#5): Use of bounding approaches should be tied to a parameter with the following levels: Really Loose (0 for min, BatteryCap,BatteryCap,TMax for max); Really Tight (use label setting where appropriate)
            CalculateEpsilonBounds();
            CalculateDeltaBounds();
            CalculateTBounds();
        }
        void CalculateEpsilonBounds()
        {
            //ISSUE (#9): Make sure epsilonMax matches what's written in the paper by 100%!
            double epsilonMax = double.MinValue;
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                switch (swav.SiteType)
                {
                    case SiteTypes.Customer:
                        epsilonMax = Math.Min(BatteryCapacity(VehicleCategories.EV), swav.ServiceDuration * RechargingRate(swav)); //Utils.Calculators.MaxSOCGainAtSite(s, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV), maxStayDuration: s.ServiceDuration);
                        break;
                    case SiteTypes.ExternalStation://TODO: Unit test the above utility function. It should give us MaxSOCGainAtSite s with EV.
                        epsilonMax = BatteryCapacity(VehicleCategories.EV);
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
            bool useLooseBounds = false;//This is the one that'll be tied to the user-selected parameter
            if (useLooseBounds)
            {
                foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                    swav.UpdateDeltaBounds(theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity, 0.0);
            }
            else
            {
                CalculateDeltaMinsViaLabelSetting();
                CalculateDeltaMaxsViaLabelSetting();
            }
        }
        //TODO: finish these two methods
        void CalculateDeltaMinsViaLabelSetting()
        { 
        }
        void CalculateDeltaMaxsViaLabelSetting()
        {
        }
        void CalculateTBounds()
        {
            bool useLooseBounds = false;//This is the one that'll be tied to the user-selected parameter
            if (useLooseBounds)
            {
                foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                    swav.UpdateTBounds(theProblemModel.CRD.TMax, 0.0);
            }
            else
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
                }
            }
        }
        void PopulateSubLists()
        {
            depots = new List<SiteWithAuxiliaryVariables>();
            customers = new List<SiteWithAuxiliaryVariables>();
            externalStations = new List<SiteWithAuxiliaryVariables>();
            foreach(SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                switch(swav.SiteType)
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
            preprocessedSWAVs = preprocessedSWAVs_list.ToArray();
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
                    outcome.Add(new SiteWithAuxiliaryVariables(swav));
            }
            return outcome;
        }
        void SetFirstAndLastNodeIndices()
        {
            for(int i = 0; i < NumPreprocessedSWAVs; i++)
            {
                if(preprocessedSWAVs[i].SiteType== SiteTypes.Customer)
                {
                    if (FirstCustomerNodeIndex == int.MaxValue)
                        FirstCustomerNodeIndex = i;
                    if (LastCustomerNodeIndex == int.MinValue)
                        LastCustomerNodeIndex = i;
                }
                if (preprocessedSWAVs[i].SiteType == SiteTypes.ExternalStation)
                {
                    if (FirstESNodeIndex == int.MaxValue)
                        FirstESNodeIndex = i;
                    if (LastESNodeIndex == int.MinValue)
                        LastESNodeIndex = i;
                }
            }
        }
        void PopulateAuxiliaryBoundArraysFromSWAVs()
        {
            //Make sure you invoke this method when preprocessedSWAVs have been created!
            minValue_Epsilon = new double[NumPreprocessedSWAVs];
            maxValue_Epsilon = new double[NumPreprocessedSWAVs];
            minValue_Delta = new double[NumPreprocessedSWAVs];
            maxValue_Delta = new double[NumPreprocessedSWAVs];
            minValue_T = new double[NumPreprocessedSWAVs];
            maxValue_T = new double[NumPreprocessedSWAVs];
            for (int i=0;i<preprocessedSWAVs.Length;i++)
            {
                minValue_Epsilon[i] = preprocessedSWAVs[i].EpsilonMin;
                maxValue_Epsilon[i] = preprocessedSWAVs[i].EpsilonMax;
                minValue_Delta[i] = preprocessedSWAVs[i].DeltaMin;
                maxValue_Delta[i] = preprocessedSWAVs[i].DeltaMax;
                minValue_T[i] = preprocessedSWAVs[i].TES;
                maxValue_T[i] = preprocessedSWAVs[i].TLS;
            }
        }
        protected void SetBigMvalues()
        {
            BigDelta = new double[NumPreprocessedSWAVs][];
            BigT = new double[NumPreprocessedSWAVs][];

            for (int i = 0; i < NumPreprocessedSWAVs; i++)
            {
                BigDelta[i] = new double[NumPreprocessedSWAVs];
                BigT[i] = new double[NumPreprocessedSWAVs];

                for (int j = 0; j < NumPreprocessedSWAVs; j++)
                {
                    BigDelta[i][j] = maxValue_Delta[j] - minValue_Delta[i] - minValue_Epsilon[i];
                    BigT[i][j] = maxValue_T[i] - minValue_T[j];
                }
            }
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
            minValue_T = new double[NumPreprocessedSWAVs];
            maxValue_T = new double[NumPreprocessedSWAVs];
            minValue_Delta = new double[NumPreprocessedSWAVs];
            maxValue_Delta = new double[NumPreprocessedSWAVs];
            minValue_Epsilon = new double[NumPreprocessedSWAVs];
            maxValue_Epsilon = new double[NumPreprocessedSWAVs];

            for (int j = 0; j < NumPreprocessedSWAVs; j++)
            {
                Site s = preprocessedSWAVs[j];
                Site theDepot = Depots[0];

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
            Site[] allSites = new Site[theProblemModel.SRD.GetAllSitesArray().Length];
            allSites = theProblemModel.SRD.GetAllSitesArray();
            double[] delta_Min = new double[allSites.Length]; //final outcome

            List<FlexibleStringMultiDoubleTuple> tempListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>(); //List of SiteID, epsilon_Max, delta_Min
            List<FlexibleStringMultiDoubleTuple> permListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>(); //List of SiteID, epsilon_Max, delta_Min

            Site theDepot = allSites[0];
            tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(theDepot.ID, epsilon_Max[0], 0.0));

            for (int j = 1; j < allSites.Length; j++)
            {
                Site s = allSites[j];
                double epsilonMax = epsilon_Max[j];
                double tempDeltaMin = Math.Max(0, EnergyConsumption(s, theDepot, VehicleCategories.EV) - epsilonMax);
                tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(s.ID, epsilonMax, tempDeltaMin));
            }

            while (tempListOfSiteAuxiliaryBounds.Count != 0)
            {
                FlexibleStringMultiDoubleTuple siteToPerm = new FlexibleStringMultiDoubleTuple(tempListOfSiteAuxiliaryBounds.First().ID);
                int indexToRemove = 0;
                for (int i = 0; i < tempListOfSiteAuxiliaryBounds.Count; i++)
                {
                    if (tempListOfSiteAuxiliaryBounds[i].Item2 < siteToPerm.Item2)
                    {
                        siteToPerm = new FlexibleStringMultiDoubleTuple(tempListOfSiteAuxiliaryBounds[i]);
                        indexToRemove = i;
                    }
                }
                tempListOfSiteAuxiliaryBounds.RemoveAt(indexToRemove);
                permListOfSiteAuxiliaryBounds.Add(siteToPerm);
                for (int i = 0; i < tempListOfSiteAuxiliaryBounds.Count; i++)
                {
                    FlexibleStringMultiDoubleTuple theOtherNodesTempData = tempListOfSiteAuxiliaryBounds[i];
                    double minEnergyConsumption = theProblemModel.SRD.GetEVEnergyConsumption(theOtherNodesTempData.ID, siteToPerm.ID);
                    double tempDeltaMin = Math.Min(theOtherNodesTempData.Item2, Math.Max(0, siteToPerm.Item2 + minEnergyConsumption - theOtherNodesTempData.Item1));
                    if ((tempDeltaMin <= 0.001) && (theOtherNodesTempData.Item1 <= 0.001))
                        Console.WriteLine("Error with delta min for node " + theOtherNodesTempData.ID + "!");
                    tempListOfSiteAuxiliaryBounds[i].SetItem2(tempDeltaMin);
                }
            }
            if (allSites.Length != permListOfSiteAuxiliaryBounds.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMinViaLabelSetting could not produce proper delta bounds hence preprocessedSites.Length!=permListOfSiteAuxiliaryBounds.Count");
            for (int i = 0; i < allSites.Length; i++)
            {
                for (int j = 0; j < permListOfSiteAuxiliaryBounds.Count; j++)
                    if (allSites[i].ID == permListOfSiteAuxiliaryBounds[j].ID)
                    {
                        delta_Min[i] = permListOfSiteAuxiliaryBounds[j].Item2;
                        break;
                    }
            }
            return delta_Min;
        }

        protected double[] SetDeltaMaxViaLabelSetting(double[] epsilon_Max)
        {
            Site[] allSites = new Site[theProblemModel.SRD.GetAllSitesArray().Length];
            allSites = theProblemModel.SRD.GetAllSitesArray();
            double[] delta_Max = new double[allSites.Length]; //final outcome

            List<FlexibleStringMultiDoubleTuple> tempListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>(); // List of SiteID, epsilon_Max, delta_Max, deltaPrime_Max
            List<FlexibleStringMultiDoubleTuple> permListOfSiteAuxiliaryBounds = new List<FlexibleStringMultiDoubleTuple>();

            Site theDepot = allSites[0];
            tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(theDepot.ID, epsilon_Max[0], 0.0, BatteryCapacity(VehicleCategories.EV)));

            for (int j = 1; j < allSites.Length; j++)
            {
                Site s = allSites[j];
                double epsilonMax = epsilon_Max[j];
                double tempDeltaMax = BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(theDepot, s, VehicleCategories.EV);
                double tempDeltaPrimeMax = Math.Min(BatteryCapacity(VehicleCategories.EV), (tempDeltaMax + epsilonMax));
                tempListOfSiteAuxiliaryBounds.Add(new FlexibleStringMultiDoubleTuple(s.ID, epsilonMax, tempDeltaMax, tempDeltaPrimeMax));
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
                    double tempDeltaMax = Math.Max(tempListOfSiteAuxiliaryBounds[i].Item2, siteToPerm.Item3 - minEnergyConsumption);
                    double tempDeltaPrimeMax = Math.Min(BatteryCapacity(VehicleCategories.EV), (tempDeltaMax + tempListOfSiteAuxiliaryBounds[i].Item1));
                    tempListOfSiteAuxiliaryBounds[i].SetItem2(tempDeltaMax);
                    tempListOfSiteAuxiliaryBounds[i].SetItem3(tempDeltaPrimeMax);
                }
            }
            if (allSites.Length != permListOfSiteAuxiliaryBounds.Count)
                throw new System.Exception("XCPlexVRPBase.SetDeltaMaxViaLabelSetting could not produce proper delta bounds hence preprocessedSites.Length!=permListOfSiteAuxiliaryBounds.Count");
            for (int i = 0; i < allSites.Length; i++)
            {
                for (int j = 0; j < permListOfSiteAuxiliaryBounds.Count; j++)
                    if (allSites[i].ID == permListOfSiteAuxiliaryBounds[j].ID)
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
