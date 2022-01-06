using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Readers;
using MPMFEVRP.Utils;

namespace MPMFEVRP.MFGVRPCGH.RawMaterial
{
    public class MixedFleetGVRPMaterial
    {
        SiteRelatedData srd;                                    public SiteRelatedData SRD { get { return srd; } }
        VehicleRelatedData vrd;                                 public VehicleRelatedData VRD { get { return vrd; } }
        ContextRelatedData crd;                                 public ContextRelatedData CRD { get { return crd; } }
        ExperimentationParameters experimentationParameters;    public ExperimentationParameters ExperimentationParameters { get { return experimentationParameters; } }
        string inputFileName;                                   public string InputFileName { get { return inputFileName; } }

        MixedFleetGVRPMaterial(KoyuncuYavuzReader KYreader, ExperimentationParameters experimentationParameters)
        {
            inputFileName = KYreader.GetRecommendedOutputFileFullName();
            int numNodes = KYreader.GetSiteArray().Length;
            int numVehicleCategories = KYreader.GetVehicleArray().Length;

            double[,] distance;// = new double[numNodes, numNodes];
            if (KYreader.GetDistanceMatrix() != null) //This is the case when distances are given in the data file (asymmetric, or whatever)
            {
                distance = (double[,])KYreader.GetDistanceMatrix().Clone();
            }
            else //This is the case when distances have not been given in the data file, but they were calculated
            {
                if (KYreader.IsLongLat())//Haversine calculations
                {
                    distance = Calculators.HaversineDistance(KYreader.GetXcoordidates(), KYreader.GetYcoordidates());
                }
                else//Euclidean calculations
                {
                    distance = Calculators.EuclideanDistance(KYreader.GetXcoordidates(), KYreader.GetYcoordidates());
                }
            }

            double[,] timeConsumption = new double[numNodes, numNodes];
            double[,,] energyConsumption = new double[numNodes, numNodes, numVehicleCategories];
            VehicleCategories[] vcArray = new VehicleCategories[numVehicleCategories];
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    for (int v = 0; v < numVehicleCategories; v++)
                    {
                        vcArray[v] = KYreader.GetVehicleArray()[v].Category;
                        if (vcArray[v] == VehicleCategories.EV)
                            energyConsumption[i, j, v] = distance[i, j] * KYreader.GetVehicleArray()[v].ConsumptionRate;
                        else
                            energyConsumption[i, j, v] = 0.0;
                    }
                    timeConsumption[i, j] = distance[i, j] / KYreader.GetTravelSpeed();
                }
            srd = new SiteRelatedData(KYreader.GetNumberOfCustomers(), KYreader.GetNumberOfES(), numNodes, KYreader.GetSiteArray(), distance, timeConsumption, energyConsumption);
            vrd = new VehicleRelatedData(numVehicleCategories, KYreader.GetVehicleArray());
            crd = new ContextRelatedData(KYreader.GetTravelSpeed(), srd.GetSingleDepotSite().DueDate, KYreader.GetRefuelCostofGas(), KYreader.GetRefuelCostAtDepot(), KYreader.GetRefuelCostInNetwork(), KYreader.GetRefuelCostOutNetwork(), KYreader.IsLongLat()); //ISSUE (#5) For LAMBDA we entered 2 for now; we'd love to experiment on it.

            this.experimentationParameters = experimentationParameters;
        }
    }
}
        