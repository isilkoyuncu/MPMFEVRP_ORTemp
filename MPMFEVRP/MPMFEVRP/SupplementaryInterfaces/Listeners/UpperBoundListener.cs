﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.SupplementaryInterfaces.Listeners
{
    public interface UpperBoundListener : IListener
    {
        void OnUpperBoundUpdate(double newUpperBound);
    }
}
