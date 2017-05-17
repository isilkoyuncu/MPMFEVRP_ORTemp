using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return result;
        }

        public static ISolution CreateSolutionByName(String solutionName, ProblemModelBase problemData)
        {
            var allSolutions = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(SolutionBase).IsAssignableFrom(p))
                .Where(type => typeof(SolutionBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            ISolution createdSolution = (ISolution)Activator.CreateInstance(typeof(NEW_RouteBasedSolution));

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
