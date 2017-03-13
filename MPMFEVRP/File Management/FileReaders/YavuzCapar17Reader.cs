using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Interfaces;
using Instance_Generation.Other;
using Instance_Generation.Utility;

namespace Instance_Generation.FileReaders
{
    class YavuzCapar17Reader : IRawReader
    {
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;
        int nSiteRows;
        string[] ID;
        string[] Type;
        double[] X;
        double[] Y;
        double[,] distance;
        Vehicle[] V;//TODO A new vehicle constructor is needed
        int numCustomers = 0;


        System.IO.StreamReader sr;

        public YavuzCapar17Reader()
        {

        }
        public YavuzCapar17Reader(string sourceDirectory, string file_name, string file_extension)
        {
            this.sourceDirectory = sourceDirectory;
            this.file_name = file_name;
            this.file_extension = file_extension;
            fullFilename = StringOperations.CombineFullFileName(file_name, file_extension, sourceDirectory: sourceDirectory);
            sr = new System.IO.StreamReader(fullFilename);
        }
        public YavuzCapar17Reader(string fullFilename)
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
            string[] allRows = wholeFile.Split(new char[] { '\n' });
            int blankRowPosition = 0;
            while (allRows[blankRowPosition] != "\r")
                blankRowPosition++;

            int nTabularRows = blankRowPosition - 1;
            ID = new string[nTabularRows];
            Type = new string[nTabularRows];
            X = new double[nTabularRows];
            Y = new double[nTabularRows];
            char[] cellSeparator = new char[] { '\t', '\r', ' ' };
            string[] cellsInCurrentRow;
            for (int r = 1; r <= nTabularRows; r++)
            {
                //while (allRows[r].Contains("  "))
                //{
                //    int indexOfDoubleSpace = allRows[r].IndexOf("  ");
                //    allRows[r].Remove(indexOfDoubleSpace, 2);
                //    allRows[r].Insert(indexOfDoubleSpace, " ");
                //}
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                ID[r - 1] = cellsInCurrentRow[0];
                Type[r - 1] = cellsInCurrentRow[1];
                X[r - 1] = double.Parse(cellsInCurrentRow[2]);
                Y[r - 1] = double.Parse(cellsInCurrentRow[3]);
            }

            V = new Vehicle[2];
            //V[0] = new Vehicle("YavuzCapar17AFV", VehicleCategories.EV, 0, 30, 0.25, 0.35);
            //V[1] = new Vehicle("YavuzCapar17GDV", VehicleCategories.GDV, 0, 0, 0, 1);
        }
        public string getRecommendedOutputFileFullName()
        {
            string output = sourceDirectory;
            output += "YavuzCapar17_";//This is the prefix
            output += file_name;
            output += file_extension;
            return output;
        }
        public string[] getIDColumn() { return ID; }
        public string[] getTypeColumn() { return Type; }
        public bool usesGeographicPositions() { return false; }
        public bool needToShuffleCustomers() { return false; }
        public double[] getXorLongitudeColumn() { return X; }
        public double[] getYorLatitudeColumn() { return Y; }
        public double[] getDemandColumn() { return Enumerable.Repeat(0.0, ID.Length).ToArray(); }
        public double[] getReadyTimeColumn() { return Enumerable.Repeat(0.0, ID.Length).ToArray(); }
        public double[] getDueDateColumn() { return Enumerable.Repeat(480.0, ID.Length).ToArray(); }
        public double[] getServiceDurationColumn() { return Enumerable.Repeat(30.0, ID.Length).ToArray(); }
        public double[] getRechargingRate() { return Enumerable.Repeat(1.0, ID.Length).ToArray(); }//TODO change this to be based on gamma, where the rate given here applies to external stations only
        public double[,] getPrizeMatrix() { return null; }
        public double[,] getDistanceMatrix(){ return distance; }
        public Vehicle[] getVehicleRows(){ return V; }
        public double getTravelSpeed() { return 45.0 / 60.0; }
        public int getNumCustomers() { return numCustomers; }
        public int getNumESS() { return -1; }
        public string getInputFileType() { return "YavuzCapar_17"; /*"KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17"*/}
    }
}
