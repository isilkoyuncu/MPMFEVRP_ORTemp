using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Models;
using MPMFEVRP.Implementations.Solutions;

namespace MPMFEVRP.Implementations.Algorithms
{
    class Outsource2Cplex : AlgorithmBase
    {
        XCPlexParameters XcplexParam;

        public Outsource2Cplex() : base() 
        {
            algorithmParameters.AddParameter(new Parameter(ParameterID.XCPLEX_FORMULATION, "XCplex formulation", new List<object>() { XCPlex_Formulation.NodeDuplicating, XCPlex_Formulation.ArcDuplicating }, XCPlex_Formulation.ArcDuplicating, ParameterType.ComboBox));
        }

        public override string GetName()
        {
            return "Outsource to CPLEX";
        }
        public void Initialize()
        {
            
        }

        public bool IsSupportingStepwiseSolutionCreation()
        {
            return false;
        }

        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedInitialize(ProblemModelBase problemModel)
        {
            base.model = problemModel;
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;
            XcplexParam = new XCPlexParameters();
        }

        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }

        public override void SpecializedRun()
        {
            XCPlexBase CPlexExtender = null;
            switch ((XCPlex_Formulation)algorithmParameters.GetParameter(ParameterID.XCPLEX_FORMULATION).Value)
            {
                case XCPlex_Formulation.NodeDuplicating:
                    CPlexExtender = new XCPlex_NodeDuplicatingFormulation(model, XcplexParam);
                    break;
                case XCPlex_Formulation.ArcDuplicating:
                    CPlexExtender = new XCPlex_ArcDuplicatingFormulation(model, XcplexParam);
                    break;
            }
            CPlexExtender.ExportModel("model.lp");
            CPlexExtender.Solve_and_PostProcess();
            //decompressArcDuplicatingFormulationVariables(CPlexExtender.AllValues);

            //Given that the model is solved, we need to update status and statistics from it
            status = (AlgorithmSolutionStatus)((int)CPlexExtender.SolutionStatus);
            stats.LowerBound = CPlexExtender.LowerBound_XCPlex;
            stats.UpperBound = CPlexExtender.UpperBound_XCPlex;
            NEW_RouteBasedSolution optimalSolution = (NEW_RouteBasedSolution)CPlexExtender.GetCompleteSolution(typeof(NEW_RouteBasedSolution));
        }

        void DecompressArcDuplicatingFormulationVariables(double[] allVariableValues)
        {
            double[][][] X_value;
            double[][][] Y_value;
            double[][] U_value;
            double[] T_value;
            double[] delta_value;
            double[] epsilon_value;

            int numCustomers = model.SRD.NumCustomers;
            int numES = model.SRD.NumES;
            int counter = 0;
            X_value = new double[numCustomers+1][][];
            for (int i = 0; i <= numCustomers; i++)
            {
                X_value[i] = new double[numCustomers + 1][];
                for (int j = 0; j <= numCustomers; j++)
                {
                    X_value[i][j] = new double[model.VRD.NumVehicleCategories];
                    for (int v = 0; v < model.VRD.NumVehicleCategories; v++)
                        X_value[i][j][v] = allVariableValues[counter++];
                }
            }

            Y_value = new double[numCustomers + 1][][];
            for (int i = 0; i <= numCustomers; i++)
            {
                Y_value[i] = new double[numES][];
                for (int r = 0; r < numES; r++)
                {
                    Y_value[i][r] = new double[numCustomers + 1];
                    for (int j = 0; j <= numCustomers; j++)
                        Y_value[i][r][j] = allVariableValues[counter++];
                }
            }

            U_value = new double[numCustomers + 1][];
            for (int j = 0; j <= numCustomers; j++)
            {
                U_value[j] = new double[model.VRD.NumVehicleCategories];
                for (int v = 0; v < model.VRD.NumVehicleCategories; v++)

                    U_value[j][v] = allVariableValues[counter++];
            }

            T_value = new double[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
                T_value[j] = allVariableValues[counter++];

            delta_value = new double[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
                delta_value[j] = allVariableValues[counter++];

            epsilon_value = new double[numCustomers + 1];
            for (int j = 0; j <= numCustomers; j++)
                epsilon_value[j] = allVariableValues[counter++];
        }
        // TODO this does not belong to here
        //private void writeSolution(AbstractXCPlexFormulation CPlexExtender)
        //{
        //    string status;
        //    double objValue, CPUtime, optGap;
        //    string path = "C:/Users/ikoyuncu/Desktop/MPMFEVRP/Version1.0/Version1.0/bin/x64/Debug/Our instances/Output/";
        //    System.IO.StreamWriter sw;
        //    sw = new System.IO.StreamWriter(path + fromProblem.NumCustomers + "C_4ES_"+fromProblem.NumVehicles[0]+"EV_Node_2.txt");
        //    sw.WriteLine("Problem Characteristics");
        //    sw.WriteLine("NumCustomers\tNumES\tNumNodes\tEV");
        //    sw.WriteLine("{0}\t{1}\t{2}\t{3}", fromProblem.NumCustomers, fromProblem.NumES, (fromProblem.NumCustomers + fromProblem.NumES + 1), 1);
        //    status = CPlexExtender.SolutionStatus.ToString();
        //    objValue = CPlexExtender.BestObjValue;
        //    optGap = CPlexExtender.MIPRelativeGap;
        //    CPUtime = CPlexExtender.CPUtime;
        //    sw.WriteLine("Solution Characteristics");
        //    sw.WriteLine("Status\tObjective Value\tOptimality Gap\tCPU Time");
        //    sw.WriteLine("{0}\t{1}\t{2}\t{3}", status, objValue, optGap, CPUtime);
        //    sw.Close();
        //}
    }
}
