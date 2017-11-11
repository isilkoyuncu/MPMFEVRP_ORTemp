using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.AlgorithmDomain
{
    public class SiteAuxiliaryBounds
    {
        string siteID; public string SiteID { get { return siteID; }set { siteID = value; } }
        double epsilon_Max; public double Epsilon_Max { get { return epsilon_Max; } set { epsilon_Max = value; } }
        double epsilon_Min; public double Epsilon_Min { get { return epsilon_Min; } set { epsilon_Min = value; } }
        double delta_Max; public double Delta_Max { get { return delta_Max; } set { delta_Max = value; } }
        double delta_Min; public double Delta_Min { get { return delta_Min; } set { delta_Min = value; } }
        double t_Max; public double T_Max { get { return t_Max; } set { t_Max = value; } }
        double t_Min; public double T_Min { get { return t_Min; } set { t_Min = value; } }

        public SiteAuxiliaryBounds(string siteID, double epsilon_Max, double epsilon_Min, double delta_Max, double delta_Min, double t_Max, double t_Min)
        {
            this.siteID = siteID;
            this.epsilon_Max = epsilon_Max;
            this.epsilon_Min = epsilon_Min;
            this.delta_Max = delta_Max;
            this.delta_Min = delta_Min;
            this.t_Max = t_Max;
            this.t_Min = t_Min;
        }
    }

    public class AllSitesAuxiliaryBounds : List<SiteAuxiliaryBounds>
    {
        public double[] GetEpsilonMax()
        {
            double[] epsilonMax = new double[Count];
            for(int i=0; i<Count; i++)
            {
                epsilonMax[i] = this[i].Epsilon_Max;
            }
            return epsilonMax;
        }
        public double[] GetEpsilonMin()
        {
            double[] epsilonMin = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                epsilonMin[i] = this[i].Epsilon_Min;
            }
            return epsilonMin;
        }
        public double[] GetDeltaMax()
        {
            double[] deltaMax = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                deltaMax[i] = this[i].Delta_Max;
            }
            return deltaMax;
        }
        public double[] GetDeltaMin()
        {
            double[] deltaMin = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                deltaMin[i] = this[i].Delta_Min;
            }
            return deltaMin;
        }
        public double[] GetTMax()
        {
            double[] tMax = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                tMax[i] = this[i].T_Max;
            }
            return tMax;
        }
        public double[] GetTMin()
        {
            double[] tMin = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                tMin[i] = this[i].T_Min;
            }
            return tMin;
        }
    }
}
