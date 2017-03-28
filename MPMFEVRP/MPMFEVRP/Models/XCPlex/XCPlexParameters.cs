using System.Collections.Generic;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Models.XCPlex
{
    public class XCPlexParameters
    {
        double errorTolerance; public double ErrorTolerance { get { return errorTolerance; } }
        bool limitComputationTime; public bool LimitComputationTime { get { return limitComputationTime; } }
        XCPlexRelaxation relaxation; public XCPlexRelaxation Relaxation { get { return relaxation; } }
        CustomerSet custSet; public CustomerSet CustSet { get { return custSet; } } public bool TSP { get { return (custSet != null); } }
        VehicleCategories vehCategory; public VehicleCategories VehCategory { get { return vehCategory; } }
        //the additionals, for whatever need they may serve:
        Dictionary<ParameterID, Parameter> optionalCPlexParameters; public Dictionary<ParameterID, Parameter> OptionalCPlexParameters { get { return optionalCPlexParameters; } }

        public XCPlexParameters(
            double errorTolerance = 0.00001,
            bool limitComputationTime = false,
            XCPlexRelaxation relaxation = XCPlexRelaxation.None,
            CustomerSet custSet = null,
            VehicleCategories vehCategory = VehicleCategories.GDV,
            Dictionary<ParameterID, Parameter> optionalCPlexParameters = null
            )
        {
            this.errorTolerance = errorTolerance;
            this.limitComputationTime = limitComputationTime;
            this.relaxation = relaxation;
            this.optionalCPlexParameters = optionalCPlexParameters;
            this.custSet = custSet;
            this.vehCategory = vehCategory;
            if (optionalCPlexParameters == null)
                this.optionalCPlexParameters = new Dictionary<ParameterID, Parameter>();
        }
    }
}
