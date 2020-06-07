using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions.Writers;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.ProblemModels;

namespace RunFromConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string TSPModelName = "AFV Optimize Single Customer Set";
            string problemName = "EV vs GDV Maximum Profit VRP";
            string workingFolder = @"G:\My Drive\0-Research\5-2-profit max heterogenous gvrp\EMH - profits\testInput-lessadoption\";
            int minNumberOfEVs = 1;
            int maxNumberOfEVs = 4;
            double timeLimit = 60.0;


            IAlgorithm theAlgorithm = new CGA_ExploitingGDVs_ProfitMax(timeLimit);
            string[] fileNames = Directory.GetFiles(workingFolder).OrderBy(x => x).ToArray();
            for (int i = 0; i < fileNames.Length; i++)
            {
                for(int j = minNumberOfEVs; j <= maxNumberOfEVs; j++)
                {
                    IProblem theProblem = ProblemUtil.CreateProblemByFileName(problemName, Path.Combine(workingFolder, fileNames[i]), j);
                    Console.WriteLine("Problem loaded from file " + theProblem.PDP.InputFileName);
                    Type TSPModelType = XCPlexUtil.GetXCPlexModelTypeByName(TSPModelName);
                    EVvsGDV_ProblemModel theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(typeof(EVvsGDV_MaxProfit_VRP_Model), theProblem, TSPModelType);
                    Console.WriteLine("Problem model is created from problem " + theProblem.PDP.InputFileName);
                    theAlgorithm.Initialize(theProblemModel);
                    Console.WriteLine("Algorithm " + theAlgorithm.ToString() + " is initialized.");
                    Console.WriteLine("Algorithm " + theAlgorithm.ToString() + " started running.");
                    theAlgorithm.Run();
                    Console.WriteLine("Algorithm " + theAlgorithm.ToString() + " finished.");
                    Console.WriteLine("================");
                    theAlgorithm.Conclude();
                    ISolution theSolution = theAlgorithm.Solution;
                    if (theSolution == null)
                    {
                        IWriter writer = new IndividualSolutionWriter(theProblemModel.InputFileName, theAlgorithm.GetOutputSummary(), null, null);
                        writer.Write();
                    }
                    else
                    {
                        Console.WriteLine("Solution " + theSolution.ToString() + " started writing.");
                        string algParam = "";
                        if (theAlgorithm.GetName() == "CGA with Exploiting GDVs")
                        {
                            Exploiting_GDVs_Flowchart specialAlg = (Exploiting_GDVs_Flowchart)theAlgorithm.AlgorithmParameters.GetParameter(MPMFEVRP.Models.ParameterID.ALG_FLOWCHART).Value;
                            algParam = specialAlg.ToString();
                        }
                        IWriter writer = new IndividualSolutionWriter(theProblemModel.InputFileName, theAlgorithm.GetOutputSummary(), theSolution.GetOutputSummary(), theSolution.GetWritableSolution(), algParam, j.ToString());
                        writer.Write();
                    }
                    Console.WriteLine("**************RESET************");
                    theAlgorithm.Reset();
                }   
            }
            Console.Read();
        }
    }
}
