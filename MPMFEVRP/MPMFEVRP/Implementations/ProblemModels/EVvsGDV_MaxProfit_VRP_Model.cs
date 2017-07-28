using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Implementations.ProblemModels
{
    public class EVvsGDV_MaxProfit_VRP_Model: EVvsGDV_ProblemModel
    {
        string problemName;
        
        public EVvsGDV_MaxProfit_VRP_Model()
        {
            EVvsGDV_MaxProfit_VRP problem = new EVvsGDV_MaxProfit_VRP();
            problemName = problem.GetName();
            objectiveFunctionType = problem.ObjectiveFunctionType;
            coverConstraintType = problem.CoverConstraintType;

            PopulateCompatibleSolutionTypes();
            CreateCustomerSetArchive();
        }//empty constructor
        public EVvsGDV_MaxProfit_VRP_Model(EVvsGDV_MaxProfit_VRP problem)
        {
            pdp = new ProblemDataPackage(problem.PDP);
            inputFileName = problem.PDP.InputFileName;
            problemName = problem.GetName();
            objectiveFunctionType = problem.ObjectiveFunctionType;
            coverConstraintType = problem.CoverConstraintType;

            EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
            GDV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.GDV, tSP: true));

            PopulateCompatibleSolutionTypes();
            CreateCustomerSetArchive();
        }
        

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }
        public override string GetName()
        {
            return "EV vs GDV Profit Maximization Problem's Model";
        }
        public override string GetNameOfProblemOfModel()
        {
            return problemName;
        }

        public override ISolution GetRandomSolution(int seed, Type solutionType)
        {
            throw new NotImplementedException();
        }

        public override bool CheckFeasibilityOfSolution(ISolution solution)
        {
            throw new NotImplementedException();

            Type solutionType = solution.GetType();

            if (solutionType == typeof(CustomerSetBasedSolution))
            {
                CustomerSetBasedSolution csbs = (CustomerSetBasedSolution)solution;
                return CheckFeasibilityOfSolution(csbs);
            }
            else if (solutionType == typeof(RouteBasedSolution))
            {
                RouteBasedSolution rbs = (RouteBasedSolution)solution;
                return CheckFeasibilityOfSolution(rbs);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("solution plugged into EVvsGDV_MaxProfit_VRP_Model.CheckFeasibilityOfSolution is incompatible!");
                return false;
            }
        }
        bool CheckFeasibilityOfSolution(CustomerSetBasedSolution solution)
        {
            throw new NotImplementedException();

            bool outcome = true;
            //TODO check for any infeasibility and return false as soon as one is found!
            return outcome;
        }
        public override double CalculateObjectiveFunctionValue(ISolution solution)
        {
            throw new NotImplementedException();

        }

        public override bool CompareTwoSolutions(ISolution solution1, ISolution solution2)
        {
            throw new NotImplementedException();
        }

        void PopulateCompatibleSolutionTypes()
        {
            compatibleSolutions = new List<Type>()
            {
                typeof(CustomerSetBasedSolution),
                typeof(RouteBasedSolution)
            };
        }
        void CreateCustomerSetArchive()
        {
            archiveAllCustomerSets = true;
            customerSetArchive = new CustomerSetList();
        }

        // These following 3 methods exports calculated information as a text file
        void ExportDistancesAsTxt()
        {
            System.IO.StreamWriter sw;
            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_distances.txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("Instance Name", inputFileName);
            for (int i = 0; i < pdp.SRD.NumNodes; i++)
            {
                sw.WriteLine();
                for (int j = 0; j < pdp.SRD.NumNodes; j++)
                {
                    sw.Write(pdp.SRD.Distance[i, j] + " ");
                }
            }
            sw.Close();
        }
        void ExportTravelDurationAsTxt()
        {
            System.IO.StreamWriter sw;
            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_travelDurations.txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("Instance Name", inputFileName);
            for (int i = 0; i < pdp.SRD.NumNodes; i++)
            {
                sw.WriteLine();
                for (int j = 0; j < pdp.SRD.NumNodes; j++)
                {
                    sw.Write(pdp.SRD.TimeConsumption[i, j] + " ");
                }
            }
            sw.Close();
        }
        void ExportEnergyConsumpionAsTxt()
        {
            System.IO.StreamWriter sw;
            String fileName = inputFileName;
            fileName = fileName.Replace(".txt", "");
            string outputFileName = fileName + "_energies.txt";
            sw = new System.IO.StreamWriter(outputFileName);
            sw.WriteLine("Instance Name", inputFileName);
            for (int i = 0; i < pdp.SRD.NumNodes; i++)
            {
                sw.WriteLine();
                for (int j = 0; j < pdp.SRD.NumNodes; j++)
                {
                    sw.Write(pdp.SRD.EnergyConsumption[i, j, 0] + " ");
                }
            }
            sw.Close();
        }
    }
}
