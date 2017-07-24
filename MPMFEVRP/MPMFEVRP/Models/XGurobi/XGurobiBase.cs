using System;
using Gurobi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Models.XGurobi
{
    public abstract class XGurobiBase: GRBEnv
    {
        protected ProblemModelBase problemModel;
        protected XCPlexParameters xCplexParam;
        protected GRBVar variable_type = GRBVar; // TODO I dont think this gives the var type
        protected List<GRBVar> allVariables_list;
        protected GRBVar[] allVariables_array;
        protected List<GRBConstr> allConstraints_list;
        protected GRBConstr[] allConstraints_array;
        protected XCPlexSolutionStatus solutionStatus;
        protected bool optimalCompleteSolutionObtained;
        protected bool variableValuesObtained;
        protected bool reducedCostsObtained;
        protected double lowerBound;
        protected double upperBound;
        protected double cpuTime;
        public double[] AllValues { get { return GetValues(); } }
        public double[] AllReducedCosts { get { return GetReducedCosts(); } }
        public int[] AllVariableBasisStatuses { get { return GetBasisStatuses(); } }
        public double[] AllSlacks { get { return GetSlacks(); } }
        public double[] AllShadowPrices { get { return GetDuals(); } }

        public Cplex.BasisStatus[] AllConstraintBasisStatuses { get { return GetBasisStatuses(allConstraints_array); } }
        public XCPlexSolutionStatus SolutionStatus { get { return solutionStatus; } }
        public bool OptimalCompleteSolutionObtained { get { return optimalCompleteSolutionObtained; } }
        public bool VariableValuesObtained { get { return variableValuesObtained; } }
        public bool ReducedCostsObtained { get { return reducedCostsObtained; } }
        public double LowerBound_XCPlex { get { return lowerBound; } }
        public double UpperBound_XCPlex { get { return upperBound; } }
        public double CPUtime { get { return cpuTime; } }

        Dictionary<String, Object> DecisionVariables = new Dictionary<string, object>();
        public XGurobiBase()
        {
            //try {

            // Warehouse demand in thousands of units
            double[] Demand = new double[] { 15, 18, 14, 20 };

            // Plant capacity in thousands of units
            double[] Capacity = new double[] { 20, 22, 17, 19, 18 };

            // Fixed costs for each plant
            double[] FixedCosts =
                new double[] { 12000, 15000, 17000, 13000, 16000 };

            // Transportation costs per thousand units
            double[,] TransCosts =
                new double[,] { { 4000, 2000, 3000, 2500, 4500 },
              { 2500, 2600, 3400, 3000, 4000 },
              { 1200, 1800, 2600, 4100, 3000 },
              { 2200, 2600, 3100, 3700, 3200 } };

            // Number of plants and warehouses
            int nPlants = Capacity.Length;
            int nWarehouses = Demand.Length;

            // Model
            GRBEnv env = new GRBEnv();
            GRBModel model = new GRBModel(env);

            model.ModelName = "facility";

            // Plant open decision variables: open[p] == 1 if plant p is open.
            GRBVar[] open = new GRBVar[nPlants];
            for (int p = 0; p < nPlants; ++p)
            {
                open[p] = model.AddVar(0, 1, FixedCosts[p], GRB.BINARY, "Open" + p);
            }

            // Transportation decision variables: how much to transport from
            // a plant p to a warehouse w
            GRBVar[,] transport = new GRBVar[nWarehouses, nPlants];
            for (int w = 0; w < nWarehouses; ++w)
            {
                for (int p = 0; p < nPlants; ++p)
                {
                    transport[w, p] =
                        model.AddVar(0, GRB.INFINITY, TransCosts[w, p], GRB.CONTINUOUS,
                                     "Trans" + p + "." + w);
                }
            }

            // The objective is to minimize the total fixed and variable costs
            model.ModelSense = GRB.MINIMIZE;

            // Production constraints
            // Note that the right-hand limit sets the production to zero if
            // the plant is closed
            for (int p = 0; p < nPlants; ++p)
            {
                GRBLinExpr ptot = 0.0;
                for (int w = 0; w < nWarehouses; ++w)
                    ptot.AddTerm(1.0, transport[w, p]);
                model.AddConstr(ptot <= Capacity[p] * open[p], "Capacity" + p);
            }

            // Demand constraints
            for (int w = 0; w < nWarehouses; ++w)
            {
                GRBLinExpr dtot = 0.0;
                for (int p = 0; p < nPlants; ++p)
                    dtot.AddTerm(1.0, transport[w, p]);
                model.AddConstr(dtot == Demand[w], "Demand" + w);
            }

            // Guess at the starting point: close the plant with the highest
            // fixed costs; open all others

            // First, open all plants
            for (int p = 0; p < nPlants; ++p)
            {
                open[p].Start = 1.0;
            }

            // Now close the plant with the highest fixed cost
            Console.WriteLine("Initial guess:");
            double maxFixed = -GRB.INFINITY;
            for (int p = 0; p < nPlants; ++p)
            {
                if (FixedCosts[p] > maxFixed)
                {
                    maxFixed = FixedCosts[p];
                }
            }
            for (int p = 0; p < nPlants; ++p)
            {
                if (FixedCosts[p] == maxFixed)
                {
                    open[p].Start = 0.0;
                    Console.WriteLine("Closing plant " + p + "\n");
                    break;
                }
            }

            // Use barrier to solve root relaxation
            model.Parameters.Method = GRB.METHOD_BARRIER;

            // Solve
            model.Optimize();

            // Print solution
            Console.WriteLine("\nTOTAL COSTS: " + model.ObjVal);
            Console.WriteLine("SOLUTION:");
            for (int p = 0; p < nPlants; ++p)
            {
                if (open[p].X > 0.99)
                {
                    Console.WriteLine("Plant " + p + " open:");
                    for (int w = 0; w < nWarehouses; ++w)
                    {
                        if (transport[w, p].X > 0.0001)
                        {
                            Console.WriteLine("  Transport " +
                                transport[w, p].X + " units to warehouse " + w);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Plant " + p + " closed!");
                }
            }

            // Dispose of model and env
            model.Dispose();
            env.Dispose();

            //} catch (GRBException e) {
            //  Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
            //}
        }
        double[] GetValues()
        {
            double[] allValues = new double[allVariables_array.Length];
            for (int i = 0; i < allVariables_array.Length; i++)
            {
                allValues[i] = allVariables_array[i].X;
            }
            return allValues;
        }
        double[] GetReducedCosts()
        {
            double[] allReducedCosts = new double[allVariables_array.Length];
            for (int i = 0; i < allVariables_array.Length; i++)
            {
                allReducedCosts[i] = allVariables_array[i].RC;
            }
            return allReducedCosts;
        }
        int []GetBasisStatuses()
        {
            int[] allBasisStatuses = new int[allVariables_array.Length];
            for (int i = 0; i < allVariables_array.Length; i++)
            {
                allBasisStatuses[i] = allVariables_array[i].VBasis;
            }
            return allBasisStatuses;
        }
        double[] GetSlacks()
        {
            double[] allSlacks = new double[allConstraints_array.Length];
            for (int i = 0; i < allConstraints_array.Length; i++)
            {
                allSlacks[i] = allConstraints_array[i].Slack;
            }
            return allSlacks;
        }
        double[] GetDuals()
        {
            double[] allDuals = new double[allConstraints_array.Length];
            for (int i = 0; i < allConstraints_array.Length; i++)
            {
                allDuals[i] = allConstraints_array[i].Pi;
            }
            return allDuals;
        }
    }
}
