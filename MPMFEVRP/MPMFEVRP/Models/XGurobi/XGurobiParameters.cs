﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MPMFEVRP.Domains.AlgorithmDomain;
//using MPMFEVRP.Domains.SolutionDomain;
//using MPMFEVRP.Domains.ProblemDomain;

//namespace MPMFEVRP.Models.XGurobi
//{
//    public class XGurobiParameters
//    {
//        public static List<ParameterID> recognizedOptionalCplexParameters = new List<ParameterID>() { ParameterID.CUTS_FACTOR, ParameterID.MIP_EMPHASIS, ParameterID.MIP_SEARCH, ParameterID.THREADS };

//        double errorTolerance; public double ErrorTolerance { get { return errorTolerance; } }
//        bool limitComputationTime; public bool LimitComputationTime { get { return limitComputationTime; } }
//        double runtimeLimit_Seconds; public double RuntimeLimit_Seconds { get { return runtimeLimit_Seconds; } }
//        XGurobiRelaxation relaxation; public XGurobiRelaxation Relaxation { get { return relaxation; } }
//        bool tSP; public bool TSP { get { return tSP; } }
//        VehicleCategories vehCategory; public VehicleCategories VehCategory { get { return vehCategory; } }
//        //the additionals, for whatever need they may serve:
//        Dictionary<ParameterID, Parameter> optionalCPlexParameters; public Dictionary<ParameterID, Parameter> OptionalCPlexParameters { get { return optionalCPlexParameters; } }

//        public XGurobiParameters(
//            double errorTolerance = 0.00001,
//            bool limitComputationTime = false,
//            double runtimeLimit_Seconds = double.MaxValue,
//            XGurobiRelaxation relaxation = XGurobiRelaxation.None,
//            bool tSP = false,
//            VehicleCategories vehCategory = VehicleCategories.GDV,
//            Dictionary<ParameterID, Parameter> optionalCPlexParameters = null
//            )
//        {
//            this.errorTolerance = errorTolerance;
//            this.limitComputationTime = limitComputationTime;
//            this.runtimeLimit_Seconds = runtimeLimit_Seconds;
//            this.relaxation = relaxation;
//            this.optionalCPlexParameters = optionalCPlexParameters;
//            this.tSP = tSP;
//            this.vehCategory = vehCategory;
//            if (optionalCPlexParameters == null)
//                this.optionalCPlexParameters = new Dictionary<ParameterID, Parameter>();
//        }

//        public void UpdateForDirectAlgorithmUse(AlgorithmParameters algParams)//This is used when all parameters of an algorithm are set directly by the user, not in a depp level automatically as part of a bigger task. Some of them may need to be passed to CPlex.
//        {
//            //We assume runtime seconds exists because that's a default parameter. The user, however, has a choice to enter a big-M for it!
//            runtimeLimit_Seconds = algParams.GetParameter(ParameterID.RUNTIME_SECONDS).GetDoubleValue();
//        }
//    }
//}