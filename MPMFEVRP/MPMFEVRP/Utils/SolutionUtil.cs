using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions.Readers;

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
