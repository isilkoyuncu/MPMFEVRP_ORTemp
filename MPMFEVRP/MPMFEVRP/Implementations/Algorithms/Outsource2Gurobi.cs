//using MPMFEVRP.Domains.AlgorithmDomain;
//using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
//using MPMFEVRP.Implementations.Algorithms;
//using MPMFEVRP.Models;
//using MPMFEVRP.Models.XCPlex;
//using System;
//using System.Collections.Generic;
//using Gurobi;

//namespace MPMFEVRP.Implementations.Algorithms
//{
//    public class Outsource2Gurobi : AlgorithmBase
//    {
//        XCPlexParameters XcplexParam;

//        GRBEnv env;
//        GRBModel model;
//        public Outsource2Gurobi()
//        {
//            AddSpecializedParameters();
//        }
//        public override void AddSpecializedParameters()
//        {
//            algorithmParameters.AddParameter(new InputOrOutputParameter(ParameterID.ALG_XCPLEX_FORMULATION, "XCplex formulation", new List<object>() { XCPlex_Formulation.NodeDuplicating, XCPlex_Formulation.ArcDuplicating, XCPlex_Formulation.NodeDuplicatingwoU, XCPlex_Formulation.ArcDuplicatingwoU }, XCPlex_Formulation.NodeDuplicatingwoU, UserInputObjectType.ComboBox));
//        }

//        public override string GetName()
//        {
//            return "Outsource to Gurobi";
//        }

//        public override string[] GetOutputSummary()
//        {
//            throw new NotImplementedException();
//        }

//        public override void SpecializedConclude()
//        {
//            int optimstatus = model.Status;

//            if (optimstatus == GRB.Status.INF_OR_UNBD)
//            {
//                model.Parameters.Presolve = 0;
//                model.Optimize();
//                optimstatus = model.Status;
//            }

//            if (optimstatus == GRB.Status.OPTIMAL)
//            {
//                double objval = model.ObjVal;
//                Console.WriteLine("Optimal objective: " + objval);
//            }
//            else if (optimstatus == GRB.Status.INFEASIBLE)
//            {
//                Console.WriteLine("Model is infeasible");

//                // compute and write out IIS

//                model.ComputeIIS();
//                model.Write("model.ilp");
//            }
//            else if (optimstatus == GRB.Status.UNBOUNDED)
//            {
//                Console.WriteLine("Model is unbounded");
//            }
//            else
//            {
//                Console.WriteLine("Optimization was stopped with status = "
//                                   + optimstatus);
//                double gap = model.MIPGap;
//                Console.WriteLine("gap = " + gap.ToString());
//            }

//        }

//        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
//        {
//            base.theProblemModel = theProblemModel;
//            status = AlgorithmSolutionStatus.NotYetSolved;
//            stats.UpperBound = double.MaxValue;

//            XcplexParam = new XCPlexParameters(
//                limitComputationTime: true,
//                runtimeLimit_Seconds: algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue(),
//                optionalCPlexParameters: algorithmParameters.GetIntersectingParameters(XCPlexParameters.recognizedOptionalCplexParameters));
//        }

//        public override void SpecializedReset()
//        {
//            // Dispose of model and env
//            model.Dispose();
//            env.Dispose();
//        }

//        public override void SpecializedRun()
//        {
//            Outsource2Cplex.BuildXcplexModel(theProblemModel, XcplexParam, algorithmParameters);
//            env = new GRBEnv("output.txt");
//            model = new GRBModel(env, "model.lp");
//            model.GetEnv().Set(GRB.DoubleParam.TimeLimit, algorithmParameters.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue());
//            model.Optimize();
//        }
//    }
//}
