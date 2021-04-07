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
    class ErdoganMiller_Hooks12Reader : IRawReader
    {
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;
        string[] ID;
        string[] Type;
        double[] X;
        double[] Y;
        Vehicle[] V;
        int numCustomers=0;
        int numESS = 0;
        
        System.IO.StreamReader sr;

        public ErdoganMiller_Hooks12Reader() { }
        public ErdoganMiller_Hooks12Reader(string sourceDirectory, string file_name, string file_extension)
        {
            this.sourceDirectory = sourceDirectory;
            this.file_name = file_name;
            this.file_extension = file_extension;
            fullFilename = StringOperations.CombineFullFileName(file_name, file_extension, sourceDirectory: sourceDirectory);
            sr = new System.IO.StreamReader(fullFilename);
        }
        public ErdoganMiller_Hooks12Reader(string fullFilename)
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
            char[] cellSeparator = new char[] { '\t', '\r' , ' '};
            string[] cellsInCurrentRow;
            for (int r = 1; r <= nTabularRows; r++)
            {
                cellsInCurrentRow = allRows[r].Split(cellSeparator,StringSplitOptions.RemoveEmptyEntries);
                ID[r - 1] = cellsInCurrentRow[0];
                Type[r - 1] = cellsInCurrentRow[1];
                if (Type[r - 1] == "c")
                    numCustomers++;
                if (Type[r - 1] == "f")
                    numESS++;
                X[r - 1] = double.Parse(cellsInCurrentRow[2]);
                Y[r - 1] = double.Parse(cellsInCurrentRow[3]);
            }

            V = new Vehicle[2];
        }
        public string getRecommendedOutputFileFullName()
        {
            string output = sourceDirectory;
            output += "EMH12_";//This is the prefix
            output += file_name;
            output += file_extension;
            return output;
        }
        public string[] getIDColumn() { return ID; }
        public string[] getTypeColumn() { return Type; }
        public bool usesGeographicPositions() { return true; }
        public bool needToShuffleCustomers() { return true; }
        public double[] getXorLongitudeColumn() { return X; }
        public double[] getYorLatitudeColumn() { return Y; }
        public double[] getDemandColumn() { return Enumerable.Repeat(0.0, ID.Length).ToArray(); }
        public double[] getReadyTimeColumn() { return Enumerable.Repeat(0.0, ID.Length).ToArray(); }
        public double[] getDueDateColumn() { return Enumerable.Repeat(660.0, ID.Length).ToArray(); }
        public double[] getServiceDurationColumn() {
            double[] toReturnServiceDuration = new double[ID.Length];
            for(int i=0; i<=numESS; i++)
            {
                toReturnServiceDuration[i] = 0.0;
            }
            for (int i = numESS+1; i < ID.Length; i++)
            {
                toReturnServiceDuration[i] = 30.0;
            }
            return toReturnServiceDuration;
        }
        public double[] getRechargingRates() { return Enumerable.Repeat(4.0, ID.Length).ToArray(); }//TODO change this to be based on gamma, where the rate given here applies to external stations only
        public double getESRechargingRate() { return 4.0; }
        public double[,] getPrizeMatrix() { return null; }
        public double[,] getDistanceMatrix() { return null; }
        public Vehicle[] getVehicleRows() { return V; }
        public double getTravelSpeed() { return 40.0 / 60.0; }
        public int getNumCustomers() { return numCustomers; }
        public int getNumESS() { return numESS; }
        public string getInputFileType() { return "EMH_12"; /*"KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17"*/}
    }
}
