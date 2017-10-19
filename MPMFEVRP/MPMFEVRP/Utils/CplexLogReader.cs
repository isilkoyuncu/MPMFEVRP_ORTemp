﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Utils
{
    class CplexLogReader
    {
        //File related input
        string sourceDirectory;
        string file_name;
        string file_extension;
        string fullFilename;

        //Output related
        List<double> incumbents;
        public List<double> Incumbents { get {return incumbents; } }
        List<double> seconds;
        public List<double> Seconds { get { return seconds; } }
        string[] cplexLogSummary; public string[] CplexLogSummary { get { return cplexLogSummary; } }

        System.IO.StreamReader sr;

        public CplexLogReader() { }

        public CplexLogReader(string fullFilename)
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
            incumbents = new List<double>();
            seconds = new List<double>();
            string[] allRows = wholeFile.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            double incumbent=-1.0; double second=-1.0;
            for (int i=0; i<allRows.Length; i++)
            {
                int startInc = -1; int indexOfafter = -1; int startSec = -1; int endSec = -1;

                if (allRows[i].Contains("Found incumbent of value "))
                {
                    startInc = allRows[i].IndexOf("value ") + 6;
                    indexOfafter = allRows[i].IndexOf("after ");
                    string incumb = allRows[i].Substring(startInc, ((indexOfafter - 1)-startInc));
                    incumbent = double.Parse(incumb);
                    startSec = allRows[i].IndexOf("after ")+6;
                    endSec = allRows[i].IndexOf("sec", startSec)-1;
                    second = double.Parse(allRows[i].Substring(startSec, endSec-startSec));
                    incumbents.Add(incumbent);
                    seconds.Add(second);
                }
            }
            cplexLogSummary = new string[incumbents.Count+1];
            cplexLogSummary[0] = "Incumbent value\tSeconds";
            for (int i=0; i<incumbents.Count; i++)
            {
                string inc = incumbents[i].ToString();
                string sec = seconds[i].ToString();
                string row = inc + "\t" + sec;
                cplexLogSummary[i+1] = row;
            }
        }
    }
}
