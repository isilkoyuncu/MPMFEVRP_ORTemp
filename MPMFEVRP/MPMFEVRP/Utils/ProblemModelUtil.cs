using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using MPMFEVRP.Models.XCPlex;

namespace MPMFEVRP.Utils
{
    public class ProblemModelUtil
    {
        public static List<String> GetCompatibleProblemModelNames(IProblem problem)
        {
            List<String> result = new List<string>();

            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(p))
                .Where(type => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var problemModel in allProblemModels)
            {
                if (problemModel.GetMethod("GetNameOfProblemOfModel").Invoke(Activator.CreateInstance(problemModel), null).ToString() == problem.GetName())
                    result.Add(problemModel.GetMethod("GetName").Invoke(Activator.CreateInstance(problemModel), null).ToString());
            }

            return result;
        }

        public static EVvsGDV_ProblemModel CreateProblemModelByName(String problemModelName)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(p))
                .Where(type => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            EVvsGDV_ProblemModel createdProblemModel = (EVvsGDV_ProblemModel)Activator.CreateInstance(typeof(EVvsGDV_MaxProfit_VRP_Model));

            foreach (var problemModel in allProblemModels)
            {
                createdProblemModel = (EVvsGDV_ProblemModel)Activator.CreateInstance(problemModel);
                if (createdProblemModel.GetName() == problemModelName)
                {
                    return createdProblemModel;
                }
            }

            return createdProblemModel;
        }
        public static EVvsGDV_ProblemModel CreateProblemModelByProblemName(String problemName)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(p))
                .Where(type => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            EVvsGDV_ProblemModel createdProblemModel = (EVvsGDV_ProblemModel)Activator.CreateInstance(typeof(EVvsGDV_MaxProfit_VRP_Model));

            foreach (var problemModel in allProblemModels)
            {
                createdProblemModel = (EVvsGDV_ProblemModel)Activator.CreateInstance(problemModel);
                if (createdProblemModel.GetNameOfProblemOfModel() == problemName)
                {
                    return createdProblemModel;
                }
            }

            return createdProblemModel;
        }

        public static EVvsGDV_ProblemModel CreateProblemModelByProblem(Type theProblemModelType, IProblem problem)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(p))
                .Where(type => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            EVvsGDV_ProblemModel createdProblemModel;

            foreach (var problemModel in allProblemModels)
                if (problemModel == theProblemModelType)
                {
                    createdProblemModel = (EVvsGDV_ProblemModel)Activator.CreateInstance(problemModel, problem);
                    if (createdProblemModel.GetNameOfProblemOfModel() == problem.GetName())
                    {
                        return createdProblemModel;
                    }
                }

            return null;
        }

        public static EVvsGDV_ProblemModel CreateProblemModelByProblem(Type theProblemModelType, IProblem problem, Type TSPModelType)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(p))
                .Where(type => typeof(EVvsGDV_ProblemModel).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            EVvsGDV_ProblemModel createdProblemModel;

            foreach (var problemModel in allProblemModels)
                if (problemModel == theProblemModelType)
                {
                    createdProblemModel = (EVvsGDV_ProblemModel)Activator.CreateInstance(problemModel, problem, TSPModelType);
                    if (createdProblemModel.GetNameOfProblemOfModel() == problem.GetName())
                    {
                        return createdProblemModel;
                    }
                }

            return null;
        }

        public static void ArrangeNodesIntoLists(EVvsGDV_ProblemModel problemModel, 
            out int numCustomers, out int numES, 
            out List<int> customerSiteNodeIndices, out List<int> depotPlusCustomerSiteNodeIndices, out List<int> ESSiteNodeIndices)
        {
            numCustomers = problemModel.SRD.NumCustomers;
            numES = problemModel.SRD.NumES;

            customerSiteNodeIndices = new List<int>();
            depotPlusCustomerSiteNodeIndices = new List<int>();
            ESSiteNodeIndices = new List<int>();

            for (int orgSiteIndex = 0; orgSiteIndex < problemModel.SRD.NumNodes; orgSiteIndex++)
            {
                switch (problemModel.SRD.GetSiteByID(problemModel.SRD.GetSiteID(orgSiteIndex)).SiteType)
                {
                    case SiteTypes.Depot:
                        depotPlusCustomerSiteNodeIndices.Add(orgSiteIndex);
                        break;
                    case SiteTypes.Customer:
                        customerSiteNodeIndices.Add(orgSiteIndex);
                        depotPlusCustomerSiteNodeIndices.Add(orgSiteIndex);
                        break;
                    case SiteTypes.ExternalStation:
                        ESSiteNodeIndices.Add(orgSiteIndex);
                        break;
                    default:
                        throw new System.Exception("Site type incompatible!");
                }
            }
        }       
    }
}
