using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Solutions.Readers
{
    public class KoyuncuYavuzSolutionReader
    {
        //File related input
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;

        //Input related

        //Output related
        string[] instanceSolutionSummary;
        public string[] InstanceSolutionSummary { get { return instanceSolutionSummary; } }

        System.IO.StreamReader sr;
        public KoyuncuYavuzSolutionReader()
        {
        }

        public KoyuncuYavuzSolutionReader(string fullFilename)
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
            List<string> outputSumm = new List<string>();
            int blankRowPosition = 0;
            char[] cellSeparator = new char[] { '\t', '\r', ' '};
            string[] cellsInCurrentRow;
            while (allRows[blankRowPosition] != "\r")
            {
                cellsInCurrentRow = allRows[blankRowPosition].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                outputSumm.Add(cellsInCurrentRow[cellsInCurrentRow.Length-1]);
                blankRowPosition++;
            }
            int nGDV = 0;
            int nEV = 0;
            int nESs = 0;
            while (allRows[blankRowPosition+2] != "\r")
            {
                cellsInCurrentRow = allRows[blankRowPosition+2].Split(cellSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (cellsInCurrentRow[1].Contains("EV"))
                {
                    nEV++;
                }
                else if (cellsInCurrentRow[1].Contains("GDV"))
                {
                    nGDV++;
                }
                else
                    throw new NotImplementedException();
                if (cellsInCurrentRow[0].Contains("BD"))
                {
                    nESs++;
                }
                blankRowPosition++;
            }
            outputSumm.Add(nEV.ToString());
            outputSumm.Add(nGDV.ToString());
            outputSumm.Add(nESs.ToString());
            int nTabularRows = blankRowPosition;
            instanceSolutionSummary = new string[outputSumm.Count];
            instanceSolutionSummary = outputSumm.ToArray();
        }
    }
}
