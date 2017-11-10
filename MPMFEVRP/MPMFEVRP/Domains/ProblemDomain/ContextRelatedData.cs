namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ContextRelatedData
    {
        double travelSpeed;   //This is miles per minute
        double tMax;    //Leghts of the workday, minutes
        //int lambda; //Max number of recharges per EV in a workday

        ////ISSUE (#6): The following are all public and what's even more dangerous is that they all have setters (only used by ProblemDataPackage). This must be corrected by forcing the ProblemDataPackage to use the full constructor below from its internally kept data. As for giving these fields to outside, new public Get...() methods must be developed and then used as the only way.
        public double TravelSpeed { get { return travelSpeed; } set { travelSpeed = value; } }
        public double TMax { get { return tMax; } set { tMax = value; } }
        //public int Lambda { get { return lambda; } set { lambda = value; } }
        public ContextRelatedData() { }
        public ContextRelatedData(double travelSpeed,double tMax, int lambda)
        {
            this.travelSpeed = travelSpeed;
            this.tMax = tMax;
            //this.lambda = lambda;
        }
        public ContextRelatedData(ContextRelatedData twinCRD)
        {
            travelSpeed = twinCRD.TravelSpeed;
            tMax = twinCRD.TMax;
            //lambda = twinCRD.Lambda;
        }
    }
}
