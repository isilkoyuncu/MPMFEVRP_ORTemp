using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using MPMFEVRP.Models;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Utils;


namespace MPMFEVRP.Implementations.Algorithms
{
    public class PathBasedApproach : AlgorithmBase
    {
        //Problem parameters
        int numberOfEVs;
        int numberOfGDVs;
        int totalNumVeh;
        int numCustomers;
        int numNonESNodes;
        int numVehCategories;
        Site theDepot;
        List<SiteWithAuxiliaryVariables>  externalStations = new List<SiteWithAuxiliaryVariables>();
        List<SiteWithAuxiliaryVariables> nonESNodes = new List<SiteWithAuxiliaryVariables>();
        List<SiteWithAuxiliaryVariables> allOriginalSWAVs = new List<SiteWithAuxiliaryVariables>();

        RefuelingPathGenerator rpg;
        RefuelingPathList rpl; public RefuelingPathList RPL { get => rpl; }//Non-dominated refueling path list
        AllPairsShortestPaths apss;

        CustomerSetBasedSolution solution = new CustomerSetBasedSolution();
        List<CustomerSetBasedSolution> allSolutions;

        public override void AddSpecializedParameters()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            //Problem param
            this.theProblemModel = theProblemModel;
            numberOfEVs = theProblemModel.ProblemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            numberOfGDVs = theProblemModel.ProblemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            totalNumVeh = numberOfEVs + numberOfGDVs;
            numCustomers = theProblemModel.SRD.NumCustomers;
            theDepot = theProblemModel.SRD.GetSingleDepotSite();
            numNonESNodes = numCustomers + 1; // customers + 1(for the depot)
            numVehCategories = theProblemModel.VRD.NumVehicleCategories;
            PreprocessSwavs();
            CalculateNondominatedRefuelingPaths();

