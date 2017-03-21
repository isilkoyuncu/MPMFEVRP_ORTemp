using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations;

namespace MPMFEVRP.Models.XCPlex
{
    public abstract class XCPlexBase : Cplex
    {
        protected ProblemModelBase problemModel;
        protected XCPlexParameters xCplexParam;
        protected NumVarType variable_type = NumVarType.Int;
        protected List<INumVar> allVariables_list;
        protected INumVar[] allVariables_array;
        protected List<IRange> allConstraints_list;
        protected IRange[] allConstraints_array;
        protected XCPlexSolutionStatus solutionStatus;
        protected bool optimalCompleteSolutionObtained;
        protected bool variableValuesObtained;
        protected bool reducedCostsObtained;
        protected double lowerBound;
        protected double upperBound;
        protected double cpuTime;
        public double[] AllValues { get { return GetValues(allVariables_array); } }
        public double[] AllReducedCosts { get { return GetReducedCosts(allVariables_array); } }
        public Cplex.BasisStatus[] AllVariableBasisStatuses { get { return GetBasisStatuses(allVariables_array); } }
        public double[] AllSlacks { get { return GetSlacks(allConstraints_array); } }
        public double[] AllShadowPrices { get { return GetDuals(allConstraints_array); } }
        public Cplex.BasisStatus[] AllConstraintBasisStatuses { get { return GetBasisStatuses(allConstraints_array); } }
        public XCPlexSolutionStatus SolutionStatus { get { return solutionStatus; } }
        public bool OptimalCompleteSolutionObtained { get { return optimalCompleteSolutionObtained; } }
        public bool VariableValuesObtained { get { return variableValuesObtained; } }
        public bool ReducedCostsObtained { get { return reducedCostsObtained; } }
        public double LowerBound_XCPlex { get { return lowerBound; } }
        public double UpperBound_XCPlex { get { return upperBound; } }
        public double CPUtime { get { return cpuTime; } }

        Dictionary<String, Object> DecisionVariables = new Dictionary<string, object>();

        public XCPlexBase() { }
        public XCPlexBase(ProblemModelBase problemModel, XCPlexParameters xCplexParam)
        {
            this.problemModel = problemModel;
            this.xCplexParam = xCplexParam;
            if ((xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming) //TODO after creating alg domain and enums this error will be resolved.
                //||(fromAlgorithm.Parameters.Relaxation == XCPlexRelaxation.AssignmentProblem)
                )
                variable_type = NumVarType.Float;

            //now we are ready to put the model together and then solve it
            //Define the variables
            DefineDecisionVariables();
            //Objective function
            AddTheObjectiveFunction();
            //Constraints
            AddAllConstraints();
            //Cplex parameters
            SetCplexParameters();
            //output variables
            initializeOutputVariables();
        }

        protected void Initialize()
        {
            DefineDecisionVariables();
            AddTheObjectiveFunction();
            AddAllConstraints();
        }

        protected abstract void DefineDecisionVariables();
        protected abstract void AddTheObjectiveFunction();
        protected abstract void AddAllConstraints();
        public abstract string GetDescription_AllVariables_Array();

        protected void AddOneDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1)
        {
            var dv = NumVarArray(length1, lowerBound, upperBound, type);
            DecisionVariables.Add(name, dv);
        }

        protected void AddTwoDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1, int length2)
        {
            var dv = new INumVar[length1][];
            for (int i = 0; i < length1; i++)
            {
                dv[i] = NumVarArray(length2, lowerBound, upperBound, type);
            }
            DecisionVariables.Add(name, dv);
        }

        protected void AddTwoDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1, int[] length2)
        {
            var dv = new INumVar[length1][];
            for (int i = 0; i < length1; i++)
            {
                dv[i] = NumVarArray(length2[i], lowerBound, upperBound, type);
            }
            DecisionVariables.Add(name, dv);
        }

        public T[] GetOneDimensionalDecisionVariableResult<T>(String name)
        {
            return GetValues(((INumVar[])DecisionVariables[name]))      // get values of the decision variable
                .Select(x => (T)Convert.ChangeType(x, typeof(T)))       // convert it to generic T
                .ToArray();
        }

        public T[][] GetTwoDimensionalDecisionVariableResult<T>(String name)
        {
            return ((INumVar[][])DecisionVariables[name])
                .Select(x => GetValues(x)                               // for each row, get values
                    .Select(y => (T)Convert.ChangeType(y, typeof(T)))   // convert each to generic T
                    .ToArray())
                .ToArray();
        }

        protected INumVar[] GetOneDimensionalDecisionVariableValue(String name)
        {
            return ((INumVar[])DecisionVariables[name]);
        }

        protected INumVar[][] GetTwoDimensionalDecisionVariableValue(String name)
        {
            return ((INumVar[][])DecisionVariables[name]);
        }

        // Shortcuts
        // DV1D : Decision Variable 1-Dimensional etc.
        protected void AddDV1D(String name, double lowerBound, double upperBound, NumVarType type, int length1)
        {
            AddOneDimensionalDecisionVariable(name, lowerBound, upperBound, type, length1);
        }

        protected void AddDV2D(String name, double lowerBound, double upperBound, NumVarType type, int length1, int length2)
        {
            AddTwoDimensionalDecisionVariable(name, lowerBound, upperBound, type, length1, length2);
        }

        public T[] DV1DResult<T>(String name)
        {
            return GetOneDimensionalDecisionVariableResult<T>(name);
        }

        public T[][] DV2DResult<T>(String name)
        {
            return GetTwoDimensionalDecisionVariableResult<T>(name);
        }

        protected INumVar[] DV1D(String name)
        {
            return GetOneDimensionalDecisionVariableValue(name);
        }

        protected INumVar[][] DV2D(String name)
        {
            return GetTwoDimensionalDecisionVariableValue(name);
        }
    }
}
