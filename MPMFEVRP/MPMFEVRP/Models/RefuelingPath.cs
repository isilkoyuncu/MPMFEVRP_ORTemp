﻿using MPMFEVRP.Domains.ProblemDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Models
{
    /// <summary>
    /// A refueling path assuming VP refueling policy.
    /// The path is considered from the departure at a non-ES origin (i.e., from) to arrival at another non-ES destination (i.e., to).
    /// Time-feasibility of the path is calculated at construction and recorded in the boolean timeFeasible. Similarly energy-feasibility is in energyFeasible. Overall feasibility is returned via the boolean Feasible.
    /// </summary>
    public class RefuelingPath
    {
        SiteWithAuxiliaryVariables origin;                      public SiteWithAuxiliaryVariables Origin { get => origin; }
        SiteWithAuxiliaryVariables destination;                 public SiteWithAuxiliaryVariables Destination { get => destination; }
        List<SiteWithAuxiliaryVariables> refuelingStops;        public List<SiteWithAuxiliaryVariables> RefuelingStops { get => refuelingStops; }
        double totalDistance;                                   public double TotalDistance { get => totalDistance; } //Total distance is fixed no matter what the refueling policy is.
        double totalTravelTime;                                 public double TotalTravelTime { get => totalTravelTime; } //Total travel time is fixed no matter what the refueling policy is.
        double fixedRefuelingTime;                              public double FixedRefuelingTime { get => fixedRefuelingTime; } //Fixed refueling time
        double totalRefuelingTime_LB;                           public double TotalRefuelingTime_LB { get => totalRefuelingTime_LB; } //Total refueling time depends on the refueling policy
        double totalEnergyConsumption;                          public double TotalEnergyConsumption { get => totalEnergyConsumption; }//Total energy consumption is fixed no matter what the refueling policy is.
        double firstArcEnergyConsumption;                       public double FirstArcEnergyConsumption { get => firstArcEnergyConsumption; } //First arc energy consumption is fixed no matter what the refueling policy is.
        double lastArcEnergyConsumption;                        public double LastArcEnergyConsumption { get => lastArcEnergyConsumption; } //Last arc energy consumption is fixed no matter what the refueling policy is.
        bool timeFeasible;                                      public bool TimeFeasible { get => timeFeasible; }
        bool energyFeasible;                                    public bool EnergyFeasible { get => energyFeasible; }
                                                                public bool Feasible { get => (timeFeasible && energyFeasible); }
        double maximumEnergyRefueledOnRoad;                     public double MaximumEnergyRefueledOnRoad { get => maximumEnergyRefueledOnRoad; }
        double minimumEnergyRefueledOnRoad;                     public double MinimumEnergyRefueledOnRoad { get => minimumEnergyRefueledOnRoad; }



        //double minimumDepartureSOEAtOrigin;
        //public double MinimumDepartureSOEAtOrigin { get => minimumDepartureSOEAtOrigin; }
        //double minimumArrivalSOEAtDestination;
        //public double MinimumArrivalSOEAtDestination { get => minimumArrivalSOEAtDestination; }
        //double maximumArrivalSOEAtDestination;
        //public double MaximumArrivalSOEAtDestination { get => maximumArrivalSOEAtDestination; }
        //double maximumArrivalSOEAtOrigin;
        //public double MaximumArrivalSOEAtOrigin { get => maximumArrivalSOEAtOrigin; }


        //double maximumArrivalTimeAtOrigin;
        //public double MaximumArrivalTimeAtOrigin { get => maximumArrivalTimeAtOrigin; }

        //double minimumDepartureTimeAtDestination;
        //public double MinimumDepartureTimeAtDestination { get => minimumDepartureTimeAtDestination; }

        //double minimumArrivalTimeAtDestination;
        //public double MinimumArrivalTimeAtDestination { get => minimumArrivalTimeAtDestination; }

        //double maximumArrivalTimeAtDestination;
        //public double MaximumArrivalTimeAtDestination { get => maximumArrivalTimeAtDestination; }

        //double minimumArrivalTimeAtOrigin;
        //public double MinimumArrivalTimeAtOrigin { get => minimumArrivalTimeAtOrigin; }




        //This field is under development.
        //RefuelingPathESTypes esTypesOnThePath; 
        //public RefuelingPathESTypes ESTypesOnThePath { get => esTypesOnThePath; }
        /// <summary>
        /// Creates a RefuelingPath between two non-ES nodes and through a (ordered) set of ES nodes. 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="refuelingStops">An ordered set of ES nodes. Cannot be null! Can contain 0 elements.</param>
        public RefuelingPath(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> refuelingStops, SiteRelatedData SRD, VehicleRelatedData VRD, RechargingDurationAndAllowableDepartureStatusFromES rp = RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
        {
            //verification:
            if (origin.SiteType == SiteTypes.ExternalStation)
                throw new Exception("The origin of a RefuelingPath cannot be an ES!");
            if (destination.SiteType == SiteTypes.ExternalStation)
                throw new Exception("The destination of a RefuelingPath cannot be an ES!");
            if (refuelingStops == null)
                throw new Exception("RefuelingPath cannot be constructed with null refuelingStops!");
            foreach (SiteWithAuxiliaryVariables s in refuelingStops)
                if (s.SiteType != SiteTypes.ExternalStation)
                    throw new Exception("All refuelingStops must be ES!");

            //implementation:
            this.origin = origin;
            this.destination = destination;
            this.refuelingStops = refuelingStops;
            RechargingDurationAndAllowableDepartureStatusFromES refuelingPolicy = rp;
            Vehicle theEV = VRD.GetTheVehicleOfCategory(VehicleCategories.EV);

            totalDistance = 0.0;            
            totalTravelTime = 0.0;
            totalRefuelingTime_LB = 0.0;
            totalEnergyConsumption = 0.0;
            energyFeasible = false;
            timeFeasible = false;

            SiteWithAuxiliaryVariables from = origin;
            List<SiteWithAuxiliaryVariables> to_inSequence = new List<SiteWithAuxiliaryVariables>();
            for (int i = 0; i < refuelingStops.Count; i++)
            {
                to_inSequence.Add(refuelingStops[i]);
            }
            to_inSequence.Add(destination);
            while (to_inSequence.Count > 0)
            {
                string from_id = from.ID;
                SiteWithAuxiliaryVariables to = to_inSequence[0];
                string to_id = to.ID;
                totalDistance += SRD.GetDistance(from_id, to_id);
                totalTravelTime += SRD.GetTravelTime(from_id, to_id);
                if (to.SiteType == SiteTypes.ExternalStation)
                {
                    double refuelingTime = 0.0;//(0.1*theEV.BatteryCapacity)/theEV.MaxChargingRate;
                    if (refuelingPolicy == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
                        refuelingTime += Math.Max(0, (from.DeltaMax - SRD.GetDistance(from_id, to_id) - SRD.GetDistance(to_id, to_inSequence[1].ID) - to_inSequence[1].DeltaMin) / (Math.Min(theEV.MaxChargingRate,to.RechargingRate)));
                    else if (refuelingPolicy == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
                        refuelingTime += ((to.EpsilonMax - to.DeltaMax) / (Math.Min(theEV.MaxChargingRate, to.RechargingRate)));
                    else if (refuelingPolicy == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                        refuelingTime += (to.EpsilonMax / (Math.Min(theEV.MaxChargingRate, to.RechargingRate)));
                    totalRefuelingTime_LB += refuelingTime;
                }
                double energyConsumption = SRD.GetEVEnergyConsumption(from_id, to_id);
                totalEnergyConsumption += energyConsumption;
                if (from.SiteType != SiteTypes.ExternalStation)
                    firstArcEnergyConsumption = energyConsumption;
                if (to.SiteType != SiteTypes.ExternalStation)
                    lastArcEnergyConsumption = energyConsumption;
                    energyFeasible = energyConsumption <= from.DeltaPrimeMax;

                from = to;
                to_inSequence.RemoveAt(0);
            }
            double maximumDepartureTimeAtOrigin = destination.TauMax - totalTravelTime - totalRefuelingTime_LB;
            timeFeasible = (maximumDepartureTimeAtOrigin >= (origin.TauMin + origin.ServiceDuration));

            if (refuelingStops.Count != 0)
            {
                maximumEnergyRefueledOnRoad = VRD.GetTheVehicleOfCategory(VehicleCategories.EV).BatteryCapacity + totalEnergyConsumption - firstArcEnergyConsumption - lastArcEnergyConsumption;
                minimumEnergyRefueledOnRoad = Math.Max(0.0, destination.DeltaMin - origin.DeltaMax + TotalEnergyConsumption);
            }
            else
            {
                maximumEnergyRefueledOnRoad = 0.0;
                minimumEnergyRefueledOnRoad = 0.0;
            }
            //minimumDepartureSOEAtOrigin = firstArcEnergyConsumption;
            //if (refuelingStops.Count == 0)
            //{
            //    minimumArrivalSOEAtDestination = destination.DeltaMin;
            //    maximumArrivalSOEAtDestination = origin.DeltaPrimeMax - lastArcEnergyConsumption;
            //}
            //else
            //{
            //    minimumArrivalSOEAtDestination = destination.DeltaMin;
            //    maximumArrivalSOEAtDestination = refuelingStops.Last().DeltaPrimeMax - lastArcEnergyConsumption;
            //}
            //maximumArrivalSOEAtOrigin = origin.DeltaMax;

            //minimumArrivalTimeAtOrigin = origin.TauMin;
            //maximumArrivalTimeAtOrigin = origin.TauMax;

            //minimumDepartureTimeAtDestination = origin.TauMin + minimumTotalTime + destination.ServiceDuration;
            //minimumArrivalTimeAtDestination = destination.TauMin;
            //maximumArrivalTimeAtDestination = destination.TauMax;


        }
        public List<string> GetRefuelingStopIDs()
        {
            List<string> outcome = new List<string>();
            foreach (SiteWithAuxiliaryVariables swav in refuelingStops)
                outcome.Add(swav.ID);
            return outcome;
        }
        public RefuelingPathDominance CheckDominance(RefuelingPath theOtherRP)
        {
            List<int> signs = new List<int>();//-1 for the incumbent (this), +1 from the challenger (theOtherRP), 0 for equality

            signs.Add(Math.Sign(firstArcEnergyConsumption - theOtherRP.firstArcEnergyConsumption));
            signs.Add(Math.Sign(lastArcEnergyConsumption - theOtherRP.lastArcEnergyConsumption));
            signs.Add(Math.Sign(totalDistance - theOtherRP.totalDistance));
            signs.Add(Math.Sign((totalTravelTime+totalRefuelingTime_LB) - (theOtherRP.totalTravelTime + theOtherRP.totalRefuelingTime_LB)));

            int nFavorIncumbent = signs.Where(x => x.Equals(-1)).Count();
            int nFavorChallenger = signs.Where(x => x.Equals(1)).Count();

            if(nFavorIncumbent > 0)
            {
                if (nFavorChallenger > 0)
                    return RefuelingPathDominance.NeitherDominates;
                else
                {

                    return RefuelingPathDominance.IncumbentDominates;
                }
            }
            else//nFavorIncumbent = 0
            {
                if (nFavorChallenger > 0)
                    return RefuelingPathDominance.ChallengerDominates;
                else
                    return RefuelingPathDominance.BothAreTheSame;
            }
        }
    }
}
