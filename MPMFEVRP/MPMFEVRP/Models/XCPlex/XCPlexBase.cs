using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Interfaces;

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
            XCPlexRelaxation relaxation;
            relaxation = xCplexParam.Relaxation;
            if ((xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
               //||(xCplexParam.Relaxation == XCPlexRelaxation.AssignmentProblem)
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
        public abstract SolutionBase GetCompleteSolution(Type SolutionType);//TODO Figure out how to make this work with a run-time-selected Solution type

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

        protected void initializeOutputVariables()
        {
            //Initializing outputs
            this.solutionStatus = XCPlexSolutionStatus.NotYetSolved;
            this.optimalCompleteSolutionObtained = false;
            this.variableValuesObtained = false;
            this.reducedCostsObtained = false;
            this.lowerBound = double.MinValue;
            this.upperBound = double.MaxValue;
        }
        protected void RefineDecisionVariables(PartialSolution PS)
        {
            double[] allLB = PS.GetAllDecisionVariableLowerBounds();
            double[] allUB = PS.GetAllDecisionVariableUpperBounds();
            for (int dvIndex = 0; dvIndex < allVariables_array.Length; dvIndex++)
            {
                allVariables_array[dvIndex].LB = allLB[dvIndex];
                allVariables_array[dvIndex].UB = allUB[dvIndex];
            }
        }
        protected void SetCplexParameters()
        {
            //SetOut(null);
            if (xCplexParam.LimitComputationTime)
                SetParam(Cplex.DoubleParam.TiLim, xCplexParam.RuntimeLimit_Seconds);
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.MIP_EMPHASIS))
                SetParam(Cplex.Param.Emphasis.MIP, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.MIP_EMPHASIS].Value.ToString().Substring(1)));
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.MIP_SEARCH))
                SetParam(Cplex.Param.MIP.Strategy.Search, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.MIP_SEARCH].Value.ToString().Substring(1)));
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.CUTS_FACTOR))
                SetParam(Cplex.DoubleParam.CutsFactor, double.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.CUTS_FACTOR].Value.ToString()));
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.THREADS))
                SetParam(Cplex.Param.Threads, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.THREADS].Value.ToString()));
        }
        public void Solve_and_PostProcess(PartialSolution specifiedSubproblemRoot = null)
        {
            initializeOutputVariables();
            if (specifiedSubproblemRoot != null)
            {
                RefineDecisionVariables(specifiedSubproblemRoot);
            }
            DateTime beginTime = new DateTime();
            DateTime endTime = new DateTime();
            beginTime = DateTime.Now;
            Solve();
            endTime = DateTime.Now;
            cpuTime = (endTime - beginTime).TotalSeconds;
            Status originalCplexStatus = GetStatus();
            string originalCplexStatus_string = originalCplexStatus.ToString();
            switch (originalCplexStatus_string)
            {
                case "Infeasible":
                    solutionStatus = XCPlexSolutionStatus.Infeasible;
                    return;
                //break;//unreachable because of the "return" in the previous line
                case "Unknown":
                    solutionStatus = XCPlexSolutionStatus.NoFeasibleSolutionFound;
                    break;
                case "Feasible":
                    solutionStatus = XCPlexSolutionStatus.Feasible;
                    break;
                case "Optimal":
                    solutionStatus = XCPlexSolutionStatus.Optimal;
                    break;
                default:
                    System.Windows.Forms.MessageBox.Show("Cplex didn't throw an exception, but got an unrecognized status!");
                    break;
            }

            if (xCplexParam.Relaxation == XCPlexRelaxation.None)
            {
                //Not relaxed, outcome can be Unknown, Feasible or Optimal (Can't be infeasible, if it was, it we wouldn't come this far)
                //Obtain a lower bound
                lowerBound = GetBestObjValue();
                //if at least feasible, obtain an upper bound and complete solution giving the upper bound
                if (solutionStatus > 0)
                {
                    //Obtain upper bound value
                    upperBound = GetObjValue();
                    //Obtain X and maybe Y values so a complete solution can be constructed from them 
                    optimalCompleteSolutionObtained = (solutionStatus == XCPlexSolutionStatus.Optimal);
                }
            }//if not relaxed
            else//relaxed
            {
                //If we're here we know we have an optimal solution
                //Obtain a lower bound
                upperBound = GetObjValue();
                lowerBound = upperBound;//Because the solution is optimal
                                        //Obtain X and maybe Y values so a complete solution can be constructed from them 
                if (ValidateCompletenessOfRelaxedSolution())
                    optimalCompleteSolutionObtained = true;
            }//if relaxed
        }
        bool ValidateCompletenessOfRelaxedSolution()
        {
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                return ValidateIntegralityOfLPRelaxation();
            //We should never get here
            return false;
        }
        bool ValidateIntegralityOfLPRelaxation()
        {
            double[] allValues = GetValues(allVariables_array);
            for (int dvInd = 0; dvInd < allValues.Length; dvInd++)
                if (Math.Min(Math.Abs(allVariables_array[dvInd].UB - allValues[dvInd]), Math.Abs(allValues[dvInd] - allVariables_array[dvInd].LB)) > xCplexParam.ErrorTolerance)
                    return false;
            return true;
        }
    }
}
