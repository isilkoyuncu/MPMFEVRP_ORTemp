using System.Collections.Generic;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlexParameters
    {
        public static List<Domains.AlgorithmDomain.ParameterID> recognizedOptionalCplexParameters = new List<Domains.AlgorithmDomain.ParameterID> (){ Domains.AlgorithmDomain.ParameterID.CUTS_FACTOR, Domains.AlgorithmDomain.ParameterID.MIP_EMPHASIS, Domains.AlgorithmDomain.ParameterID.MIP_SEARCH, Domains.AlgorithmDomain.ParameterID.THREADS };

        double errorTolerance; public double ErrorTolerance { get { return errorTolerance; } }
        bool limitComputationTime; public bool LimitComputationTime { get { return limitComputationTime; } }
        double runtimeLimit_Seconds; public double RuntimeLimit_Seconds { get { return runtimeLimit_Seconds; } }
        XCPlexRelaxation relaxation; public XCPlexRelaxation Relaxation { get { return relaxation; } }
        bool tSP; public bool TSP { get { return tSP; } }
        VehicleCategories vehCategory; public VehicleCategories VehCategory { get { return vehCategory; } }
        //the additionals, for whatever need they may serve:
        Dictionary<Domains.AlgorithmDomain.ParameterID, InputOrOutputParameter> optionalCPlexParameters; public Dictionary<Domains.AlgorithmDomain.ParameterID, InputOrOutputParameter> OptionalCPlexParameters { get { return optionalCPlexParameters; } }

        public XCPlexParameters(
            double errorTolerance = 0.00001,
            bool limitComputationTime = false,
            double runtimeLimit_Seconds = double.MaxValue,
            XCPlexRelaxation relaxation = XCPlexRelaxation.None,
            bool tSP = false,
            VehicleCategories vehCategory = VehicleCategories.GDV,
            Dictionary<Domains.AlgorithmDomain.ParameterID, InputOrOutputParameter> optionalCPlexParameters = null
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
                this.optionalCPlexParameters = new Dictionary<Domains.AlgorithmDomain.ParameterID, InputOrOutputParameter>();
        }

        public void UpdateForDirectAlgorithmUse(InputOrOutputParameterSet algParams)//This is used when all parameters of an algorithm are set directly by the user, not in a depp level automatically as part of a bigger task. Some of them may need to be passed to CPlex.
        {
            //We assume runtime seconds exists because that's a default parameter. The user, however, has a choice to enter a big-M for it!
            runtimeLimit_Seconds = algParams.GetParameter(Domains.AlgorithmDomain.ParameterID.RUNTIME_SECONDS).GetDoubleValue();
        }
    }
}
