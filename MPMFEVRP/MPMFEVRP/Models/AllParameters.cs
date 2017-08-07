using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Models
{
    
    public class AllParameters
    {
        Dictionary<ParameterID, Parameter> allParameters = new Dictionary<ParameterID, Parameter>();

        public AllParameters() { }

        public Dictionary<ParameterID, Parameter> GetAllParameters()
        {
            return allParameters;
        }

        public Dictionary<ParameterID, Parameter> GetIntersectingParameters(List<ParameterID> TheOtherList)
        {
            Dictionary<ParameterID, Parameter> outcome = new Dictionary<ParameterID, Parameter>();
            foreach (KeyValuePair<ParameterID,Parameter> entry in allParameters)
            {
                if (TheOtherList.Contains(entry.Key))
                {
                    outcome.Add(entry.Key, entry.Value);
                }
            }
            return outcome;
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
