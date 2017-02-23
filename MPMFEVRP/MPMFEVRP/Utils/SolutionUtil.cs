﻿using MPMFEVRP.Implementations.Solutions;
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
                .Where(p => typeof(ISolution).IsAssignableFrom(p))
                .Where(type => typeof(ISolution).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var solution in allSolutions)
            {
                result.Add(solution.GetMethod("GetName").Invoke(Activator.CreateInstance(solution), null).ToString());
            }

            return result;
        }

        public static ISolution CreateSolutionByName(String solutionName, IProblemModel problemData)
        {
            var allSolutions = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ISolution).IsAssignableFrom(p))
                .Where(type => typeof(ISolution).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            throw new NotImplementedException();

            //TODO uncomment these after writing default solution

            //ISolution createdSolution = (ISolution)Activator.CreateInstance(typeof(DefaultSolution));

            //foreach (var solution in allSolutions)
            //{
            //    createdSolution = (ISolution)Activator.CreateInstance(solution, problemData);
            //    if (createdSolution.GetName() == solutionName)
            //    {
            //        return createdSolution;
            //    }
            //}

            //return createdSolution;
        }
    }
}
