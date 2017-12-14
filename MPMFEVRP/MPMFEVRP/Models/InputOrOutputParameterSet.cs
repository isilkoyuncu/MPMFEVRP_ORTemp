using System.Collections.Generic;

namespace MPMFEVRP.Models
{

    public class InputOrOutputParameterSet
    {
        Dictionary<ParameterID, InputOrOutputParameter> allParameters = new Dictionary<ParameterID, InputOrOutputParameter>();

        public InputOrOutputParameterSet() { }

        public Dictionary<ParameterID, InputOrOutputParameter> GetAllParameters()
        {
            return allParameters;
        }

        public Dictionary<ParameterID, InputOrOutputParameter> GetIntersectingParameters(List<ParameterID> TheOtherList)
        {
            Dictionary<ParameterID, InputOrOutputParameter> outcome = new Dictionary<ParameterID, InputOrOutputParameter>();
            foreach (KeyValuePair<ParameterID,InputOrOutputParameter> entry in allParameters)
            {
                if (TheOtherList.Contains(entry.Key))
                {
                    outcome.Add(entry.Key, entry.Value);
                }
            }
            return outcome;
        }

        public InputOrOutputParameter GetParameter(ParameterID id)
        {
            return allParameters[id];
        }

        public void UpdateParameter(ParameterID id, object val)
        {
            allParameters[id].Value = val;
        }

        public void AddParameter(InputOrOutputParameter p)
        {
            allParameters.Add(p.ID, p);
        }

        public int UpdateParameters(InputOrOutputParameterSet theOtherSet)
        {
            int output = 0;
            foreach (ParameterID pid in theOtherSet.allParameters.Keys)
            {
                bool theSameKey = allParameters.ContainsKey(pid);
                bool theSameValue = allParameters[pid] != theOtherSet.allParameters[pid];
                if (allParameters.ContainsKey(pid) && allParameters[pid] != theOtherSet.allParameters[pid])
                {
                    allParameters[pid] = theOtherSet.allParameters[pid];
                    output++;
                }
            }
            return output;
        }
    }
}
