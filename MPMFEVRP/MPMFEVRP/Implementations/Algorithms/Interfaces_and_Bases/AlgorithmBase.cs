using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases
{
    public abstract class AlgorithmBase : IAlgorithm
    {
        protected ISolution bestSolutionFound;
        public ISolution Solution { get { return bestSolutionFound; } }

        protected EVvsGDV_ProblemModel theProblemModel;

        protected InputOrOutputParameterSet algorithmParameters;
        public InputOrOutputParameterSet AlgorithmParameters { get { return algorithmParameters; } }

        protected AlgorithmSolutionStatus status;
        public AlgorithmSolutionStatus Status { get { return status; } }

        protected AlgorithmStatistics stats = new AlgorithmStatistics();
        public AlgorithmStatistics Stats { get { return stats; } }

        protected BackgroundWorker backgroundWorker;

        public AlgorithmBase()
        {
            algorithmParameters = new InputOrOutputParameterSet();
            AddParameters();
        }
        
        public void AddParameters()
        {
            algorithmParameters.AddParameter(
                new InputOrOutputParameter(
                    ParameterID.ALG_RUNTIME_SECONDS,
                    "Runtime Seconds",
                    new List<Object>() { 10.0, 30.0, 60.0, 120.0, 300.0, 600.0, 900.0, 1200.0, 1800.0, 3600.0, 36000.0 },
                    600.0,
                    UserInputObjectType.ComboBox));

            algorithmParameters.AddParameter(
                new InputOrOutputParameter(
                    ParameterID.ALG_SOLUTION_TYPES,
                    "Solution Types",
                    SolutionUtil.GetAllSolutionNames().ToList<object>(),
                    SolutionUtil.GetAllSolutionNames()[0],
                    UserInputObjectType.ComboBox));
        }
        public abstract void AddSpecializedParameters();

        public void Initialize(EVvsGDV_ProblemModel theProblemModel)
        {
            // common initialize for all algorithms
            this.theProblemModel = theProblemModel;
            //TODO is this necessary?
            //this.bestSolutionFound = SolutionUtil.CreateSolutionByName(algorithmParameters.GetParameter(ParameterID.ALG_SOLUTION_TYPES).GetStringValue(), model);
            SpecializedInitialize(theProblemModel);
        }

        public abstract void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel);

        public void setBackgroundWorker(BackgroundWorker bg)
        {
            backgroundWorker = bg;
        }

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

        public abstract string[] GetOutputSummary();
    }
}
