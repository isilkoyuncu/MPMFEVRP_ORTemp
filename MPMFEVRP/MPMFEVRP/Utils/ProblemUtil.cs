using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Problems.Readers;

namespace MPMFEVRP.Utils
{
    public class ProblemUtil
    {
        public static List<String> GetAllRawDataResourceNames(string prefix)
        {
            List<string> result = new List<string>();

            ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            foreach (DictionaryEntry resource in resourceSet)
            {
                if (resource.Key.ToString().StartsWith(prefix))
                    result.Add(resource.Key.ToString());
            }

            return result;
        }

        public static List<String> GetAllProblemNames()
        {
            List<String> result = new List<string>();

            var allProblems = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IProblem).IsAssignableFrom(p))
                .Where(type => typeof(IProblem).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var problem in allProblems)
            {
                result.Add(problem.GetMethod("GetName").Invoke(Activator.CreateInstance(problem), null).ToString());
            }

            return result;
        }

        public static IProblem CreateProblemByResourceName(String resName)
        {
            String problemData = Properties.Resources.ResourceManager.GetString(resName);
            return CreateProblemByRawData(problemData);
        }

        public static IProblem CreateProblemByRawData(String rawData)
        {
            KoyuncuYavuzReader KYreader = new KoyuncuYavuzReader();
            KYreader.ProcessRawDataFromFile(rawData);

            ProblemDataPackage dataPackage = new ProblemDataPackage(KYreader);
            IProblem theProblem = new EVvsGDV_MaxProfit_VRP(dataPackage);

            return theProblem;
        }

        // TODO this random problem needs to be changed
        public static IProblem CreateRandomProblem(int numberOfJobs, int dueDateLowerLimit, int dueDateUpperLimit, int processingTimeLowerLimit, int processingTimeUpperLimit)
        {
            IProblem problem = new DefaultProblem();
            Random r = new Random();
            for (int i = 0; i < numberOfJobs; i++)
            {
                //problem.Jobs.Add(new Job(r.Next(processingTimeLowerLimit, processingTimeUpperLimit), r.Next(dueDateLowerLimit, dueDateUpperLimit), "Job " + i));
            }
            return problem;
        }

        public static IProblem CreateProblemByName(String problemName)
        {
            List<String> result = new List<string>();

            var allProblems = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IProblem).IsAssignableFrom(p))
                .Where(type => typeof(IProblem).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            IProblem createdProblem = (IProblem)Activator.CreateInstance(typeof(DefaultProblem));

            foreach (var problem in allProblems)
            {
                createdProblem = (IProblem)Activator.CreateInstance(problem);
                if (createdProblem.GetName() == problemName)
                {
                    return createdProblem;
                }
            }

            return createdProblem;
        }
    }
}
