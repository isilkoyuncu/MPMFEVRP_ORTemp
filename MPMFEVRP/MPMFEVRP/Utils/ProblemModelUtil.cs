using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Domains.ProblemDomain;

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

        public static ProblemModelBase CreateProblemModelByProblem(IProblem problem)
        {
            var allProblemModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ProblemModelBase).IsAssignableFrom(p))
                .Where(type => typeof(ProblemModelBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            ProblemModelBase createdProblemModel = (ProblemModelBase)Activator.CreateInstance(typeof(EVvsGDV_MaxProfit_VRP_Model),problem);

            foreach (var problemModel in allProblemModels)
            {
                createdProblemModel = (ProblemModelBase)Activator.CreateInstance(problemModel,problem);
                if (createdProblemModel.GetNameOfProblemOfModel() == problem.GetName())
                {
                    return createdProblemModel;
                }
            }

            return createdProblemModel;
        }

        public static void ArrangeNodesIntoLists(ProblemModelBase problemModel, 
            out int numCustomers, out int numES, 
            out List<int> customerSiteNodeIndices, out List<int> depotPlusCustomerSiteNodeIndices, out List<int> ESSiteNodeIndices)
        {
            numCustomers = problemModel.SRD.NumCustomers;
            numES = problemModel.SRD.NumES;

            customerSiteNodeIndices = new List<int>();
            depotPlusCustomerSiteNodeIndices = new List<int>();
            ESSiteNodeIndices = new List<int>();

            for (int orgSiteIndex = 0; orgSiteIndex < problemModel.SRD.SiteArray.Length; orgSiteIndex++)
            {
                switch (problemModel.SRD.SiteArray[orgSiteIndex].SiteType)
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
