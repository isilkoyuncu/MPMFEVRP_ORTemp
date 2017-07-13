using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class CustomerSetVehicleAssignment
    {
        List<CustomerSet> assigned2EV; public List<CustomerSet> Assigned2EV { get { return assigned2EV; } }
        List<CustomerSet> assigned2GDV; public List<CustomerSet> Assigned2GDV { get { return assigned2GDV; } }

        public CustomerSetVehicleAssignment()
        {
            assigned2EV = new List<CustomerSet>();
            assigned2GDV = new List<CustomerSet>();
        }

    }
}
