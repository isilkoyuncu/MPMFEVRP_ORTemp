using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Models
{
    
    public class AlgorithmParameters
    {
        Dictionary<ParameterID, Parameter> allParameters = new Dictionary<ParameterID, Parameter>();

        public AlgorithmParameters() { }

        public Dictionary<ParameterID, Parameter> GetAllParameters()
        {
            return allParameters;
        }

        public Parameter GetParameter(ParameterID id)
        {
            return allParameters[id];
        }

        public void UpdateParameter(ParameterID id, object val)
        {
            allParameters[id].Value = val;
        }

        public void AddParameter(Parameter p)
        {
            allParameters.Add(p.ID, p);
        }
    }
}
