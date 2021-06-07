using MPMFEVRP.Implementations.Problems.Readers;
using MPMFEVRP.Utils;
using System.Collections.Generic;
using System.Linq;
using System;
using MPMFEVRP.Models;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ProblemDataPackage
    {
        string inputFileName;   //For record only
        public string InputFileName { get { return inputFileName; } set { inputFileName = value; } }

        SiteRelatedData srd;           public SiteRelatedData SRD { get { return srd; } }
        VehicleRelatedData vrd;        public VehicleRelatedData VRD { get { return vrd; } }
        ContextRelatedData crd;        public ContextRelatedData CRD { get { return crd; } }

        public ProblemDataPackage() { }
        public ProblemDataPackage(KoyuncuYavuzReader reader)
        {
            inputFileName = reader.GetRecommendedOutputFileFullName();
            int numNodes = reader.GetSiteArray().Length;
            int numVehicleCategories = reader.GetVehicleArray().Length;

            double[,] distance = new double[numNodes, numNodes];
            if (reader.GetDistanceMatrix() != null)
            { distance = (double[,])reader.GetDistanceMatrix().Clone(); }//This is the case when distances are given in the data file (asymmetric, or whatever)
            else
            {
                if (reader.IsLongLat())//Haversine calculations
                {
                    distance = Calculators.HaversineDistance(reader.GetXcoordidates(), reader.GetYcoordidates());
                }
                else//Euclidean calculations
                {
                    distance = Calculators.EuclideanDistance(reader.GetXcoordidates(), reader.GetYcoordidates());
                }
            }//This is the case when distances have not been given in the data file, but they were calculated

            double[,] timeConsumption = new double[numNodes, numNodes];
            double[,,] energyConsumption = new double[numNodes, numNodes, numVehicleCategories];
            VehicleCategories[] vcArray = new VehicleCategories[numVehicleCategories];
            for (int i = 0; i < numNodes; i++)
                for (int j = 0; j < numNodes; j++)
                {
                    for (int v = 0; v < numVehicleCategories; v++)
                    {
                        vcArray[v] = reader.GetVehicleArray()[v].Category;
                        if (vcArray[v] == VehicleCategories.EV)
                            energyConsumption[i, j, v] = distance[i, j] * reader.GetVehicleArray()[v].ConsumptionRate;
                        else
                            energyConsumption[i, j, v] = 0.0;
                    }
                    timeConsumption[i, j] = distance[i, j] / reader.GetTravelSpeed();
                }
            srd = new SiteRelatedData(reader.GetNumberOfCustomers(), reader.GetNumberOfES(), numNodes, reader.GetSiteArray(), distance,timeConsumption,energyConsumption);
            vrd = new VehicleRelatedData(numVehicleCategories, reader.GetVehicleArray());
            crd = new ContextRelatedData(reader.GetTravelSpeed(), srd.GetSingleDepotSite().DueDate, reader.GetRefuelCostofGas(), reader.GetRefuelCostAtDepot(), reader.GetRefuelCostInNetwork(), reader.GetRefuelCostOutNetwork()); //ISSUE (#5) For LAMBDA we entered 2 for now; we'd love to experiment on it.
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
