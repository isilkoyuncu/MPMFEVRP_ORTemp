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
            numESS = int.Parse(cellsInCurrentRow[1]);
            cellsInCurrentRow = allRows[2].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
            double refuelTimeMinutes = double.Parse(cellsInCurrentRow[1]);
            gamma = 24.0 / refuelTimeMinutes;
            int blankRowPosition = 4;
            while (allRows[blankRowPosition] != "\r")
                blankRowPosition++;

            int nTabularRows = blankRowPosition - 1;
            X = new double[nTabularRows];
            Y = new double[nTabularRows];
            for (int r = 1; r <= nTabularRows; r++)
            {
                cellsInCurrentRow = allRows[r].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                X[r - 1] = double.Parse(cellsInCurrentRow[0]);
                Y[r - 1] = double.Parse(cellsInCurrentRow[1]);
            }

            V = new Vehicle[2];
        }
        bool RowIsBlank(string theRow)
        {
            List<string> endOfLineStrings = new List<string>() { "\n", "\r", "\r\n" };
            string cleanedRow = theRow.Replace("\t", "").Replace(" ", "");
            return ((endOfLineStrings.Contains(cleanedRow)) || (cleanedRow == ""));
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
        public double[] getDemandColumn() { return null; }
        public double[] getReadyTimeColumn() { return null; }
        public double[] getDueDateColumn() { return null; }
        public double[] getServiceDurationColumn() { return null; }

        //public double[] getReadyTimeColumn() { return Enumerable.Repeat(0.0, ID.Length).ToArray(); }
        //public double[] getDueDateColumn() { return Enumerable.Repeat(480.0, ID.Length).ToArray(); }

        //public double[] getServiceDurationColumn()
        //{
        //    double[] toReturnServiceDuration = new double[ID.Length];
        //    for (int i = 0; i <= numESS; i++)
        //    {
        //        toReturnServiceDuration[i] = 0.0;
        //    }
        //    for (int i = numESS + 1; i < ID.Length; i++)
        //    {
        //        toReturnServiceDuration[i] = 30.0;
        //    }
        //    return toReturnServiceDuration;
        //}
        public double[] getRechargingRate() { return Enumerable.Repeat(1.0, X.Length).ToArray(); }//TODO change this to be based on gamma, where the rate given here applies to external stations only
        public double[,] getPrizeMatrix() { return null; }
        public double[,] getDistanceMatrix(){ return distance; }
        public Vehicle[] getVehicleRows(){ return V; }
        public double getTravelSpeed() { return 45.0 / 60.0; }
        public int getNumCustomers() { return numCustomers; }
        public int getNumESS() { return numESS; }
        public string getInputFileType() { return "YavuzCapar_17"; /*"KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17"*/}
    }
}
