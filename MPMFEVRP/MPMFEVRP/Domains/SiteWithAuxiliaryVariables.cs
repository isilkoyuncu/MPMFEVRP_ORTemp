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
        double tLS; public double TLS { get { return tLS; } }
        double tES; public double TES { get { return tES; } }
        double deltaPrimeMax; public double DeltaPrimeMax { get { return deltaPrimeMax; } }

        public SiteWithAuxiliaryVariables() { }
        public SiteWithAuxiliaryVariables(double epsilonMax, double epsilonMin, double deltaMax, double deltaMin, double tLS, double tES, double deltaPrimeMax=double.MaxValue)
        {
            this.epsilonMax = epsilonMax;
            this.epsilonMin = epsilonMin;
            this.deltaMax = deltaMax;
            this.deltaMin = deltaMin;
            this.tLS = tLS;
            this.tES = tES;
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
            tLS = twinSWAV.tLS;
            tES = twinSWAV.tES;
            deltaPrimeMax = twinSWAV.deltaPrimeMax;
        }
        public SiteWithAuxiliaryVariables(Site baseSite):base(baseSite)
        {
            epsilonMax = -1;
            epsilonMin = -1;
            deltaMax = -1;
            deltaMin = -1;
            tLS = -1;
            tES = -1;
            deltaPrimeMax = -1;
        }
        public void UpdateEpsilonBounds(double epsilonMax, double epsilonMin = 0.0)
        {
            this.epsilonMax = epsilonMax;
            this.epsilonMin = epsilonMin;
        }
        public void UpdateDeltaBounds(double deltaMax, double deltaMin, double deltaPrimeMax)
        {
            this.deltaMax = deltaMax;
            this.deltaMin = deltaMin;
            this.deltaPrimeMax = deltaPrimeMax;
        }
        public void UpdateDeltaMin(double deltaMin)
        {
            this.deltaMin = deltaMin;
        }
        public void UpdateDeltaMax(double deltaMax)
        {
            this.deltaMax = deltaMax;
        }
        public void UpdateDeltaPrimeMax(double deltaPrimeMax)
        {
            this.deltaPrimeMax = deltaPrimeMax;
        }
        public void UpdateTBounds(double tLS, double tES)
        {
            this.tLS = tLS;
            this.tES = tES;
        }    
    }
}
