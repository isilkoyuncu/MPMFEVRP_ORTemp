﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;

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
        private string inputFileName;
        private string[] inputString;

        //Output related
        string[] instanceSolutionSummary;
        public string[] InstanceSolutionSummary { get { return instanceSolutionSummary; } }
        private string[] headerRow;
        public string[] HeaderRow { get { return headerRow; } }

        string outputFileName;

        System.IO.StreamReader sr;
        List<string> outputSumm;
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
            int nTabularRows = blankRowPosition;
            instanceSolutionSummary = new string[nTabularRows];
            instanceSolutionSummary = outputSumm.ToArray();
        }
        public string[] GetHeaderRow()
        {
            List<string> outputSumm = new List<string>();
            int nTabularRows = outputSumm.Count() - 1;
            for (int i = 0; i < nTabularRows; i++)
            {
                String toRemove = outputSumm[i];
                instanceSolutionSummary[i] = toRemove.Substring(0, toRemove.IndexOf(":"));
            }
            return headerRow;
        }
    }
}