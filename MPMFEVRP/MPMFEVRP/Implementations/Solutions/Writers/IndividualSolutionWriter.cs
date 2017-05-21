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

        private ISolution theSolution;
        private string inputFileName;
        private string[] algorithmOutputSummary;
        private string[] solutionOutputSummary;
        private string[] writableSolution;

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

            //TODO Make sure everything is passed into this constructor and used appropriately
            //verify input
            //Verify();
            //process
            sw = new System.IO.StreamWriter(inputFileName);
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
            sw.WriteLine("Instance Name\t{0}", inputFileName);
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
