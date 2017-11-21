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
        double deltaMin; public double EeltaMin { get { return deltaMin; } }
        double tMax; public double Tmax { get { return tMax; } }
        double tMin; public double Tmin { get { return tMin; } }

        public SiteWithAuxiliaryVariables() { }

        public SiteWithAuxiliaryVariables(double epsilonMax, double epsilonMin, double deltaMax, double deltaMin, double tMax, double tMin)
        {
            this.epsilonMax = epsilonMax;
            this.epsilonMin = epsilonMin;
            this.deltaMax = deltaMax;
            this.deltaMin = deltaMin;
            this.tMax = tMax;
            this.tMin = tMin;
        }

        public SiteWithAuxiliaryVariables(SiteWithAuxiliaryVariables twinSWAV)
        {
            this.epsilonMax = twinSWAV.epsilonMax;
            this.epsilonMin = twinSWAV.epsilonMin;
            this.deltaMax = twinSWAV.deltaMax;
            this.deltaMin = twinSWAV.deltaMin;
            this.tMax = twinSWAV.tMax;
            this.tMin = twinSWAV.tMin;
        }
    }
}
