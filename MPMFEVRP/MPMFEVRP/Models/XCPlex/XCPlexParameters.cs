using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using System.Collections.Generic;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlexParameters
    {
        public static List<ParameterID> recognizedOptionalCplexParameters = new List<ParameterID>() { ParameterID.ALG_CUTS_FACTOR, ParameterID.ALG_MIP_EMPHASIS, ParameterID.ALG_MIP_SEARCH, ParameterID.ALG_THREADS };

        double errorTolerance; public double ErrorTolerance { get { return errorTolerance; } }
        bool limitComputationTime; public bool LimitComputationTime { get { return limitComputationTime; } }
        double runtimeLimit_Seconds; public double RuntimeLimit_Seconds { get { return runtimeLimit_Seconds; } }
        XCPlexRelaxation relaxation; public XCPlexRelaxation Relaxation { get { return relaxation; } }
        bool tSP; public bool TSP { get { return tSP; } }
        VehicleCategories vehCategory; public VehicleCategories VehCategory { get { return vehCategory; } }
        bool tighterAuxBounds; public bool TighterAuxBounds{get {return tighterAuxBounds;} }
        //the additionals, for whatever need they may serve:
        Dictionary<ParameterID, InputOrOutputParameter> optionalCPlexParameters; public Dictionary<ParameterID, InputOrOutputParameter> OptionalCPlexParameters { get { return optionalCPlexParameters; } }

        public XCPlexParameters(
            double errorTolerance = 0.00001,
            bool limitComputationTime = false,
            double runtimeLimit_Seconds = double.MaxValue,
            XCPlexRelaxation relaxation = XCPlexRelaxation.None,
            bool tSP = false,
            VehicleCategories vehCategory = VehicleCategories.GDV,
            Dictionary<ParameterID, InputOrOutputParameter> optionalCPlexParameters = null,
            bool tighterAuxBounds = false
            )
        {
            this.errorTolerance = errorTolerance;
            this.limitComputationTime = limitComputationTime;
            this.runtimeLimit_Seconds = runtimeLimit_Seconds;
            this.relaxation = relaxation;
            this.optionalCPlexParameters = optionalCPlexParameters;
            this.tSP = tSP;
            this.vehCategory = vehCategory;
            if (optionalCPlexParameters == null)
                this.optionalCPlexParameters = new Dictionary<ParameterID, InputOrOutputParameter>();
            this.tighterAuxBounds = tighterAuxBounds;
        }

        public void UpdateForDirectAlgorithmUse(InputOrOutputParameterSet algParams)//This is used when all parameters of an algorithm are set directly by the user, not in a depp level automatically as part of a bigger task. Some of them may need to be passed to CPlex.
        {
            //We assume runtime seconds exists because that's a default parameter. The user, however, has a choice to enter a big-M for it!
            runtimeLimit_Seconds = algParams.GetParameter(ParameterID.ALG_RUNTIME_SECONDS).GetDoubleValue();
        }
    }
}
