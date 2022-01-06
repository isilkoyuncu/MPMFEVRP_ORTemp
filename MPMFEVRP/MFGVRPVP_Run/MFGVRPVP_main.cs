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
    public class MFGVRPVP_Run
    {
        public static void Main(string[] args)
        {
            string TSPModelName = "AFV Optimize Single Customer Set";
            string maxProfitProblemName = "EV vs GDV Maximum Profit VRP";
            string minVMTProblemName = "Erdogan & Miller-Hooks Problem";
            string problemName = maxProfitProblemName;
            int minNumberOfEVs = 7;
            int maxNumberOfEVs = 7;
            IAlgorithm theAlgorithm;
            string algorithmName = "cga";
            string algorithmParam = "GE2";
            EVvsGDV_ProblemModel theProblemModel;
            int randomSeed = 50;

            Console.WriteLine("Please enter the input file folder:");
            string folderName = Console.ReadLine();
            Console.WriteLine("Please enter the time limit (seconds) per instance");
            double timeLimit = Convert.ToDouble(Console.ReadLine());
            Console.WriteLine("Min VMT problem? (y/n):");
            string isMinimization = Console.ReadLine();
            Console.WriteLine("Random Seed:");
            randomSeed = Convert.ToInt32(Console.ReadLine());
            if (isMinimization == "Y" || isMinimization == "y")
            {
                problemName = minVMTProblemName;
                Console.WriteLine("Number of EVs available?");
                minNumberOfEVs = Convert.ToInt32(Console.ReadLine());
                maxNumberOfEVs = minNumberOfEVs;
                Console.WriteLine("Algorithm: (cplex/cga)");
                algorithmName = Console.ReadLine();
                if (algorithmName == "cplex" || algorithmName == "CPLEX")
                {
                    Console.WriteLine("Algoritgm parameters: (adf/ndf)");
                    algorithmParam = Console.ReadLine();
                    if (algorithmParam == "adf" || algorithmParam == "ndf")
                        theAlgorithm = new Outsource2Cplex(timeLimit, algorithmParam, folderName);
                    else
                        throw new Exception("An unknown algorithm parameter cannot be used...");
                }
                else if (algorithmName == "cga" || algorithmName == "CGA")
                {
                    Console.WriteLine("Algoritgm parameters: (ge0-ge3)");
                    algorithmParam = Console.ReadLine();
                    if (algorithmParam == "ge0" || algorithmParam == "ge1" || algorithmParam == "ge2" || algorithmParam == "ge3")
                        theAlgorithm = new CGA_ExploitingGDVs(timeLimit, algorithmParam, randomSeed, folderName);
                    else
                        throw new Exception("An unknown algorithm parameter cannot be used...");
                }
                else
                    throw new Exception("An unknown algorithm type cannot be revoked...");
            }
            else if (isMinimization == "N" || isMinimization == "n")
            {
                problemName = maxProfitProblemName;
                Console.WriteLine("Please enter the min EVs desired:");
                minNumberOfEVs = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Please enter the max EVs desired:");
                maxNumberOfEVs = Convert.ToInt32(Console.ReadLine());
                theAlgorithm = new CGA_ExploitingGDVs_ProfitMax(timeLimit, randomSeed, folderName);
            }
            else
            {
                throw new Exception("An unknown problem type cannot be solved...");
            }

            string workingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), folderName, @"Input\");
            string[] fileNames = Directory.GetFiles(workingFolder).OrderBy(x => x).ToArray();
            Dictionary<int, string> fileDict = new Dictionary<int, string>();
            foreach (string s in fileNames)
            {
                int index = s.Replace(workingFolder, "").IndexOf('_');
                string temp = s.Replace(workingFolder, "").Substring(0, index);
                fileDict.Add(Convert.ToInt32(temp), s);
            }
            fileDict = fileDict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<int, string> kvp in fileDict)
            {
                for (int j = minNumberOfEVs; j <= maxNumberOfEVs; j++)
                {
                    IProblem theProblem = ProblemUtil.CreateProblemByFileName(problemName, Path.Combine(workingFolder, kvp.Value), j);
                    Console.WriteLine("Problem loaded from file " + theProblem.PDP.InputFileName);
                    Type TSPModelType = XCPlexUtil.GetXCPlexModelTypeByName(TSPModelName);
                    if (isMinimization == "y" || isMinimization == "Y")
                        theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(typeof(EMH_ProblemModel), theProblem, TSPModelType);
                    else
                        theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(typeof(EVvsGDV_MaxProfit_VRP_Model), theProblem, TSPModelType);
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
