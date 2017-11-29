using ILOG.Concert;
using ILOG.CPLEX;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPMFEVRP.Models.XCPlex
{
    public abstract class XCPlexBase : Cplex
    {
        protected EVvsGDV_ProblemModel theProblemModel;
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
        protected int numberOfTimesSolveMethodCalled;
        protected double totalTimeInSolve;
        protected Dictionary<string, int> numberOfTimesSolveFoundStatus;
        protected Dictionary<string, double> totalTimeInSolveOnStatus;
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
        public double CPUtime { get { return cpuTime; } }//seconds
        public int NumberOfTimesSolveMethodCalled { get { return numberOfTimesSolveMethodCalled; } }
        public double TotalTimeInSolve { get { return totalTimeInSolve; } }
        public Dictionary<string, int> NumberOfTimesSolveFoundStatus { get { return numberOfTimesSolveFoundStatus; } }
        public Dictionary<string, double> TotalTimeInSolveOnStatus { get { return totalTimeInSolveOnStatus; } }

        Dictionary<String, Object> DecisionVariables = new Dictionary<string, object>();

        protected VehicleCategories[] vehicleCategories = new VehicleCategories[] { VehicleCategories.EV, VehicleCategories.GDV };
        protected int numVehCategories;
        public System.IO.TextWriter TWoutput;

        public XCPlexBase()
        {
            numberOfTimesSolveFoundStatus = new Dictionary<string, int>();
            totalTimeInSolveOnStatus = new Dictionary<string, double>();
        }
        public XCPlexBase(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters xCplexParam)
        {
            numberOfTimesSolveFoundStatus = new Dictionary<string, int>();
            totalTimeInSolveOnStatus = new Dictionary<string, double>();

            this.theProblemModel = theProblemModel;

            numVehCategories = theProblemModel.VRD.NumVehicleCategories;
            if (numVehCategories < vehicleCategories.Length) { throw new System.Exception("XCPlexBase number of VehicleCategories are different than problemModel.VRD.NumVehicleCategories"); }

            this.xCplexParam = xCplexParam;
            XCPlexRelaxation relaxation;
            relaxation = xCplexParam.Relaxation;
            if ((xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                //||(xCplexParam.Relaxation == XCPlexRelaxation.AssignmentProblem)
                )
                variable_type = NumVarType.Float;

            //Model Specific Initialization
            Initialize();

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
            InitializeOutputVariables();
        }

        protected abstract void Initialize();

        protected abstract void DefineDecisionVariables();
        protected abstract void AddTheObjectiveFunction();
        protected abstract void AddAllConstraints();
        public abstract string GetDescription_AllVariables_Array();
        public abstract SolutionBase GetCompleteSolution(Type SolutionType);//ISSUE (#5) Figure out how to make this work with a run-time-selected Solution type

        protected void AddOneDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1, out INumVar[] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1];
            NumVarType newType = type;
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                newType = NumVarType.Float;
            for (int i = 0; i < length1; i++)
            {
                dv_name = name + "_(" + i.ToString() + ")";
                dv[i] = NumVar(lowerBound, upperBound, newType, dv_name);
                allVariables_list.Add(dv[i]);
            }
            DecisionVariables.Add(dv_name, dv);
        }
        protected void AddOneDimensionalDecisionVariable(String name, double[] lowerBound, double[] upperBound, NumVarType type, int length1, out INumVar[] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1];
            NumVarType newType = type;
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                newType = NumVarType.Float;
            for (int i = 0; i < length1; i++)
            {
                dv_name = name + "_(" + i.ToString() + ")";
                dv[i] = NumVar(lowerBound[i], upperBound[i], newType, dv_name);
                allVariables_list.Add(dv[i]);
            }
            DecisionVariables.Add(dv_name, dv);
        }

        protected void AddTwoDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1, int[] length2, out INumVar[][] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1][];
            NumVarType newType = type;
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                newType = NumVarType.Float;
            for (int i = 0; i < length1; i++)
            {
                dv[i] = new INumVar[length2[i]];
                for (int j = 0; j < length2[i]; j++)
                {
                    dv_name = name + "_(" + i.ToString() + "," + j.ToString() + ")";
                    dv[i][j] = NumVar(lowerBound, upperBound, newType, dv_name);
                    allVariables_list.Add(dv[i][j]);
                }
            }
            DecisionVariables.Add(name, dv);
        }
        protected void AddTwoDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1, int length2, out INumVar[][] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1][];
            for (int i = 0; i < length1; i++)
            {
                dv[i] = new INumVar[length2];
                NumVarType newType = type;
                if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                    newType = NumVarType.Float;
                for (int j = 0; j < length2; j++)
                {
                    dv_name = name + "_(" + i.ToString() + "," + j.ToString() + ")";
                    dv[i][j] = NumVar(lowerBound, upperBound, newType, dv_name);
                    allVariables_list.Add(dv[i][j]);
                }
            }
            DecisionVariables.Add(dv_name, dv);
        }
        protected void AddTwoDimensionalDecisionVariable(String name, double[][] lowerBound, double[][] upperBound, NumVarType type, int length1, int length2, out INumVar[][] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1][];
            NumVarType newType = type;
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                newType = NumVarType.Float;
            for (int i = 0; i < length1; i++)
            {
                dv[i] = new INumVar[length2];
                for (int j = 0; j < length2; j++)
                {
                    dv_name = name + "_(" + i.ToString() + "," + j.ToString() + ")";
                    dv[i][j] = NumVar(lowerBound[i][j], upperBound[i][j], newType, dv_name);
                    allVariables_list.Add(dv[i][j]);
                }
            }
            DecisionVariables.Add(dv_name, dv);
        }

        protected void AddThreeDimensionalDecisionVariable(String name, double lowerBound, double upperBound, NumVarType type, int length1, int length2, int length3, out INumVar[][][] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1][][];
            NumVarType newType = type;
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                newType = NumVarType.Float;
            for (int i = 0; i < length1; i++)
            {
                dv[i] = new INumVar[length2][];
                for (int j = 0; j < length2; j++)
                {
                    dv[i][j] = new INumVar[length3];
                    for (int v = 0; v < length3; v++)
                    {
                        dv_name = name + "_(" + i.ToString() + "," + j.ToString() + "," + v.ToString() + ")";
                        dv[i][j][v] = NumVar(lowerBound, upperBound, newType, dv_name);
                        allVariables_list.Add(dv[i][j][v]);
                    }
                }
            }
            DecisionVariables.Add(dv_name, dv);
        }
        protected void AddThreeDimensionalDecisionVariable(String name, double[][][] lowerBound, double[][][] upperBound, NumVarType type, int length1, int length2, int length3, out INumVar[][][] dv)
        {
            String dv_name = name;
            dv = new INumVar[length1][][];
            NumVarType newType = type;
            if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
                newType = NumVarType.Float;
            for (int i = 0; i < length1; i++)
            {
                dv[i] = new INumVar[length2][];
                for (int j = 0; j < length2; j++)
                {
                    dv[i][j] = new INumVar[length3];
                    for (int v = 0; v < length3; v++)
                    {
                        dv_name = name + "_(" + i.ToString() + "," + j.ToString() + "," + v.ToString() + ")";
                        dv[i][j][v] = NumVar(lowerBound[i][j][v], upperBound[i][j][v], newType, dv_name);
                        allVariables_list.Add(dv[i][j][v]);
                    }
                }
            }
            DecisionVariables.Add(dv_name, dv);
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
        protected INumVar[][][] GetThreeDimensionalDecisionVariableValue(String name)
        {
            return ((INumVar[][][])DecisionVariables[name]);
        }

        protected void SetCplexParameters()
        {
            if (xCplexParam.LimitComputationTime)
                SetParam(Cplex.DoubleParam.TiLim, xCplexParam.RuntimeLimit_Seconds);
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_MIP_EMPHASIS))
                SetParam(Cplex.Param.Emphasis.MIP, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.ALG_MIP_EMPHASIS].Value.ToString().Substring(1)));
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_MIP_SEARCH))
                SetParam(Cplex.Param.MIP.Strategy.Search, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.ALG_MIP_SEARCH].Value.ToString().Substring(1)));
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_CUTS_FACTOR))
                SetParam(Cplex.DoubleParam.CutsFactor, double.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.ALG_CUTS_FACTOR].Value.ToString()));
            if (xCplexParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_THREADS))
                SetParam(Cplex.Param.Threads, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.ALG_THREADS].Value.ToString()));
            SetParam(Cplex.Param.MIP.Display, 4);
        }
        protected void InitializeOutputVariables()
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

        public void Solve_and_PostProcess(PartialSolution specifiedSubproblemRoot = null)
        {
            InitializeOutputVariables();
            if (specifiedSubproblemRoot != null)
            {
                RefineDecisionVariables(specifiedSubproblemRoot);
            }
            DateTime beginTime = new DateTime();
            DateTime endTime = new DateTime();
            beginTime = DateTime.Now;
            ExportModel("mmmmodel.lp");
            Output();
            //ISSUE (#5): Turn the following two lines on/off if you want to output the log file as a text to the MPMFEVRP directory instead of output window
            //TWoutput = System.IO.File.CreateText("CplexLog.txt");
            //SetOut(TWoutput);
            Solve();
            endTime = DateTime.Now;
            cpuTime = (endTime - beginTime).TotalSeconds;
            numberOfTimesSolveMethodCalled++;
            totalTimeInSolve += cpuTime;
            Status originalCplexStatus = GetStatus();
            string originalCplexStatus_string = originalCplexStatus.ToString();
            switch (originalCplexStatus_string)
            {
                case "Infeasible":
                    solutionStatus = XCPlexSolutionStatus.Infeasible;
                    break;
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
            //now updating the statistics per status
            if (!numberOfTimesSolveFoundStatus.ContainsKey(originalCplexStatus_string))
            {
                numberOfTimesSolveFoundStatus.Add(originalCplexStatus_string, 0);
                totalTimeInSolveOnStatus.Add(originalCplexStatus_string, 0.0);
            }
            numberOfTimesSolveFoundStatus[originalCplexStatus_string]++;
            totalTimeInSolveOnStatus[originalCplexStatus_string] += cpuTime;
            if (originalCplexStatus_string == "Infeasible")
                return;

            if (xCplexParam.Relaxation == XCPlexRelaxation.None)
            {
                //Not relaxed, outcome can be Unknown, Feasible or Optimal (Can't be infeasible, if it was, it we wouldn't come this far)
                //Obtain a lower bound
                lowerBound = GetBestObjValue(); //TODO test that this works for each objective function type
                //if at least feasible, obtain an upper bound and complete solution giving the upper bound
                if (solutionStatus > 0)
                {
                    //Obtain upper bound value
                    upperBound = GetObjValue(); //TODO test that this works for each objective function type
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
            //if (xCplexParam.Relaxation == XCPlexRelaxation.LinearProgramming)
            //    return ValidateIntegralityOfLPRelaxation();
            ////We should never get here
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

        protected double GetVarCostPerMile(VehicleCategories vehicleCategory)
        {
            return theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).VariableCostPerMile;
        }
        protected double GetVehicleFixedCost(VehicleCategories vehicleCategory)
        {
            return theProblemModel.VRD.GetTheVehicleOfCategory(vehicleCategory).FixedCost;
        }

        public abstract string GetModelName();

        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    ClearModel();
                    ClearCallbacks();
                    ClearCuts();
                    ClearLazyConstraints();
                    ClearUserCuts();
                    End();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        
        // Shortcuts
        // DV1D : Decision Variable 1-Dimensional etc.
        protected void AddDV1D(String name, double lowerBound, double upperBound, NumVarType type, int length1, out INumVar[] dv)
        {
            AddOneDimensionalDecisionVariable(name, lowerBound, upperBound, type, length1, out dv);
        }
        protected void AddDV2D(String name, double lowerBound, double upperBound, NumVarType type, int length1, int length2, out INumVar[][] dv)
        {
            AddTwoDimensionalDecisionVariable(name, lowerBound, upperBound, type, length1, length2, out dv);
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
