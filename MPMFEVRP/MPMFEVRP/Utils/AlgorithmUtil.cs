using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Utils
{
    public class AlgorithmUtil
    {
        public static IAlgorithm CreateAlgorithmByName(String algorithmName)
        {
            var allAlgorithms = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IAlgorithm).IsAssignableFrom(p))
                .Where(type => typeof(IAlgorithm).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();


            IAlgorithm createdAlgorithm = (IAlgorithm)Activator.CreateInstance(typeof(BestOfRandom));

            foreach (var algorithm in allAlgorithms)
            {
                createdAlgorithm = (IAlgorithm)Activator.CreateInstance(algorithm);
                if (createdAlgorithm.GetName() == algorithmName)
                {
                    return createdAlgorithm;
                }
            }

            return createdAlgorithm;
        }

        public static List<String> GetAllAlgorithmNames()
        {
            List<String> result = new List<string>();

            var allAlgorithms = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IAlgorithm).IsAssignableFrom(p))
                .Where(type => typeof(IAlgorithm).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var algorithm in allAlgorithms)
            {
                result.Add(algorithm.GetMethod("GetName").Invoke(Activator.CreateInstance(algorithm), null).ToString());
            }

            return result;
        }
    }
}
