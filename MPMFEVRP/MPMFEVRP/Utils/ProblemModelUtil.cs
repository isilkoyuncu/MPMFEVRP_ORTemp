using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Utils
{
    public class ProblemModelUtil
    {
        public static List<String> GetCompatibleProblemModelNames(IProblem problem)
        {
            List<String> result = new List<string>();

            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IProblemModel).IsAssignableFrom(p))
                .Where(type => typeof(IProblemModel).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var problemModel in allProblemModels)
            {
                if(problemModel.NameOfProblemOfModel == problem.GetName())
                    result.Add(problemModel.GetMethod("GetName").Invoke(Activator.CreateInstance(problemModel), null).ToString());
            }

            return result;
        }

    }
}
