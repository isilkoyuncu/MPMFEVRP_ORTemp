//using System;
//using Gurobi;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MPMFEVRP.Implementations.Solutions;
//using MPMFEVRP.Domains.AlgorithmDomain;
//using MPMFEVRP.Domains.SolutionDomain;
//using MPMFEVRP.Interfaces;

//namespace MPMFEVRP.Models.XGurobi
//{
//    public abstract class XGurobiBase:GRBModel
//    {
//        protected GRBEnv gurobi_environment;
//        protected ProblemModelBase problemModel;
//        protected XGurobiParameters xGurobiParam;
//        //protected GRBVar variable_type = GRBVar; // TODO I dont think this gives the var type
//        protected List<GRBVar> allVariables_list;
//        protected GRBVar[] allVariables_array;
//        protected List<GRBConstr> allConstraints_list;
//        protected GRBConstr[] allConstraints_array;
//        protected XGurobiSolutionStatus solutionStatus;
//        protected bool optimalCompleteSolutionObtained;
//        protected bool variableValuesObtained;
//        protected bool reducedCostsObtained;
//        protected double lowerBound;
//        protected double upperBound;
//        protected double cpuTime;
//        public double[] AllValues => GetValues();
//        public double[] AllReducedCosts => GetReducedCosts();
//        public int[] AllVariableBasisStatuses => GetBasisStatuses();
//        public double[] AllSlacks => GetSlacks();
//        public double[] AllShadowPrices => GetDuals();
//        public double[] AllConstraintBasisStatuses => GetConstraintBasisStatuses();
//        public XGurobiSolutionStatus SolutionStatus { get { return solutionStatus; } }
//        public bool OptimalCompleteSolutionObtained { get { return optimalCompleteSolutionObtained; } }
//        public bool VariableValuesObtained { get { return variableValuesObtained; } }
//        public bool ReducedCostsObtained { get { return reducedCostsObtained; } }
//        public double LowerBound_XGurobi { get { return lowerBound; } }
//        public double UpperBound_XGurobi { get { return upperBound; } }
//        public double CPUtime { get { return cpuTime; } }

//        Dictionary<String, Object> DecisionVariables = new Dictionary<string, object>();


//        public XGurobiBase(GRBEnv Gurobi_Environment):base(Gurobi_Environment) { }
//        public XGurobiBase(GRBEnv Gurobi_Environment, ProblemModelBase problemModel, XGurobiParameters xGurobiParam) : base(Gurobi_Environment)
//        {
//            gurobi_environment = Gurobi_Environment;
//            this.problemModel = problemModel;
//            this.xGurobiParam = xGurobiParam;
//            XGurobiRelaxation relaxation;
//            relaxation = xGurobiParam.Relaxation;
            
//            //now we are ready to put the model together and then solve it
//            //Define the variables
//            DefineDecisionVariables();
//            if ((xGurobiParam.Relaxation == XGurobiRelaxation.LinearProgramming)
//                //||(xCplexParam.Relaxation == XCPlexRelaxation.AssignmentProblem)
//                )
//                SetAllVariablesType(GRB.CONTINUOUS);
//            //Objective function
//            AddTheObjectiveFunction();
//            //Constraints
//            AddAllConstraints();
//            //Cplex parameters
//            SetGurobiParameters();
//            //output variables
//            InitializeOutputVariables();
//        }

//        protected void Initialize() // TODO this has no references, i.e. we never use this initialize 
//        {
//            DefineDecisionVariables();
//            AddTheObjectiveFunction();
//            AddAllConstraints();
//        }
//        protected abstract void DefineDecisionVariables();
//        protected abstract void AddTheObjectiveFunction();
//        protected abstract void AddAllConstraints();
//        public abstract string GetDescription_AllVariables_Array();
//        public abstract SolutionBase GetCompleteSolution(Type SolutionType);//TODO Figure out how to make this work with a run-time-selected Solution type
//        protected void InitializeOutputVariables()
//        {
//            //Initializing outputs
//            this.solutionStatus = XGurobiSolutionStatus.NotYetSolved;
//            this.optimalCompleteSolutionObtained = false;
//            this.variableValuesObtained = false;
//            this.reducedCostsObtained = false;
//            this.lowerBound = double.MinValue;
//            this.upperBound = double.MaxValue;
//        }
//        protected void RefineDecisionVariables(PartialSolution PS)
//        {
//            double[] allLB = PS.GetAllDecisionVariableLowerBounds();
//            double[] allUB = PS.GetAllDecisionVariableUpperBounds();
//            for (int dvIndex = 0; dvIndex < allVariables_array.Length; dvIndex++)
//            {
//                allVariables_array[dvIndex].LB = allLB[dvIndex];
//                allVariables_array[dvIndex].UB = allUB[dvIndex];
//            }
//        }
//        protected void SetGurobiParameters()
//        {
//            //SetOut(null);
//            if (xGurobiParam.LimitComputationTime)
//                Set(GRB.DoubleParam.TimeLimit, xGurobiParam.RuntimeLimit_Seconds);
//            if (xGurobiParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_MIP_EMPHASIS))
//                Set(GRB.IntParam.MIPFocus, int.Parse(xGurobiParam.OptionalCPlexParameters[ParameterID.ALG_MIP_EMPHASIS].Value.ToString().Substring(1)));
//            //if (xGurobiParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_MIP_SEARCH))
//            //Set(GRB.IntParam.MIP.Strategy.Search, int.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.ALG_MIP_SEARCH].Value.ToString().Substring(1)));
//            //if (xGurobiParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_CUTS_FACTOR))
//            //Set(GRB.IntParam.CutsFactor, double.Parse(xCplexParam.OptionalCPlexParameters[ParameterID.ALG_CUTS_FACTOR].Value.ToString()));
//            if (xGurobiParam.OptionalCPlexParameters.ContainsKey(ParameterID.ALG_THREADS))
//                Set(GRB.IntParam.Threads, int.Parse(xGurobiParam.OptionalCPlexParameters[ParameterID.ALG_THREADS].Value.ToString()));
//        }
//        public void Solve_and_PostProcess(PartialSolution specifiedSubproblemRoot = null)
//        {
//            InitializeOutputVariables();
//            if (specifiedSubproblemRoot != null)
//            {
//                RefineDecisionVariables(specifiedSubproblemRoot);
//            }
//            DateTime beginTime = new DateTime();
//            DateTime endTime = new DateTime();
//            beginTime = DateTime.Now;
//            //ExportModel("model.lp");
//            Optimize();
//            endTime = DateTime.Now;
//            cpuTime = (endTime - beginTime).TotalSeconds;
//            int originalGurobiStatus = Status;
//            solutionStatus = (XGurobiSolutionStatus)originalGurobiStatus;

