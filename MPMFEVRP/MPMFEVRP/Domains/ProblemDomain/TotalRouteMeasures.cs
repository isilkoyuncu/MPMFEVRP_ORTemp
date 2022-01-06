using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.MFGVRPCGH.RawMaterial;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class TotalRouteMeasures
    {
        int numRoutes=1; public int NumRoutes { get { return numRoutes; } }
        int[] numVehicles = new int[2];
        double avgTotalTravelTime = 0.0; public double TotalTravelTime { get { return avgTotalTravelTime; } } //Note that this travel time includes the service durations and ES visits if any
        double avgTotalTravelDistance = 0.0; public double TotalTravelDistance { get { return avgTotalTravelDistance; } }
        int numESVisits = 0;
        int numCustomers;
        double dMax;
        double tMax;
        EVvsGDV_ProblemModel theProblemModel;
        MixedFleetGVRPMaterial theMaterial;
        Site theDepot;
        List<SiteWithAuxiliaryVariables> ESs, customers;
        List<string> roundTripVisitDepot = new List<string>();
        List<double> shortestEsVisitTravelTimeList = new List<double>();
        List<double> shortestEsVisitDistanceList = new List<double>();

        public TotalRouteMeasures(EVvsGDV_ProblemModel theProblemModel, Site theDepot, List<SiteWithAuxiliaryVariables> ESs, List<SiteWithAuxiliaryVariables> customers)
        {
            this.theProblemModel = theProblemModel;
            this.theDepot = theDepot;
            this.ESs = ESs;
            this.customers = customers;
            dMax = theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).ConsumptionRate;
            tMax = theDepot.DueDate;
            numCustomers = theProblemModel.SRD.NumCustomers;
            SetListOfShortestTimeAndDistanceOfEsVisitPairs();
            numVehicles[0] = theProblemModel.GetNumVehicles(VehicleCategories.EV);
            numVehicles[1] = theProblemModel.GetNumVehicles(VehicleCategories.GDV);
        }
        public TotalRouteMeasures(MixedFleetGVRPMaterial theMaterial, Site theDepot, List<SiteWithAuxiliaryVariables> ESs, List<SiteWithAuxiliaryVariables> customers)
        {
            this.theMaterial = theMaterial;
            this.theDepot = theDepot;
            this.ESs = ESs;
            this.customers = customers;
            dMax = theMaterial.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity / theMaterial.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).ConsumptionRate;
            tMax = theDepot.DueDate;
            numCustomers = theMaterial.SRD.NumCustomers;
            SetListOfShortestTimeAndDistanceOfEsVisitPairs();
            numVehicles[0] = theMaterial.ExperimentationParameters.NumberOfEVs;
            numVehicles[1] = theMaterial.ExperimentationParameters.NumberOfGDVs;
        }
        public void SetNumberOfVehiclesNeeded()
        {
            bool stoppingCondition = IsTimeFeasible() && IsDistanceFeasible();
            bool timeFeas = IsTimeFeasible();
            bool distFeas = IsDistanceFeasible();

            while (!stoppingCondition)
            {
                //System.Windows.Forms.MessageBox.Show("Both time and distance infeasible with: " + numRoutes + " routes and " + numESVisits + " ES visits.");
                while (!IsTimeFeasible())
                {
                    //System.Windows.Forms.MessageBox.Show("Time infeasible with: " + numRoutes + " routes and " + numESVisits + " ES visits.");
                    numRoutes++;
                    numESVisits = 0;
                }
                //System.Windows.Forms.MessageBox.Show("Time feasible with: " + numRoutes + " routes and " + numESVisits + " ES visits.");
                while (!IsDistanceFeasible())
                {
                    //System.Windows.Forms.MessageBox.Show("Distance infeasible with: " + numRoutes + " routes and " + numESVisits + " ES visits.");
                    numESVisits++;
                }
                //System.Windows.Forms.MessageBox.Show("Distance feasible with: " + numRoutes + " routes and " + numESVisits + " ES visits.");
                stoppingCondition = IsTimeFeasible() && IsDistanceFeasible();
            }
        }
        bool IsTimeFeasible()
        {
            avgTotalTravelTime = GetAvgTotalTravelTime();
            if (numRoutes >= avgTotalTravelTime / tMax)
                return true;
            else
                return false;
        }
        bool IsDistanceFeasible()
        {
            avgTotalTravelDistance = GetAvgTotalTravelDistance();
            double batteriesNeeded = (avgTotalTravelDistance / dMax);
            int fullBatteriesNeeded = (int)Math.Ceiling(batteriesNeeded);
            if (numVehicles[1] >= numRoutes) //If #GDVs greater than or equal to the number of routes we need, then we should say it is distance feasible in this case
                return true;
            else if (numRoutes + numESVisits >= fullBatteriesNeeded)
                return true;
            else
                return false;
        }
        double GetAvgTotalTravelDistance()
        {
            double avgTTCustomerDist = GetAvgTotalCustomerDistance();
            double avgTTDepotDist = GetAvgDepotDistance();
            double avgTTESDist = GetAvgTotalESDistance();
            double TotalTT = avgTTCustomerDist + avgTTDepotDist + avgTTESDist;
            return (GetAvgTotalCustomerDistance() + GetAvgDepotDistance() + GetAvgTotalESDistance());
        }
        double GetAvgTotalCustomerDistance()
        {
            double totalDistance = 0.0;
            double[] minDistancesFrom = new double[numCustomers];
            string[] idsOfminDistancesFrom = new string[numCustomers];
            double[] minDistancesTo = new double[numCustomers];
            string[] idsOfminDistancesTo = new string[numCustomers];

            for (int i = 0; i < customers.Count; i++)
            {
                Site customer = customers[i];
                double minDistance = double.MaxValue;
                string minDistanceID = "";
                if (Distance(customer, theDepot) < minDistance)
                {
                    minDistance = Distance(customer, theDepot);
                    minDistanceID = theDepot.ID;
                }
                for (int j = 0; j < customers.Count; j++)
                    if (customer.ID != customers[j].ID)
                    {
                        Site to = customers[j];
                        if (Distance(customer, to) < minDistance)
                        {
                            minDistance = Distance(customer, to);
                            minDistanceID = to.ID;
                        }
                    }
                minDistancesFrom[i] = minDistance;
                idsOfminDistancesFrom[i] = minDistanceID;

                minDistance = double.MaxValue;
                minDistanceID = "";

                if (Distance(theDepot, customer) < minDistance)
                {
                    minDistance = Distance(theDepot, customer);
                    minDistanceID = theDepot.ID;
                }
                for (int j = 0; j < customers.Count; j++)
                    if (customer.ID != customers[j].ID && customers[j].ID != idsOfminDistancesFrom[i])
                    {
                        Site from = customers[j];
                        if (Distance(from, customer) < minDistance)
                        {
                            minDistance = Distance(from, customer);
                            minDistanceID = from.ID;
                        }
                    }
                minDistancesTo[i] = minDistance;
                idsOfminDistancesTo[i] = minDistanceID;
                if (idsOfminDistancesFrom[i] == theDepot.ID && idsOfminDistancesTo[i] == theDepot.ID)
                    roundTripVisitDepot.Add(customer.ID);

                totalDistance = totalDistance + minDistancesFrom[i] + minDistancesTo[i];
            }

            return (totalDistance / 2.0);
        }
        double GetAvgDepotDistance()
        {
            double depotDistance = 0.0;

            double[] distancesFromDepot = new double[numCustomers];
            string[] idsOfdistancesFromDepot = new string[numCustomers];
            double[] distancesToDepot = new double[numCustomers];
            string[] idsOfdistancesToDepot = new string[numCustomers];

            for (int j = 0; j < customers.Count; j++)
            {
                distancesFromDepot[j] = Distance(theDepot, customers[j]);
                idsOfdistancesFromDepot[j] = customers[j].ID;

                distancesToDepot[j] = Distance(customers[j], theDepot);
                idsOfdistancesToDepot[j] = customers[j].ID;
            }
            Array.Sort(distancesFromDepot, idsOfdistancesFromDepot);
            Array.Sort(distancesToDepot, idsOfdistancesToDepot);

            int nArcsSelectedByDepot = 0;
            int i = 0;

            while (nArcsSelectedByDepot < 2 * numRoutes)
            {
                int count = 0;
                if (roundTripVisitDepot.Contains(idsOfdistancesFromDepot[i])) //TODO this is written as if distances are symmetric, if not this condition needs to be fixed!!
                {
                    count = 2;
                    depotDistance = depotDistance + distancesFromDepot[i] + distancesToDepot[i];
                }
                else
                {
                    count = 1;
                    depotDistance = depotDistance + Math.Min(distancesFromDepot[i], distancesToDepot[i]);
                }
                i++;
                nArcsSelectedByDepot = nArcsSelectedByDepot + count;
            }
            return depotDistance / 2.0;
        }
        double GetAvgTotalESDistance()  
        {
            double totalEsDistance = 0.0;

            for (int i = 0; i < numESVisits; i++)
                totalEsDistance = totalEsDistance + shortestEsVisitDistanceList[i];

            return (totalEsDistance / 2.0);
        }
        double GetAvgTotalTravelTime()
        {
            return (GetAvgTotalCustomerTravelTime() + GetAvgDepotTravelTime() + GetAvgTotalESTravelTime());
        }
        double GetAvgTotalCustomerTravelTime()
        {
            double totalTravelTime = 0.0;
            double[] minDurationsFrom = new double[numCustomers];
            string[] idsOfminDurationFrom = new string[numCustomers];
            double[] minDurationsTo = new double[numCustomers];
            string[] idsOfminDurationTo = new string[numCustomers];        

            for (int i = 0; i < customers.Count; i++)
            {
                Site customer = customers[i];
                double minDuration = double.MaxValue;
                string minDurationID = "";
                if(TravelTime(customer,theDepot)<minDuration)
                {
                    minDuration = TravelTime(customer,theDepot);
                    minDurationID = theDepot.ID;
                }
                for (int j = 0; j < customers.Count; j++)
                    if (customer.ID != customers[j].ID)
                    {
                        Site to = customers[j];
                        if (TravelTime(customer, to) < minDuration)
                        {
                            minDuration = TravelTime(customer, to);
                            minDurationID = to.ID;
                        }
                    }
                minDurationsFrom[i] = minDuration;
                idsOfminDurationFrom[i] = minDurationID;

                minDuration = double.MaxValue;
                minDurationID = "";

                if (TravelTime(theDepot,customer) < minDuration)
                {
                    minDuration = TravelTime(theDepot,customer);
                    minDurationID = theDepot.ID;
                }
                for (int j = 0; j < customers.Count; j++)
                    if (customer.ID != customers[j].ID && customers[j].ID!= idsOfminDurationFrom[i])
                    {
                        Site from = customers[j];
                        if (TravelTime(from, customer) < minDuration)
                        {
                            minDuration = TravelTime(from, customer);
                            minDurationID = from.ID;
                        }
                    }
                minDurationsTo[i] = minDuration;
                idsOfminDurationTo[i] = minDurationID;
                if (idsOfminDurationFrom[i] == theDepot.ID && idsOfminDurationTo[i] == theDepot.ID)
                    roundTripVisitDepot.Add(customer.ID);
                
                totalTravelTime = totalTravelTime + minDurationsFrom[i] + minDurationsTo[i];
            }
            double totalServiceTime = 0.0;
            foreach (Site customer in customers)
            {
                totalServiceTime = totalServiceTime + customer.ServiceDuration;
            }

            return ((totalTravelTime/2.0)+totalServiceTime);
        }
        double GetAvgDepotTravelTime()
        { 
            double depotTravelTime = 0.0;

            double[] durationsFromDepot = new double[numCustomers];
            string[] idsOfdurationFromDepot = new string[numCustomers];
            double[] durationsToDepot = new double[numCustomers];
            string[] idsOfdurationToDepot = new string[numCustomers];

            for (int j = 0; j < customers.Count; j++)
            {
                durationsFromDepot[j] = TravelTime(theDepot, customers[j]);
                idsOfdurationFromDepot[j] = customers[j].ID;

                durationsToDepot[j] = TravelTime(customers[j], theDepot);
                idsOfdurationToDepot[j] = customers[j].ID;
            }
            Array.Sort(durationsFromDepot, idsOfdurationFromDepot);
            Array.Sort(durationsToDepot, idsOfdurationToDepot);

            int nArcsSelectedByDepot = 0;
            int i = 0;

            while (nArcsSelectedByDepot < 2 * numRoutes)
            {
                int count = 0;
                if (roundTripVisitDepot.Contains(idsOfdurationFromDepot[i])) //TODO this is written as if distances are symmetric, if not this condition needs to be fixed!!
                {
                    count = 2;
                    depotTravelTime = depotTravelTime + durationsFromDepot[i] + durationsToDepot[i];
                }
                else
                {
                    count = 1;
                    depotTravelTime = depotTravelTime + Math.Min(durationsFromDepot[i],durationsToDepot[i]);
                }
                i++;
                nArcsSelectedByDepot = nArcsSelectedByDepot + count;
            }
            return depotTravelTime / 2.0;
        }
        double GetAvgTotalESTravelTime() //Note: TotalESTravelTime contains the refueling duration as well!!  
        {
            double totalESTravelTime = 0.0;
            double refuelingDuration = GetSingleRefuelDuration() * numESVisits;

            for (int i = 0; i < numESVisits; i++)
                totalESTravelTime = totalESTravelTime + shortestEsVisitTravelTimeList[i];

            return ((totalESTravelTime / 2.0) + refuelingDuration);
        }
        double GetSingleRefuelDuration()
        {
            switch (theProblemModel.RechargingDuration_status)
            {
                case RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full:
                    return theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity / theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate;
                case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full:
                    return GetMinimumEnergyConsumptionEnRouteInMinutesFromSiteToES();
                case RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial:
                    return 0.0;
                default:
                    throw new Exception("RechargingDuration_status is not specified!");
            }
        }
        double GetMinimumEnergyConsumptionEnRouteInMinutesFromSiteToES()
        {
            double minTime = double.MaxValue;
            foreach (Site es in ESs)//Note that these are all ESs, there should not be an ES which has 0 recharging rate!! Otherwise, it'll give a div/0 error.
                if (EvEnergyConsumption(theDepot,es) / (Math.Min(es.RechargingRate, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate)) < minTime)
                    minTime = EvEnergyConsumption(theDepot,es) / (Math.Min(es.RechargingRate, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate));

            foreach(Site es in ESs)
                foreach(Site customer in customers)
                    if (EvEnergyConsumption(customer,es) / (Math.Min(es.RechargingRate, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate)) < minTime)
                        minTime = EvEnergyConsumption(customer,es) / (Math.Min(es.RechargingRate, theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).MaxChargingRate));
            return minTime;
        }
        void SetListOfShortestTimeAndDistanceOfEsVisitPairs()
        {
            foreach (Site from in customers)
                foreach (Site es in ESs)
                    foreach (Site to in customers)
                        if (from.ID != to.ID)
                        {
                            shortestEsVisitTravelTimeList.Add(TravelTime(from, es) + TravelTime(es, to));
                            shortestEsVisitDistanceList.Add(Distance(from, es) + Distance(es, to));
                        }

            shortestEsVisitTravelTimeList.Sort();
            shortestEsVisitDistanceList.Sort();
        }
        double TravelTime(Site from, Site to)
        {
            return theProblemModel.SRD.GetTravelTime(from.ID, to.ID);
        }
        double Distance(Site from, Site to)
        {
            return theProblemModel.SRD.GetDistance(from.ID, to.ID);
        }
        double EvEnergyConsumption(Site from, Site to)
        {
            return theProblemModel.SRD.GetEVEnergyConsumption(from.ID, to.ID);
        }
    }
}
