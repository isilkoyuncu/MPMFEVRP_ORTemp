using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class SiteWithAuxiliaryVariables : Site
    {
        double epsilonMax; public double EpsilonMax { get { return epsilonMax; } }
        double epsilonMin; public double EpsilonMin { get { return epsilonMin; } }
        double deltaMax; public double DeltaMax { get { return deltaMax; } }
        double deltaMin; public double DeltaMin { get { return deltaMin; } }
        //The following are named LS and ES for latest and earliest start times, respectively; to avoid confusion with the problem's TMax
        double tauMax; public double TauMax { get { return tauMax; } }
        double tauMin; public double TauMin { get { return tauMin; } }
        double deltaPrimeMax; public double DeltaPrimeMax { get { return deltaPrimeMax; } }

        public SiteWithAuxiliaryVariables() { }
        public SiteWithAuxiliaryVariables(double epsilonMax, double epsilonMin, double deltaMax, double deltaMin, double tauMax, double tauMin, double deltaPrimeMax=double.MaxValue)
        {
            this.epsilonMax = epsilonMax;
            this.epsilonMin = epsilonMin;
            this.deltaMax = deltaMax;
            this.deltaMin = deltaMin;
            this.tauMax = tauMax;
            this.tauMin = tauMin;
            this.deltaPrimeMax = deltaPrimeMax;
        }
        public SiteWithAuxiliaryVariables ShallowCopy()
        {
            return (SiteWithAuxiliaryVariables) this.MemberwiseClone();
        }
        public SiteWithAuxiliaryVariables(SiteWithAuxiliaryVariables twinSWAV)
        {
            epsilonMax = twinSWAV.epsilonMax;
            epsilonMin = twinSWAV.epsilonMin;
            deltaMax = twinSWAV.deltaMax;
            deltaMin = twinSWAV.deltaMin;
            tauMax = twinSWAV.tauMax;
            tauMin = twinSWAV.tauMin;
            deltaPrimeMax = twinSWAV.deltaPrimeMax;
        }
        public SiteWithAuxiliaryVariables(Site baseSite):base(baseSite)
        {
            epsilonMax = -1;
            epsilonMin = -1;
            deltaMax = -1;
            deltaMin = -1;
            tauMax = -1;
            tauMin = -1;
            deltaPrimeMax = -1;
        }
        public void UpdateRefueledEnergyOnArrivalNodeBounds(double epsilonMax, double epsilonMin = 0.0)
        {
            this.epsilonMax = epsilonMax;
            this.epsilonMin = epsilonMin;
        }
        public void UpdateArrivalSOEBounds(double deltaMax, double deltaMin, double deltaPrimeMax)
        {
            this.deltaMax = deltaMax;
            this.deltaMin = deltaMin;
            this.deltaPrimeMax = deltaPrimeMax;
        }
        public void UpdateMinArrivalSOE(double deltaMin)
        {
            this.deltaMin = deltaMin;
        }
        public void UpdateMaxArrivalSOE(double deltaMax)
        {
            this.deltaMax = deltaMax;
        }
        public void UpdateMaxDepartureSOE(double deltaPrimeMax)
        {
            this.deltaPrimeMax = deltaPrimeMax;
        }
        public void UpdateArrivalTimeBounds(double tauMax, double tauMin)
        {
            this.tauMax = tauMax;
            this.tauMin = tauMin;
        }    
    }
}
