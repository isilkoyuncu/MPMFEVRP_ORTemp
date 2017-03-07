﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Utils;

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

        //Speed and distance
        double travelSpeed;
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
            string[] allRows = wholeFile.Split(new char[] { '\n' });
            int blankRowPosition = 0;
            while (allRows[blankRowPosition] != "\r")
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
            prize = new double[nTabularRows][];

        char[] cellSeparator = new char[] { '\t', '\r'};
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
                prize[r - 1] = new double[cellsInCurrentRow.Length - 9];
                for (int i = 0; i < prize[r - 1].Length; i++)
                    prize[r - 1][i] = double.Parse(cellsInCurrentRow[9 + i]);
            }
            PopulateSiteArray();

            int blankRowPosition2 = blankRowPosition+1;
            while (allRows[blankRowPosition2] != "\r")
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
            }
            PopulateVehicleArray();

            cellsInCurrentRow = allRows[blankRowPosition2+1].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            travelSpeed= double.Parse(cellsInCurrentRow[1]);
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
                for (int i = 0; i < id.Length; i++)
                {
                    cellsInCurrentRow = allRows[i + blankRowPosition2 + 3].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < id.Length; j++)
                    {
                        distance[i, j] = double.Parse(cellsInCurrentRow[j]);
                    }
                }
            }
        }


        public string GetRecommendedOutputFileFullName()
        {
            string output = "";// sourceDirectory;
            output += "KoyuncuYavuz_";//This is the prefix
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
                site = new Site(id[i], type[i], x[i], y[i], demand[i], readyTime[i], dueDate[i], serviceDuration[i], rechargingRate[i], prize[i]);
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
                vehicle = new Vehicle(vehID[i], VehCat, vehLoadCap[i], vehBatteryCap[i], vehConsumpRate[i], vehFixedCost[i], vehVariableCost[i], vehMaxChargingRate[i]);
                vehicleArray[i] = vehicle;
            }
        }
        public Vehicle[] GetVehicleArray()
        {
            if (vehicleArray == null)
                PopulateVehicleArray();

            return vehicleArray;
        }
        public double GetTravelSpeed() { return travelSpeed; }
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
    }
}
