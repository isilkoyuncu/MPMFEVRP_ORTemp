using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Interfaces;
using Instance_Generation.Utility;
using Instance_Generation.Other;
using Instance_Generation.FormSections;

namespace Instance_Generation.FileWriters
{
    class KoyuncuYavuzFileWriter : IWriter
    {
        string filename;
        int numNodes;
        Vehicle_RelatedData VehData;
        CommonCoreData CCData;

        //Site-related section, organized in columns
        string[] nodeID;
        string[] nodeType;
        double[] X;//[numNodes] X coordinate
        double[] Y;//[numNodes] Y coordinate
        double[] Demand;//[numNodes]
        double[] TimeWindowStart;//[numNodes]
        double[] TimeWindowEnd;//[numNodes]
        double[] CustomerServiceDuration;//[numNodes] typically 30-60 (minutes)
        double[] Gamma;//[numNodes] Charging rate (KHW/minute)
        double[] RefuelingCostPerKWH; //[numNodes] Refueling cost ($/KHW)
        double[,] Prize;//[2, numNodes] [vehicle - 0:EV, 1:GDV]
        double TravelSpeed;//miles per minute
        double RefuelCostOfGasPerGallon;
        double RefuelCostAtDepotPerKWH;
        double RefuelCostInNetworkPerKWH;
        double RefuelCostOutNetworkPerKWH;
        bool UseGeogPosition;//true if an instance uses geographic positions
        double[,] Distance;//[numNodes-1][numNodes-1]

        System.IO.StreamWriter sw;

        public KoyuncuYavuzFileWriter()
        {
            //Empty constructor
        }
        public KoyuncuYavuzFileWriter(string filename,
            int numNodes,
            Vehicle_RelatedData VehData,
            CommonCoreData CCData,
            string[] nodeID,
            string[] nodeType,
            double[] X,
            double[] Y,
            double[] Demand,
            double[] TimeWindowStart,
            double[] TimeWindowEnd,
            double[] CustomerServiceDuration,
            double[] Gamma,
            double[] RefuelingCostPerKWH,
            double[,] Prize,
            double TravelSpeed,
            double RefuelCostOfGasPerGallon,
            double RefuelCostAtDepotPerKWH,
            double RefuelCostInNetworkPerKWH,
            double RefuelCostOutNetworkPerKWH,
            bool UseGeogPosition,
            double[,] Distance)
        {
            //Take all input
            this.filename = filename + ".txt";
            this.numNodes = numNodes;
            this.VehData = VehData;
            this.CCData = CCData;
            this.nodeID = nodeID;
            this.nodeType = nodeType;
            this.X = X;
            this.Y = Y;
            this.Demand = Demand;
            this.TimeWindowStart = TimeWindowStart;
            this.TimeWindowEnd = TimeWindowEnd;
            this.CustomerServiceDuration = CustomerServiceDuration;
            this.Gamma = Gamma;
            this.RefuelingCostPerKWH = RefuelingCostPerKWH;
            this.Prize = Prize;
            this.TravelSpeed = TravelSpeed;
            this.RefuelCostOfGasPerGallon = RefuelCostOfGasPerGallon;
            this.RefuelCostAtDepotPerKWH = RefuelCostAtDepotPerKWH;
            this.RefuelCostInNetworkPerKWH = RefuelCostInNetworkPerKWH;
            this.RefuelCostOutNetworkPerKWH = RefuelCostOutNetworkPerKWH;
            this.UseGeogPosition = UseGeogPosition;
            this.Distance = Distance;
            //TODO Make sure everything is passed into this constructor and used appropriately
            //verify input
            //Verify();
            //process
            sw = new System.IO.StreamWriter(this.filename);
        }
        void Verify()
        {
            //TODO Turn this back on and check
            //int nSites = nCustomers + nNonDepotExternalStations + 1;
            //if (xCoordinate.Length != nSites)
            //    throw new Exception("X vector length is not equal to nCustomers+nNonDepotExternalStations+1!");
            //if (yCoordinate.Length != nSites)
            //    throw new Exception("Y vector length is not equal to nCustomers+nNonDepotExternalStations+1!");
            //if (isInternalStation.Length != nSites)
            //    throw new Exception("isInternalStation vector length is not equal to nCustomers+nNonDepotExternalStations+1!");
            //if (isExternalStation.Length != nSites)
            //    throw new Exception("isExternalStation vector length is not equal to nCustomers+nNonDepotExternalStations+1!");
            //if (distance.GetLength(0) != distance.GetLength(1))
            //    throw new Exception("Distance matrix is not a square!");
            //if (distance.GetLength(0) != nSites)
            //    throw new Exception("Distance matrix doesn't have nCustomers+nNonDepotExternalStations+1 rows and columns!");
        }
        public void Write()
        {
            WriteSiteRelatedData();
            WriteVehicleRelatedData();
            WriteBottomOverallData();
            WriteDistanceData();
            sw.Flush();
            sw.Close();
        }
        void WriteInfoAboutFile()
        {
        }
        void WriteSiteRelatedData()
        {
            sw.WriteLine("StringID\tType\tx\ty\tdemand\tReadyTime\tDueDate\tServiceDuration\tRechargingRate\tRefuelingCost($perKWH)\tEVPrize\tGDVPrize");
            for (int i = 0; i < numNodes; i++)
            {
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}", nodeID[i], nodeType[i], X[i], Y[i], Demand[i], TimeWindowStart[i], TimeWindowEnd[i], CustomerServiceDuration[i], Gamma[i],RefuelingCostPerKWH[i], Prize[0, i], Prize[1, i]);
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}", nodeID[i], nodeType[i], X[i], Y[i], Demand[i], TimeWindowStart[i], TimeWindowEnd[i], CustomerServiceDuration[i], Gamma[i],RefuelingCostPerKWH[i], Prize[0, i], Prize[1, i]);
            }
            sw.WriteLine();
        }
        void WriteVehicleRelatedData()
        {
            sw.WriteLine(StringOperations.CombineAndTabSeparateArray(Vehicle.GetHeaderRow()));
            sw.WriteLine(StringOperations.CombineAndTabSeparateArray(VehData.SelectedEV.GetIndividualRow()));
            sw.WriteLine(StringOperations.CombineAndTabSeparateArray(VehData.SelectedGDV.GetIndividualRow()));
            sw.WriteLine();
        }
        void WriteBottomOverallData()
        {
            sw.WriteLine("Average Velocity\t{0}", TravelSpeed);
            sw.WriteLine("Refuel Cost of Gas\t{0}", RefuelCostOfGasPerGallon);
            sw.WriteLine("Refuel Cost At Depot\t{0}", RefuelCostAtDepotPerKWH);
            sw.WriteLine("Refuel Cost In Network\t{0}", RefuelCostInNetworkPerKWH);
            sw.WriteLine("Refuel Cost Out Network\t{0}", RefuelCostOutNetworkPerKWH);
            sw.WriteLine();
        }

        void WriteDistanceData()
        {
            string position;
            if (UseGeogPosition)
            {
                position = "Long-Lat";
            }
            else
            {
                position = "X-Y";
            }       
            sw.WriteLine("Positions\t{0}", position);
            sw.WriteLine();
            if (Distance != null)
            {
                sw.WriteLine("Distances");
                for (int i = 0; i < numNodes; i++)
                {
                    for (int j = 0; j < numNodes; j++)
                    {
                        sw.Write(Distance[i, j] + "\t");
                    }
                    sw.WriteLine();
                }
            }
            sw.WriteLine();
        }
    }
}
