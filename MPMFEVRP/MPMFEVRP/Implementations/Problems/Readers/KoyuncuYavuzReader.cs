using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Utils;

namespace MPMFEVRP.Implementations.Problems
{
    class KoyuncuYavuzReader : IReader
    {
        //File related input
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;

        //Site related input
        string[] ID;
        string[] Type;
        double[] X;
        double[] Y;
        double[] Demand;
        double[] ReadyTime;
        double[] DueDate;
        double[] ServiceDuration;
        double[] RechargingRate;
        double[][] prize;

        //Vehicle related input
        string[] VehID;
        string[] VehCategory;
        int[] VehLoadCap;
        double[] VehBatteryCap;
        double[] VehConsumpRate;
        double[] VehFixedCost;
        double[] VehVariableCost;
        double[] VehMaxChargingRate;

        //Speed and distance
        double TravelSpeed;
        double[,] Distance = null;

        //Intermediate steps related
        int numCustomers = 0;
        int numESS = 0;
        Site site;
        Vehicle vehicle;
        string distType;

        //Output related fields
        Site[] siteArray; public Site[] SiteArray { get { return siteArray; } }
        Vehicle[] vehicleArray; public Vehicle[] VehicleArray { get { return vehicleArray; } }

        System.IO.StreamReader sr;

        public KoyuncuYavuzReader()
        {

        }
        public KoyuncuYavuzReader(string sourceDirectory, string file_name, string file_extension)
        {
            this.sourceDirectory = sourceDirectory;
            this.file_name = file_name;
            this.file_extension = file_extension;
            fullFilename = StringOperations.CombineFullFileName(file_name, file_extension, sourceDirectory: sourceDirectory);
            sr = new System.IO.StreamReader(fullFilename);
        }
        public KoyuncuYavuzReader(string fullFilename)
        {
            this.fullFilename = fullFilename;
            string[] separatedFullFileName = StringOperations.SeparateFullFileName(fullFilename);
            sourceDirectory = separatedFullFileName[0];
            file_name = separatedFullFileName[1];
            file_extension = separatedFullFileName[2];
            sr = new System.IO.StreamReader(fullFilename);
        }

