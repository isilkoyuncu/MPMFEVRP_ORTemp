using System.Collections.Generic;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlexParameters
    {
        public enum XCPlexRelaxation { None, LinearProgramming };
        double errorTolerance; public double ErrorTolerance { get { return errorTolerance; } }
        bool limitComputationTime; public bool LimitComputationTime { get { return limitComputationTime; } }
        XCPlexRelaxation relaxation; public XCPlexRelaxation Relaxation { get { return relaxation; } }
        Dictionary<ParameterID, Parameter> optionalCPlexParameters; public Dictionary<ParameterID, Parameter> OptionalCPlexParameters { get { return optionalCPlexParameters; } }

        public XCPlexParameters(
            double errorTolerance = 0.00001,
            bool limitComputationTime = false,
            XCPlexRelaxation relaxation = XCPlexRelaxation.None,
            Dictionary<ParameterID, Parameter> optionalCPlexParameters = null
            )
        {
            this.errorTolerance = errorTolerance;
            this.limitComputationTime = limitComputationTime;
            this.relaxation = relaxation;
            this.optionalCPlexParameters = optionalCPlexParameters;
            if (optionalCPlexParameters == null)
                this.optionalCPlexParameters = new Dictionary<ParameterID, Parameter>();
        }
    }
}
