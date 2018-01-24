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
        int numCustomers = 0;
        int numESS = 0;
        int numSites = 0;
        double gamma = 0.0;
        double[] X;
        double[] Y;
        double[,] distance;
        Vehicle[] V;//TODO A new vehicle constructor is needed

        System.IO.StreamReader sr;

        public YavuzCapar17Reader() { }

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
            string[] allRows = wholeFile.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            char[] cellSeparator = new char[] { '\t' };
            string[] cellsInCurrentRow;
            cellsInCurrentRow = allRows[0].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            numCustomers = int.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[1].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            numESS = int.Parse(cellsInCurrentRow[1]) + 1; //+1 is for the ESS at the depot, YC does not count the depot in their numESS reports on the file
            numSites = numCustomers + numESS + 1;
            cellsInCurrentRow = allRows[2].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            double refuelTimeMinutes = double.Parse(cellsInCurrentRow[1]);
            gamma = 24.0 / refuelTimeMinutes;

            int blankRowPosition = 5;
            while (allRows[blankRowPosition] != "")
                blankRowPosition++;

            int nTabularRows = blankRowPosition - 4;
            if (nTabularRows != numSites)
                throw new Exception("number of rows is different than the total number of sites");
            double[] tempX = new double[nTabularRows];
            double[] tempY = new double[nTabularRows];
            for (int r = 5; r < nTabularRows+4; r++)
            {
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                tempX[r - 5] = double.Parse(cellsInCurrentRow[0]);
                tempY[r - 5] = double.Parse(cellsInCurrentRow[1]);
            }
            tempX[nTabularRows-1] = tempX[0];
            tempY[nTabularRows-1] = tempY[0];
            SortXY(tempX, tempY);//Sorts as follows: first depot, then ESs, then customers
            double[,] tempDistance = new double[nTabularRows, nTabularRows];
            string[] distanceRows = allRows[blankRowPosition + 2].Split(new string[] { "\r" }, StringSplitOptions.None);
            for (int i = 0; i < nTabularRows-1; i++)
            {
                for (int j = 0; j < nTabularRows-1; j++)
                {
                    cellsInCurrentRow = distanceRows[i].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                    tempDistance[i,j] = double.Parse(cellsInCurrentRow[j]);
                }
            }
            for (int j = 0; j < nTabularRows-1; j++)
            {
                tempDistance[nTabularRows-1, j] = tempDistance[0, j];
            }
            for (int i = 0; i < nTabularRows-1; i++)
            {
                tempDistance[i, nTabularRows-1] = tempDistance[i, 0];
            }
            SortDistances(tempDistance);
            V = new Vehicle[2];
        }
        void SortXY(double[] tempX, double[] tempY)
        {
            X = new double[numSites];
            Y = new double[numSites];
            int nodeCounter = 0;
            X[nodeCounter] = tempX[0];
            Y[nodeCounter] = tempY[0];
            nodeCounter++;
            for (int i=numCustomers+1; i<=numCustomers+numESS; i++)
            {
                X[nodeCounter] = tempX[i];
                Y[nodeCounter] = tempY[i];
                nodeCounter++;
            }
            for (int i = 1; i <= numCustomers; i++)
            {
                X[nodeCounter] = tempX[i];
                Y[nodeCounter] = tempY[i];
                nodeCounter++;
            }
        }
        void SortDistances(double[,] tempDist)
        {
            distance = new double[numSites, numSites];
            //First from 0 to 0, then to ESs and then to customers
            int jCounter = 0;
            distance[0,jCounter++] = tempDist[0,0];
            for (int j = numCustomers + 1; j <= numCustomers + numESS; j++)
                distance[0, jCounter++] = tempDist[0, j];
            for (int j = 1; j <= numCustomers; j++)
                distance[0, jCounter++] = tempDist[0, j];

            //Second from each ES to 0, then to ESs and then to customers
            for (int i = 1; i <= numESS; i++)
            {
                jCounter = 0;
                distance[i, jCounter++] = tempDist[numCustomers+i, 0];
                for (int j = numCustomers + 1; j <= numCustomers + numESS; j++)
                    distance[i, jCounter++] = tempDist[numCustomers + i, j];
                for (int j = 1; j <= numCustomers; j++)
                    distance[i, jCounter++] = tempDist[numCustomers + i, j];
            }

            //Third from each customer to 0, then to ESs and then to customers
            for (int i = numESS+1; i < numSites; i++)
            {
                jCounter = 0;
                distance[i, jCounter++] = tempDist[i-numESS, 0];
                for (int j = numCustomers + 1; j <= numCustomers + numESS; j++)
                    distance[i, jCounter++] = tempDist[i - numESS, j];
                for (int j = 1; j <= numCustomers; j++)
                    distance[i, jCounter++] = tempDist[i - numESS, j];
            }
        }
        public string getRecommendedOutputFileFullName()
        {
            string output = sourceDirectory;
            output += "YavuzCapar17_";//This is the prefix
            output += file_name;
            output += file_extension;
            return output;
        }
        public string[] getIDColumn() { return null; }
        public string[] getTypeColumn() { return null; }
        public bool usesGeographicPositions() { return false; }
        public bool needToShuffleCustomers() { return false; }
        public double[] getXorLongitudeColumn() { return X; }
        public double[] getYorLatitudeColumn() { return Y; }
        public double[] getDemandColumn() { return Enumerable.Repeat(0.0, X.Length).ToArray(); }
        public double[] getReadyTimeColumn() { return Enumerable.Repeat(0.0, X.Length).ToArray(); }
        public double[] getDueDateColumn() { return Enumerable.Repeat(480.0, X.Length).ToArray(); }
        public double[] getServiceDurationColumn()
        {
            double[] toReturnServiceDuration = new double[X.Length];
            for (int i = 0; i <= numESS; i++)
            {
                toReturnServiceDuration[i] = 0.0;
            }
            for (int i = numESS + 1; i < X.Length; i++)
            {
                toReturnServiceDuration[i] = 30.0;
            }
            return toReturnServiceDuration;
        }
        public double[] getRechargingRate() { return Enumerable.Repeat((24.0/30.0), X.Length).ToArray(); }//TODO change this to be based on gamma, where the rate given here applies to external stations only
        public double[,] getPrizeMatrix() { return null; }
        public double[,] getDistanceMatrix(){ return distance; }
        public Vehicle[] getVehicleRows(){ return V; }
        public double getTravelSpeed() { return 45.0 / 60.0; }
        public int getNumCustomers() { return numCustomers; }
        public int getNumESS() { return numESS; }
        public string getInputFileType() { return "YavuzCapar_17"; /*"KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17"*/}
    }
}
