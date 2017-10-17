using System.Collections.Generic;

namespace MPMFEVRP.SupplementaryInterfaces.Listeners
{
    public interface TimeSpentAccountListener
    {
        void OnChangeOfTimeSpentAccount(Dictionary<string,double> newTimeSpentAccount);
    }
}
