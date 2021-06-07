namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ContextRelatedData
    {
        double travelSpeed;   //This is miles per minute
        double tMax;    //Leghts of the workday, minutes
        double refuelCostofGas;
        double refuelCostAtDepot;
        double refuelCostInNetwork;
        double refuelCostOutNetwork;

        ////ISSUE (#6): The following are all public and what's even more dangerous is that they all have setters (only used by ProblemDataPackage). This must be corrected by forcing the ProblemDataPackage to use the full constructor below from its internally kept data. As for giving these fields to outside, new public Get...() methods must be developed and then used as the only way.
        public double TravelSpeed { get { return travelSpeed; } set { travelSpeed = value; } }
        public double TMax { get { return tMax; } set { tMax = value; } }
        public double RefuelCostofGas { get { return refuelCostofGas; } set { refuelCostofGas = value; } }
        public double RefuelCostAtDepot { get { return refuelCostAtDepot; } set { refuelCostAtDepot = value; } }
        public double RefuelCostInNetwork { get { return refuelCostInNetwork; } set { refuelCostInNetwork = value; } }
        public double RefuelCostOutNetwork { get { return refuelCostOutNetwork; } set { refuelCostOutNetwork = value; } }
        
        public ContextRelatedData() { }
        public ContextRelatedData(double travelSpeed,double tMax, double refuelCostofGas, double refuelCostAtDepot, double refuelCostInNetwork, double refuelCostOutNetwork)
        {
            this.travelSpeed = travelSpeed;
            this.tMax = tMax;
            this.refuelCostofGas = refuelCostofGas;
            this.refuelCostAtDepot = refuelCostAtDepot;
            this.refuelCostInNetwork = refuelCostInNetwork;
            this.refuelCostOutNetwork = refuelCostOutNetwork;
        }
        public ContextRelatedData(ContextRelatedData twinCRD)
        {
            travelSpeed = twinCRD.TravelSpeed;
            tMax = twinCRD.TMax;
            refuelCostofGas = twinCRD.refuelCostofGas;
            refuelCostAtDepot = twinCRD.refuelCostAtDepot;
            refuelCostInNetwork = twinCRD.refuelCostInNetwork;
            refuelCostOutNetwork = twinCRD.refuelCostOutNetwork;
        }
    }
}
