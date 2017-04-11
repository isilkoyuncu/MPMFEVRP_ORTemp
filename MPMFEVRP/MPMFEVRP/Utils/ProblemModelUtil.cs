using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.ProblemModels;

namespace MPMFEVRP.Utils
{
    public class ProblemModelUtil
    {
        public static List<String> GetCompatibleProblemModelNames(IProblem problem)
        {
            List<String> result = new List<string>();

            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ProblemModelBase).IsAssignableFrom(p))
                .Where(type => typeof(ProblemModelBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var problemModel in allProblemModels)
            {
                if (problemModel.GetMethod("GetNameOfProblemOfModel").Invoke(Activator.CreateInstance(problemModel), null).ToString() == problem.GetName())
                    result.Add(problemModel.GetMethod("GetName").Invoke(Activator.CreateInstance(problemModel), null).ToString());
            }

            return result;
        }

        public static ProblemModelBase CreateProblemModelByName(String problemModelName)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ProblemModelBase).IsAssignableFrom(p))
                .Where(type => typeof(ProblemModelBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            ProblemModelBase createdProblemModel = (ProblemModelBase)Activator.CreateInstance(typeof(EVvsGDV_MaxProfit_VRP_Model));

            foreach (var problemModel in allProblemModels)
            {
                createdProblemModel = (ProblemModelBase)Activator.CreateInstance(problemModel);
                if (createdProblemModel.GetName() == problemModelName)
                {
                    return createdProblemModel;
                }
            }

            return createdProblemModel;
        }
        public static ProblemModelBase CreateProblemModelByProblemName(String problemName)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ProblemModelBase).IsAssignableFrom(p))
                .Where(type => typeof(ProblemModelBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            ProblemModelBase createdProblemModel = (ProblemModelBase)Activator.CreateInstance(typeof(EVvsGDV_MaxProfit_VRP_Model));

            foreach (var problemModel in allProblemModels)
            {
                createdProblemModel = (ProblemModelBase)Activator.CreateInstance(problemModel);
                if (createdProblemModel.GetNameOfProblemOfModel() == problemName)
                {
                    return createdProblemModel;
                }
            }

            return createdProblemModel;
        }
    }
}