            //Solution stat
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            allSolutions = new List<CustomerSetBasedSolution>();
        }
        public override void SpecializedRun()
        {
            throw new NotImplementedException();
        }
        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }
        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }
        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }

        //Calculate upper and lower bounds for auxiliary variables
        void PreprocessSwavs()
        {
            foreach (Site s in theProblemModel.SRD.GetAllSitesList())
            {
                allOriginalSWAVs.Add(new SiteWithAuxiliaryVariables(s));
            }           
                externalStations = new List<SiteWithAuxiliaryVariables>();
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                if (swav.SiteType == SiteTypes.ExternalStation)
                    externalStations.Add(swav);
                else
                    nonESNodes.Add(swav);


            CalculateBoundsForAllOriginalSWAVs();
        }
        void CalculateBoundsForAllOriginalSWAVs()
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
            CalculateDeltaMinsViaLabelSetting();
            CalculateDeltaMaxsViaLabelSetting();
        }
        void CalculateDeltaMinsViaLabelSetting()
        {
            List<SiteWithAuxiliaryVariables> tempSWAVs = new List<SiteWithAuxiliaryVariables>(allOriginalSWAVs);
            List<SiteWithAuxiliaryVariables> permSWAVs = new List<SiteWithAuxiliaryVariables>();

            foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                if (swav.SiteType == SiteTypes.Depot)
                    swav.UpdateDeltaMin(0.0);
                else
                    swav.UpdateDeltaMin(Math.Max(0, EnergyConsumption(swav, theDepot, VehicleCategories.EV) - swav.EpsilonMax));
           
            while (tempSWAVs.Count != 0)
            {
                SiteWithAuxiliaryVariables swavToPerm = tempSWAVs.First();
                foreach (SiteWithAuxiliaryVariables swav in tempSWAVs)
                    if (swav.DeltaMin < swavToPerm.DeltaMin)
                    {
                        swavToPerm = swav;
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
                    swav.UpdateDeltaMax(BatteryCapacity(VehicleCategories.EV) - EnergyConsumption(theDepot, swav, VehicleCategories.EV));
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

            //Revisiting the depot and its duplicates
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                if ((swav.X == theDepot.X) && (swav.Y == theDepot.Y))
                    if (swav.SiteType != SiteTypes.Customer)
                        swav.UpdateDeltaMax(BatteryCapacity(VehicleCategories.EV) - GetMinEnergyConsumptionFromNonDepotToDepot());
        }
        void CalculateTBounds()
        {
            double tLS = double.MinValue;
            double tES = double.MaxValue;
            Site theDepot = theProblemModel.SRD.GetSingleDepotSite();
            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
            {
                if (swav.X == theDepot.X && swav.Y == theDepot.Y)
                {
                    if (swav.SiteType != SiteTypes.Depot)
                    {
                        tLS = theProblemModel.CRD.TMax - GetMinTravelTimeFromDepotDuplicateToDepotThroughANode(swav);
                        tES = GetMinTravelTimeFromDepotToDepotDuplicateThroughANode(swav);
                    }
                    else
                    {
                        tLS = theProblemModel.CRD.TMax;
                        tES = 0.0;
                    }
                }
                else
                {
                    tLS = theProblemModel.CRD.TMax - TravelTime(swav, theDepot);
                    tES = TravelTime(theDepot, swav);
                }
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
        double GetMinTravelTimeFromDepotDuplicateToDepotThroughANode(SiteWithAuxiliaryVariables depotDuplicate)
        {
            Site theDepot = theProblemModel.SRD.GetSingleDepotSite();
            double minTravelTime = double.MaxValue;
            foreach (SiteWithAuxiliaryVariables otherSwav in allOriginalSWAVs)
                if (otherSwav.X != theDepot.X || otherSwav.Y != theDepot.Y)
                    minTravelTime = Math.Min(minTravelTime, TravelTime(depotDuplicate, otherSwav) + TravelTime(otherSwav, theDepot));

            return minTravelTime;
        }
        double GetMinTravelTimeFromDepotToDepotDuplicateThroughANode(SiteWithAuxiliaryVariables depotDuplicate)
        {
            Site theDepot = theProblemModel.SRD.GetSingleDepotSite();
            double minTravelTime = double.MaxValue;
            foreach (SiteWithAuxiliaryVariables otherSwav in allOriginalSWAVs)
                if (otherSwav.X != theDepot.X || otherSwav.Y != theDepot.Y)
                    minTravelTime = Math.Min(minTravelTime, TravelTime(theDepot, otherSwav) + TravelTime(otherSwav, depotDuplicate));

            return minTravelTime;
        }
        double GetMinEnergyConsumptionFromNonDepotToDepot()
        {
            Site theDepot = theProblemModel.SRD.GetSingleDepotSite();
            double eMinToDepot = double.MaxValue;

            foreach (SiteWithAuxiliaryVariables swav in allOriginalSWAVs)
                if ((swav.X != this.theDepot.X) || (swav.Y != this.theDepot.Y))
                    eMinToDepot = Math.Min(eMinToDepot, EnergyConsumption(swav, this.theDepot, VehicleCategories.EV));

            return eMinToDepot;
        }

        void CalculateNondominatedRefuelingPaths()
        {
            rpg = new RefuelingPathGenerator(theProblemModel);
            rpl = new RefuelingPathList();
            for (int i = 0; i < numNonESNodes; i++)
                for (int j = 0; j < numNonESNodes; j++)
                    if (i != j)
                        rpl.AddRange(rpg.GenerateNonDominatedBetweenODPair(nonESNodes[i], nonESNodes[j], theProblemModel.SRD));

            //rpl.AddRange(rpg.GenerateNonDominatedBetweenODPair(nonESNodes[1], nonESNodes[2], externalStations, theProblemModel.SRD));
        }
        public void CalculateNDRP_Dynamically(double departureSOE_i, double departureTime_i)
        {

        }
        //Necessarry shortcuts
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
        public override string GetName()
        {
            return "Path-based Approach";
        }
        public override bool setListener(IListener listener)
        {
            throw new NotImplementedException();
        }
    }
}
