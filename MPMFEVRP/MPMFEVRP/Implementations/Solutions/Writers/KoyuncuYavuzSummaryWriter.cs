using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Solutions.Writers
{
    public class KoyuncuYavuzSummaryWriter : IWriter
    {
        String fileName;
        System.IO.StreamWriter sw;
        private string outputFileName;
        private List<string[]> solutionSummaryList;

        public KoyuncuYavuzSummaryWriter()
        {

        }
        public KoyuncuYavuzSummaryWriter(string fileName, List<string[]> solutionSummaryList)
        {
            this.fileName = fileName;
            this.solutionSummaryList = solutionSummaryList;
            outputFileName = fileName.Remove(fileName.IndexOf(".txt"),4)+"_summary.txt";
            sw = new System.IO.StreamWriter(outputFileName);
        }

        public void WriteHeader()
        {
            sw.WriteLine("InstanceName\tAlgorithmName\tParameter1\tParameter2\tCPUtime\tSolutionStatus\tUB(BestInt)\tLB(Relaxed)\tGap");
        }
        public void Write()
        {
            WriteHeader();
            for (int i = 0; i < solutionSummaryList.Count; i++)
            {
                for (int j = 0; j < solutionSummaryList[i].Length; j++)
                {
                    sw.Write(solutionSummaryList[i][j]+"\t");
                }
                sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
        }
    }
}
