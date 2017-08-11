﻿using System;
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
    public class EVvsGDV_MinCost_VRP_Model : EVvsGDV_ProblemModel
    {
        string problemName;

        public EVvsGDV_MinCost_VRP_Model()
        {
            EVvsGDV_MinCost_VRP problem = new EVvsGDV_MinCost_VRP();
            problemName = problem.GetName();
            //objectiveFunctionType = problem.ObjectiveFunctionType;
            //coverConstraintType = problem.CoverConstraintType;
            //rechargingDuration_status = RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full; //TODO delete these because these are unnecessary. Without data, this problem model is useless and we have this empty constructor just to show model on the form

            PopulateCompatibleSolutionTypes();
            CreateCustomerSetArchive();
        }//empty constructor
        public EVvsGDV_MinCost_VRP_Model(EVvsGDV_MinCost_VRP problem)
        {
            pdp = new ProblemDataPackage(problem.PDP);
            inputFileName = problem.PDP.InputFileName;
            problemName = problem.GetName();
            objectiveFunctionType = problem.ObjectiveFunctionType;
            coverConstraintType = problem.CoverConstraintType;
            SetNumVehicles();
            rechargingDuration_status = (RechargingDurationAndAllowableDepartureStatusFromES)problemCharacteristics.GetParameter(ParameterID.PRB_RECHARGING_ASSUMPTION).Value;

            EV_TSPSolver = new XCPlex_NodeDuplicatingFormulation(this, new XCPlexParameters(vehCategory: VehicleCategories.EV, tSP: true));
            //EV_TSPSolver.ExportModel("EV_tsp_minCostModel.lp");
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
            return "EV vs GDV Cost Minimization Problem's Model";
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
                System.Windows.Forms.MessageBox.Show("solution plugged into EVvsGDV_MinCost_VRP_Model.CheckFeasibilityOfSolution is incompatible!");
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


    }
}
