﻿using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;

namespace MPMFEVRP.Implementations.Algorithms
{
    public class Outsource2Cplex : AlgorithmBase
    {
        XCPlexParameters XcplexParam;
        XCPlexBase CPlexExtender = null;
        string folder { get; set; }

        public Outsource2Cplex() : base() 
        {
            AddSpecializedParameters();
        }

        public Outsource2Cplex(double tilim, string clplexFormulation, string f = null) : base()
        {
            AddSpecializedParameters();
            algorithmParameters.UpdateParameter(ParameterID.ALG_RUNTIME_SECONDS, tilim);
            switch (clplexFormulation)
            {
                case "adf":
                    algorithmParameters.UpdateParameter(ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
                    break;
                case "ndf":
                    algorithmParameters.UpdateParameter(ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);
                    break;
                case "mixedEVRP":
                    algorithmParameters.UpdateParameter(ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.MixedEVRPwRefuelingPaths);
                    break;
                default:
                    throw new Exception("Unknown cplex formulation.");
            }
            folder = f;
        }
        public override void AddSpecializedParameters()
        {
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_XCPLEX_FORMULATION, "XCplex formulation", new List<object>() { XCPlex_Formulation.ArcDuplicatingwoU, XCPlex_Formulation.MixedEVRPwRefuelingPaths, XCPlex_Formulation.ETSP, XCPlex_Formulation.TSP }, XCPlex_Formulation.TSP, UserInputObjectType.ComboBox));
            //Optional Cplex parameters. One added as an example, the others can be added here and commented out when not needed
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_THREADS, "# of Threads", ListPossibleNumOfThreads(), 0 ,UserInputObjectType.ComboBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_RELAXATION, "Cplex Relaxation", new List<object>() { XCPlexRelaxation.None, XCPlexRelaxation.LinearProgramming }, XCPlexRelaxation.None, UserInputObjectType.ComboBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_TIGHTER_AUX_BOUNDS, "Tighter Auxiliary Bounds", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_LOG_OUTPUT_TYPE, "CPLEX Log Output Type", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_XCPLEX_OUTPUT_LEVEL, "Cplex Output Level", new List<object>() { XCPlexOutputLevels.NoDisplay, XCPlexOutputLevels.DispIntFeasSolns, XCPlexOutputLevels.DispNodesMIPContrlBasic, XCPlexOutputLevels.DispNodesMIPContrlIntermNodeCuts, XCPlexOutputLevels.DispNodesMIPContrlAdvRootLP, XCPlexOutputLevels.DispNodesMIPContrlAdvPlusAllLP }, XCPlexOutputLevels.DispNodesMIPContrlAdvPlusAllLP, UserInputObjectType.ComboBox));
            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_EXPORT_LP_MODEL, "Export LP Model", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));      
        }

        public override string GetName()
        {
            return "Outsource to CPLEX";
        }

        public void Initialize(){}

        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            base.theProblemModel = theProblemModel;
            status = AlgorithmSolutionStatus.NotYetSolved;
            stats.UpperBound = double.MaxValue;

            XcplexParam = new XCPlexParameters(
                limitComputationTime: true,
                runtimeLimit_Seconds: algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue(),
                optionalCPlexParameters: algorithmParameters.GetIntersectingParameters(XCPlexParameters.recognizedOptionalCplexParameters),
                relaxation: (XCPlexRelaxation)algorithmParameters.GetParameter(ParameterID.ALG_RELAXATION).Value,
                tighterAuxBounds: algorithmParameters.GetParameter(ParameterID.ALG_TIGHTER_AUX_BOUNDS).GetBoolValue(),
                cplexLogOutputFile: algorithmParameters.GetParameter(ParameterID.ALG_LOG_OUTPUT_TYPE).GetBoolValue(),
                cplexLogDisplay: algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).GetIntValue(),
                exportLpModel: algorithmParameters.GetParameter(ParameterID.ALG_EXPORT_LP_MODEL).GetBoolValue()
                );
            
        }

        public override void SpecializedRun()
        {
            CPlexExtender = BuildXcplexModel(theProblemModel, XcplexParam, algorithmParameters);
            CPlexExtender.Solve_and_PostProcess();
        }
        public static XCPlexBase BuildXcplexModel(EVvsGDV_ProblemModel theProblemModel, XCPlexParameters XcplexParam, InputOrOutputParameterSet algorithmParameters)
        {
            XCPlexBase model;
            switch ((XCPlex_Formulation)algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value)
            {
                case XCPlex_Formulation.MixedEVRPwRefuelingPaths:
                    model = new XCPlex_MixedFleetEVRPwRefuelingPathsVP(theProblemModel, XcplexParam, theProblemModel.CoverConstraintType);
                    break;
                case XCPlex_Formulation.ArcDuplicatingwoU:
                    model = new XCPlex_ArcDuplicatingFormulation_woU(theProblemModel, XcplexParam, theProblemModel.CoverConstraintType);
                    break;
                case XCPlex_Formulation.ETSP:
                    model = new XCPlex_ETSP_VP(theProblemModel, XcplexParam, theProblemModel.CoverConstraintType);
                    break;
                case XCPlex_Formulation.TSP:
                    model = new XCPlex_TSP(theProblemModel, XcplexParam, theProblemModel.CoverConstraintType);
                    break;
                default:
                    throw new Exception("XCplex model type does not exist, thus cannot be built.");

            }
            if(XcplexParam.ExportLpModel)
                model.ExportModel(theProblemModel.GetInstanceName(theProblemModel.InputFileName) + "-" +((XCPlex_Formulation)algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value).ToString() + "- model.lp");
            return model;
        }

        public override void SpecializedConclude()
        {
            //Given that the instance is solved, we need to update status and statistics from it
            status = (AlgorithmSolutionStatus)((int)CPlexExtender.SolutionStatus);
            stats.RunTimeMilliSeconds = (long)CPlexExtender.CPUtime;
            stats.LowerBound = CPlexExtender.LowerBound_XCPlex;
            stats.UpperBound = CPlexExtender.UpperBound_XCPlex;
            if (((XCPlex_Formulation)algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value) == XCPlex_Formulation.ArcDuplicating)
                DecompressArcDuplicatingFormulationVariables(CPlexExtender.AllValues);
            GetOutputSummary();
            //Create solution based on status: Not yet solved, infeasible, no feasible soln found, feasible, optimal
            switch (status)
            {
                case AlgorithmSolutionStatus.NotYetSolved:
                    {
                        //Actual Run Time:N/A, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
                        bestSolutionFound = new RouteBasedSolution();
                        break;
                    }
                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
                    {
                        //Actual Run Time:Report, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
                        bestSolutionFound = new RouteBasedSolution();
                        break;
                    }
                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
                    {
                        //Actual Run Time=Limit:Report, Complete Solution-LB:N/A, Best Solution-UB:N/A, Best Solution Found:N/A
                        bestSolutionFound = new RouteBasedSolution();
                        break;
                    }
                case AlgorithmSolutionStatus.Feasible:
                    {
                        //Actual Run Time=Limit:Report, Complete Solution-LB:Report, Best Solution-UB:Report, Best Solution Found:Report
                        bestSolutionFound = (RouteBasedSolution)CPlexExtender.GetCompleteSolution(typeof(RouteBasedSolution));
                        break;
                    }
                case AlgorithmSolutionStatus.Optimal:
                    {
                        //Actual Run Time:Report<Limit, Complete Solution-LB = Best Solution-UB:Report, Best Solution Found:Report
                        bestSolutionFound = (RouteBasedSolution)CPlexExtender.GetCompleteSolution(typeof(RouteBasedSolution));
                        break;
                    }
                default:
                    break;
            }
            bestSolutionFound.Status = status;
            bestSolutionFound.UpperBound = CPlexExtender.UpperBound_XCPlex;
            bestSolutionFound.LowerBound = CPlexExtender.LowerBound_XCPlex;
            


       }

        public override void SpecializedReset()
        {
            CPlexExtender.ClearModel();
            CPlexExtender.Dispose();
            CPlexExtender.End();
            GC.Collect();
        }

        public override string[] GetOutputSummary()
        {
            List<string> list = new List<string>{
                 //Algorithm Name has to be the first entry for output file name purposes
                "Algorithm Name: " + GetName()+ "-" +algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value.ToString(),
                //Run time limit has to be the second entry for output file name purposes
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).Value.ToString(),
                
                //Optional
                "Parameter: " + algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Description + "-" + algorithmParameters.GetParameter(ParameterID.ALG_XCPLEX_FORMULATION).Value.ToString(),
                //algorithmParameters.GetAllParameters();
                //var asString = string.Join(";", algorithmParameters.GetAllParameters());
                //list.Add(asString);
                
                //Necessary statistics
                "CPU Run Time(sec): " + stats.RunTimeMilliSeconds.ToString(),
                "Solution Status: " + status.ToString()
            };
            switch (status)
            {
                case AlgorithmSolutionStatus.NotYetSolved:
                    {
                        break;
                    }
                case AlgorithmSolutionStatus.Infeasible://When it is profit maximization, we shouldn't observe this case
                    {
                        break;
                    }
                case AlgorithmSolutionStatus.NoFeasibleSolutionFound:
                    {
                        break;
                    }
                default:
                    {
                        list.Add("UB(Best Int): " + stats.UpperBound.ToString());
                        list.Add("LB(Relaxed): " + stats.LowerBound.ToString());
                        break;
                    }
            }
            string[] toReturn = new string[list.Count];
            toReturn = list.ToArray();
            return toReturn;
        }

        public bool IsSupportingStepwiseSolutionCreation()
        {
            return false;
        }

        List<object> ListPossibleNumOfThreads()
        {
            int tCount = Environment.ProcessorCount;
            List<object> noThreads = new List<object>();
            for (int t = 0; t <= tCount; t++)
            {
                noThreads.Add(t);
            }
            return noThreads;
        }
        void DecompressArcDuplicatingFormulationVariables(double[] allVariableValues)
        {
            double[][][] X_value;
            double[][][] Y_value;
            double[][] U_value;
            double[] T_value;
            double[] delta_value;
            double[] epsilon_value;

            int numCustomers = theProblemModel.SRD.NumCustomers;
            int numES = theProblemModel.SRD.NumES;
            int counter = 0;
            X_value = new double[numCustomers+1][][];
            for (int i = 0; i <= numCustomers; i++)
            {
                X_value[i] = new double[numCustomers + 1][];
                for (int j = 0; j <= numCustomers; j++)
                {
                    X_value[i][j] = new double[theProblemModel.VRD.NumVehicleCategories];
                    for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)
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
                U_value[j] = new double[theProblemModel.VRD.NumVehicleCategories];
                for (int v = 0; v < theProblemModel.VRD.NumVehicleCategories; v++)

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

        public override bool setListener(IListener listener)
        {
            return false;
        }
    }
}
