using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Readers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Utils
{
    public class SolutionUtil
    {
        public static List<String> GetAllSolutionNames()
        {
            List<String> result = new List<string>();

            var allSolutions = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(SolutionBase).IsAssignableFrom(p))
                .Where(type => typeof(SolutionBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var solution in allSolutions)
            {
                result.Add(solution.GetMethod("GetName").Invoke(Activator.CreateInstance(solution), null).ToString());
            }
            result.Sort();
            return result;
        }
        public static string[] ReadSolutionByFileName(String fullFileName)
        {
            string[] solutionSummary;
            KoyuncuYavuzSolutionReader KYSolnReader = new KoyuncuYavuzSolutionReader(fullFileName);
            KYSolnReader.Read();
            solutionSummary = KYSolnReader.InstanceSolutionSummary;
            return solutionSummary;
        }
        public static string[] ReadCplexLogByFileName(String fullFileName)
        {
            string[] cplexLog;
            CplexLogReader cplexLogFileReader = new CplexLogReader(fullFileName);
            cplexLogFileReader.Read();
            cplexLog = cplexLogFileReader.CplexLogSummary;
            return cplexLog;
        }

        public static string[] ReadExploitingGDVvsPlainOutputByFileName(String fullFileName)
        {
            string[] output = new string[3];
            //CplexLogReader cplexLogFileReader = new CplexLogReader(fullFileName);
            //cplexLogFileReader.Read();
            //output = cplexLogFileReader.CplexLogSummary;
            return output;
        }
        public static ISolution CreateSolutionByName(String solutionName, EVvsGDV_ProblemModel problemData)
        {
            var allSolutions = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(SolutionBase).IsAssignableFrom(p))
                .Where(type => typeof(SolutionBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            ISolution createdSolution = (ISolution)Activator.CreateInstance(typeof(RouteBasedSolution));

            foreach (var solution in allSolutions)
            {
                createdSolution = (ISolution)Activator.CreateInstance(solution, problemData);

                if (createdSolution.GetName() == solutionName)
                {
                    return createdSolution;
                }
            }
            return createdSolution;
        }
    }
}