//            if (xGurobiParam.Relaxation == XGurobiRelaxation.None)
//            {
//                //Not relaxed, outcome can be Unknown, Feasible or Optimal (Can't be infeasible, if it was, we wouldn't come this far)

//                //ObjVal: Objective value for current solution
//                //ObjBound: Best available objective bound (LB: minimization, UB: maximization)

//                //Obtain a lower bound
//                lowerBound = ObjVal;//TODO this shouldn't be LB all the time, depends on the objective function type
//                //if at least feasible, obtain an upper bound and complete solution giving the upper bound
//                if (solutionStatus > 0)
//                {
//                    //Obtain upper bound value
//                    upperBound = ObjBoundC;
//                    //Obtain X and maybe Y values so a complete solution can be constructed from them 
//                    optimalCompleteSolutionObtained = (solutionStatus == XGurobiSolutionStatus.Optimal);
//                }
//            }//if not relaxed
//            else//relaxed
//            {
//                //If we're here we know we have an optimal solution
//                //Obtain a lower bound
//                upperBound = ObjBoundC;
//                lowerBound = upperBound;//Because the solution is optimal
//                                        //Obtain X and maybe Y values so a complete solution can be constructed from them 
//                if (ValidateCompletenessOfRelaxedSolution())
//                    optimalCompleteSolutionObtained = true;
//            }//if relaxed
//        }
//        bool ValidateCompletenessOfRelaxedSolution()
//        {
//            if (xGurobiParam.Relaxation == XGurobiRelaxation.LinearProgramming)
//                return ValidateIntegralityOfLPRelaxation();
//            //We should never get here
//            return false;
//        }
//        bool ValidateIntegralityOfLPRelaxation()
//        {
//            double[] allValues = GetValues();
//            for (int dvInd = 0; dvInd < allValues.Length; dvInd++)
//                if (Math.Min(Math.Abs(allVariables_array[dvInd].UB - allValues[dvInd]), Math.Abs(allValues[dvInd] - allVariables_array[dvInd].LB)) > xGurobiParam.ErrorTolerance)
//                    return false;
//            return true;
//        }
//        void SetAllVariablesType(char varType)
//        {
//            for (int i = 0; i < allVariables_array.Length; i++)
//            {          
//            if (allVariables_array[i].VType != varType)
//                {
//                    allVariables_array[i].VType = varType;
//                }
//            }
//        }
//        double[] GetValues()
//        {
//            double[] allValues = new double[allVariables_array.Length];
//            for (int i = 0; i < allVariables_array.Length; i++)
//            {
//                allValues[i] = allVariables_array[i].X;
//            }
//            return allValues;
//        }
//        double[] GetReducedCosts()
//        {
//            double[] allReducedCosts = new double[allVariables_array.Length];
//            for (int i = 0; i < allVariables_array.Length; i++)
//            {
//                allReducedCosts[i] = allVariables_array[i].RC;
//            }
//            return allReducedCosts;
//        }
//        int[] GetBasisStatuses()
//        {
//            int[] allBasisStatuses = new int[allVariables_array.Length];
//            for (int i = 0; i < allVariables_array.Length; i++)
//            {
//                allBasisStatuses[i] = allVariables_array[i].VBasis;
//            }
//            return allBasisStatuses;
//        }
//        double[] GetSlacks()
//        {
//            double[] allSlacks = new double[allConstraints_array.Length];
//            for (int i = 0; i < allConstraints_array.Length; i++)
//            {
//                allSlacks[i] = allConstraints_array[i].Slack;
//            }
//            return allSlacks;
//        }
//        double[] GetDuals()
//        {
//            double[] allDuals = new double[allConstraints_array.Length];
//            for (int i = 0; i < allConstraints_array.Length; i++)
//            {
//                allDuals[i] = allConstraints_array[i].Pi;
//            }
//            return allDuals;
//        }
//        double[] GetConstraintBasisStatuses()
//        {
//            double[] allConstraintBasisStatuses = new double[allConstraints_array.Length];
//            for (int i = 0; i < allConstraints_array.Length; i++)
//            {
//                allConstraintBasisStatuses[i] = allConstraints_array[i].CBasis;
//            }
//            return allConstraintBasisStatuses;
//        }
//    }
//}
