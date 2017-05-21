﻿using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestRandom;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Interfaces
{
    public abstract class SolutionBase : ISolution
    {
        protected List<string> ids;
        public List<string> IDs { get { return ids; } }

        protected bool isComplete;
        public bool IsComplete { get { return isComplete; } }

        protected double objectiveFunctionValue;
        public double ObjectiveFunctionValue { get { return objectiveFunctionValue; } }

        protected double lowerBound;
        public double LowerBound { get { return lowerBound; } set { lowerBound = value; } }

        protected double upperBound;
        public double UpperBound { get { return upperBound; } set { upperBound = value; } }

        protected List<AssignedRoute> routes;
        public List<AssignedRoute> Routes { get { return routes; } }

        protected AlgorithmSolutionStatus status;
        public AlgorithmSolutionStatus Status { get { return status; } set { status = value; } }

        public SolutionBase()
        {
            ids = new List<string>();
        }

        public abstract string GetName();
        public abstract ISolution GenerateRandom();
        public abstract ComparisonResult CompareTwoSolutions(ISolution solution1, ISolution solution2);
        public abstract void View(IProblem problem);
        public abstract List<ISolution> GetAllChildren();
        public abstract void TriggerSpecification();
        public abstract string[] GetOutputSummary();
        public abstract string[] GetWritableSolution();
    }
}