        public void Read()
        {
            string wholeFile = sr.ReadToEnd();
            sr.Close();
            ProcessRawDataFromFile(wholeFile);
        }
        public void ProcessRawDataFromFile(string wholeFile)
        {
            string[] allRows = wholeFile.Split(new char[] { '\n' });
            int blankRowPosition = 0;
            while (allRows[blankRowPosition] != "\r")
                blankRowPosition++;

            int nTabularRows = blankRowPosition - 1;
            ID = new string[nTabularRows];
            Type = new string[nTabularRows];
            X = new double[nTabularRows];
            Y = new double[nTabularRows];
            Demand = new double[nTabularRows];
            ReadyTime = new double[nTabularRows];
            DueDate = new double[nTabularRows];
            ServiceDuration = new double[nTabularRows];
            RechargingRate = new double[nTabularRows];
            prize = new double[nTabularRows][];

            Site[] siteArray = new Site[nTabularRows];

        char[] cellSeparator = new char[] { '\t', '\r'};
            string[] cellsInCurrentRow;
            for (int r = 1; r <= nTabularRows; r++)
            {
                //Read site related data
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                ID[r - 1] = cellsInCurrentRow[0];
                Type[r - 1] = cellsInCurrentRow[1];
                if (Type[r - 1].Contains("c"))
                    numCustomers++;
                if (Type[r - 1].Contains("e"))
                    numESS++;
                X[r - 1] = double.Parse(cellsInCurrentRow[2]);
                Y[r - 1] = double.Parse(cellsInCurrentRow[3]);
                Demand[r - 1] = double.Parse(cellsInCurrentRow[4]);
                ReadyTime[r - 1] = double.Parse(cellsInCurrentRow[5]);
                DueDate[r - 1] = double.Parse(cellsInCurrentRow[6]);
                ServiceDuration[r - 1] = double.Parse(cellsInCurrentRow[7]);
                RechargingRate[r - 1] = double.Parse(cellsInCurrentRow[8]);
                prize[r - 1] = new double[cellsInCurrentRow.Length - 9];
                for (int i = 0; i < prize[r - 1].Length; i++)
                    prize[r - 1][i] = double.Parse(cellsInCurrentRow[9 + i]);
            }

            int blankRowPosition2 = blankRowPosition+1;
            while (allRows[blankRowPosition2] != "\r")
                blankRowPosition2++;
            int nTabularRows2 = blankRowPosition2 - 1;
            VehID = new string[nTabularRows2- nTabularRows - 2];
            VehCategory = new string[nTabularRows2 - nTabularRows - 2];
            VehLoadCap = new int[nTabularRows2 - nTabularRows - 2];
            VehBatteryCap = new double[nTabularRows2 - nTabularRows - 2];
            VehConsumpRate = new double[nTabularRows2 - nTabularRows - 2];
            VehFixedCost = new double[nTabularRows2 - nTabularRows - 2];
            VehVariableCost = new double[nTabularRows2 - nTabularRows - 2];
            VehMaxChargingRate = new double[nTabularRows2 - nTabularRows - 2];

            Vehicle[] vehicleArray = new Vehicle[VehID.Length];

            for (int r = nTabularRows+3; r <= nTabularRows2; r++)
            {
                //Read vehicle related data
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                VehID[r - nTabularRows - 3] = cellsInCurrentRow[0];
                VehCategory[r - nTabularRows - 3] = cellsInCurrentRow[1];
                VehLoadCap[r - nTabularRows - 3] = int.Parse(cellsInCurrentRow[2]);
                VehBatteryCap[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[3]);
                VehConsumpRate[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[4]);
                VehFixedCost[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[5]);
                VehVariableCost[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[6]);
                VehMaxChargingRate[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[7]);

            }
            cellsInCurrentRow = allRows[blankRowPosition2+1].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            TravelSpeed= double.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[blankRowPosition2 + 3].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (cellsInCurrentRow.Contains("Long-Lat")) //Distance matrix exists
            {
                distType = "Long-Lat";
            }
            else
            {
                distType = "X-Y";
            }

            if (allRows.Contains("Distances")) //Distance matrix exists
            {
                for (int i = 0; i < ID.Length; i++)
                {
                    cellsInCurrentRow = allRows[i + blankRowPosition2 + 3].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < ID.Length; j++)
                    {
                        Distance[i, j] = double.Parse(cellsInCurrentRow[j]);
                    }
                }
            }
        }
        public string getRecommendedOutputFileFullName()
        {
            string output = sourceDirectory;
            output += "KoyuncuYavuz_";//This is the prefix
            output += file_name;
            output += file_extension;
            return output;
        }
        public Site[] getSiteArray()
        {
            Site[]  siteArray = new Site[ID.Length]; 
            for(int i=0; i < ID.Length; i++)
            {
                site = new Site(ID[i], Type[i], X[i], Y[i], Demand[i], ReadyTime[i], DueDate[i], ServiceDuration[i], RechargingRate[i], prize[i]);
                siteArray[i] = site;
            }
            return siteArray;
        }
        public int getNumberOfCustomers() { return numCustomers; }
        public int getNumberOfES() { return numESS; }
        public Vehicle[] getVehicleArray()
        {
            Vehicle[]  vehicleArray = new Vehicle[VehID.Length];
            for (int i = 0; i < VehID.Length; i++)
            {
                VehicleCategories VehCat;
                switch (VehCategory[i])
                {
                    case "EV":
                        VehCat = VehicleCategories.EV;
                        break;
                    case "GDV":
                        VehCat = VehicleCategories.GDV;
                        break;
                    default:
                        VehCat = VehicleCategories.EV;
                        break;
                }
                vehicle = new Vehicle(VehID[i], VehCat, VehLoadCap[i], VehBatteryCap[i], VehConsumpRate[i], VehFixedCost[i], VehVariableCost[i], VehMaxChargingRate[i]);
                vehicleArray[i] = vehicle;
            }
            return vehicleArray;
        }
        public double getTravelSpeed() { return TravelSpeed; }
        public double[,] getDistanceMatrix() { return Distance; }
        public bool isLongLat()
        {
            if(distType=="Long-Lat")
            {
                return true;
            }
            else if(distType == "X-Y")
            {
                return false;
            }
            throw new Exception("Check the file long-lat or X-Y is not specified!");
        }
    }
}
