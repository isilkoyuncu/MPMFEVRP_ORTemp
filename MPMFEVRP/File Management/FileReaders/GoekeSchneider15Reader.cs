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
    class GoekeSchneider15Reader : IRawReader
    {
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;
        string[] ID;
        string[] Type;
        double[] X;
        double[] Y;
        double[] demand;
        double[] readyTime;
        double[] dueDate;
        double[] serviceTime;
        double[] gamma;
        double[,] distance;
        Vehicle[] V;
        double velocity;
        int numCustomers = 0;


        System.IO.StreamReader sr;

        public GoekeSchneider15Reader()
        {

        }
        public GoekeSchneider15Reader(string sourceDirectory, string file_name, string file_extension)
        {
            this.sourceDirectory = sourceDirectory;
            this.file_name = file_name;
            this.file_extension = file_extension;
            fullFilename = StringOperations.CombineFullFileName(file_name, file_extension, sourceDirectory: sourceDirectory);
            sr = new System.IO.StreamReader(fullFilename);
        }
        public GoekeSchneider15Reader(string fullFilename)
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
            int count = 0;
            while (!allRows[count].Contains("numVeh"))
                count++;

            int nTabularRows = count - 2;
            ID = new string[nTabularRows];
            Type = new string[nTabularRows];
            X = new double[nTabularRows];
            Y = new double[nTabularRows];
            demand = new double[nTabularRows];
            readyTime = new double[nTabularRows];
            dueDate = new double[nTabularRows];
            serviceTime = new double[nTabularRows];
            char[] cellSeparator = new char[] { '\t', '\r', ' ' };
            string[] cellsInCurrentRow;
            for (int r = 1; r <= nTabularRows; r++)
            {
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                ID[r - 1] = cellsInCurrentRow[0];
                Type[r - 1] = cellsInCurrentRow[1];
                X[r - 1] = double.Parse(cellsInCurrentRow[2]);
                Y[r - 1] = double.Parse(cellsInCurrentRow[3]);
                demand[r - 1] = double.Parse(cellsInCurrentRow[4]);
                readyTime[r - 1] = double.Parse(cellsInCurrentRow[5]);
                dueDate[r - 1] = double.Parse(cellsInCurrentRow[6]);
                serviceTime[r - 1] = double.Parse(cellsInCurrentRow[7]);
            }
        }
        public string getRecommendedOutputFileFullName()
        {
            string output = sourceDirectory;
            output += "Goeke15_";//This is the prefix
            output += file_name;
            output += file_extension;
            return output;
        }
        public string[] getIDColumn() { return ID; }
        public string[] getTypeColumn() { return Type; }
        public bool usesGeographicPositions() { return false; }
        public bool needToShuffleCustomers() { return true; }
        public double[] getXorLongitudeColumn() { return X; }
        public double[] getYorLatitudeColumn() { return Y; }
        public double[] getDemandColumn() { return demand; }
        public double[] getReadyTimeColumn() { return readyTime; }
        public double[] getDueDateColumn() { return dueDate; }
        public double[] getServiceDurationColumn() { return serviceTime; }
        public double[] getRechargingRate() { return gamma; }
        public double[,] getPrizeMatrix() { return null; }
        public double[,] getDistanceMatrix() { return distance; }
        public Vehicle[] getVehicleRows() { return V; }
        public double getTravelSpeed() { return velocity; }
        public int getNumCustomers() { return numCustomers; }
        public int getNumESS() { return -1; }
        public string getInputFileType() { return "Goeke_15"; /*"KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17"*/}

    }
}
