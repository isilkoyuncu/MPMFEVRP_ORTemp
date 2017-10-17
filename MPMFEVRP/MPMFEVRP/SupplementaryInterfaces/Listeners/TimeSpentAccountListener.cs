using System.Collections.Generic;

namespace MPMFEVRP.SupplementaryInterfaces.Listeners
{
    public interface TimeSpentAccountListener : IListener
    {
        void OnChangeOfTimeSpentAccount(Dictionary<string,double> newTimeSpentAccount);
    }
}
