using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Models.XCPlex
{
    public abstract class XCPlexBase : Cplex
    {
        Dictionary<String, Object> DecisionVariables = new Dictionary<string, object>();

        public XCPlexBase() { }

        protected void Initialize()
        {
            DefineDecisionVariables();
            SetObjectiveFunction();
            AddConstraints();
        }

        protected abstract void DefineDecisionVariables();
        protected abstract void SetObjectiveFunction();
        protected abstract void AddConstraints();

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
