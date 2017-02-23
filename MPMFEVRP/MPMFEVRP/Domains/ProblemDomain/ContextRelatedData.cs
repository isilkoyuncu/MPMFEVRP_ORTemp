using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ContextRelatedData
    {
        double travelSpeed;   //This is miles per minute
        double tMax;    //Leghts of the workday, minutes
        int lambda; //Max number of recharges per EV in a workday

        public double TravelSpeed { get { return travelSpeed; } set {; } }
        public double TMax { get { return tMax; } set {; } }
        public int Lambda { get { return lambda; } set {; } }
        public ContextRelatedData() { }
        public ContextRelatedData(double travelSpeed,double tMax, int lambda)
        {
            this.travelSpeed = travelSpeed;
            this.tMax = tMax;
            this.lambda = lambda;
        }
        public ContextRelatedData(ContextRelatedData twinCRD)
        {
            travelSpeed = twinCRD.TravelSpeed;
            tMax = twinCRD.TMax;
            lambda = twinCRD.Lambda;
        }
    }
}
