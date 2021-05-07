using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class Arc
    {
        ArcType arcType; public ArcType ArcType { get => arcType; }
        SiteWithAuxiliaryVariables origin; public SiteWithAuxiliaryVariables Origin { get => origin; }
        SiteWithAuxiliaryVariables destination; public SiteWithAuxiliaryVariables Destination { get => destination; }
        List<SiteWithAuxiliaryVariables> refuelingStops; public List<SiteWithAuxiliaryVariables> RefuelingStops { get => refuelingStops; }
        double arcDistance; public double ArcDistance { get => arcDistance; }
        double arcTravelDuration; public double ArcTravelDuration { get => arcTravelDuration; }
        double arcRefuelingDurationFF; public double ArcRefuelingDurationFF { get => arcRefuelingDurationFF; }
        double arcEnergyConsumption; public double ArcEnergyConsumption { get => arcEnergyConsumption; }
        double minRefuelingDuration; public double MinRefuelingDuration { get => minRefuelingDuration; }
        double maxRefuelingDuration; public double MaxRefuelingDuration { get => maxRefuelingDuration; }
        double minEnergyRefueled; public double MinEnergyRefueled { get => minEnergyRefueled; }
        double maxEnergyRefueled; public double MaxEnergyRefueled { get => maxEnergyRefueled; }
        double firstLegEnergyConsumption; public double FirstLegEnergyConsumption { get => firstLegEnergyConsumption; }
        double lastLegEnergyConsumption; public double LastLegEnergyConsumption { get => lastLegEnergyConsumption; }
        bool timeFeasible; public bool TimeFeasible { get => timeFeasible; }
        bool energyFeasible; public bool EnergyFeasible { get => energyFeasible; }
        public bool Feasible { get => (timeFeasible && energyFeasible); }
        double minimumDepartureSOEAtOrigin; public double MinimumDepartureSOEAtOrigin { get => minimumDepartureSOEAtOrigin; }
        double maximumDepartureTimeAtOrigin; public double MaximumDepartureTimeAtOrigin { get => maximumDepartureTimeAtOrigin; }
        public Arc(SiteWithAuxiliaryVariables origin, SiteWithAuxiliaryVariables destination, List<SiteWithAuxiliaryVariables> refuelingStops, SiteRelatedData SRD, RechargingDurationAndAllowableDepartureStatusFromES refuelingPolicy)
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

            arcDistance = 0.0;
            arcTravelDuration = 0.0;
            energyFeasible = true;
            SiteWithAuxiliaryVariables from = origin;
            List<SiteWithAuxiliaryVariables> to_inSequence;
            if (refuelingStops.Count == 0)
            {
                arcType = ArcType.DirectArc;
                string from_id = from.ID;
                SiteWithAuxiliaryVariables to = destination;
                string to_id = to.ID;
                arcDistance += SRD.GetDistance(from_id, to_id);
                arcTravelDuration += SRD.GetTravelTime(from_id, to_id);               
                double energyConsumption = SRD.GetEVEnergyConsumption(from_id, to_id);
                arcEnergyConsumption = energyConsumption;
                minRefuelingDuration = 0.0;
                maxRefuelingDuration = 0.0;
                minEnergyRefueled = 0.0;
                maxEnergyRefueled = 0.0;
                arcRefuelingDurationFF = 0.0;
                firstLegEnergyConsumption = energyConsumption/2.0;
                lastLegEnergyConsumption = energyConsumption/2.0;
                if (energyConsumption > from.DeltaPrimeMax)
                    energyFeasible = false;
            }
            else
            {
                to_inSequence = new List<SiteWithAuxiliaryVariables>();
                arcType = ArcType.RefuelingArc;
                for (int i = 0; i < refuelingStops.Count; i++)
                    to_inSequence.Add(refuelingStops[i]);
                to_inSequence.Add(destination);
                do
                {
                    string from_id = from.ID;
                    SiteWithAuxiliaryVariables to = to_inSequence[0];
                    string to_id = to.ID;
                    arcDistance += SRD.GetDistance(from_id, to_id);
                    arcTravelDuration += SRD.GetTravelTime(from_id, to_id);
                    if (to.SiteType == SiteTypes.ExternalStation)
                    {
                        if (refuelingPolicy == RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full)
                            minRefuelingDuration += (to.EpsilonMax / to.RechargingRate);
                        else if (refuelingPolicy == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full)
                        {
                            minEnergyRefueled += SRD.GetEVEnergyConsumption(from_id, to_id);
                            maxEnergyRefueled += to.EpsilonMax;
                            minRefuelingDuration += (SRD.GetEVEnergyConsumption(from_id, to_id) / to.RechargingRate);
                            maxRefuelingDuration += to.EpsilonMax / to.RechargingRate;
                        }
                        else if(refuelingPolicy == RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial)
                        {
                            minEnergyRefueled += 0.0;
                            maxEnergyRefueled += to.EpsilonMax;
                            minRefuelingDuration += 0;
                            maxRefuelingDuration += to.EpsilonMax / to.RechargingRate;
                        }
                    }
                    double energyConsumption = SRD.GetEVEnergyConsumption(from_id, to_id);
                    arcEnergyConsumption += energyConsumption;
                    if (from.SiteType != SiteTypes.ExternalStation)//from origin to first ES
                    {
                        firstLegEnergyConsumption = energyConsumption;
                    }
                    else if (to.SiteType != SiteTypes.ExternalStation)//from last ES to destination
                    {
                        lastLegEnergyConsumption = energyConsumption;
                    }
                    else//between two ESs
                    {  }
                    if (energyConsumption > from.DeltaPrimeMax)
                        energyFeasible = false;
                    from = to;
                    to_inSequence.RemoveAt(0);
                } while (to_inSequence.Count > 0);
            }          
            minimumDepartureSOEAtOrigin = firstLegEnergyConsumption;
            maximumDepartureTimeAtOrigin = destination.TauMax - arcTravelDuration - minRefuelingDuration;
            timeFeasible = (maximumDepartureTimeAtOrigin >= (origin.TauMin + origin.ServiceDuration));
        }

    }
}
