﻿using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Interfaces
{
    public abstract class AlgorithmBase : IAlgorithm
    {
        protected ISolution bestSolutionFound;
        public ISolution Solution { get { return bestSolutionFound; } }

        protected ProblemModelBase model;

        protected AlgorithmParameters algorithmParameters;
        public AlgorithmParameters AlgorithmParameters { get { return algorithmParameters; } }
        protected AlgorithmSolutionStatus status;
        protected AlgorithmStatistics stats = new AlgorithmStatistics();
        public AlgorithmBase()
        {
            algorithmParameters = new AlgorithmParameters();

            algorithmParameters.AddParameter(
                new Parameter(
                    ParameterID.RUNTIME_SECONDS,
                    "Runtime Seconds",
                    new List<Object>() { 10.0, 30.0, 60.0, 120.0, 300.0, 600.0, 900.0, 1200.0, 1800.0, 3600.0, 36000.0 },
                    600.0,
                    ParameterType.ComboBox));

            algorithmParameters.AddParameter(
                new Parameter(
                    ParameterID.SOLUTION_TYPES,
                    "Solution Types",
                    SolutionUtil.GetAllSolutionNames().ToList<object>(),
                    SolutionUtil.GetAllSolutionNames()[0],
                    ParameterType.ComboBox));
        }

        public void Initialize(ProblemModelBase model)
        {
            // common initialize for all algorithms
            this.model = model;
            this.bestSolutionFound = SolutionUtil.CreateSolutionByName(algorithmParameters.GetParameter(ParameterID.SOLUTION_TYPES).GetStringValue(), model);
            SpecializedInitialize(model);
        }

        public abstract void SpecializedInitialize(ProblemModelBase model);

        public void Run()
        {
            // TODO common run for all algorithms
            SpecializedRun();
        }

        public abstract void SpecializedRun();

        public void Conclude()
        {
            // TODO common conclude for all algorithms
            SpecializedConclude();
        }

        public abstract void SpecializedConclude();

        public void Reset()
        {
            // TODO common reset for all algorithms
            SpecializedReset();
        }

        public abstract void SpecializedReset();

        public abstract string GetName();

        public override string ToString()
        {
            return GetName() + " (" + string.Join("; ",
                algorithmParameters.GetAllParameters()
                    .Select(p => p.Key.GetDescription() + ": " + p.Value.GetValue<object>().ToString()).ToList()) + ")";
        }
    }
}