using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Utils;
using MPMFEVRP.Implementations.Problems.Readers;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ProblemDataPackage
    {
        string inputFileName;   //For record only
        public string InputFileName { get { return inputFileName; } set { inputFileName = value; } }
        SiteRelatedData srd;
        public SiteRelatedData SRD { get { return srd; } set { srd = value; } }
        VehicleRelatedData vrd;
        public VehicleRelatedData VRD { get { return vrd; } set { vrd = value; } }
        ContextRelatedData crd;
        public ContextRelatedData CRD { get { return crd; } set { crd = value; } }

        public ProblemDataPackage() { }
        public ProblemDataPackage(KoyuncuYavuzReader reader)
        {
            inputFileName = reader.GetRecommendedOutputFileFullName();
            srd = new SiteRelatedData();
            vrd = new VehicleRelatedData();
            crd = new ContextRelatedData();

            srd.NumCustomers = reader.GetNumberOfCustomers();
            srd.NumES = reader.GetNumberOfES();
            srd.NumNodes = srd.NumCustomers + srd.NumES + 1;
            vrd.NumVehicleCategories = reader.GetVehicleArray().Length;
            vrd.NumVehicles = new int[vrd.NumVehicleCategories];
            for (int v = 0; v < vrd.NumVehicleCategories; v++)
                vrd.NumVehicles[v] = srd.NumCustomers;//TODO We entered numCustomers as the available number of vehicles in a category to make it unrestrictive. Limiting the numbers of vehicles is something we'd love to experiment on, and thus, this point will have to be clarified later on.

            srd.SiteArray = new Site[srd.NumNodes];
            for (int s = 0; s < srd.NumNodes; s++)
            {
                srd.SiteArray[s] = new Site(reader.GetSiteArray()[s]);
            }

            vrd.VehicleArray = new Vehicle[vrd.NumVehicleCategories];
            for (int v = 0; v < vrd.NumVehicleCategories; v++)
            {
                vrd.VehicleArray[v] = new Vehicle(reader.GetVehicleArray()[v]);
            }

            //Assign travel speed
            crd.TravelSpeed = reader.GetTravelSpeed();

            //Assign Distance matrix if any
            srd.Distance = new double[srd.NumNodes, srd.NumNodes];
            if (reader.GetDistanceMatrix() != null)//This is the case when distances are given in the data file (asymmetric, or whatever)
            {
                srd.Distance = (double[,])reader.GetDistanceMatrix().Clone();
            }
            else//The distances are not given in the data file, but they have to be calculated
            {
                if (reader.IsLongLat())//Haversine calculations
                {
                    for (int i = 0; i < srd.NumNodes; i++)
                    {
                        for (int j = 0; j < srd.NumNodes; j++)
                        {
                            srd.Distance[i, j] = Calculators.HaversineDistance(srd.SiteArray[i].X, srd.SiteArray[i].Y, srd.SiteArray[j].X, srd.SiteArray[j].Y);
                        }
                    }
                }
                else//Euclidean calculations
                {
                    for (int i = 0; i < srd.NumNodes; i++)
                    {
                        for (int j = 0; j < srd.NumNodes; j++)
                        {
                            srd.Distance[i, j] = Calculators.EuclideanDistance(srd.SiteArray[i].X, srd.SiteArray[j].X, srd.SiteArray[i].Y, srd.SiteArray[j].Y);
                        }
                    }
                }
            }

            crd.TMax = -1;
            for (int s = 0; s < srd.NumNodes; s++)
            {
                if (crd.TMax < srd.SiteArray[s].DueDate)
                    crd.TMax = srd.SiteArray[s].DueDate;
            }

            srd.EnergyConsumption = new double[srd.NumNodes, srd.NumNodes, vrd.NumVehicleCategories];
            srd.TimeConsumption = new double[srd.NumNodes, srd.NumNodes];

            for (int i = 0; i < srd.NumNodes; i++)
                for (int j = 0; j < srd.NumNodes; j++)
                {
                    for (int vc = 0; vc < vrd.NumVehicleCategories; vc++)
                    {
                        if (vrd.VehicleArray[vc].Category == VehicleCategories.EV)
                            srd.EnergyConsumption[i, j, vc] = srd.Distance[i, j] / (vrd.VehicleArray[vc].BatteryCapacity / vrd.VehicleArray[vc].ConsumptionRate);
                        else
                            srd.EnergyConsumption[i, j, vc] = 0.0;
                    }
                    srd.TimeConsumption[i, j] = srd.Distance[i, j] / crd.TravelSpeed;
                }

            crd.Lambda = 2;//TODO We entered 2 for now; we'd love to experiment on it.

        }
        public ProblemDataPackage(ProblemDataPackage twinPDP)
        {
            inputFileName = twinPDP.InputFileName;

            srd = new SiteRelatedData(twinPDP.SRD);
            vrd = new VehicleRelatedData(twinPDP.VRD);
            crd = new ContextRelatedData(twinPDP.CRD);
        }
    }
}
