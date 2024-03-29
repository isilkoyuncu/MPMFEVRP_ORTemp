﻿using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Implementations.Problems.Readers
{
    public class KoyuncuYavuzReader : IReader
    {
        //File related input
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;

        //Site related input
        string[] id;
        string[] type;
        double[] x;
        double[] y;
        double[] demand;
        double[] readyTime;
        double[] dueDate;
        double[] serviceDuration;
        double[] rechargingRate;
        double[] refuelingCostPerKWH;
        double[][] prize;

        //Vehicle related input
        string[] vehID;
        string[] vehCategory;
        int[] vehLoadCap;
        double[] vehBatteryCap;
        double[] vehConsumpRate;
        double[] vehFixedCost;
        double[] vehVariableCost;
        double[] vehMaxChargingRate;
        double[] vehFixedRefuelingTime;

        //Speed,  and distance
        double travelSpeed;
        double refuelCostofGas;
        double refuelCostAtDepot;
        double refuelCostInNetwork;
        double refuelCostOutNetwork;

        double[,] distance = null;

        //Intermediate steps related
        int numCustomers = 0;
        int numESS = 0;
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
            string[] allRows = wholeFile.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            int blankRowPosition = 0;
            while (!RowIsBlank(allRows[blankRowPosition]))
                blankRowPosition++;

            int nTabularRows = blankRowPosition - 1;
            id = new string[nTabularRows];
            type = new string[nTabularRows];
            x = new double[nTabularRows];
            y = new double[nTabularRows];
            demand = new double[nTabularRows];
            readyTime = new double[nTabularRows];
            dueDate = new double[nTabularRows];
            serviceDuration = new double[nTabularRows];
            rechargingRate = new double[nTabularRows];
            refuelingCostPerKWH = new double[nTabularRows];
            prize = new double[nTabularRows][];

            char[] cellSeparator = new char[] { '\t' };
            string[] cellsInCurrentRow;
            for (int r = 1; r <= nTabularRows; r++)
            {
                //Read site related data
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                id[r - 1] = cellsInCurrentRow[0];
                type[r - 1] = cellsInCurrentRow[1];
                if (type[r - 1].Contains("c"))
                    numCustomers++;
                if (type[r - 1].Contains("e"))
                    numESS++;
                x[r - 1] = double.Parse(cellsInCurrentRow[2]);
                y[r - 1] = double.Parse(cellsInCurrentRow[3]);
                demand[r - 1] = double.Parse(cellsInCurrentRow[4]);
                readyTime[r - 1] = double.Parse(cellsInCurrentRow[5]);
                dueDate[r - 1] = double.Parse(cellsInCurrentRow[6]);
                serviceDuration[r - 1] = double.Parse(cellsInCurrentRow[7]);
                rechargingRate[r - 1] = double.Parse(cellsInCurrentRow[8]);
                refuelingCostPerKWH[r - 1] = double.Parse(cellsInCurrentRow[9]);
                prize[r - 1] = new double[cellsInCurrentRow.Length - 9];
                for (int i = 0; i < prize[r - 1].Length; i++)
                    prize[r - 1][i] = double.Parse(cellsInCurrentRow[9 + i]);
            }
            PopulateSiteArray();

            int blankRowPosition2 = blankRowPosition+1;
            while (!RowIsBlank(allRows[blankRowPosition2]))
                blankRowPosition2++;
            int nTabularRows2 = blankRowPosition2 - 1;
            vehID = new string[nTabularRows2- nTabularRows - 2];
            vehCategory = new string[nTabularRows2 - nTabularRows - 2];
            vehLoadCap = new int[nTabularRows2 - nTabularRows - 2];
            vehBatteryCap = new double[nTabularRows2 - nTabularRows - 2];
            vehConsumpRate = new double[nTabularRows2 - nTabularRows - 2];
            vehFixedCost = new double[nTabularRows2 - nTabularRows - 2];
            vehVariableCost = new double[nTabularRows2 - nTabularRows - 2];
            vehMaxChargingRate = new double[nTabularRows2 - nTabularRows - 2];
            vehFixedRefuelingTime = new double[nTabularRows2 - nTabularRows - 2];

            for (int r = nTabularRows+3; r <= nTabularRows2; r++)
            {
                //Read vehicle related data
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                vehID[r - nTabularRows - 3] = cellsInCurrentRow[0];
                vehCategory[r - nTabularRows - 3] = cellsInCurrentRow[1];
                vehLoadCap[r - nTabularRows - 3] = int.Parse(cellsInCurrentRow[2]);
                vehBatteryCap[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[3]);
                vehConsumpRate[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[4]);
                vehFixedCost[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[5]);
                vehVariableCost[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[6]);
                vehMaxChargingRate[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[7]);
                vehFixedRefuelingTime[r - nTabularRows - 3] = double.Parse(cellsInCurrentRow[8]);
            }
            PopulateVehicleArray();

            cellsInCurrentRow = allRows[blankRowPosition2+1].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            travelSpeed= double.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[blankRowPosition2 + 2].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            refuelCostofGas = double.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[blankRowPosition2 + 3].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            refuelCostAtDepot = double.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[blankRowPosition2 + 4].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            refuelCostInNetwork = double.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[blankRowPosition2 + 5].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            refuelCostOutNetwork = double.Parse(cellsInCurrentRow[1]);

            cellsInCurrentRow = allRows[blankRowPosition2 + 7].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (cellsInCurrentRow.Contains("Long-Lat")) //Distance matrix exists
            {
                distType = "Long-Lat";
            }
            else
            {
                distType = "X-Y";
            }
            for (int r = 0; r < allRows.Length; r++)
            {
                if ((allRows[r].Replace("\t", "").Replace(" ", "")).Contains("Distances"))
                {
                    distance = new double[id.Length, id.Length];
                    for (int i = r + 1; i <= r + id.Length; i++)
                    {
                        cellsInCurrentRow = allRows[i].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < id.Length; j++)
                        {
                            distance[i-(r+1), j] = double.Parse(cellsInCurrentRow[j]);
                        }
                    }
                    break;
                }
            }
        }
        bool RowIsBlank(string theRow)
        {
            List<string> endOfLineStrings = new List<string>() { "\n", "\r", "\r\n" };
            string cleanedRow = theRow.Replace("\t", "").Replace(" ", "");
            return ((endOfLineStrings.Contains(cleanedRow))||(cleanedRow==""));
        }
        bool CustomerInfeasible(int c)
        {
            if (CustomerDirectRouteExceedsWorkdayLength(c))//11-hour day (660 minutes)
                return true;
            if (CustomerCannotBeReachedWithAtMostOneESVisit(c))
                return true;
            return false;
        }
        bool CustomerDirectRouteExceedsWorkdayLength(int c)
        {
            double minStay = 0.0;
            if (type[c].Contains("c"))
            {
                minStay = serviceDuration[c];
            }
            return ((distance[c,0]+distance[0, c]) / travelSpeed + minStay > dueDate[0]); 
        }
        bool CustomerCannotBeReachedWithAtMostOneESVisit(int c)
        {
            if ((distance[c, 0] + distance[0, c]) <= vehBatteryCap[0]/vehConsumpRate[0])
                return false;
            for (int r=0; r< id.Length; r++)
                if(type[r].Contains("e"))
                if (distance[0, r] <= vehBatteryCap[0] / vehConsumpRate[0])
                    if (distance[r, c] + distance[c, 0] <= vehBatteryCap[0] / vehConsumpRate[0])
                        if ((distance[0, r] + distance[r, c] + distance[c, 0]) / travelSpeed + serviceDuration[c] + 15.0 <= dueDate[0])
                            return false;
            return true;
        }
        
        public string GetRecommendedOutputFileFullName()
        {
            string output = "";// sourceDirectory;
            //output += "KoyuncuYavuz_";//This is the prefix//TODO: Adding the prefix was causing in some places to see the prefix before source directory, which doesn't make any sense. So, I (MY) have disabled the addition of this prefix for now.
            output += file_name;
            output += file_extension;
            return output;
        }
        void PopulateSiteArray()
        {
            Site site;

            siteArray = new Site[id.Length];
            for (int i = 0; i < id.Length; i++)
            {
                site = new Site(id[i], type[i], x[i], y[i], demand[i], readyTime[i], dueDate[i], serviceDuration[i], rechargingRate[i], refuelingCostPerKWH[i], prize[i]);
                siteArray[i] = site;
            }
        }
        public Site[] GetSiteArray()
        {
            if (siteArray == null)
                PopulateSiteArray();
            
            return siteArray;
        }
        public int GetNumberOfCustomers() { return numCustomers; }
        public int GetNumberOfES() { return numESS; }
        void PopulateVehicleArray()
        {
            Vehicle vehicle;

            vehicleArray = new Vehicle[vehID.Length];
            for (int i = 0; i < vehID.Length; i++)
            {
                VehicleCategories VehCat;
                switch (vehCategory[i])
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
                
                vehicle = new Vehicle(vehID[i], VehCat, vehLoadCap[i], vehBatteryCap[i], vehConsumpRate[i], vehFixedCost[i], vehVariableCost[i], vehMaxChargingRate[i], vehFixedRefuelingTime[i]);
                vehicleArray[i] = vehicle;
            }
        }
        //public Vehicle(string[] allInput)//ISSUE (#7): This constructor should not exist as it should never be the responsibility of the vehicle class to know how to decipher a 7-part string array, it should be the responsibility of the reader that reads the data
        //{
        //    if (allInput.Length != 7)
        //        throw new Exception("Expecting exactly 7 inputs into the 'Vehicle' class, because that's how many fields it has!");
        //    //TODO Parse all input to what they need to be and then set the field values appropriately; An example is below, test that it actually works.
        //    id = allInput[0];
        //    //category = allInput[1];
        //    loadCapacity = int.Parse(allInput[2]);
        //    batteryCapacity = double.Parse(allInput[3]);
        //    consumptionRate = double.Parse(allInput[4]);
        //    fixedCost = double.Parse(allInput[5]);
        //    variableCostPerMile = double.Parse(allInput[6]);
        //    maxChargingRate = double.Parse(allInput[7]);

        //    if (!int.TryParse(allInput[1], out loadCapacity))
        //        throw new Exception("load capacity was not successfully parsed from the input!");
        //    //TODO When this Vehicle constructor does all parsing correctly, you still need to verify that all fields were actually filled so we can use them later
        //}
        public Vehicle[] GetVehicleArray()
        {
            if (vehicleArray == null)
                PopulateVehicleArray();

            return vehicleArray;
        }
        public double GetTravelSpeed() { return travelSpeed; }
        public double GetRefuelCostofGas() { return refuelCostofGas; }
        public double GetRefuelCostAtDepot() { return refuelCostAtDepot; }
        public double GetRefuelCostInNetwork() { return refuelCostInNetwork; }
        public double GetRefuelCostOutNetwork() { return refuelCostOutNetwork; }


        public double[,] GetDistanceMatrix() { return distance; }
        public bool IsLongLat()
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
        public double[] GetXcoordidates()
        {
            double[] xCoords = new double[siteArray.Length];
            for(int s=0;s<siteArray.Length;s++)
            {
                xCoords[s] = siteArray[s].X;
            }
            return xCoords;
        }
        public double[] GetYcoordidates()
        {
            double[] yCoords = new double[siteArray.Length];
            for (int s = 0; s < siteArray.Length; s++)
            {
                yCoords[s] = siteArray[s].Y;
            }
            return yCoords;
        }
    }
}
