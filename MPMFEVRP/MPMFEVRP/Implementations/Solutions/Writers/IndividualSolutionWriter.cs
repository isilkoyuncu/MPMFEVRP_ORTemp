using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Implementations.Solutions.Writers
{
    public class IndividualSolutionWriter : IWriter
    {
        System.IO.StreamWriter sw;

        private string inputFileName;
        private string[] algorithmOutputSummary;
        private string[] solutionOutputSummary;
        private string[] writableSolution;
        string outputFileName;

        public IndividualSolutionWriter()
        {
            //Empty constructor
        }
        public IndividualSolutionWriter(string inputFileName, string[] algorithmOutputSummary, string[] solutionOutputSummary, string[] writableSolution)
        {
            this.inputFileName = inputFileName;
            this.algorithmOutputSummary = algorithmOutputSummary;
            this.solutionOutputSummary = solutionOutputSummary;
            this.writableSolution = writableSolution;

            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");

            string runtimeLimit = "";
            runtimeLimit = algorithmOutputSummary[1].Remove(0, algorithmOutputSummary[1].IndexOf("-") + 1);

            string algorithmName = "";
            algorithmName = algorithmOutputSummary[0].Remove(0, algorithmOutputSummary[0].IndexOf(":") + 1);

            outputFileName = fileName+algorithmName+" Runtime Limit-"+runtimeLimit+".txt";
            //TODO Make sure everything is passed into this constructor and used appropriately
            //verify input
            //Verify();
            //process
            sw = new System.IO.StreamWriter(outputFileName);
        }

        void Verify()
        {
            throw new NotImplementedException();
        }
        public void Write()
        {
            WriteStatistics();
            WriteSolution();
            sw.Flush();
            sw.Close();
        }
        private void WriteStatistics()
        {
            sw.WriteLine("Instance Name:\t{0}", inputFileName);
            for (int i = 0; i < algorithmOutputSummary.Length; i++)
                sw.WriteLine(algorithmOutputSummary[i]);
            for (int i = 0; i < solutionOutputSummary.Length; i++)
                sw.WriteLine(solutionOutputSummary[i]);
            sw.WriteLine();
        }
        private void WriteSolution()
        {
            for (int i = 0; i < writableSolution.Length; i++)
                sw.WriteLine(writableSolution[i]);
            sw.WriteLine();
        }

    }
}
